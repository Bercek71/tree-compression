using System.Text;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair
{
    /// <summary>
    /// Implementation of the original TreeRepair compression algorithm that works directly on tree structures.
    /// This approach avoids linearizing the tree, preserving the hierarchical structure during compression.
    /// </summary>
    public class OriginalTreeRepairStrategy : ICompressionStrategy<IDependencyTreeNode>
    {
        private readonly int _minFrequency;
        private int _nextRuleId = 1;
        private readonly Dictionary<string, IDependencyTreeNode> _subtreeRegistry = new();
        private readonly Dictionary<string, int> _subtreeFrequency = new();
        private readonly Dictionary<string, string> _grammarRules = new();

        /// <summary>
        /// Initializes a new instance of the OriginalTreeRepairStrategy.
        /// </summary>
        /// <param name="minFrequency">Minimum frequency for a subtree to be considered for replacement.</param>
        public OriginalTreeRepairStrategy(int minFrequency = 2)
        {
            _minFrequency = minFrequency;
        }

        /// <summary>
        /// Compresses a dependency tree using the original TreeRepair algorithm.
        /// </summary>
        public CompressedTree Compress(IDependencyTreeNode? tree)
        {
            ArgumentNullException.ThrowIfNull(tree);
            
            // Reset state for new compression
            _subtreeRegistry.Clear();
            _subtreeFrequency.Clear();
            _grammarRules.Clear();
            _nextRuleId = 1;

            // Phase 1: Count all subtree frequencies
            CountSubtreeFrequencies(tree);

            // Phase 2: Identify and replace frequent subtrees with grammar rules
            var compressedTree = ReplaceFrequentSubtrees(tree);

            // Convert compressed tree to byte array for storage
            var treeData = SerializeTree(compressedTree);

            return new CompressedTree
            {
                Structure = treeData,
                Metadata = new Dictionary<string, string>(_grammarRules),
                CompressedNode = compressedTree,
            };
        }

        /// <summary>
        /// Decompresses a compressed tree back to its original form.
        /// </summary>
        public IDependencyTreeNode Decompress(CompressedTree? compressedTree)
        {
            ArgumentNullException.ThrowIfNull(compressedTree);

            // Extract serialized tree and grammar rules
            var treeData = compressedTree.Structure;
            var rules = compressedTree.Metadata;

            // Deserialize the compressed tree
            var compressedTreeRoot = DeserializeTree(treeData);

            // Expand grammar rules
            return ExpandGrammarRules(compressedTreeRoot, rules);
        }

        #region Private Compression Methods

        /// <summary>
        /// Counts the frequency of each unique subtree in the tree.
        /// </summary>
        private void CountSubtreeFrequencies(IDependencyTreeNode node)
        {
            // Process left children
            foreach (var child in node.LeftChildren)
            {
                CountSubtreeFrequencies(child);
            }

            // Process right children
            foreach (var child in node.RightChildren)
            {
                CountSubtreeFrequencies(child);
            }

            // Calculate a hash for this subtree
            var subtreeHash = CalculateSubtreeHash(node);

            // Register and count the subtree
            if (_subtreeRegistry.TryAdd(subtreeHash, node))
            {
                _subtreeFrequency[subtreeHash] = 0;
            }
            
            _subtreeFrequency[subtreeHash]++;
        }

        /// <summary>
        /// Identifies frequent subtrees and replaces them with grammar rules.
        /// </summary>
        private IDependencyTreeNode ReplaceFrequentSubtrees(IDependencyTreeNode node)
        {
            while (true)
            {
                // Find most frequent subtree
                var mostFrequentSubtree = _subtreeFrequency.Where(kv => kv.Value >= _minFrequency && kv.Key.Length > 10) // Skip trivial subtrees
                    .OrderByDescending(kv => kv.Value * kv.Key.Length) // Prioritize by compression gain
                    .FirstOrDefault();

                if (mostFrequentSubtree.Key == null) return node;
                // Create a new grammar rule
                var ruleName = $"R{_nextRuleId++}";
                _grammarRules[ruleName] = mostFrequentSubtree.Key;

                // Replace all occurrences of this subtree with the rule
                var replacementNode = new DependencyTreeNode(ruleName);
                ReplaceSubtrees(node, mostFrequentSubtree.Key, replacementNode);

                // Update frequency counts
                _subtreeFrequency.Remove(mostFrequentSubtree.Key);

                // No more frequent subtrees to replace
            }
        }

        /// <summary>
        /// Replaces all occurrences of a subtree with a grammar rule reference.
        /// </summary>
        private void ReplaceSubtrees(IDependencyTreeNode root, string subtreeHash, IDependencyTreeNode replacement)
        {
            // Check if current subtree matches the target
            if (CalculateSubtreeHash(root) == subtreeHash)
            {
                // Replace this entire subtree with the replacement node
                // (Note: In a real implementation, this would modify the parent to point to replacement)
                return;
            }

            // Process left children (create a new list to avoid modification during iteration)
            var leftChildren = new List<IDependencyTreeNode>(root.LeftChildren);
            for (var i = 0; i < leftChildren.Count; i++)
            {
                var child = leftChildren[i];
                if (CalculateSubtreeHash(child) == subtreeHash)
                {
                    // Replace this child with the replacement node
                    root.LeftChildren.RemoveAt(i);
                    root.LeftChildren.Insert(i, CloneNode(replacement));
                }
                else
                {
                    // Recursively process this child
                    ReplaceSubtrees(child, subtreeHash, replacement);
                }
            }

            // Process right children (create a new list to avoid modification during iteration)
            var rightChildren = new List<IDependencyTreeNode>(root.RightChildren);
            for (var i = 0; i < rightChildren.Count; i++)
            {
                var child = rightChildren[i];
                if (CalculateSubtreeHash(child) == subtreeHash)
                {
                    // Replace this child with the replacement node
                    root.RightChildren.RemoveAt(i);
                    root.RightChildren.Insert(i, CloneNode(replacement));
                }
                else
                {
                    // Recursively process this child
                    ReplaceSubtrees(child, subtreeHash, replacement);
                }
            }
        }

        /// <summary>
        /// Calculates a hash representation of a subtree that uniquely identifies its structure.
        /// </summary>
        private static string CalculateSubtreeHash(IDependencyTreeNode node)
        {
            var hash = new StringBuilder();
            
            // Add node value
            hash.Append(node.Value.ToString() ?? "null");
            
            // Left children
            hash.Append('(');
            foreach (var child in node.LeftChildren)
            {
                hash.Append(CalculateSubtreeHash(child));
                hash.Append(',');
            }
            hash.Append(')');
            
            // Right children
            hash.Append('[');
            foreach (var child in node.RightChildren)
            {
                hash.Append(CalculateSubtreeHash(child));
                hash.Append(',');
            }
            hash.Append(']');
            
            return hash.ToString();
        }

        #endregion

        #region Decompression and Serialization Methods

        /// <summary>
        /// Creates a clone of a node.
        /// </summary>
        private static IDependencyTreeNode CloneNode(IDependencyTreeNode node)
        {
            var clone = new DependencyTreeNode(node.Value.ToString() ?? "");
            
            // Clone left children
            foreach (var childClone in node.LeftChildren.Select(CloneNode))
            {
                clone.AddLeftChild(childClone);
            }
            
            // Clone right children
            foreach (var childClone in node.RightChildren.Select(CloneNode))
            {
                clone.AddRightChild(childClone);
            }
            
            return clone;
        }

        /// <summary>
        /// A dictionary cache to store parsed subtrees to avoid redundant parsing
        /// </summary>
        private readonly Dictionary<string, IDependencyTreeNode> _subtreeCache = new();

        /// <summary>
        /// Expands grammar rules in the compressed tree to reconstruct the original tree.
        /// </summary>
        private IDependencyTreeNode ExpandGrammarRules(IDependencyTreeNode compressedNode, Dictionary<string, string> rules)
        {
            // Check if this node is a rule reference
            var nodeValue = compressedNode.Value.ToString() ?? "";
            
            if (nodeValue.StartsWith("R") && rules.TryGetValue(nodeValue, out var subtreeHash))
            {
                // Check if we've already parsed this subtree hash
                if (_subtreeCache.TryGetValue(subtreeHash, out var expandedNode)) return CloneNode(expandedNode);
                // Parse the subtree from its hash and cache it
                expandedNode = ParseSubtreeFromHash(subtreeHash, rules);
                _subtreeCache[subtreeHash] = expandedNode;

                // Return a clone of the expanded node
                return CloneNode(expandedNode);
            }
            
            // Not a rule reference, process children recursively
            var result = new DependencyTreeNode(nodeValue);
            
            foreach (var child in compressedNode.LeftChildren)
            {
                result.AddLeftChild(ExpandGrammarRules(child, rules));
            }
            
            foreach (var child in compressedNode.RightChildren)
            {
                result.AddRightChild(ExpandGrammarRules(child, rules));
            }
            
            return result;
        }

        /// <summary>
        /// Parses a subtree from its hash representation.
        /// </summary>
        private IDependencyTreeNode ParseSubtreeFromHash(string subtreeHash, Dictionary<string, string> rules)
        {
            // Parse the node value - everything before the first parenthesis
            var openParenIndex = subtreeHash.IndexOf('(');
            if (openParenIndex == -1) return new DependencyTreeNode(subtreeHash);
            
            var nodeValue = subtreeHash.Substring(0, openParenIndex);
            var node = new DependencyTreeNode(nodeValue);
            
            // Parse left children from everything between '(' and ')'
            var closeParenIndex = FindMatchingClosingBracket(subtreeHash, openParenIndex, '(', ')');
            if (closeParenIndex > openParenIndex + 1)
            {
                var leftChildrenStr = subtreeHash.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
                ParseChildren(leftChildrenStr, node, true, rules);
            }
            
            // Parse right children from everything between '[' and ']'
            var openBracketIndex = subtreeHash.IndexOf('[', closeParenIndex);
            if (openBracketIndex == -1) return node;
            var closeBracketIndex = FindMatchingClosingBracket(subtreeHash, openBracketIndex, '[', ']');
            if (closeBracketIndex <= openBracketIndex + 1) return node;
            var rightChildrenStr = subtreeHash.Substring(openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1);
            ParseChildren(rightChildrenStr, node, false, rules);

            return node;
        }

        /// <summary>
        /// Finds the matching closing bracket for a given opening bracket.
        /// </summary>
        private static int FindMatchingClosingBracket(string text, int openIndex, char openChar, char closeChar)
        {
            var depth = 1;
            for (var i = openIndex + 1; i < text.Length; i++)
            {
                if (text[i] == openChar) depth++;
                else if (text[i] == closeChar)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1; // No matching bracket found
        }

        /// <summary>
        /// Parses a comma-separated list of child subtrees and adds them to the parent node.
        /// </summary>
        private void ParseChildren(string childrenStr, IDependencyTreeNode parent, bool isLeftChildren, Dictionary<string, string> rules)
        {
            var startIndex = 0;
            var depth = 0;
            
            for (var i = 0; i < childrenStr.Length; i++)
            {
                var c = childrenStr[i];

                switch (c)
                {
                    case '(':
                    case '[':
                        depth++;
                        break;
                    case ')':
                    case ']':
                        depth--;
                        break;
                    case ',' when depth == 0:
                    {
                        // Found a top-level comma, parse the child subtree
                        if (i > startIndex)
                        {
                            var childHash = childrenStr.Substring(startIndex, i - startIndex);
                            if (!string.IsNullOrWhiteSpace(childHash))
                            {
                                var childNode = ParseSubtreeFromHash(childHash, rules);
                                if (isLeftChildren)
                                    parent.AddLeftChild(childNode);
                                else
                                    parent.AddRightChild(childNode);
                            }
                        }
                        startIndex = i + 1;
                        break;
                    }
                }
            }
            
            // Parse the last child
            if (startIndex >= childrenStr.Length) return;
            {
                var childHash = childrenStr.Substring(startIndex);
                if (string.IsNullOrWhiteSpace(childHash)) return;
                var childNode = ParseSubtreeFromHash(childHash, rules);
                if (isLeftChildren)
                    parent.AddLeftChild(childNode);
                else
                    parent.AddRightChild(childNode);
            }
        }

        /// <summary>
        /// Serializes a tree structure to a byte array with optimized encoding.
        /// </summary>
        private byte[] SerializeTree(IDependencyTreeNode tree)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);
            // Write grammar rules count and data
            writer.Write(_grammarRules.Count);
            foreach (var rule in _grammarRules)
            {
                WriteCompactString(writer, rule.Key);   // Rule name (e.g., "R1")
                WriteCompactString(writer, rule.Value); // Rule definition
            }
                
            // Write the compressed tree structure
            SerializeNodeCompact(tree, writer);
                
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Writes a string to the binary writer with space-efficient encoding.
        /// </summary>
        private static void WriteCompactString(BinaryWriter writer, string value)
        {
            // For short strings, use a single byte length prefix
            // For longer strings, use a more complex encoding
            var stringBytes = Encoding.UTF8.GetBytes(value);
            
            if (stringBytes.Length < 128)
            {
                writer.Write((byte)stringBytes.Length);
            }
            else
            {
                // For longer strings, use 7 bits for length with continuation bit
                var length = stringBytes.Length;
                do
                {
                    var chunk = (byte)(length & 0x7F);
                    length >>= 7;
                    if (length > 0) chunk |= 0x80; // Set continuation bit
                    writer.Write(chunk);
                } while (length > 0);
            }
            
            writer.Write(stringBytes);
        }

        /// <summary>
        /// Reads a compact string from the binary reader.
        /// </summary>
        private static string ReadCompactString(BinaryReader reader)
        {
            // Read the string length
            var length = 0;
            var shift = 0;
            byte chunk;
            
            do
            {
                chunk = reader.ReadByte();
                length |= (chunk & 0x7F) << shift;
                shift += 7;
            } while ((chunk & 0x80) != 0);
            
            // Read the string bytes
            var stringBytes = reader.ReadBytes(length);
            return Encoding.UTF8.GetString(stringBytes);
        }

        /// <summary>
        /// Serializes a single node and its children with compact encoding.
        /// </summary>
        private static void SerializeNodeCompact(IDependencyTreeNode node, BinaryWriter writer)
        {
            // Write node value using compact string encoding
            WriteCompactString(writer, node.Value.ToString() ?? "");
            
            // Write children counts using variable-length encoding
            WriteVarInt(writer, node.LeftChildren.Count);
            WriteVarInt(writer, node.RightChildren.Count);
            
            // Write all children
            foreach (var child in node.LeftChildren)
            {
                SerializeNodeCompact(child, writer);
            }
            
            foreach (var child in node.RightChildren)
            {
                SerializeNodeCompact(child, writer);
            }
        }

        /// <summary>
        /// Writes an integer using variable-length encoding to minimize space.
        /// </summary>
        private static void WriteVarInt(BinaryWriter writer, int value)
        {
            // Use variable-length encoding (similar to Protocol Buffers)
            // Small numbers use fewer bytes
            do
            {
                var chunk = (byte)(value & 0x7F);
                value >>= 7;
                if (value > 0) chunk |= 0x80; // Set continuation bit
                writer.Write(chunk);
            } while (value > 0);
        }

        /// <summary>
        /// Reads a variable-length encoded integer.
        /// </summary>
        private static int ReadVarInt(BinaryReader reader)
        {
            var value = 0;
            var shift = 0;
            byte chunk;
            
            do
            {
                chunk = reader.ReadByte();
                value |= (chunk & 0x7F) << shift;
                shift += 7;
            } while ((chunk & 0x80) != 0);
            
            return value;
        }

        /// <summary>
        /// Deserializes a byte array back to a tree structure.
        /// </summary>
        private IDependencyTreeNode DeserializeTree(byte[] treeData)
        {
            _subtreeCache.Clear(); // Clear cache before deserialization

            using var memoryStream = new MemoryStream(treeData);
            using var reader = new BinaryReader(memoryStream);
            // Read grammar rules
            var ruleCount = reader.ReadInt32();
            var rules = new Dictionary<string, string>(ruleCount);
                
            for (var i = 0; i < ruleCount; i++)
            {
                var ruleName = ReadCompactString(reader);
                var ruleDefinition = ReadCompactString(reader);
                rules[ruleName] = ruleDefinition;
            }
                
            // Read and expand the tree with rules
            var compressedTree = DeserializeNodeCompact(reader);
            return ExpandGrammarRules(compressedTree, rules);
        }

        /// <summary>
        /// Deserializes a single node and its children using compact encoding.
        /// </summary>
        private static IDependencyTreeNode DeserializeNodeCompact(BinaryReader reader)
        {
            // Read node value
            var value = ReadCompactString(reader);
            var node = new DependencyTreeNode(value);
            
            // Read children counts
            var leftChildCount = ReadVarInt(reader);
            var rightChildCount = ReadVarInt(reader);
            
            // Read left children
            for (var i = 0; i < leftChildCount; i++)
            {
                node.AddLeftChild(DeserializeNodeCompact(reader));
            }
            
            // Read right children
            for (var i = 0; i < rightChildCount; i++)
            {
                node.AddRightChild(DeserializeNodeCompact(reader));
            }
            
            return node;
        }

        #endregion
    }
}