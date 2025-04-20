using System.Text;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;

public class TreeRepairOptimizedStrategy(
    ITreeRePairEncoder? encoder = null,
    int minFrequency = 2,
    int maxN = 10,
    int minN = 2)
    : ICompressionStrategy<IDependencyTreeNode>
{
    private readonly ITreeRePairEncoder _encoder = encoder ?? new DepthFirstEncoder();
    private readonly Dictionary<string, string> _grammarRules = new();
    private int _nextNonTerminal = 256; // Start from ASCII 256 to avoid conflicts

    public CompressedTree Compress(IDependencyTreeNode? tree)
    {
        ArgumentNullException.ThrowIfNull(tree);

        // Clear any previous rules
        _grammarRules.Clear();

        // Step 1: Convert tree to linearized sequence
        var sequence = _encoder.EncodeTree(tree).ToList();

        // Step 2: Apply RePair compression
        CompressSequence(sequence);

        return new CompressedTree
        {
            Structure = ConvertSequenceToByteArray(sequence),
            Metadata = new Dictionary<string, string>(_grammarRules) // Create a copy
        };
    }

    private void CompressSequence(List<string> sequence)
    {
        var n = maxN;
        
        while (n >= minN)
        {
            if (n > sequence.Count)
            {
                n--;
                continue;
            }
            
            // Find most frequent n-gram in linear time
            var (nGram, positions) = FindMostFrequentNGram(sequence, n);
            
            // If no frequent n-gram found, reduce n and try again
            if (nGram == null || positions.Count < minFrequency)
            {
                n--;
                continue;
            }
            
            // Create a new symbol for this n-gram
            var newSymbol = ((char)_nextNonTerminal++).ToString();
            _grammarRules[newSymbol] = nGram;
            
            // Replace all occurrences in linear time
            ReplaceOccurrences(sequence, positions, n, newSymbol);
            
            // Keep trying with same n if we found a replacement
        }
    }
    
    // Linear time n-gram frequency counter using hashing
    private (string? nGram, List<int> positions) FindMostFrequentNGram(List<string> sequence, int n)
    {
        // No n-grams if sequence is too short
        if (sequence.Count < n) return (null, new List<int>());
        
        var nGramMap = new Dictionary<string, List<int>>();
        
        // Use a sliding window for linear-time n-gram detection
        var window = new List<string>(n);
        
        // Prime the window with first n-1 elements
        for (var i = 0; i < n - 1 && i < sequence.Count; i++)
        {
            window.Add(sequence[i]);
        }
        
        // Slide window through sequence in linear time
        for (var i = n - 1; i < sequence.Count; i++)
        {
            // Complete the window
            window.Add(sequence[i]);
            
            // Get string representation
            var nGramStr = string.Join(" ", window);
            
            // Record position
            if (!nGramMap.TryGetValue(nGramStr, out var positions))
            {
                positions = [];
                nGramMap[nGramStr] = positions;
            }
            
            positions.Add(i - n + 1);
            
            // Slide window by removing oldest element
            window.RemoveAt(0);
        }
        
        // Find most frequent in linear time relative to unique n-grams (bounded by n)
        string? mostFrequentNGram = null;
        List<int>? mostFrequentPositions = null;
        var maxFrequency = minFrequency - 1;
        
        foreach (var entry in nGramMap)
        {
            if (entry.Value.Count > maxFrequency)
            {
                maxFrequency = entry.Value.Count;
                mostFrequentNGram = entry.Key;
                mostFrequentPositions = entry.Value;
            }
            else if (entry.Value.Count == maxFrequency && 
                     mostFrequentNGram != null && 
                     string.Compare(entry.Key, mostFrequentNGram, StringComparison.Ordinal) < 0)
            {
                // Tie-breaking by lexicographical order
                mostFrequentNGram = entry.Key;
                mostFrequentPositions = entry.Value;
            }
        }
        
        return (mostFrequentNGram, mostFrequentPositions ?? new List<int>());
    }
    
    // Linear time replacement - O(n)
    private void ReplaceOccurrences(List<string> sequence, List<int> positions, int n, string newSymbol)
    {
        // Sort positions in descending order to avoid index shifting
        positions.Sort((a, b) => b.CompareTo(a));
        
        // Instead of list operations for each position, rebuild sequence in one pass
        var newSequence = new List<string>(sequence.Count);
        var currentPos = 0;
        var positionIndex = positions.Count - 1; // Start with the smallest position
        
        // Process sequence in one pass
        while (currentPos < sequence.Count)
        {
            // Check if current position needs replacement
            if (positionIndex >= 0 && currentPos == positions[positionIndex])
            {
                // Add replacement symbol
                newSequence.Add(newSymbol);
                
                // Skip the n tokens that form the n-gram
                currentPos += n;
                positionIndex--;
            }
            else
            {
                // Copy token as is
                newSequence.Add(sequence[currentPos]);
                currentPos++;
            }
        }
        
        // Update original sequence
        sequence.Clear();
        sequence.AddRange(newSequence);
    }

    public IDependencyTreeNode Decompress(CompressedTree? compressedTree)
    {
        ArgumentNullException.ThrowIfNull(compressedTree);

        // Clear previous rules
        _grammarRules.Clear();

        // Extract grammar rules from metadata
        if (compressedTree.Metadata is { } rules)
        {
            foreach (var rule in rules)
                _grammarRules[rule.Key] = rule.Value;
        }

        // Convert byte array to sequence
        var sequence = ConvertByteArrayToSequence(compressedTree.Structure);
        
        // Fully expand the sequence - now in linear time
        var expandedSequence = ExpandSequenceLinear(sequence);

        // Convert expanded sequence back to tree
        var index = 0;
        return DecodeTree(expandedSequence, ref index);
    }

    // Linear time expansion using a single pass
    private List<string> ExpandSequenceLinear(List<string> sequence)
    {
        // Order rules for consistent application
        var orderedRules = _grammarRules
            .OrderBy(r => Convert.ToInt32(r.Key[0]))
            .ToList();
        
        // Pre-process the expansions to handle nested rules
        var processedRules = new Dictionary<string, string[]>();
        
        // Process rules from simplest to most complex
        foreach (var rule in orderedRules)
        {
            var tokens = rule.Value.Split(' ');
            
            // Expand any nested rules already processed
            for (var i = 0; i < tokens.Length; i++)
            {
                if (!processedRules.TryGetValue(tokens[i], out var expansion)) continue;
                // Replace with expanded value
                var newTokens = new List<string>();
                    
                // Add tokens before this one
                for (var j = 0; j < i; j++)
                    newTokens.Add(tokens[j]);
                    
                // Add expanded tokens
                newTokens.AddRange(expansion);
                    
                // Add tokens after this one
                for (var j = i + 1; j < tokens.Length; j++)
                    newTokens.Add(tokens[j]);
                    
                tokens = newTokens.ToArray();
                i--; // Reprocess current position in case of nested expansions
            }
            
            // Store fully expanded rule
            processedRules[rule.Key] = tokens;
        }
        
        // Now expand the sequence in a single pass
        var result = new List<string>();
        
        foreach (var token in sequence)
        {
            if (processedRules.TryGetValue(token, out var expansion))
            {
                result.AddRange(expansion);
            }
            else
            {
                result.Add(token);
            }
        }
        
        return result;
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
            node.AddLeftChild(child);
        }

        // Skip separator
        if (index < sequence.Count && sequence[index] == "|")
            index++;

        // Right children
        while (index < sequence.Count && sequence[index] != "]")
        {
            var child = DecodeTree(sequence, ref index);
            node.AddRightChild(child);
        }

        // Skip closing bracket
        if (index < sequence.Count && sequence[index] == "]")
            index++;

        return node;
    }

    private static byte[] ConvertSequenceToByteArray(List<string> sequence)
    {
        var sb = new StringBuilder(sequence.Count * 8); // Pre-allocate capacity
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