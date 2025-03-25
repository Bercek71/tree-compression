using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.Filters;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

public abstract class FilterFactory<T> where T : ITreeNode
{
    public static IFilter CreateTextToTreeFilter(ITreeCreationStrategy<T> strategy) => new TextToTreeFilter<T>(strategy);
    public static IFilter CreateCompressionFilter(ICompressionStrategy<T> strategy) => new CompressionFilter<T>(strategy);
    public static IFilter CreateDecompressionFilter(ICompressionStrategy<T> strategy) => new DecompressionFilter<T>(strategy);
}
