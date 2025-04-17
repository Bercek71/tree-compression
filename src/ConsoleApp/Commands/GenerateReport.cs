
using System.Globalization;
using ConsoleApp.Framework;
using ConsoleApp.Utils;
using CsvHelper;
using CsvHelper.Configuration;
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
        
        public TimeSpan TextToTreeDuration { get; set; }
        public TimeSpan CompressionTime { get; set; }
        public TimeSpan DecompressionTime { get; set; }
        public long CompressedSize { get; set; }
        
        
        
        
    }
    
    [Argument("directory", "Directory to scan for files.")]
    public string DirPath { get; set; } = string.Empty;
    
    public void Execute()
    {

        var processTimer = new ProcessTimer();
        var nlpCompressor = new NaturalLanguageTreeCompressing(new TreeRepairOptimizedStrategy(maxN: 10), processTimer);
        
        var files = Directory.GetFiles(DirPath, "*", SearchOption.AllDirectories)
            .Select(file => new FileInfo(file))
            .OrderBy(file => file.Length) // Sort by size
            .ToList();
        
        Console.WriteLine($"Found {files.Count} files in {DirPath}");
        
        var report = new List<FileReport>();
        
        var failedFiles = new List<string>();

        var stopwatch = new StopWatch();
        
        foreach (var file in files)
        {
            try
            {

                Console.WriteLine($"Processing file: {file.Name} ({file.Length} bytes)");
                var fileContent = File.ReadAllText(file.FullName);

                var compressedTree = nlpCompressor.Compress(fileContent);

                _ = nlpCompressor.Decompress(compressedTree);

                var compressionRatio = (double)compressedTree.Structure.Length / fileContent.Length;

                var compressedSize = compressedTree.Structure.Length;
                

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
                };

                report.Add(fileReport);
            }
            catch
            {
                failedFiles.Add(file.Name);
                continue;
            }
        }
        
        //write report to csv file using csv helper
        var csvFilePath = Path.Combine(DirPath, "report.csv");
        using (var writer = new StreamWriter(csvFilePath))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(report);
        }

        Console.WriteLine($"Report byl úspěšně uložen do: {csvFilePath}");
        Console.WriteLine($"Failed files: {string.Join(", ", failedFiles)}");


    }
}