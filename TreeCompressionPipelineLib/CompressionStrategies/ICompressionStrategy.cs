using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.CompressionStrategies;

public interface ICompressionStrategy
{
    CompressedTree Compress(ITreeNode? tree);
    ITreeNode Decompress(CompressedTree? compressedTree);
}
