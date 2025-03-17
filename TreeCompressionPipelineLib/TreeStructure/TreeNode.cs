namespace TreeCompressionPipeline.TreeStructure;

// Tree structure
public class TreeNode(object value) : ITreeNode
{
    public object Value { get; init; } = value;
    public IList<ITreeNode> Children { get; } = new List<ITreeNode>();

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        Accept(visitor);
        return visitor.ToString();
    }

    public void AddChild(ITreeNode child)
    {
        Children.Add(child);
    }

    public void Accept(ITreeVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
        {
            child.Accept(visitor);
        }
    }
}