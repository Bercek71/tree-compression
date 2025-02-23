using System.Text;

namespace TreeStructures;


public class GrammarCompressor<T>
{
    private readonly Dictionary<string, string> _grammarRules = new();
    private readonly Dictionary<string, TreeNode<T>> _uniqueSubtrees = new();
    private int _ruleCounter = 1;

    public string CompressTree(TreeNode<T> node)
    {
        // Compress left and right children first
        var leftCompressed = node.Left.Select(CompressTree).ToList();
        var rightCompressed = node.Right.Select(CompressTree).ToList();
        
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(node.Value);
        if (leftCompressed.Count == 0)
        {
            stringBuilder.Append($"|L:{string.Join(",", leftCompressed)}");
        }
        if (rightCompressed.Count == 0)
        {
            stringBuilder.Append($"|R:{string.Join(",", rightCompressed)}");
        }

        var subtreeKey = stringBuilder.ToString(); 
        

        // If already compressed, return the rule
        if (_uniqueSubtrees.ContainsKey(subtreeKey))
        {
            return _grammarRules[subtreeKey];
        }
        else
        {
            // Assign a new rule for this subtree
            var rule = "R" + _ruleCounter++;
            _grammarRules[subtreeKey] = rule;
            _uniqueSubtrees[subtreeKey] = node;
            return rule;
        }
    }

    public void ToBinaryFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(stream);
        // Write the rule counter
        writer.Write(_ruleCounter);

        // Write the number of grammar rules
        writer.Write(_grammarRules.Count);

        // Write each grammar rule
        foreach (var rule in _grammarRules)
        {
            WriteString(writer, rule.Key);   // Writing the key (subtree structure)
            WriteString(writer, rule.Value); // Writing the rule (compressed representation)
        }
    }
    
    private static void WriteString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length);    // Write the length of the string
        writer.Write(bytes);           // Write the string as bytes
    }
    
    public void PrintGrammar()
    {
        Console.WriteLine("Generated Grammar Rules:");
        foreach (var rule in _grammarRules)
        {
            Console.WriteLine($"{rule.Value} -> {rule.Key}");
        }
    }
}