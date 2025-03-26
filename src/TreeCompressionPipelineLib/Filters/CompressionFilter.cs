using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

/// <summary>
/// Filtr pro kompresi stromové struktury.
/// </summary>
/// <param name="strategy">
/// Strategie komprese stromu.
/// </param>
/// <typeparam name="T">
/// Typ stromové struktury, která bude komprimována.
/// </typeparam>
public class CompressionFilter<T>(ICompressionStrategy<T> strategy) : FilterBase<T, CompressedTree>
{
    protected override CompressedTree ProcessData(T tree)
    {
        return strategy.Compress(tree);
    }
}