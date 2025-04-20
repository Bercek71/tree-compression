namespace TreeCompressionPipeline.TreeStructure;

public class XmlTreeNode(string name) : IOrderedTreeNode
{
    public List<IOrderedTreeNode> Children { get; } = [];
    public IOrderedTreeNode? Parent { get; set; }

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

    public object Value { get; set; } = name;
}