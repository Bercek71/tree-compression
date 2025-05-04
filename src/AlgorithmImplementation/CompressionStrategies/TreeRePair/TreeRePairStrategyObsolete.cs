using System.Text;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

[Obsolete("Nepoužívá se v aktuální verzi. Použijte TreeRePairStrategy.")]
public class TreeRePairStrategyObsolete : ICompressionStrategy<IDependencyTreeNode>
{
    private readonly Dictionary<string, string> _grammarRules = new();
    private int _nextNonTerminal = 256; // Start from ASCII 256 to avoid conflicts
    private const int MinPairFrequency = 2; // Minimum frequency for compression

    public CompressedTree Compress(IDependencyTreeNode? tree)
    {
        ArgumentNullException.ThrowIfNull(tree);

        // Step 1: Convert tree to linearized sequence
        var sequence = new List<string>();
        EncodeTree(tree, sequence);

        // Step 2: Apply RePair compression
        CompressSequence(sequence);

        return new CompressedTree
        {
            Structure = ConvertSequenceToByteArray(sequence),
            Metadata = _grammarRules
        };
    }

    private void CompressSequence(List<string> sequence)
    {
        while (true)
        {
            // Find all pairs with their positions
            var pairPositions = FindPairPositions(sequence);
            if (pairPositions.Count == 0) break;

            // Find the most frequent pair
            var bestPair = pairPositions
                .OrderByDescending(p => p.Value.Count)
                .ThenBy(p => p.Key) // Deterministic tie-breaking
                .First();

            var pair = bestPair.Key;
            var positions = bestPair.Value;

            var newSymbol = ((char)_nextNonTerminal).ToString();
            _nextNonTerminal++;

            // Store grammar rule
            _grammarRules[newSymbol] = pair;

            // Replace occurrences in sequence (from right to left to maintain indices)
            ReplacePositions(sequence, positions, newSymbol);
        }
    }

    public IDependencyTreeNode Decompress(CompressedTree? compressedTree)
    {
        ArgumentNullException.ThrowIfNull(compressedTree);

        // Extract grammar rules from metadata
        if (compressedTree.Metadata is Dictionary<string, string> rules)
        {
            foreach (var rule in rules)
                _grammarRules[rule.Key] = rule.Value;
        }

        // Convert byte array to sequence
        var sequence = ConvertByteArrayToSequence(compressedTree.Structure);
        
        // Expand using grammar rules (bottom-up to improve efficiency)
        ExpandSequence(sequence);

        // Convert expanded sequence back to tree
        var index = 0;
        return DecodeTree(sequence, ref index);
    }

    private void ExpandSequence(List<string> sequence)
    {
        // Process rules in reverse order of creation (from complex to simple)
        var orderedRules = _grammarRules.OrderByDescending(r => Convert.ToInt32(r.Key[0]));
        
        foreach (var rule in orderedRules)
        {
            for (int i = 0; i < sequence.Count; i++)
            {
                if (sequence[i] != rule.Key) continue;
                
                var expansion = rule.Value.Split(' ');
                sequence.RemoveAt(i);
                sequence.InsertRange(i, expansion);
                i += expansion.Length - 1; // Skip the newly inserted elements
            }
        }
    }

    private static void EncodeTree(IDependencyTreeNode node, List<string> sequence)
    {
        sequence.Add(node.Value.ToString() ?? "");

        foreach (var child in node.LeftChildren)
            EncodeTree(child, sequence);

        sequence.Add("|");

        foreach (var child in node.RightChildren)
            EncodeTree(child, sequence);

        sequence.Add("]");
    }

    private static IDependencyTreeNode DecodeTree(List<string> sequence, ref int index)
    {
        if (index >= sequence.Count || sequence[index] == "]")
            return null!;

        var value = sequence[index++];
        var node = new DependencyTreeNode(value);

        // Left children
        while (index < sequence.Count && sequence[index] != "]" && sequence[index] != "|")
        {
            var child = DecodeTree(sequence, ref index);
            if (child != null)
                node.AddLeftChild(child);
        }

        // Skip separator
        if (index < sequence.Count && sequence[index] == "|")
            index++;

        // Right children
        while (index < sequence.Count && sequence[index] != "]")
        {
            var child = DecodeTree(sequence, ref index);
            if (child != null)
                node.AddRightChild(child);
        }

        if (index < sequence.Count && sequence[index] == "]")
            index++;

        return node;
    }

    private static Dictionary<string, List<int>> FindPairPositions(List<string> sequence)
    {
        var pairPositions = new Dictionary<string, List<int>>();
        var pairFrequency = new Dictionary<string, int>();

        // First pass: count frequencies
        for (var i = 0; i < sequence.Count - 1; i++)
        {
            var pair = $"{sequence[i]} {sequence[i + 1]}";
            if (!pairFrequency.TryAdd(pair, 1))
                pairFrequency[pair]++;
        }

        // Second pass: collect positions for frequent pairs
        for (var i = 0; i < sequence.Count - 1; i++)
        {
            var pair = $"{sequence[i]} {sequence[i + 1]}";
            if (pairFrequency[pair] < MinPairFrequency) continue;
            if (!pairPositions.ContainsKey(pair))
                pairPositions[pair] = [];
            pairPositions[pair].Add(i);
        }

        return pairPositions;
    }

    private static void ReplacePositions(List<string> sequence, List<int> positions, string newSymbol)
    {
        // Process positions from right to left to avoid index shifting issues
        for (var i = positions.Count - 1; i >= 0; i--)
        {
            var pos = positions[i];
            sequence.RemoveRange(pos, 2);
            sequence.Insert(pos, newSymbol);
        }
    }

    private static byte[] ConvertSequenceToByteArray(List<string> sequence)
    {
        // Use StringBuilder for better performance with large sequences
        var sb = new StringBuilder();
        for (var i = 0; i < sequence.Count; i++)
        {
            sb.Append(sequence[i]);
            if (i < sequence.Count - 1)
                sb.Append(' ');
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static List<string> ConvertByteArrayToSequence(byte[] byteArray)
    {
        return Encoding.UTF8.GetString(byteArray).Split(' ').ToList();
    }
}