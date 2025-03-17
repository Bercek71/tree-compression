using TreeCompressionAlgorithms.TreeCreationalStrategies;
using TreeCompressionPipeline;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms;

public class NaturalLanguageTreeCompressing(ICompressionStrategy compressionStrategy) : ITreeCompressor
{
    public ICompressionStrategy CompressionStrategy { get; } = compressionStrategy;
    
    public Pipeline CompressingPipeline { get; } = new Pipeline()
        {
            ProcessObserver = new ProcessMonitor()
        }
        .AddFilter(FilterFactory.CreateTextToTreeFilter(new UdPipeCreateTreeStrategy()))
        .AddFilter(FilterFactory.CreateCompressionFilter(compressionStrategy));
    
    public Pipeline DecompressingPipeline { get; } = new Pipeline()
            {
                ProcessObserver = new ProcessMonitor()
            }
            .AddFilter(FilterFactory.CreateDecompressionFilter(compressionStrategy));
    
    
    public CompressedTree Compress(string text)
    {
        return CompressingPipeline.Process(text) as CompressedTree ?? throw new InvalidOperationException();
    }

    public string Decompress(CompressedTree compressedTree)
    {
        return DecompressingPipeline.Process(compressedTree) as string ?? throw new InvalidOperationException();
    }
}