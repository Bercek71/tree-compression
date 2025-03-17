namespace TreeCompressionPipeline.TreeStructure;

public interface ITreeNode
{
    object Value { get; }
    IList<ITreeNode> Children { get; }
    void AddChild(ITreeNode child);
    void Accept(ITreeVisitor visitor);
}
