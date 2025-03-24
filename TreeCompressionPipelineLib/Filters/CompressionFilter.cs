using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

public class CompressionFilter<T>(ICompressionStrategy<T> strategy) : FilterBase<T, CompressedTree>
{
    protected override CompressedTree ProcessData(T tree)
    {
        return strategy.Compress(tree);
    }
}