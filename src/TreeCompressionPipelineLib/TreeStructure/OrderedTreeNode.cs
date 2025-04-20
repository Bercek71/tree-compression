namespace TreeCompressionPipeline.TreeStructure;

public class OrderedTreeNode(object value) : IOrderedTreeNode
{
    public IOrderedTreeNode? Parent { get; set; }
    public object Value { get; set; } = value;
    public List<IOrderedTreeNode> Children { get; } = [];

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        Accept(visitor);
        return visitor.ToString();
    }

    public void AddChild(IOrderedTreeNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public void Accept(IOrderedTreeVisitor visitor)
    {
        visitor.Visit(this);
        foreach (var child in Children)
        {
            child.Accept(visitor);
        }
    }
}