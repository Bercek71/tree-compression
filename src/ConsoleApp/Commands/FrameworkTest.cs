#if DEBUG
using System.ComponentModel;
using ConsoleApp.Framework;
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies;
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


        var compressor = new NaturalLanguageTreeCompressing(new TreeRePairStrategy());

        //Read sentence from Resources/Texts/Test.txt file

        if (!File.Exists(InputFile))
        {
            throw new FileNotFoundException("The input file does not exist.", InputFile);
        }
        
        var testingSentence = File.ReadAllText(InputFile);
        Console.WriteLine($"Testing sentence: {testingSentence.Length}");
        var compressedTree = compressor.Compress(testingSentence);
        
        var decompressedText = compressor.Decompress(compressedTree);
        
        Console.WriteLine($"Decompressed Text: {decompressedText}");
        Console.WriteLine($"Original Sentence size: {testingSentence.Length}");
        


    }
}
#endif