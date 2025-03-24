using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

public interface ITreeCompressor<T> where T : ITreeNode
{
    
    protected ICompressionStrategy<T> CompressionStrategy { get; }
    protected Pipeline CompressingPipeline { get; }
    protected Pipeline DecompressingPipeline { get; }
    
    CompressedTree Compress(string text);
    string Decompress(CompressedTree compressedTree);
}