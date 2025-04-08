using TreeCompressionAlgorithms.TreeCreationalStrategies;
using TreeCompressionPipeline;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms;

public class NaturalLanguageTreeCompressing(ICompressionStrategy<IDependencyTreeNode> compressionStrategy) : ITreeCompressor<IDependencyTreeNode>
{
    public ICompressionStrategy<IDependencyTreeNode> CompressionStrategy { get; } = compressionStrategy;
    
    public Pipeline CompressingPipeline { get; } = new Pipeline()
        {
            ProcessObserver = new ProcessMonitor()
        }
        .AddFilter(FilterFactory<IDependencyTreeNode>.CreateTextToTreeFilter(new UdPipeCreateTreeStrategy()))
        .AddFilter(FilterFactory<IDependencyTreeNode>.CreateCompressionFilter(compressionStrategy));
    
    public Pipeline DecompressingPipeline { get; } = new Pipeline()
            {
                ProcessObserver = new ProcessMonitor()
            }
            .AddFilter(FilterFactory<IDependencyTreeNode>.CreateDecompressionFilter(compressionStrategy));
    
    
    public CompressedTree Compress(string text)
    {
        return CompressingPipeline.Process(text) as CompressedTree ?? throw new InvalidOperationException();
    }

    public CompressedTree Compress(Stream stream)
    {
        var text = new StreamReader(stream).ReadToEnd();
        return Compress(text);
    }

    public string Decompress(CompressedTree compressedTree)
    {
        var reuslt = DecompressingPipeline.Process(compressedTree) as IDependencyTreeNode ?? throw new InvalidOperationException();
        return reuslt.ToString() ?? "";
    }
}