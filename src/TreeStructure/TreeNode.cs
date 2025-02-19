namespace TreeStructure;

public class TreeNode<T>(T value)
{
    public TreeNode<T>? Left { get; set; }
    public TreeNode<T>? Right { get; set; }
    public T Value { get; set; } = value;
}