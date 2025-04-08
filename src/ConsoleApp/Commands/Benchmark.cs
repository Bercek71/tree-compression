using System.ComponentModel;
using ConsoleApp.Framework;
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace ConsoleApp.Commands;

[Description("Benchmarking command")]
public class Benchmark : ICommand
{


    [Argument("strategy", "Strategy of compression to benchmark\n You can choose either 'TreeRePair' or 'LZ77'.",
        false)]
    public string CompressionStrategy { get; set; } = "TreeRePair";
    
    public void Execute()
    {
        Console.WriteLine("Benchmarking command executed.");
        Console.WriteLine($"Compression strategy: {CompressionStrategy}");

        ICompressionStrategy<IDependencyTreeNode> strategy;

        if (string.Equals(CompressionStrategy, "TreeRePair", StringComparison.CurrentCultureIgnoreCase))
        {
            strategy = new TreeRePairStrategy();
        }
        else
        {
            Console.WriteLine($"Unknown strategy: {CompressionStrategy}");
            throw new ArgumentException("Unknown strategy.");
        }
        
        var compressor = new NaturalLanguageTreeCompressing(strategy);

        //TODO: Get benchmark files
        //compressor.Compress();
        
        
        Console.WriteLine("Benchmarking completed.");
    }
}