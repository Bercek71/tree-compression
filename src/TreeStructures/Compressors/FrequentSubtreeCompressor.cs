namespace TreeStructures.Compressors;

public class FrequentSubtreeCompressor<T> : ITreeCompressor<T>
{
    private readonly Dictionary<string, int> _frequency = new();
    private readonly Dictionary<string, string> _patterns = new();
    private int _patternCounter = 1;

    public string CompressTree(TreeNode<T> node)
    {
        CountSubtrees(node);
        return ReplaceFrequentSubtrees(node);
    }

    private void CountSubtrees(TreeNode<T> node)
    {
        var key = NodeToString(node);
        _frequency.TryAdd(key, 0);
        _frequency[key]++;

        foreach (var child in node.Left)
        {
            CountSubtrees(child);
        }
    }

    private string ReplaceFrequentSubtrees(TreeNode<T> node)
    {
        var key = NodeToString(node);
        if (_frequency[key] > 1 && !_patterns.ContainsKey(key))
        {
            _patterns[key] = "F" + _patternCounter++;
        }
        return _patterns.GetValueOrDefault(key, key);
    }

    public TreeNode<T> DecompressTree(string compressedTree)
    {
        throw new NotImplementedException();
    }

    private string NodeToString(TreeNode<T> node)
    {
        return node.Value?.ToString() + "(" + string.Join(",", node.Left.Select(ReplaceFrequentSubtrees)) + ")";
    }
}