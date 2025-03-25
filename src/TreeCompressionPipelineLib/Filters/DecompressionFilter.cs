using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

public class DecompressionFilter<T>(ICompressionStrategy<T> strategy) : FilterBase<CompressedTree , T> where T : ITreeNode
{

    protected override T ProcessData(CompressedTree compressedTree)
    {
        return strategy.Decompress(compressedTree);
    }
}
