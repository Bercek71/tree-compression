using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

public class TextToTreeFilter<T>(ITreeCreationStrategy<T> creationStrategy) : FilterBase<string, T>
{

    protected override T ProcessData(string text)
    {
        return creationStrategy.CreateTree(text);
    }

}