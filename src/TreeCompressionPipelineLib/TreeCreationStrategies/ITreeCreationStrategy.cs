using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.TreeCreationStrategies;

public interface ITreeCreationStrategy<T>
{
    public T CreateTree(string text);
}