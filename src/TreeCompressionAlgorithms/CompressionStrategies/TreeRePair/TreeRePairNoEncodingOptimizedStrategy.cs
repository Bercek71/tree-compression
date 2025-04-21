using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies.TreeRePair
{
    /// <summary>
    /// A complete TreeRePair implementation that carefully preserves the entire tree structure.
    /// </summary>
    public class TreeRePairNoEncodingOptimizedStrategy : ICompressionStrategy<IDependencyTreeNode>
    {
        private readonly int _minFrequency;
        private readonly int _maxIterations;
        private readonly bool _debug;
        
        // Dictionary to track rules
        private Dictionary<string, Digram> _rules;
        private int _ruleCounter;

        /// <summary>
        /// Represents a digram (pair of nodes) for compression.
        /// </summary>
        private class Digram
        {
            public string FirstValue { get; }
            public string SecondValue { get; }
            
            public Digram(string firstValue, string secondValue)
            {
                FirstValue = firstValue;
                SecondValue = secondValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the TreeRePairStrategy.
        /// </summary>
        public TreeRePairNoEncodingOptimizedStrategy(int minFrequency = 2, int maxIterations = 100, bool debug = false)
        {
            _minFrequency = minFrequency;
            _maxIterations = maxIterations;
            _debug = debug;
            _rules = new Dictionary<string, Digram>();
        }

        /// <summary>
        /// Compresses a dependency tree using the TreeRePair algorithm.
        /// </summary>
        public CompressedTree Compress(IDependencyTreeNode tree)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            // Reset state
            _rules = new Dictionary<string, Digram>();
            _ruleCounter = 0;

            // Create a working copy for compression
            IDependencyTreeNode workingCopy = DeepCopyTree(tree);
            
            // Identify and replace frequently occurring digrams
            ApplyCompression(workingCopy);

            // Serialize the compressed tree
            return new CompressedTree
            {
                Structure = DependencyTreeNode.SerializeToBytes(workingCopy),
                Metadata = SerializeRules(_rules)
            };
        }

        /// <summary>
        /// Applies compression by replacing frequent digrams with rule references.
        /// </summary>
        private void ApplyCompression(IDependencyTreeNode tree)
        {
            for (int iteration = 0; iteration < _maxIterations; iteration++)
            {
                // Find digram frequencies
                var frequencies = FindDigramFrequencies(tree);
                if (frequencies.Count == 0)
                    break;

                // Find most frequent digram
                var mostFrequent = frequencies
                    .Where(kv => kv.Value >= _minFrequency)
                    .OrderByDescending(kv => kv.Value)
                    .FirstOrDefault();

                if (mostFrequent.Key == null || mostFrequent.Value < _minFrequency)
                    break;

                // Parse digram
                string[] parts = mostFrequent.Key.Split('|');
                if (parts.Length != 2)
                    continue;

                string firstValue = parts[0];
                string secondValue = parts[1];

                // Skip if both are already rule references
                if (firstValue.StartsWith("R") && secondValue.StartsWith("R"))
                    continue;

                // Create a new rule
                string ruleName = $"R{_ruleCounter++}";
                _rules[ruleName] = new Digram(firstValue, secondValue);

                if (_debug)
                {
                    Console.WriteLine($"Created rule {ruleName}: {firstValue}|{secondValue} (freq: {mostFrequent.Value})");
                }

                // Replace digrams in the tree
                bool replaced = ReplaceDigramsInTree(tree, firstValue, secondValue, ruleName);
                if (!replaced)
                    break;
            }
        }

        /// <summary>
        /// Finds all digrams in the tree and counts their frequencies.
        /// </summary>
        private Dictionary<string, int> FindDigramFrequencies(IDependencyTreeNode root)
        {
            Dictionary<string, int> frequencies = new Dictionary<string, int>();

            void TraverseTree(IDependencyTreeNode node)
            {
                // Count digrams in left children
                CountDigramsInList(node.LeftChildren, frequencies);

                // Count digrams in right children
                CountDigramsInList(node.RightChildren, frequencies);

                // Process all children recursively
                foreach (var child in node.LeftChildren)
                    TraverseTree(child);

                foreach (var child in node.RightChildren)
                    TraverseTree(child);
            }

            TraverseTree(root);
            return frequencies;
        }

        /// <summary>
        /// Counts digrams in a list of nodes.
        /// </summary>
        private void CountDigramsInList(List<IDependencyTreeNode> nodes, Dictionary<string, int> frequencies)
        {
            for (var i = 0; i < nodes.Count - 1; i++)
            {
                var firstValue = nodes[i].Value.ToString() ?? string.Empty;
                var secondValue = nodes[i + 1].Value.ToString() ?? string.Empty;

                // Skip if either node is a special node type
                if (IsSpecialNode(firstValue) || IsSpecialNode(secondValue))
                    continue;

                // Create a key for this digram
                var key = $"{firstValue}|{secondValue}";

                // Count frequency
                if (frequencies.TryGetValue(key, out var value))
                    frequencies[key] = ++value;
                else
                    frequencies[key] = 1;
            }
        }

        /// <summary>
        /// Checks if a node value is a special system node.
        /// </summary>
        private static bool IsSpecialNode(string value)
        {
            return value is "<DocumentRoot>" or "<root>";
        }

        /// <summary>
        /// Replaces digrams in the tree with rule references.
        /// </summary>
        private bool ReplaceDigramsInTree(IDependencyTreeNode root, string firstValue, string secondValue, string ruleName)
        {
            var replacedAny = false;

            TraverseTree(root);
            return replacedAny;

            void TraverseTree(IDependencyTreeNode node)
            {
                // Replace in this node's children
                if (ReplaceInList(node.LeftChildren))
                    replacedAny = true;

                if (ReplaceInList(node.RightChildren))
                    replacedAny = true;

                // Process all remaining children recursively
                // Make copies to avoid concurrent modification issues
                var leftChildren = new List<IDependencyTreeNode>(node.LeftChildren);
                var rightChildren = new List<IDependencyTreeNode>(node.RightChildren);

                foreach (var child in leftChildren.Where(child => node.LeftChildren.Contains(child)))
                {
                    TraverseTree(child);
                }

                foreach (var child in rightChildren.Where(child => node.RightChildren.Contains(child)))
                {
                    TraverseTree(child);
                }
            }

            bool ReplaceInList(List<IDependencyTreeNode> nodes)
            {
                var replaced = false;

                for (var i = 0; i < nodes.Count - 1; i++)
                {
                    if (nodes[i].Value.ToString() != firstValue ||
                        nodes[i + 1].Value.ToString() != secondValue) continue;
                    // Create rule node
                    var ruleNode = new DependencyTreeNode(ruleName);

                    // Copy first node's left children to rule node
                    foreach (var child in nodes[i].LeftChildren)
                        ruleNode.AddLeftChild(DeepCopyTree(child));

                    // Copy second node's right children to rule node
                    foreach (var child in nodes[i + 1].RightChildren)
                        ruleNode.AddRightChild(DeepCopyTree(child));

                    // Replace the two nodes with the rule node
                    nodes.RemoveAt(i);
                    nodes.RemoveAt(i);
                    nodes.Insert(i, ruleNode);

                    replaced = true;
                    i--; // Adjust index
                }

                return replaced;
            }
        }

        /// <summary>
        /// Decompresses a tree compressed with the TreeRePair algorithm.
        /// </summary>
        public IDependencyTreeNode Decompress(CompressedTree compressedTree)
        {
            if (compressedTree == null)
                throw new ArgumentNullException(nameof(compressedTree));

            try
            {
                // Deserialize the rules and compressed tree
                var rules = DeserializeRules(compressedTree.Metadata);
                IDependencyTreeNode compressedRoot = DependencyTreeNode.DeserializeFromBytes(compressedTree.Structure);

                // Build a map of expanded rules
                var expandedRules = ExpandAllRules(rules);

                // Recursively expand the compressed tree
                IDependencyTreeNode decompressedRoot = ReconstructTree(compressedRoot, expandedRules);

                return decompressedRoot;
            }
            catch (Exception ex)
            {
                if (_debug)
                    Console.WriteLine($"Decompression error: {ex.Message}");
                
                return new DependencyTreeNode("<Decompression Error>");
            }
        }

        /// <summary>
        /// Expands all rules to their final form.
        /// </summary>
        private Dictionary<string, List<SubtreeNode>> ExpandAllRules(Dictionary<string, Digram> rules)
        {
            // First create direct expansions
            var directExpansions = new Dictionary<string, List<SubtreeNode>>();
            
            foreach (var rule in rules)
            {
                var expansion = new List<SubtreeNode>
                {
                    new SubtreeNode { Value = rule.Value.FirstValue },
                    new SubtreeNode { Value = rule.Value.SecondValue }
                };
                directExpansions[rule.Key] = expansion;
            }
            
            // Iteratively expand rules that reference other rules
            bool madeChanges;
            int iterations = 0;
            do
            {
                madeChanges = false;
                iterations++;
                
                if (iterations > 100) // Safety limit
                    break;
                
                foreach (var ruleName in directExpansions.Keys.ToList())
                {
                    var expansion = directExpansions[ruleName];
                    var newExpansion = new List<SubtreeNode>();
                    bool expandedThisRule = false;
                    
                    foreach (var node in expansion)
                    {
                        if (node.Value.StartsWith("R") && directExpansions.ContainsKey(node.Value))
                        {
                            // Replace with the rule's expansion
                            foreach (var expandedNode in directExpansions[node.Value])
                            {
                                var copy = new SubtreeNode 
                                { 
                                    Value = expandedNode.Value,
                                    LeftChildren = new List<SubtreeNode>(expandedNode.LeftChildren),
                                    RightChildren = new List<SubtreeNode>(expandedNode.RightChildren)
                                };
                                newExpansion.Add(copy);
                            }
                            expandedThisRule = true;
                        }
                        else
                        {
                            // Keep as is
                            newExpansion.Add(node);
                        }
                    }
                    
                    if (expandedThisRule)
                    {
                        directExpansions[ruleName] = newExpansion;
                        madeChanges = true;
                    }
                }
            } while (madeChanges);
            
            return directExpansions;
        }
        
        /// <summary>
        /// Simple subtree node class for rule expansion.
        /// </summary>
        private class SubtreeNode
        {
            public string Value { get; init; } = string.Empty;
            public List<SubtreeNode> LeftChildren { get; init; } = [];
            public List<SubtreeNode> RightChildren { get; init; } = [];
        }

        /// <summary>
        /// Recursively reconstructs the original tree by expanding rule references.
        /// </summary>
        private IDependencyTreeNode ReconstructTree(IDependencyTreeNode node, Dictionary<string, List<SubtreeNode>> expandedRules)
        {
            string nodeValue = node.Value.ToString() ?? string.Empty;
            
            // If this is a rule reference, expand it
            if (nodeValue.StartsWith("R") && expandedRules.TryGetValue(nodeValue, out var expansion))
            {
                // Get the expanded rule
                if (expansion.Count == 0)
                    return new DependencyTreeNode("<empty>");
                
                // Create nodes for the expansion
                var expandedNodes = new List<IDependencyTreeNode>();
                foreach (var subNode in expansion)
                {
                    var treeNode = new DependencyTreeNode(subNode.Value);
                    
                    // Add left children
                    foreach (var childNode in subNode.LeftChildren.Select(leftChild => CreateNodeFromSubtree(leftChild, expandedRules)))
                    {
                        treeNode.AddLeftChild(childNode);
                    }
                    
                    // Add right children
                    foreach (var childNode in subNode.RightChildren.Select(rightChild => CreateNodeFromSubtree(rightChild, expandedRules)))
                    {
                        treeNode.AddRightChild(childNode);
                    }
                    
                    expandedNodes.Add(treeNode);
                }
                
                // If there's only one node, it becomes the result
                if (expandedNodes.Count == 1)
                {
                    var result = expandedNodes[0];
                    
                    // Add original left children from the rule node
                    foreach (var leftChild in node.LeftChildren)
                    {
                        result.AddLeftChild(ReconstructTree(leftChild, expandedRules));
                    }
                    
                    // Add original right children from the rule node
                    foreach (var rightChild in node.RightChildren)
                    {
                        result.AddRightChild(ReconstructTree(rightChild, expandedRules));
                    }
                    
                    return result;
                }
                
                // For multiple nodes, the first becomes the result, others become right children
                var resultNode = expandedNodes[0];
                
                // Connect the expanded nodes in a chain
                IDependencyTreeNode current = resultNode;
                for (var i = 1; i < expandedNodes.Count; i++)
                {
                    current.AddRightChild(expandedNodes[i]);
                    current = expandedNodes[i];
                }
                
                // Add original left children to first node
                foreach (var leftChild in node.LeftChildren)
                {
                    resultNode.AddLeftChild(ReconstructTree(leftChild, expandedRules));
                }
                
                // Add original right children to last node
                foreach (var rightChild in node.RightChildren)
                {
                    current.AddRightChild(ReconstructTree(rightChild, expandedRules));
                }
                
                return resultNode;
            }
            else
            {
                // Regular node, just process children recursively
                var result = new DependencyTreeNode(nodeValue);
                
                foreach (var leftChild in node.LeftChildren)
                {
                    result.AddLeftChild(ReconstructTree(leftChild, expandedRules));
                }
                
                foreach (var rightChild in node.RightChildren)
                {
                    result.AddRightChild(ReconstructTree(rightChild, expandedRules));
                }
                
                return result;
            }
        }
        
        /// <summary>
        /// Creates a tree node from a subtree node.
        /// </summary>
        private static IDependencyTreeNode CreateNodeFromSubtree(SubtreeNode subtree, Dictionary<string, List<SubtreeNode>> expandedRules)
        {
            if (subtree.Value.StartsWith("R") && expandedRules.TryGetValue(subtree.Value, out var expansion))
            {
                if (expansion.Count == 0)
                    return new DependencyTreeNode("<empty>");
                
                // Create nodes for expansion
                var expandedNodes = new List<IDependencyTreeNode>();
                foreach (var subNode in expansion)
                {
                    var treeNode = new DependencyTreeNode(subNode.Value);
                    
                    // Add children
                    foreach (var childNode in subNode.LeftChildren.Select(leftChild => CreateNodeFromSubtree(leftChild, expandedRules)))
                    {
                        treeNode.AddLeftChild(childNode);
                    }
                    
                    foreach (var childNode in subNode.RightChildren.Select(rightChild => CreateNodeFromSubtree(rightChild, expandedRules)))
                    {
                        treeNode.AddRightChild(childNode);
                    }
                    
                    expandedNodes.Add(treeNode);
                }
                
                // Connect nodes
                if (expandedNodes.Count == 1)
                    return expandedNodes[0];
                
                var resultNode = expandedNodes[0];
                var current = resultNode;
                
                for (var i = 1; i < expandedNodes.Count; i++)
                {
                    current.AddRightChild(expandedNodes[i]);
                    current = expandedNodes[i];
                }
                
                return resultNode;
            }
            else
            {
                // Regular node
                var node = new DependencyTreeNode(subtree.Value);
                
                // Add children
                foreach (var childNode in subtree.LeftChildren.Select(leftChild => CreateNodeFromSubtree(leftChild, expandedRules)))
                {
                    node.AddLeftChild(childNode);
                }
                
                foreach (var childNode in subtree.RightChildren.Select(rightChild => CreateNodeFromSubtree(rightChild, expandedRules)))
                {
                    node.AddRightChild(childNode);
                }
                
                return node;
            }
        }

        /// <summary>
        /// Creates a deep copy of a tree.
        /// </summary>
        private static IDependencyTreeNode DeepCopyTree(IDependencyTreeNode node)
        {
            var copy = new DependencyTreeNode(node.Value.ToString() ?? string.Empty);

            foreach (var leftChild in node.LeftChildren)
                copy.AddLeftChild(DeepCopyTree(leftChild));

            foreach (var rightChild in node.RightChildren)
                copy.AddRightChild(DeepCopyTree(rightChild));

            return copy;
        }

        /// <summary>
        /// Serializes the tree to binary.
        /// </summary>
        private byte[] SerializeTree(IDependencyTreeNode tree)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            // Dictionary for value encoding
            var valueDict = new Dictionary<string, ushort>();
            ushort nextId = 1; // 0 reserved for null
                
            // Collect all unique values
            CollectValues(tree, valueDict, ref nextId);
                
            // Write value dictionary
            writer.Write((ushort)valueDict.Count);
            foreach (var entry in valueDict)
            {
                writer.Write(entry.Value); // ID
                writer.Write(entry.Key);   // Value
            }
                
            // Write tree structure
            SerializeNode(writer, tree, valueDict);
                
            return ms.ToArray();
        }
        
        /// <summary>
        /// Collects all unique values in the tree.
        /// </summary>
        private static void CollectValues(IDependencyTreeNode node, Dictionary<string, ushort> valueDict, ref ushort nextId)
        {
            // Add this node's value if not already in dictionary
            var value = node.Value.ToString();
            if (value != null && !valueDict.ContainsKey(value))
                valueDict[value] = nextId++;
            
            // Process all children
            foreach (var child in node.LeftChildren)
                CollectValues(child, valueDict, ref nextId);
            
            foreach (var child in node.RightChildren)
                CollectValues(child, valueDict, ref nextId);
        }
        
        /// <summary>
        /// Serializes a node to binary.
        /// </summary>
        private void SerializeNode(BinaryWriter writer, IDependencyTreeNode node, Dictionary<string, ushort> valueDict)
        {
            // Write value ID
            var value = node.Value.ToString();
            if (value != null) writer.Write(valueDict[value]);

            // Write left children
            var leftCount = (byte)Math.Min(255, node.LeftChildren.Count);
            writer.Write(leftCount);
            
            if (leftCount == 255) // Extended count
                writer.Write((ushort)node.LeftChildren.Count);
            
            foreach (var child in node.LeftChildren)
                SerializeNode(writer, child, valueDict);
            
            // Write right children
            var rightCount = (byte)Math.Min(255, node.RightChildren.Count);
            writer.Write(rightCount);
            
            if (rightCount == 255) // Extended count
                writer.Write((ushort)node.RightChildren.Count);
            
            foreach (var child in node.RightChildren)
                SerializeNode(writer, child, valueDict);
        }

        /// <summary>
        /// Serializes rules for storage in metadata.
        /// </summary>
        private Dictionary<string, string> SerializeRules(Dictionary<string, Digram> rules)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var rule in rules)
                result[rule.Key] = $"{rule.Value.FirstValue}|{rule.Value.SecondValue}";

            return result;
        }

        /// <summary>
        /// Deserializes rules from metadata.
        /// </summary>
        private Dictionary<string, Digram> DeserializeRules(Dictionary<string, string> serializedRules)
        {
            Dictionary<string, Digram> result = new Dictionary<string, Digram>();

            foreach (var rule in serializedRules)
            {
                var parts = rule.Value.Split('|');
                if (parts.Length == 2)
                    result[rule.Key] = new Digram(parts[0], parts[1]);
            }

            return result;
        }

        /// <summary>
        /// Deserializes a tree from binary.
        /// </summary>
        private IDependencyTreeNode DeserializeTree(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);
            try
            {
                // Read value dictionary
                var valueCount = reader.ReadUInt16();
                var idToValue = new Dictionary<ushort, string>();
                    
                for (var i = 0; i < valueCount; i++)
                {
                    var id = reader.ReadUInt16();
                    var value = reader.ReadString();
                    idToValue[id] = value;
                }
                    
                // Deserialize tree
                return DeserializeNode(reader, idToValue);
            }
            catch (Exception ex)
            {
                if (_debug)
                    Console.WriteLine($"Deserialization error: {ex.Message}");
                    
                return new DependencyTreeNode("<Deserialization error>");
            }
        }
        
        /// <summary>
        /// Deserializes a node from binary.
        /// </summary>
        private IDependencyTreeNode DeserializeNode(BinaryReader reader, Dictionary<ushort, string> idToValue)
        {
            var valueId = reader.ReadUInt16();
            if (valueId == 0) // Null node
                throw new InvalidOperationException("Null node encountered during deserialization.");
            
            // Get the actual value
            if (!idToValue.TryGetValue(valueId, out var nodeValue))
                nodeValue = $"<Unknown:{valueId}>";
            
            var node = new DependencyTreeNode(nodeValue);
            
            // Read left children
            var leftCountByte = reader.ReadByte();
            int leftCount = leftCountByte;
            
            if (leftCountByte == 255) // Extended count
                leftCount = reader.ReadUInt16();
            
            for (var i = 0; i < leftCount; i++)
            {
                var child = DeserializeNode(reader, idToValue);
                node.AddLeftChild(child);
            }
            
            // Read right children
            var rightCountByte = reader.ReadByte();
            int rightCount = rightCountByte;
            
            if (rightCountByte == 255) // Extended count
                rightCount = reader.ReadUInt16();
            
            for (var i = 0; i < rightCount; i++)
            {
                var child = DeserializeNode(reader, idToValue);
                node.AddRightChild(child);
            }
            
            return node;
        }
    }
}