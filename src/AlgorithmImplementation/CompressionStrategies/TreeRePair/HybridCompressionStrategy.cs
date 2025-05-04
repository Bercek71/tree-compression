using System.Text;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.CompressionStrategies
{
    /// <summary>
    /// A hybrid compression strategy that combines subtree pattern mining with grammar-based compression.
    /// This strategy works well for natural language dependency trees by identifying common syntactic patterns.
    /// </summary>
    public class HybridCompressionStrategy : ICompressionStrategy<IDependencyTreeNode>
    {
        private readonly int _minFrequency;
        private readonly int _maxPatternSize;
        private readonly int _minimalGain;
        private readonly bool _useContextualCompression;
        private readonly Dictionary<string, string> _grammarRules = new();
        private readonly Dictionary<string, FrequentPattern> _patterns = new();
        private int _ruleCounter = 1;

        // Track pattern frequency and context
        private class FrequentPattern
        {
            public string Hash { get; set; }
            public int Frequency { get; set; }
            public int Size { get; set; }
            public List<string> ContextBefore { get; } = new();
            public List<string> ContextAfter { get; } = new();

            public int CompressionGain => (Size * Frequency) - (Size + Frequency);
        }

        /// <summary>
        /// Initializes a new instance of the HybridCompressionStrategy.
        /// </summary>
        /// <param name="minFrequency">Minimum frequency for a pattern to be considered for compression</param>
        /// <param name="maxPatternSize">Maximum size of patterns to look for</param>
        /// <param name="minimalGain">Minimum compression gain to actually replace a pattern</param>
        /// <param name="useContextualCompression">Whether to use contextual information for better compression</param>
        public HybridCompressionStrategy(
            int minFrequency = 2, 
            int maxPatternSize = 8, 
            int minimalGain = 5,
            bool useContextualCompression = true)
        {
            _minFrequency = minFrequency;
            _maxPatternSize = maxPatternSize;
            _minimalGain = minimalGain;
            _useContextualCompression = useContextualCompression;
        }

        /// <summary>
        /// Compresses a dependency tree using the hybrid strategy.
        /// </summary>
        public CompressedTree Compress(IDependencyTreeNode tree)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));

            // Reset state
            _grammarRules.Clear();
            _patterns.Clear();
            _ruleCounter = 1;

            // Phase 1: Identify frequent patterns and analyze their context
            IdentifyFrequentPatterns(tree);

            // Phase 2: Create a compressed representation using grammar rules
            var compressedTree = CompressTree(tree);

            // Return compressed tree with metadata
            return new CompressedTree
            {
                Structure = SerializeTree(compressedTree),
                Metadata = new Dictionary<string, string>(_grammarRules)
            };
        }

        /// <summary>
        /// Identifies frequent patterns in the tree by traversing it and analyzing subtree structures.
        /// </summary>
        private void IdentifyFrequentPatterns(IDependencyTreeNode tree)
        {
            // Convert tree to a linearized sequence for pattern mining
            var sequence = new List<string>();
            LinearizeTree(tree, sequence);

            // Use sliding window to identify patterns of various sizes
            for (int patternSize = 2; patternSize <= _maxPatternSize; patternSize++)
            {
                if (patternSize >= sequence.Count) break;

                var patternFrequency = new Dictionary<string, FrequentPattern>();

                // Slide window through sequence
                for (int i = 0; i <= sequence.Count - patternSize; i++)
                {
                    var pattern = string.Join("|", sequence.Skip(i).Take(patternSize));
                    
                    if (!patternFrequency.TryGetValue(pattern, out var freqPattern))
                    {
                        freqPattern = new FrequentPattern 
                        { 
                            Hash = pattern,
                            Frequency = 0,
                            Size = patternSize
                        };
                        patternFrequency[pattern] = freqPattern;
                    }
                    
                    freqPattern.Frequency++;
                    
                    // Store context (if enabled)
                    if (_useContextualCompression)
                    {
                        if (i > 0)
                            freqPattern.ContextBefore.Add(sequence[i - 1]);
                        
                        if (i + patternSize < sequence.Count)
                            freqPattern.ContextAfter.Add(sequence[i + patternSize]);
                    }
                }

                // Keep patterns that are frequent enough and provide compression benefit
                foreach (var pattern in patternFrequency.Values)
                {
                    if (pattern.Frequency >= _minFrequency && pattern.CompressionGain >= _minimalGain)
                    {
                        _patterns[pattern.Hash] = pattern;
                    }
                }
            }
        }

        /// <summary>
        /// Compresses the tree by replacing frequent patterns with grammar rules.
        /// </summary>
        private IDependencyTreeNode CompressTree(IDependencyTreeNode tree)
        {
            // Create a working copy to avoid modifying the original
            var workTree = CloneTree(tree);
            
            // Sort patterns by compression gain for optimal compression
            var sortedPatterns = _patterns.Values
                .OrderByDescending(p => p.CompressionGain)
                .ThenByDescending(p => p.Size)
                .ToList();
            
            // Apply pattern replacements
            foreach (var pattern in sortedPatterns)
            {
                var ruleName = $"R{_ruleCounter++}";
                _grammarRules[ruleName] = pattern.Hash;
                
                // Build the pattern as a tree structure for replacement
                var patternTree = BuildPatternTree(pattern.Hash);
                
                // Replace all occurrences with rule reference
                ReplacePatternInTree(workTree, patternTree, ruleName);
            }
            
            return workTree;
        }

        /// <summary>
        /// Linearizes a tree into a sequence of tokens for pattern mining.
        /// </summary>
        private void LinearizeTree(IDependencyTreeNode node, List<string> sequence)
        {
            if (node == null) return;
            
            // Add node tag with value
            sequence.Add($"N:{node.Value}");
            
            // Process left children with special marker
            if (node.LeftChildren.Count > 0)
            {
                sequence.Add("L:{");
                foreach (var child in node.LeftChildren)
                {
                    LinearizeTree(child, sequence);
                }
                sequence.Add("}");
            }
            
            // Process right children with special marker
            if (node.RightChildren.Count > 0)
            {
                sequence.Add("R:{");
                foreach (var child in node.RightChildren)
                {
                    LinearizeTree(child, sequence);
                }
                sequence.Add("}");
            }
        }

        /// <summary>
        /// Builds a tree from a pattern hash for comparison and replacement.
        /// </summary>
        private IDependencyTreeNode BuildPatternTree(string patternHash)
        {
            var parts = patternHash.Split('|');
            var root = new DependencyTreeNode("PatternRoot");
            
            // Build a simple tree structure from pattern parts
            int currentDepth = 0;
            IDependencyTreeNode current = root;
            
            foreach (var part in parts)
            {
                if (part.StartsWith("N:"))
                {
                    var value = part.Substring(2);
                    var node = new DependencyTreeNode(value);
                    current.AddRightChild(node);
                    current = node;
                }
                else if (part == "L:{")
                {
                    currentDepth++;
                }
                else if (part == "R:{")
                {
                    currentDepth++;
                }
                else if (part == "}")
                {
                    currentDepth--;
                    if (currentDepth > 0)
                        current = GetParent(root, current);
                }
            }
            
            return root;
        }

        /// <summary>
        /// Helper to find parent node in a tree.
        /// </summary>
        private IDependencyTreeNode GetParent(IDependencyTreeNode root, IDependencyTreeNode node)
        {
            return node.Parent ?? root;
        }

        /// <summary>
        /// Replaces all occurrences of a pattern in the tree with a rule reference.
        /// </summary>
        private void ReplacePatternInTree(IDependencyTreeNode tree, IDependencyTreeNode pattern, string ruleName)
        {
            // This is a simplified implementation that would need to be expanded in real use
            // It would search for subtree matches and replace them with rule references
            
            // Process each left child subtree
            for (int i = 0; i < tree.LeftChildren.Count; i++)
            {
                var child = tree.LeftChildren[i];
                
                // Check if this subtree matches the pattern
                if (MatchesPattern(child, pattern))
                {
                    // Replace with rule node
                    var ruleNode = new DependencyTreeNode(ruleName);
                    tree.LeftChildren[i] = ruleNode;
                }
                else
                {
                    // Recursively process this subtree
                    ReplacePatternInTree(child, pattern, ruleName);
                }
            }
            
            // Process each right child subtree
            for (int i = 0; i < tree.RightChildren.Count; i++)
            {
                var child = tree.RightChildren[i];
                
                // Check if this subtree matches the pattern
                if (MatchesPattern(child, pattern))
                {
                    // Replace with rule node
                    var ruleNode = new DependencyTreeNode(ruleName);
                    tree.RightChildren[i] = ruleNode;
                }
                else
                {
                    // Recursively process this subtree
                    ReplacePatternInTree(child, pattern, ruleName);
                }
            }
        }

        /// <summary>
        /// Checks if a subtree matches a pattern structure.
        /// </summary>
        private bool MatchesPattern(IDependencyTreeNode subtree, IDependencyTreeNode pattern)
        {
            // This is a simplified pattern matching algorithm
            // In reality, would need more sophisticated tree comparison
            
            if (subtree == null || pattern == null)
                return subtree == pattern;
                
            if (!subtree.Value.Equals(pattern.Value))
                return false;
                
            // For simplicity, we're just checking structure equality
            // A more complete implementation would use tree edit distance or similar algorithm
            if (subtree.LeftChildren.Count != pattern.LeftChildren.Count ||
                subtree.RightChildren.Count != pattern.RightChildren.Count)
                return false;
                
            // Check all children match
            for (int i = 0; i < subtree.LeftChildren.Count; i++)
            {
                if (!MatchesPattern(subtree.LeftChildren[i], pattern.LeftChildren[i]))
                    return false;
            }
            
            for (int i = 0; i < subtree.RightChildren.Count; i++)
            {
                if (!MatchesPattern(subtree.RightChildren[i], pattern.RightChildren[i]))
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Creates a clone of a tree structure to avoid modifying the original.
        /// </summary>
        private IDependencyTreeNode CloneTree(IDependencyTreeNode source)
        {
            if (source == null)
                return null;
                
            var clone = new DependencyTreeNode(source.Value.ToString());
            
            foreach (var leftChild in source.LeftChildren)
            {
                clone.AddLeftChild(CloneTree(leftChild));
            }
            
            foreach (var rightChild in source.RightChildren)
            {
                clone.AddRightChild(CloneTree(rightChild));
            }
            
            return clone;
        }

        /// <summary>
        /// Serializes a tree structure to binary format for storage.
        /// </summary>
        private byte[] SerializeTree(IDependencyTreeNode tree)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            // Write grammar rules count
            writer.Write(_grammarRules.Count);
            
            // Write all grammar rules
            foreach (var rule in _grammarRules)
            {
                writer.Write(rule.Key);
                writer.Write(rule.Value);
            }
            
            // Recursively serialize the tree structure
            SerializeNode(writer, tree);
            
            return ms.ToArray();
        }

        /// <summary>
        /// Serializes a single node and its children.
        /// </summary>
        private void SerializeNode(BinaryWriter writer, IDependencyTreeNode node)
        {
            if (node == null)
            {
                writer.Write(false); // Null node marker
                return;
            }
            
            writer.Write(true); // Non-null node marker
            writer.Write(node.Value.ToString() ?? string.Empty);
            
            // Write left children
            writer.Write(node.LeftChildren.Count);
            foreach (var child in node.LeftChildren)
            {
                SerializeNode(writer, child);
            }
            
            // Write right children
            writer.Write(node.RightChildren.Count);
            foreach (var child in node.RightChildren)
            {
                SerializeNode(writer, child);
            }
        }

        /// <summary>
        /// Decompresses a tree structure from a CompressedTree.
        /// </summary>
        public IDependencyTreeNode Decompress(CompressedTree compressedTree)
        {
            if (compressedTree == null)
                throw new ArgumentNullException(nameof(compressedTree));

            // Reset grammar rules dictionary
            _grammarRules.Clear();
            
            // Extract grammar rules from metadata
            if (compressedTree.Metadata is Dictionary<string, string> rules)
            {
                foreach (var rule in rules)
                {
                    _grammarRules[rule.Key] = rule.Value;
                }
            }
            
            // Deserialize compressed tree structure
            using var ms = new MemoryStream(compressedTree.Structure);
            using var reader = new BinaryReader(ms);
            
            // Read grammar rules count
            int ruleCount = reader.ReadInt32();
            
            // Read all grammar rules
            for (int i = 0; i < ruleCount; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                _grammarRules[key] = value;
            }
            
            // Deserialize and expand the tree structure
            var compressedNode = DeserializeNode(reader);
            return ExpandRules(compressedNode);
        }

        /// <summary>
        /// Deserializes a single node and its children.
        /// </summary>
        private IDependencyTreeNode DeserializeNode(BinaryReader reader)
        {
            bool hasNode = reader.ReadBoolean();
            if (!hasNode)
                return null;
                
            string value = reader.ReadString();
            var node = new DependencyTreeNode(value);
            
            // Read left children
            int leftCount = reader.ReadInt32();
            for (int i = 0; i < leftCount; i++)
            {
                var child = DeserializeNode(reader);
                if (child != null)
                    node.AddLeftChild(child);
            }
            
            // Read right children
            int rightCount = reader.ReadInt32();
            for (int i = 0; i < rightCount; i++)
            {
                var child = DeserializeNode(reader);
                if (child != null)
                    node.AddRightChild(child);
            }
            
            return node;
        }

        /// <summary>
        /// Expands grammar rules in a compressed tree to reconstruct the original structure.
        /// </summary>
        private IDependencyTreeNode ExpandRules(IDependencyTreeNode compressedNode)
        {
            if (compressedNode == null)
                return null;
                
            // Check if this node is a rule reference
            string value = compressedNode.Value.ToString() ?? string.Empty;
            if (_grammarRules.TryGetValue(value, out string patternHash))
            {
                // Expand rule by converting pattern back to tree
                var expandedNode = BuildPatternTree(patternHash);
                
                // Clone to avoid sharing subtrees
                return CloneTree(expandedNode);
            }
            
            // Not a rule reference, process recursively
            var node = new DependencyTreeNode(value);
            
            foreach (var leftChild in compressedNode.LeftChildren)
            {
                node.AddLeftChild(ExpandRules(leftChild));
            }
            
            foreach (var rightChild in compressedNode.RightChildren)
            {
                node.AddRightChild(ExpandRules(rightChild));
            }
            
            return node;
        }
    }
}
