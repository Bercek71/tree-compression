using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Filters;

public class TextToTreeFilter(ITreeCreationStrategy creationStrategy) : FilterBase<string, ITreeNode>
{

    protected override ITreeNode ProcessData(string text)
    {
        return creationStrategy.CreateTree(text);
    }

}