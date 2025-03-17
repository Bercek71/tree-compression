using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.TreeCreationStrategies;

public interface ITreeCreationStrategy
{
    public ITreeNode CreateTree(string text);
}