using TreeCompressionPipeline.TreeStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeCompressionPipeline.CompressionStrategies;

namespace TreeCompressionAlgorithms.CompressionStrategies
{
    public class TreeRepairStrategy : ICompressionStrategy
    {
        private readonly Dictionary<string, string> _grammarRules = new();
        private int _nextNonTerminal = 256; // Start from ASCII 256 to avoid conflicts

        public CompressedTree Compress(ITreeNode? tree)
        {
            ArgumentNullException.ThrowIfNull(tree);

            // Step 1: Convert tree to linearized sequence
            var sequence = new List<string>();
            EncodeTree(tree, sequence);

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

            // Return the compressed tree with the sequence and grammar rules
            return new CompressedTree
            {
                Structure = ConvertSequenceToByteArray(sequence),
                Metadata = _grammarRules
            };
        }

        public ITreeNode Decompress(CompressedTree? compressedTree)
        {
            ArgumentNullException.ThrowIfNull(compressedTree);

            // Step 1: Expand compressed sequence using grammar rules
            var sequence = ConvertByteArrayToSequence(compressedTree.Structure);
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

        private static void EncodeTree(ITreeNode node, List<string> sequence)
        {
            // Add the node value to the sequence
            sequence.Add(node.Value.ToString());

            // If the node has children, recursively encode each child
            if (node.Children.Any())
            {
                foreach (var child in node.Children)
                {
                    EncodeTree(child, sequence);
                }
            }

            sequence.Add("]"); // Marks end of this node
        }

        private static ITreeNode DecodeTree(List<string> sequence, ref int index)
        {
            if (index >= sequence.Count || sequence[index] == "]") return null;

            var value = sequence[index++];
            ITreeNode node = new TreeNode(value);

            // Decode children for the current node
            while (index < sequence.Count && sequence[index] != "]")
            {
                var child = DecodeTree(sequence, ref index);
                node.AddChild(child);
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
            List<string> newSequence = new List<string>();
            var i = 0;

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

        private static byte[] ConvertSequenceToByteArray(List<string> sequence)
        {
            return System.Text.Encoding.UTF8.GetBytes(string.Join(" ", sequence));
        }

        private static List<string> ConvertByteArrayToSequence(byte[] byteArray)
        {
            return System.Text.Encoding.UTF8.GetString(byteArray).Split(" ").ToList();
        }
    }
}