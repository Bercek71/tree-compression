namespace TreeStructure;

public class TreeNode<T>(T value)
{
    public List<TreeNode<T>>? Left { get; set; }
    public List<TreeNode<T>>? Right { get; set; }
    public T Value { get; set; } = value;
}