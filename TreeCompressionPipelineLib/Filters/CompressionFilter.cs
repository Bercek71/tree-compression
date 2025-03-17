using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

public class CompressionFilter(ICompressionStrategy strategy) : FilterBase<ITreeNode, CompressedTree>
{
    protected override CompressedTree ProcessData(ITreeNode tree)
    {
        return strategy.Compress(tree);
    }

}