using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies;
using TreeCompressionPipeline;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.Filters;

namespace ConsoleApp.Commands;

public class FrameworkTest : BaseCommand
{
    public override void Execute(object? parameter)
    {
        Console.WriteLine("FrameworkTest");

        var pipeline = new Pipeline()
        {
            ProcessObserver = new ProcessMonitor()
        };


        var compressor = new NaturalLanguageTreeCompressing(new TreeRepairStrategy());

        //Read sentence from Resources/Texts/Test.txt file
        var testingSentence = File.ReadAllText(Path.Combine("Resources", "Texts", "old-man-and-the-sea.txt"));
        Console.WriteLine($"Testing sentence: {testingSentence.Length}");
        var compressedTree = compressor.Compress(testingSentence);
        
        var decompressedText = compressor.Decompress(compressedTree);
        
        Console.WriteLine($"Decompressed Text: {decompressedText}");
        Console.WriteLine($"Original Sentence size: {testingSentence.Length}");
        


    }
}