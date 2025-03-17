namespace TreeCompressionPipeline.TreeStructure;

public class XmlTreeNode(string name, object value) : ITreeNode
{
    public object Value { get; } = value;
    public string Name { get; } = name;

    public IList<ITreeNode> Children { get; } = [];
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