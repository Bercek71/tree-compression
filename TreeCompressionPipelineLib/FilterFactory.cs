using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.Filters;
using TreeCompressionPipeline.TreeCreationStrategies;

namespace TreeCompressionPipeline;

public abstract class FilterFactory
{
    public static IFilter CreateTextToTreeFilter(ITreeCreationStrategy strategy) => new TextToTreeFilter(strategy);
    public static IFilter CreateCompressionFilter(ICompressionStrategy strategy) => new CompressionFilter(strategy);
    public static IFilter CreateDecompressionFilter(ICompressionStrategy strategy) => new DecompressionFilter(strategy);
}
