using System.Text;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

public class TreeRepairOptimizedStrategy : ICompressionStrategy<IDependencyTreeNode>
{
    private readonly ITreeRePairEncoder _encoder;
    private readonly Dictionary<ulong, string> _hashToRule = new();
    private readonly Dictionary<string, string> _grammarRules = new();
    private int _nextNonTerminal = 256;
    private const ulong Base = 257;
    private readonly int _minFrequency;
    private readonly int _maxN;
    private readonly int _minN;

    public TreeRepairOptimizedStrategy(ITreeRePairEncoder? encoder = null, int minFrequency = 2, int maxN = 10, int minN = 2)
    {
        _encoder = encoder ?? new DepthFirstEncoder();
        _minFrequency = minFrequency;
        _maxN = maxN;
        _minN = minN;
    }

    public CompressedTree Compress(IDependencyTreeNode? tree)
    {
        ArgumentNullException.ThrowIfNull(tree);

        var sequence = _encoder.EncodeTree(tree).ToList();
        CompressSequence(sequence);

        return new CompressedTree
        {
            Structure = ConvertSequenceToByteArray(sequence),
            Metadata = _grammarRules
        };
    }

    private void CompressSequence(List<string> sequence)
    {
        // Start with larger patterns first
        for (int n = _maxN; n >= _minN; n--)
        {
            // Continue compressing with the same n until no more patterns are found
            bool compressionOccurred;
            do
            {
                compressionOccurred = CompressOnePass(sequence, n);
            } while (compressionOccurred && sequence.Count > n);
        }
    }

    private bool CompressOnePass(List<string> sequence, int n)
    {
        if (sequence.Count < n) return false;

        // Use a single-pass approach to compute hashes and frequencies
        var nGramFreq = new Dictionary<ulong, List<int>>();
        var hashPowers = new ulong[n];
        
        // Precompute powers
        hashPowers[0] = 1;
        for (int i = 1; i < n; i++)
            hashPowers[i] = hashPowers[i - 1] * Base;
        
        // Compute initial hash
        ulong hash = 0;
        for (int i = 0; i < Math.Min(n, sequence.Count); i++)
        {
            hash = hash * Base + sequence[i][0];
        }
        
        // Add the first hash
        if (sequence.Count >= n)
        {
            nGramFreq[hash] = new List<int> { 0 };
        }
        
        // Rolling hash computation - single pass O(n)
        for (int i = 1; i <= sequence.Count - n; i++)
        {
            // Remove the outgoing character and add the incoming character
            hash = (hash - (ulong)(sequence[i - 1][0]) * hashPowers[n - 1]) * Base + (ulong)(sequence[i + n - 1][0]);
            
            if (!nGramFreq.TryGetValue(hash, out var positionsList))
            {
                nGramFreq[hash] = new List<int> { i };
            }
            else
            {
                positionsList.Add(i);
            }
        }
        
        // Find the most frequent n-gram above the minimum frequency
        KeyValuePair<ulong, List<int>> best = default;
        int maxFreq = 0;
        
        foreach (var entry in nGramFreq)
        {
            if (entry.Value.Count >= _minFrequency && entry.Value.Count > maxFreq)
            {
                maxFreq = entry.Value.Count;
                best = entry;
            }
        }
        
        if (maxFreq == 0) return false;
        
        // Verify the n-gram instances are identical (in case of hash collisions)
        var candidatePositions = best.Value;
        var firstOccurrence = sequence.Skip(candidatePositions[0]).Take(n).ToList();
        
        // Filter out positions with hash collisions
        var validPositions = new List<int>();
        foreach (var pos in candidatePositions)
        {
            bool matches = true;
            for (int i = 0; i < n; i++)
            {
                if (pos + i >= sequence.Count || sequence[pos + i] != firstOccurrence[i])
                {
                    matches = false;
                    break;
                }
            }
            if (matches) validPositions.Add(pos);
        }
        
        // If we don't have enough valid positions after filtering, skip
        if (validPositions.Count < _minFrequency) return false;
        
        // Create a new rule
        var rule = string.Join(" ", firstOccurrence);
        var newSymbol = ((char)_nextNonTerminal++).ToString();
        _grammarRules[newSymbol] = rule;
        _hashToRule[best.Key] = newSymbol;
        
        // Apply the replacement in O(n) time
        ReplaceWithNewSymbol(sequence, validPositions, newSymbol, n);
        return true;
    }

    private static void ReplaceWithNewSymbol(List<string> sequence, List<int> positions, string symbol, int n)
    {
        // Sort positions in descending order so we can replace from right to left
        // This avoids index shifting issues
        positions.Sort((a, b) => b.CompareTo(a));
        
        // Replace n-grams with the new symbol (right to left)
        foreach (var position in positions)
        {
            // Remove n elements at the position
            sequence.RemoveRange(position, n);
            // Insert the new symbol at the same position
            sequence.Insert(position, symbol);
        }
    }

    public IDependencyTreeNode Decompress(CompressedTree? compressedTree)
    {
        ArgumentNullException.ThrowIfNull(compressedTree);

        _grammarRules.Clear();
        if (compressedTree.Metadata is { } rules)
        {
            foreach (var rule in rules)
                _grammarRules[rule.Key] = rule.Value;
        }

        var sequence = ConvertByteArrayToSequence(compressedTree.Structure);
        ExpandSequence(sequence);

        var index = 0;
        return DecodeTree(sequence, ref index);
    }

    private void ExpandSequence(List<string> sequence)
    {
        // Create a topological ordering of the rules to ensure correct expansion
        var ruleGraph = BuildRuleGraph();
        var orderedRules = TopologicalSort(ruleGraph);
        
        // Expand the sequence bottom-up (from leaf rules to root)
        foreach (var rule in orderedRules)
        {
            var expansion = _grammarRules[rule].Split(' ');
            
            for (int i = 0; i < sequence.Count; i++)
            {
                if (sequence[i] != rule) continue;
                
                // Replace the rule with its expansion
                sequence.RemoveAt(i);
                sequence.InsertRange(i, expansion);
                i += expansion.Length - 1;
            }
        }
    }

    private Dictionary<string, HashSet<string>> BuildRuleGraph()
    {
        var graph = new Dictionary<string, HashSet<string>>();
        
        foreach (var rule in _grammarRules)
        {
            if (!graph.ContainsKey(rule.Key))
                graph[rule.Key] = new HashSet<string>();
                
            // Find all rules referenced in this rule's expansion
            var expansion = rule.Value.Split(' ');
            foreach (var token in expansion)
            {
                if (_grammarRules.ContainsKey(token))
                {
                    // Add an edge from this rule to the referenced rule
                    graph[rule.Key].Add(token);
                }
            }
        }
        
        return graph;
    }

    private List<string> TopologicalSort(Dictionary<string, HashSet<string>> graph)
    {
        var result = new List<string>();
        var visited = new HashSet<string>();
        var temp = new HashSet<string>();
        
        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
                TopologicalSortVisit(node, graph, visited, temp, result);
        }
        
        // Reverse to get bottom-up order (leaf rules first)
        result.Reverse();
        return result;
    }

    private void TopologicalSortVisit(
        string node, 
        Dictionary<string, HashSet<string>> graph, 
        HashSet<string> visited, 
        HashSet<string> temp, 
        List<string> result)
    {
        if (temp.Contains(node))
            throw new InvalidOperationException("Grammar contains circular references");
            
        if (visited.Contains(node))
            return;
            
        temp.Add(node);
        
        if (graph.TryGetValue(node, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                TopologicalSortVisit(neighbor, graph, visited, temp, result);
            }
        }
        
        temp.Remove(node);
        visited.Add(node);
        result.Add(node);
    }

    private static IDependencyTreeNode DecodeTree(List<string> sequence, ref int index)
    {
        if (index >= sequence.Count || sequence[index] == "]") return null!;

        var value = sequence[index++];
        var node = new DependencyTreeNode(value);

        while (index < sequence.Count && sequence[index] != "]" && sequence[index] != "|")
        {
            var child = DecodeTree(sequence, ref index);
            if (child != null)
                node.AddLeftChild(child);
        }

        if (index < sequence.Count && sequence[index] == "|")
            index++;

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

    private static byte[] ConvertSequenceToByteArray(List<string> sequence)
    {
        return Encoding.UTF8.GetBytes(string.Join(" ", sequence));
    }

    private static List<string> ConvertByteArrayToSequence(byte[] byteArray)
    {
        return Encoding.UTF8.GetString(byteArray).Split(' ').ToList();
    }
}