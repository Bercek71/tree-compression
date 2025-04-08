using System.Text;

namespace TreeCompressionPipeline.TreeStructure;

public class DependencyTreeNode(string value) : IDependencyTreeNode
{
    public object Value { get; } = value;
    public List<IDependencyTreeNode> LeftChildren { get; } = [];
    public List<IDependencyTreeNode> RightChildren { get; } = [];

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        foreach (var leftChild in LeftChildren)
        {
            stringBuilder.Append(leftChild.ToString());
            if (leftChild is DependencyTreeNode node)
            {
                if(((List<string>)[".", ",", "\""]).Contains((string)leftChild.Value))
                {
                    continue;
                }
            }
            stringBuilder.Append(' ');
        }

        if ((string)Value != "<DocumentRoot>" && (string)Value != "<root>")
        {
            stringBuilder.Append(Value);
        }

        if (RightChildren.Count > 0)
        {
            stringBuilder.Append(' ');
        }

        foreach (var rightChild in RightChildren)
        {
            stringBuilder.Append(rightChild.ToString());
            stringBuilder.Append(' ');
        }

        return stringBuilder.ToString();
    }

    public void AddLeftChild(IDependencyTreeNode child)
    {
        child.Parent = this;
        LeftChildren.Add(child);
    }

    public void AddRightChild(IDependencyTreeNode child)
    {
        child.Parent = this;
        RightChildren.Add(child);
    }

    public void Accept(ISyntacticTreeVisitor visitor)
    {
        visitor.Visit(this);
    }

    public IDependencyTreeNode? Parent { get; set; }
}