using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

/// <summary>
/// Filtr pro dekompresi stromové struktury.
/// </summary>
/// <param name="strategy">
/// Kompresní strategie, která je použita pro dekompresi stromu.
/// </param>
/// <typeparam name="T">
/// Parametr typu stromové struktury.
/// </typeparam>
public class DecompressionFilter<T>(ICompressionStrategy<T> strategy) : FilterBase<CompressedTree , T> where T : ITreeNode
{

    protected override T ProcessData(CompressedTree compressedTree)
    {
        return strategy.Decompress(compressedTree);
    }
}
