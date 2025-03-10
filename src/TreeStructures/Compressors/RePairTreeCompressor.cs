namespace TreeStructures.Compressors;

using System;
using System.Collections.Generic;
using System.Linq;

public class RePairTreeCompressor<T> : ITreeCompressor<T>
{
    private readonly Dictionary<string, string> _grammarRules = new();
    private int _nextNonTerminal = 256; // Start from ASCII 256 to avoid conflicts
    
    

    public string CompressTree(TreeNode<T> node)
    {
        // Step 1: Convert tree to linearized sequence
        List<string> sequence = [];
        EncodeTree(node, sequence);

        // Step 2: Apply RePair compression
        while (true)
        {
            var pairFreq = CountFrequentPairs(sequence);
            if (pairFreq.Count == 0) break;

            // Find the most frequent pair
            var mostFrequentPair = pairFreq.OrderByDescending(p => p.Value).First().Key;
            var newSymbol = ((char)_nextNonTerminal).ToString();
            _nextNonTerminal++;

            // Store grammar rule
            _grammarRules[newSymbol] = mostFrequentPair;

            // Replace occurrences in sequence
            sequence = ReplacePairs(sequence, mostFrequentPair, newSymbol);
        }

        
        return string.Join(" ", sequence);
    }

    public TreeNode<T> DecompressTree(string compressedTree)
    {
        if (string.IsNullOrWhiteSpace(compressedTree)) return null;

        // Step 1: Expand compressed sequence using grammar rules
        List<string> sequence = compressedTree.Split(" ").ToList();
        bool expanded;
        do
        {
            expanded = false;
            for (var i = 0; i < sequence.Count; i++)
            {
                if (!_grammarRules.ContainsKey(sequence[i])) continue;
                var expansion = _grammarRules[sequence[i]].Split(" ");
                sequence.RemoveAt(i);
                sequence.InsertRange(i, expansion);
                expanded = true;
                break; // Restart expansion after modification
            }
        } while (expanded);

        // Step 2: Convert sequence back to tree
        var index = 0;
        return DecodeTree(sequence, ref index);
    }

    private static void EncodeTree(TreeNode<T> node, List<string> sequence)
    {

        sequence.Add(node.Value.ToString());

        foreach (var child in node.Left)
            EncodeTree(child, sequence);

        sequence.Add("|"); // Delimiter to mark the end of left children

        foreach (var child in node.Right)
            EncodeTree(child, sequence);

        sequence.Add("]"); // Marks end of node
    }

    private static TreeNode<T> DecodeTree(List<string> sequence, ref int index)
    {
        if (index >= sequence.Count || sequence[index] == "]") return null;

        var value = (T)Convert.ChangeType(sequence[index++], typeof(T));
        TreeNode<T> node = new(value);

        while (index < sequence.Count && sequence[index] != "|" && sequence[index] != "]")
        {
            var child = DecodeTree(sequence, ref index);
            node.Add(child, Direction.Left);
        }

        if (index < sequence.Count && sequence[index] == "|") index++; // Skip left-right separator

        while (index < sequence.Count && sequence[index] != "]")
        {
            var child = DecodeTree(sequence, ref index);
            node.Add(child, Direction.Right);
        }

        if (index < sequence.Count && sequence[index] == "]") index++; // Skip end marker
        return node;
    }

    private Dictionary<string, int> CountFrequentPairs(List<string> sequence)
    {
        Dictionary<string, int> pairFreq = new();
        for (var i = 0; i < sequence.Count - 1; i++)
        {
            var pair = sequence[i] + " " + sequence[i + 1];
            if (!pairFreq.TryAdd(pair, 1))
                pairFreq[pair]++;
        }
        return pairFreq.Where(p => p.Value > 1).ToDictionary(p => p.Key, p => p.Value);
    }

    private static List<string> ReplacePairs(List<string> sequence, string targetPair, string newSymbol)
    {
        List<string> newSequence = [];
        int i = 0;

        while (i < sequence.Count)
        {
            if (i < sequence.Count - 1 && (sequence[i] + " " + sequence[i + 1]) == targetPair)
            {
                newSequence.Add(newSymbol);
                i += 2; // Skip replaced pair
            }
            else
            {
                newSequence.Add(sequence[i]);
                i++;
            }
        }

        return newSequence;
    }
}