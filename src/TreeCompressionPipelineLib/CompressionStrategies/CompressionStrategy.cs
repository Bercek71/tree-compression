using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.CompressionStrategies;

[Obsolete("Ukázková třída, která byla použita pro ukázání principu komprese stromové struktury.")]
public abstract class CompressionStrategy : ICompressionStrategy<ITreeNode>
{
    protected Dictionary<string, int> FindPatterns(ITreeNode tree)
    {
        return new Dictionary<string, int>(); // Implement pattern finding logic
    }

    public abstract CompressedTree Compress(ITreeNode tree);
    public abstract ITreeNode Decompress(CompressedTree compressedTree);
}