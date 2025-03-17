using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

public class DecompressionFilter(ICompressionStrategy strategy) : FilterBase<CompressedTree , ITreeNode>
{

    protected override ITreeNode ProcessData(CompressedTree compressedTree)
    {
        
        return strategy.Decompress(compressedTree);
    }
}
