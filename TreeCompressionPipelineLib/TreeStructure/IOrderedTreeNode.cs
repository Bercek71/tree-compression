namespace TreeCompressionPipeline.TreeStructure;

public interface IOrderedTreeNode : ITreeNode
{
    List<IOrderedTreeNode> Children { get; }
    IOrderedTreeNode? Parent { get; set; }
    void AddChild(IOrderedTreeNode child);
    void Accept(IOrderedTreeVisitor visitor);
}