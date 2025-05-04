using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

public interface ITreeRePairEncoder
{
    public IEnumerable<string> EncodeTree(IDependencyTreeNode node);
}