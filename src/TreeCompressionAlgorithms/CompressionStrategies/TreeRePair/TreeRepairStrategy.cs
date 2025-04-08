using System.Text;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

public class TreeRepairStrategy(ITreeRePairEncoder? encoder = null , int minFrequency = 2, int maxN = 10, int minN = 2) : ICompressionStrategy<IDependencyTreeNode>
{
    private readonly ITreeRePairEncoder _encoder = encoder ?? new DepthFirstEncoder();
    private readonly Dictionary<string, string> _grammarRules = new();
    private int _nextNonTerminal = 256; // Start from ASCII 256 to avoid conflicts

    // Minimum frequency for compression

    public CompressedTree Compress(IDependencyTreeNode? tree)
    {
        ArgumentNullException.ThrowIfNull(tree);

        // Step 1: Convert tree to linearized sequence
        var sequence = _encoder.EncodeTree(tree).ToList();

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
        var n = maxN; // Start with n-grams of size 5
        while (n >= minN)
        {
            var nGramPositions = FindNGramPositions(sequence, n);
            if (nGramPositions.Count == 0)
            {
                n--;
                continue;
            }

            // Find the most frequent n-gram
            var bestNGram = nGramPositions
                .OrderByDescending(p => p.Value.Count)
                .ThenBy(p => p.Key) // Deterministic tie-breaking
                .First();

            var nGram = bestNGram.Key;
            var positions = bestNGram.Value;

            var newSymbol = ((char)_nextNonTerminal).ToString();
            _nextNonTerminal++;

            // Store grammar rule
            _grammarRules[newSymbol] = nGram;

            // Replace occurrences in sequence (from right to left to maintain indices)
            ReplacePositions(sequence, positions, newSymbol, n);
        }
    }

    public IDependencyTreeNode Decompress(CompressedTree? compressedTree)
    {
        ArgumentNullException.ThrowIfNull(compressedTree);

        // Extract grammar rules from metadata
        if (compressedTree.Metadata is { } rules)
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
            for (var i = 0; i < sequence.Count; i++)
            {
                if (sequence[i] != rule.Key) continue;
                
                var expansion = rule.Value.Split(' ');
                sequence.RemoveAt(i);
                sequence.InsertRange(i, expansion);
                i += expansion.Length - 1; // Skip the newly inserted elements
            }
        }
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

    private Dictionary<string, List<int>> FindNGramPositions(List<string> sequence, int n)
    {
        var nGramPositions = new Dictionary<string, List<int>>();
        var nGramFrequency = new Dictionary<string, int>();

        // First pass: count frequencies
        for (var i = 0; i <= sequence.Count - n; i++)
        {
            var nGram = string.Join(" ", sequence.Skip(i).Take(n));
            if (!nGramFrequency.TryAdd(nGram, 1))
                nGramFrequency[nGram]++;
        }

        // Second pass: collect positions for frequent n-grams
        for (var i = 0; i <= sequence.Count - n; i++)
        {
            var nGram = string.Join(" ", sequence.Skip(i).Take(n));
            if (nGramFrequency[nGram] < minFrequency) continue;
            if (!nGramPositions.ContainsKey(nGram))
                nGramPositions[nGram] = new List<int>();
            nGramPositions[nGram].Add(i);
        }

        return nGramPositions;
    }

    private static void ReplacePositions(List<string> sequence, List<int> positions, string newSymbol, int n)
    {
        // Process positions from right to left to avoid index shifting issues
        for (var i = positions.Count - 1; i >= 0; i--)
        {
            var pos = positions[i];
            sequence.RemoveRange(pos, n);
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