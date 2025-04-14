//#if DEBUG
using System.ComponentModel;
using ConsoleApp.Framework;
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.Filters;

namespace ConsoleApp.Commands;

[Description("Framework testing command")]
public class FrameworkTest : ICommand
{
    [Argument("input", "The input string to compress")]
    public required string InputFile { get; set; }

    public void Execute()
    {
        Console.WriteLine($"Input file: {InputFile}");
        Console.WriteLine("FrameworkTest");


        var compressor = new NaturalLanguageTreeCompressing(new TreeRepairOptimizedStrategy(maxN: 10));

        if (!File.Exists(InputFile))
        {
            throw new FileNotFoundException("The input file does not exist.", InputFile);
        }
        
        var testingSentence = File.ReadAllText(InputFile);
        Console.WriteLine($"Testing sentence: {testingSentence.Length}");
        var compressedTree = compressor.Compress(testingSentence);
        
        var decompressedText = compressor.Decompress(compressedTree);
        
        
        Console.WriteLine($"Decompressed Text: {decompressedText}");
        
        Console.WriteLine("Decompressed Text size: " + decompressedText.Length);
        Console.WriteLine("Compressed tree size: " + compressedTree.Structure.Length);
        Console.WriteLine("Uncompressed tree size: " + testingSentence.Length);
        Console.WriteLine("Compressing ratio: " + (double)compressedTree.Structure.Length / testingSentence.Length);
        


    }
}
//#endif