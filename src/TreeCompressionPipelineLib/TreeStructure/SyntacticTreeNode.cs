using System.Text;

namespace TreeCompressionPipeline.TreeStructure;

public class SyntacticTreeNode(string value) : ISyntacticTreeNode
{
    public object Value { get; } = value;
    public List<ISyntacticTreeNode> LeftChildren { get; } = [];
    public List<ISyntacticTreeNode> RightChildren { get; } = [];

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        foreach (var leftChild in LeftChildren)
        {
            stringBuilder.Append(leftChild.ToString());
            stringBuilder.Append(' ');
        }

        stringBuilder.Append(Value);
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

    public void AddLeftChild(ISyntacticTreeNode child)
    {
        child.Parent = this;
        LeftChildren.Add(child);
    }

    public void AddRightChild(ISyntacticTreeNode child)
    {
        child.Parent = this;
        RightChildren.Add(child);
    }

    public void Accept(ISyntacticTreeVisitor visitor)
    {
        visitor.Visit(this);
    }

    public ISyntacticTreeNode? Parent { get; set; }
}