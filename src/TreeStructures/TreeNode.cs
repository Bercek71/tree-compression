namespace TreeStructures;

public class TreeNode<T>(T value)
{
    public List<TreeNode<T>> Left { get;  } = [];
    public List<TreeNode<T>> Right { get; } = [];
    public T Value { get; } = value;

    public void Add(TreeNode<T> node, Direction direction)
    {
        if (Direction.Left == direction)
        {
            Left?.Add(node);            
        }
        else
        {
            Right?.Add(node);
        }
    }
}