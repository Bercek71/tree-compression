using TreeCompressionAlgorithms.TreeCreationalStrategies;
using TreeCompressionPipeline;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms;

public class NaturalLanguageTreeCompressing(ICompressionStrategy<ISyntacticTreeNode> compressionStrategy) : ITreeCompressor<ISyntacticTreeNode>
{
    public ICompressionStrategy<ISyntacticTreeNode> CompressionStrategy { get; } = compressionStrategy;
    
    public Pipeline CompressingPipeline { get; } = new Pipeline()
        {
            ProcessObserver = new ProcessMonitor()
        }
        .AddFilter(FilterFactory<ISyntacticTreeNode>.CreateTextToTreeFilter(new UdPipeCreateTreeStrategy()))
        .AddFilter(FilterFactory<ISyntacticTreeNode>.CreateCompressionFilter(compressionStrategy));
    
    public Pipeline DecompressingPipeline { get; } = new Pipeline()
            {
                ProcessObserver = new ProcessMonitor()
            }
            .AddFilter(FilterFactory<ISyntacticTreeNode>.CreateDecompressionFilter(compressionStrategy));
    
    
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
        var reuslt = DecompressingPipeline.Process(compressedTree) as ISyntacticTreeNode ?? throw new InvalidOperationException();
        return reuslt.ToString() ?? "";
    }
}