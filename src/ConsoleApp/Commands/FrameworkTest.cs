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


        var compressor = new XmlTreeCompressing(new TreeRepairStrategy());

        var testingSentence = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<library>\n    <book>\n        <title>Introduction to XML</title>\n        <author>John Doe</author>\n        <year>2021</year>\n        <genre>Educational</genre>\n    </book>\n    <book>\n        <title>Advanced XML Techniques</title>\n        <author>Jane Smith</author>\n        <year>2023</year>\n        <genre>Technical</genre>\n    </book>\n</library>";
        var compressedTree = compressor.Compress(testingSentence);
        
        var decompressedText = compressor.Decompress(compressedTree);
        
        Console.WriteLine($"Decompressed Text: {decompressedText}");
        Console.WriteLine($"Original Sentence size: {testingSentence.Length}");
        


    }
}