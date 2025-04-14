
using System.Globalization;
using ConsoleApp.Framework;
using CsvHelper;
using CsvHelper.Configuration;
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

namespace ConsoleApp.Commands;

public class GenerateReport : ICommand
{
    private class FileReport
    {
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Type { get; set; } = string.Empty; //subfolder name
        public double CompressionRatio { get; set; }
        public TimeSpan CompressionTime { get; set; }
        public TimeSpan DecompressionTime { get; set; }
        public long CompressedSize { get; set; }
        
        
    }
    
    [Argument("directory", "Directory to scan for files.")]
    public string DirPath { get; set; } = string.Empty;
    
    public void Execute()
    {
        //Read all the files from directory and subdirectories
        //sort all found files by size
        
        //generate csv report put there 
        //file name, size, compression ratio, compression time, decompression time, compressed size, tree size
        
        var NLPCompressor = new NaturalLanguageTreeCompressing(new TreeRepairStrategy(maxN: 10));
        
        var files = Directory.GetFiles(DirPath, "*", SearchOption.AllDirectories)
            .Select(file => new FileInfo(file))
            .OrderByDescending(file => file.Length) // Sort by size
            .ToList();
        
        Console.WriteLine($"Found {files.Count} files in {DirPath}");
        
        var report = new List<FileReport>();
        
        var faildedFiles = new List<string>();
        
        foreach (var file in files)
        {
            try
            {

                Console.WriteLine($"Processing file: {file.Name} ({file.Length} bytes)");
                // Here you would typically perform compression and decompression
                //read file content
                var fileContent = File.ReadAllText(file.FullName);
                //compress
                var startTime = DateTime.Now;
                var compressedTree = NLPCompressor.Compress(fileContent);
                var compressionTime = DateTime.Now - startTime;
                //decompress
                startTime = DateTime.Now;
                var decompressedText = NLPCompressor.Decompress(compressedTree);
                var decompressionTime = DateTime.Now - startTime;
                //calculate compression ratio
                var compressionRatio = (double)compressedTree.Structure.Length / fileContent.Length;
                //calculate compression time
                var compressedSize = compressedTree.Structure.Length;


                var fileReport = new FileReport
                {
                    FileName = file.Name,
                    Size = file.Length,
                    Type = file.DirectoryName ?? string.Empty,
                    CompressionRatio = compressionRatio,
                    CompressionTime = compressionTime,
                    DecompressionTime = decompressionTime,
                    CompressedSize = compressedSize,
                };

                report.Add(fileReport);
            }
            catch
            {
                faildedFiles.Add(file.Name);
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
        Console.WriteLine($"Failed files: {string.Join(", ", faildedFiles)}");


    }
}