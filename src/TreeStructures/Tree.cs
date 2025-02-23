namespace TreeStructures;

public class Tree<T>(T rootValue)
{
    public TreeNode<T> Root { get; set; } = new(rootValue);


    public TreeNode<T>? Find(T value)
    {
        foreach (var node in Root.Left)
        {
            if (node.Value != null && node.Value.Equals(value))
                return node;
        }

        {
            foreach (var node in Root.Right)
            {
                if (node.Value != null && node.Value.Equals(value))
                    return node;
            }
        }

        return null;
    }
    
    private void PrintTree(TreeNode<T> node, string indent = "", string relation = "Root")
    {
        Console.WriteLine($"{indent}{relation}: {node.Value}");

        // Print left children first
        for (var i = 0; i < node.Left.Count; i++)
        {
            var newIndent = indent + (i == node.Left.Count - 1 && node.Right.Count == 0 ? "    " : "│   ");
            PrintTree(node.Left[i], newIndent, "(L)");
        }

        // Print right children next
        for (var i = 0; i < node.Right.Count; i++)
        {
            var newIndent = indent + (i == node.Right.Count - 1 ? "    " : "│   ");
            PrintTree(node.Right[i], newIndent, "(R)");
        }
    }

    public void PrintTree()
    {
        PrintTree(Root);
    }

}