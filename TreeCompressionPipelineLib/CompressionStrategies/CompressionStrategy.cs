using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.CompressionStrategies;

// Compression Strategies
public abstract class CompressionStrategy : ICompressionStrategy
{
    protected Dictionary<string, int> FindPatterns(ITreeNode tree)
    {
        return new Dictionary<string, int>(); // Implement pattern finding logic
    }

    public abstract CompressedTree Compress(ITreeNode tree);
    public abstract ITreeNode Decompress(CompressedTree compressedTree);
}