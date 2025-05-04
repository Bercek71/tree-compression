using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

public class DepthFirstEncoder : ITreeRePairEncoder
{
    private void EncodeTreeRec(IDependencyTreeNode node, List<string> sequence)
    {
        sequence.Add(node.Value.ToString() ?? "");

        foreach (var child in node.LeftChildren)
            EncodeTreeRec(child, sequence);

        sequence.Add("|");

        foreach (var child in node.RightChildren)
            EncodeTreeRec(child, sequence);

        sequence.Add("]");
    }

    public IEnumerable<string> EncodeTree(IDependencyTreeNode node)
    {
        var sequence = new List<string>();
        EncodeTreeRec(node, sequence);
        return sequence;
    }
}