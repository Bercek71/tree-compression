using System.Diagnostics;
using System.Globalization;
using ConsoleApp.Framework;
using ConsoleApp.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using Spectre.Console;
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline;

namespace ConsoleApp.Commands;

public class GenerateReport : ICommand
{
    private class FileReport
    {
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Type { get; set; } = string.Empty;
        public double CompressionRatio { get; set; }
        
        public long EncodedTreeSize { get; set; }
        public TimeSpan TextToTreeDuration { get; set; }
        public TimeSpan CompressionTime { get; set; }
        public TimeSpan DecompressionTime { get; set; }
        public int CompressedSize { get; set; }
    }

    [Argument("directory", "Directory to scan for files.")]
    private string DirPath { get; set; } = string.Empty;

    [Argument("output", "Output directory for the report.", false)]
    private string OutputDir { get; set; } = string.Empty;

    public void Execute()
    {
        if (string.IsNullOrEmpty(DirPath))
        {
            throw new ArgumentNullException(nameof(DirPath), "Directory path cannot be null or empty.");
        }

        var processTimer = new ProcessTimer();
        var nlpCompressor = new NaturalLanguageTreeCompressing(new TreeRepairOptimizedStrategy(maxN: 10), processTimer);

        // Display initialization message
        AnsiConsole.MarkupLine("[yellow]Initializing compression engine...[/]");

        // Scan directory - show this with a spinner
        AnsiConsole.Status()

            .Start("Scanning directory...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));

                // Get and sort files
                var files = Directory.GetFiles(DirPath, "*", SearchOption.AllDirectories)
                    .Select(file => new FileInfo(file))
                    .OrderByDescending(file => file.Length)
                    .ToList();

                Thread.Sleep(500); // Small visual delay for feedback

                // Show result outside of Status context to avoid nesting issues
                return files;
            });

        // Get files again outside of the Status context
        var files = Directory.GetFiles(DirPath, "*", SearchOption.AllDirectories)
            .Select(file => new FileInfo(file))
            .OrderByDescending(file => file.Length)
            .ToList();

        // Show file scan results
        AnsiConsole.WriteLine();
        var panel = new Panel($"Found [bold yellow]{files.Count}[/] files in [blue]{DirPath}[/]")
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 0),
            BorderStyle = new Style(Color.Green)
        };

        var statusMessage = new Rows(
            new Markup("[green]✓[/] [bold]File Scan Complete[/]"),
            panel
        );

        AnsiConsole.Write(statusMessage);
        AnsiConsole.WriteLine();

        // Prepare for processing
        var report = new List<FileReport>();
        var failedFiles = new List<string>();

        // Use progress display for file processing (separate from previous Status)
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Starting file processing...[/]");
        AnsiConsole.WriteLine();

        var progress = AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(), // Task description
                new ProgressBarColumn(), // Progress bar
                new PercentageColumn(), // Percentage
                new RemainingTimeColumn(), // Remaining time
                new SpinnerColumn(), // Spinner
            });

        progress.Start(ctx =>
        {
            // Add tasks
            var overallTask = ctx.AddTask("[green]Processing Files[/]", maxValue: files.Count);

            var fileCounter = 0;

            foreach (var file in files)
            {
                fileCounter++;

                var displayNameRaw = file.Name.Length > 20
                    ? string.Concat(file.Name.AsSpan(0, 17), "...")
                    : file.Name;
                var safeDisplayName = Markup.Escape(displayNameRaw);
                overallTask.Description = $"[green]Processing {fileCounter}/{files.Count}: {safeDisplayName}[/]";

                try
                {
                    var fileContent = File.ReadAllText(file.FullName);
                    
                    if(fileContent.Length == 0) 
                    {
                        failedFiles.Add(file.Name);
                        overallTask.Increment(1);
                        continue;
                    }

                    var compressedTree = nlpCompressor.Compress(fileContent);

                    var decompressed = nlpCompressor.Decompress(compressedTree);

                    var compressedSize = compressedTree.GetSize();

                    var compressionRatio = (double) compressedTree.GetSize() / fileContent.Length;

                    var encoder = new DepthFirstEncoder();
                    
                    if (processTimer.Node != null)
                    {
                        //assert equality with decompressed
                        if (processTimer.Node.ToString() != decompressed)
                        {
                            throw new InvalidOperationException("Decompressed text does not match original.");
                        }
                    }

                    Debug.Assert(processTimer.Node != null, "processTimer.Node != null");
                    var fileReport = new FileReport
                    {
                        FileName = file.Name,
                        Size = file.Length,
                        Type = file.DirectoryName ?? string.Empty,
                        CompressionRatio = compressionRatio,
                        CompressionTime = processTimer[ProcessType.CompressionFilter],
                        DecompressionTime = processTimer[ProcessType.DecompressionFilter],
                        TextToTreeDuration = processTimer[ProcessType.TextToTreeFilter],
                        CompressedSize = compressedSize,
                        
                        EncodedTreeSize = encoder.EncodeTree(processTimer.Node).Sum(x => x.Length)
                    };

                    report.Add(fileReport);
                }
                catch
                {
                    failedFiles.Add(file.Name);
                }

                overallTask.Increment(1);
            }
        });

        // Summarize the report data with a table
        if (report.Count > 0)
        {
            AnsiConsole.WriteLine();

            var summaryTable = new Table()
                .Title("[yellow]Compression Summary[/]")
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green);

            summaryTable.AddColumn(new TableColumn("Metric").Centered());
            summaryTable.AddColumn(new TableColumn("Value").Centered());

            // Calculate metrics
            var avgCompressionRatio = report.Average(r => r.CompressionRatio);
            var maxCompression = report.Min(r => r.CompressionRatio);
            var totalSizeBefore = report.Sum(r => r.Size);
            var totalSizeAfter = report.Sum(r => r.CompressedSize);
            var avgCompressionTime = new TimeSpan((long)report.Average(r => r.CompressionTime.Ticks));

            summaryTable.AddRow("Files Processed", report.Count.ToString());
            summaryTable.AddRow("Files Failed", failedFiles.Count.ToString());
            summaryTable.AddRow("Avg. Compression Ratio", $"{avgCompressionRatio:P2}");
            summaryTable.AddRow("Best Compression", $"{maxCompression:P2}");
            summaryTable.AddRow("Total Size Before", $"{BytesToString(totalSizeBefore)}");
            summaryTable.AddRow("Total Size After", $"{BytesToString(totalSizeAfter)}");
            summaryTable.AddRow("Total Saved", $"{BytesToString(totalSizeBefore - totalSizeAfter)}");
            summaryTable.AddRow("Avg. Compression Time", $"{avgCompressionTime.TotalMilliseconds:F1} ms");
            summaryTable.AddRow("Best Compression", $"{maxCompression:F1} ms");
            summaryTable.AddRow("Avg. Decompression Time",
                $"{report.Average(r => r.DecompressionTime.TotalMilliseconds):F1} ms");

            AnsiConsole.Write(summaryTable);
        }

        //write report to csv file using csv helper
        var csvFilePath = Path.Combine(DirPath, "report.csv");

        if (!string.IsNullOrEmpty(OutputDir))
        {
            csvFilePath = OutputDir;
        }

        using (var writer = new StreamWriter(csvFilePath))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(report);
        }

        // Show success message with saving info
        AnsiConsole.WriteLine();
        var savePanel = new Panel($"Report saved to: [blue]{csvFilePath}[/]")
        {
            Border = BoxBorder.Double,
            Padding = new Padding(1, 0),
            BorderStyle = new Style(Color.Green)
        };

        // Show failed files if any
        if (failedFiles.Count > 0)
        {
            AnsiConsole.WriteLine();

            if (failedFiles.Count < 5)
            {
                var failedFilesTable = new Table().Border(TableBorder.Rounded).BorderColor(Color.Red);
                failedFilesTable.Title("[yellow]Failed files[/]");
                failedFilesTable.AddColumn("File Name");
                foreach (var failedFile in failedFiles)
                {
                    failedFilesTable.AddRow(failedFile);
                }

                AnsiConsole.Write(failedFilesTable);
            }
            else
            {
                AnsiConsole.Write(
                    new Panel($"[bold]Failed files: {failedFiles.Count}/{files.Count}[/]").BorderColor(Color.Red));
            }
        }

        AnsiConsole.Write(savePanel);
    }


    // Helper method to format bytes to human-readable format
    private static string BytesToString(long byteCount)
    {
        string[] suf = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        if (byteCount == 0)
            return "0" + suf[0];
        var bytes = Math.Abs(byteCount);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        var num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return $"{(Math.Sign(byteCount) * num)}{suf[place]}";
    }
}