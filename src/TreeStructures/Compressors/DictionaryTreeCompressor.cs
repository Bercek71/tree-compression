namespace TreeStructures.Compressors;

public class DictionaryTreeCompressor<T> : ITreeCompressor<T>
{
    private readonly Dictionary<string, string> _dictionary = new();
    private int _idCounter = 1;

    public string CompressTree(TreeNode<T> node)
    {
        return EncodeNode(node);
    }

    private string EncodeNode(TreeNode<T> node)
    {
        var structure = NodeToString(node);
        if (_dictionary.TryGetValue(structure, out var value)) return value;
        value = "D" + _idCounter++;
        _dictionary[structure] = value;
        return value;
    }

    public TreeNode<T> DecompressTree(string compressedTree)
    {
        throw new NotImplementedException();
    }

    private string NodeToString(TreeNode<T> node)
    {
        return node.Value?.ToString() + "(" + string.Join(",", node.Left.Select(EncodeNode)) + ")";
    }
}