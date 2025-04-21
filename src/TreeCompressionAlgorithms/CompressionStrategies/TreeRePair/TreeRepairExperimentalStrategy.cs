#if EXPERIMENTAL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline.Compression
{
    public class TreeRePair
    {
        // Dictionary to track digram (subtree pattern) frequencies
        private Dictionary<string, int> digramFrequencies = new Dictionary<string, int>();
        // Dictionary to map rule names to subtrees
        private Dictionary<string, IDependencyTreeNode> rules = new Dictionary<string, IDependencyTreeNode>();
        // Counter for generating unique rule names
        private int ruleCounter = 0;

        public CompressedTree Compress(IDependencyTreeNode root)
        {
            // Step 1: Count digram frequencies
            CountDigrams(root);

            // Step 2: Replace most frequent digrams with rules
            while (digramFrequencies.Count > 0)
            {
                // Find most frequent digram
                var mostFrequentDigram = digramFrequencies
                    .OrderByDescending(kv => kv.Value)
                    .FirstOrDefault();

                // If no frequent digrams remain or frequency is too low, stop
                if (mostFrequentDigram.Key == null || mostFrequentDigram.Value < 2)
                    break;

                // Create a new rule for this digram
                string ruleName = $"R{ruleCounter++}";
                
                // Parse the digram and create a subtree for it
                var digramNodes = ParseDigram(mostFrequentDigram.Key);
                var ruleNode = CreateRuleNode(ruleName, digramNodes);
                rules[ruleName] = ruleNode;

                // Replace all occurrences of this digram with the rule
                ReplaceDigrams(root, mostFrequentDigram.Key, ruleName);

                // Recalculate digram frequencies
                digramFrequencies.Clear();
                CountDigrams(root);
            }

            return new CompressedTree(root, rules);
        }

        private void CountDigrams(IDependencyTreeNode node)
        {
            // Count digrams in the tree (pairs of adjacent nodes)
            if (node == null) return;

            // Process left children
            for (int i = 0; i < node.LeftChildren.Count - 1; i++)
            {
                string digram = GetDigramSignature(node.LeftChildren[i], node.LeftChildren[i + 1]);
                IncrementDigramFrequency(digram);
                
                // Recursively process children
                CountDigrams(node.LeftChildren[i]);
            }

            // Process right children
            for (int i = 0; i < node.RightChildren.Count - 1; i++)
            {
                string digram = GetDigramSignature(node.RightChildren[i], node.RightChildren[i + 1]);
                IncrementDigramFrequency(digram);
                
                // Recursively process children
                CountDigrams(node.RightChildren[i]);
            }

            // Process last children if they exist
            if (node.LeftChildren.Count > 0)
                CountDigrams(node.LeftChildren[node.LeftChildren.Count - 1]);
            if (node.RightChildren.Count > 0)
                CountDigrams(node.RightChildren[node.RightChildren.Count - 1]);
        }

        private void IncrementDigramFrequency(string digram)
        {
            if (digramFrequencies.ContainsKey(digram))
                digramFrequencies[digram]++;
            else
                digramFrequencies[digram] = 1;
        }

        private string GetDigramSignature(IDependencyTreeNode node1, IDependencyTreeNode node2)
        {
            // Create a unique signature for a pair of nodes
            return $"{node1.Value}-{node2.Value}";
        }

        private List<IDependencyTreeNode> ParseDigram(string digramSignature)
        {
            // Parse the digram signature back into nodes
            var parts = digramSignature.Split('-');
            var result = new List<IDependencyTreeNode>();
            
            // Create simple nodes for the digram parts
            // In a real implementation, this would need to be more sophisticated
            // to handle the actual node structure
            result.Add(new DependencyTreeNode(parts[0]));
            result.Add(new DependencyTreeNode(parts[1]));
            
            return result;
        }

        private IDependencyTreeNode CreateRuleNode(string ruleName, List<IDependencyTreeNode> children)
        {
            // Create a rule node that represents the digram
            var ruleNode = new DependencyTreeNode(ruleName);
            
            // Add the children to the rule node
            foreach (var child in children)
            {
                ruleNode.AddRightChild(child);
            }
            
            return ruleNode;
        }

        private void ReplaceDigrams(IDependencyTreeNode node, string digramSignature, string ruleName)
        {
            if (node == null) return;

            // Replace digrams in left children
            ReplaceDigramsInChildList(node.LeftChildren, digramSignature, ruleName);
            
            // Replace digrams in right children
            ReplaceDigramsInChildList(node.RightChildren, digramSignature, ruleName);

            // Recursively process all children
            foreach (var child in node.LeftChildren)
            {
                ReplaceDigrams(child, digramSignature, ruleName);
            }
            
            foreach (var child in node.RightChildren)
            {
                ReplaceDigrams(child, digramSignature, ruleName);
            }
        }

        private void ReplaceDigramsInChildList(List<IDependencyTreeNode> children, string digramSignature, string ruleName)
        {
            for (int i = 0; i < children.Count - 1; i++)
            {
                string currentDigram = GetDigramSignature(children[i], children[i + 1]);
                if (currentDigram == digramSignature)
                {
                    // Replace the two nodes with a rule reference
                    var ruleRef = new DependencyTreeNode(ruleName);
                    children.RemoveAt(i);
                    children.RemoveAt(i); // The index i+1 is now i after the first removal
                    children.Insert(i, ruleRef);
                    
                    // Since we modified the list, we need to adjust the index
                    i--;
                }
            }
        }
    }

    public class CompressedTree
    {
        public IDependencyTreeNode Root { get; }
        public Dictionary<string, IDependencyTreeNode> Rules { get; }

        public CompressedTree(IDependencyTreeNode root, Dictionary<string, IDependencyTreeNode> rules)
        {
            Root = root;
            Rules = rules;
        }

        public byte[] SerializeToBinary()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // Write the number of rules
                writer.Write(Rules.Count);

                // Write each rule
                foreach (var rule in Rules)
                {
                    writer.Write(rule.Key);
                    SerializeNode(writer, rule.Value);
                }

                // Write the compressed tree
                SerializeNode(writer, Root);

                return ms.ToArray();
            }
        }

        private void SerializeNode(BinaryWriter writer, IDependencyTreeNode node)
        {
            if (node == null)
            {
                writer.Write(false); // Indicate null node
                return;
            }

            writer.Write(true); // Indicate non-null node
            writer.Write(node.Value.ToString()); // Write node value

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

        public static CompressedTree DeserializeFromBinary(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // Read the number of rules
                int ruleCount = reader.ReadInt32();
                
                Dictionary<string, IDependencyTreeNode> rules = new Dictionary<string, IDependencyTreeNode>();
                
                // Read each rule
                for (int i = 0; i < ruleCount; i++)
                {
                    string ruleName = reader.ReadString();
                    IDependencyTreeNode ruleNode = DeserializeNode(reader, rules);
                    rules[ruleName] = ruleNode;
                }
                
                // Read the compressed tree
                IDependencyTreeNode root = DeserializeNode(reader, rules);
                
                return new CompressedTree(root, rules);
            }
        }

        private static IDependencyTreeNode DeserializeNode(BinaryReader reader, Dictionary<string, IDependencyTreeNode> rules)
        {
            bool isNonNull = reader.ReadBoolean();
            if (!isNonNull)
                return null;

            string value = reader.ReadString();
            var node = new DependencyTreeNode(value);

            // Read left children
            int leftChildCount = reader.ReadInt32();
            for (int i = 0; i < leftChildCount; i++)
            {
                var child = DeserializeNode(reader, rules);
                if (child != null)
                    node.AddLeftChild(child);
            }

            // Read right children
            int rightChildCount = reader.ReadInt32();
            for (int i = 0; i < rightChildCount; i++)
            {
                var child = DeserializeNode(reader, rules);
                if (child != null)
                    node.AddRightChild(child);
            }

            // If this is a rule reference, expand it
            if (value.StartsWith("R") && rules.ContainsKey(value))
            {
                return ExpandRule(rules[value], rules);
            }

            return node;
        }

        private static IDependencyTreeNode ExpandRule(IDependencyTreeNode ruleNode, Dictionary<string, IDependencyTreeNode> rules)
        {
            // Create a deep copy of the rule node to avoid modifying the original
            var expandedNode = new DependencyTreeNode(ruleNode.Value.ToString());
            
            // Copy and expand left children
            foreach (var child in ruleNode.LeftChildren)
            {
                string childValue = child.Value.ToString();
                if (childValue.StartsWith("R") && rules.ContainsKey(childValue))
                {
                    expandedNode.AddLeftChild(ExpandRule(rules[childValue], rules));
                }
                else
                {
                    expandedNode.AddLeftChild(DeepCopyNode(child));
                }
            }
            
            // Copy and expand right children
            foreach (var child in ruleNode.RightChildren)
            {
                string childValue = child.Value.ToString();
                if (childValue.StartsWith("R") && rules.ContainsKey(childValue))
                {
                    expandedNode.AddRightChild(ExpandRule(rules[childValue], rules));
                }
                else
                {
                    expandedNode.AddRightChild(DeepCopyNode(child));
                }
            }
            
            return expandedNode;
        }

        private static IDependencyTreeNode DeepCopyNode(IDependencyTreeNode node)
        {
            var copy = new DependencyTreeNode(node.Value.ToString());
            
            foreach (var child in node.LeftChildren)
            {
                copy.AddLeftChild(DeepCopyNode(child));
            }
            
            foreach (var child in node.RightChildren)
            {
                copy.AddRightChild(DeepCopyNode(child));
            }
            
            return copy;
        }
    }

    // Interface for the dependency tree node, assuming it's defined elsewhere

    // Example usage
    public class TreeCompressor
    {
        public static void Main()
        {
            // Create a sample tree
            var root = new DependencyTreeNode("<DocumentRoot>");
            var sentence = new DependencyTreeNode("This is a test sentence .");
            root.AddRightChild(sentence);

            // Compress the tree
            var treeRePair = new TreeRePair();
            var compressedTree = treeRePair.Compress(root);

            // Serialize to binary
            byte[] binaryData = compressedTree.SerializeToBinary();

            // Deserialize from binary (for verification)
            var decompressedTree = CompressedTree.DeserializeFromBinary(binaryData);

            // Output the original and decompressed trees
            Console.WriteLine("Original tree: " + root.ToString());
            Console.WriteLine("Decompressed tree: " + decompressedTree.Root.ToString());

            // Output compression statistics
            Console.WriteLine($"Original size: {EstimateTreeSize(root)} bytes");
            Console.WriteLine($"Compressed size: {binaryData.Length} bytes");
            Console.WriteLine($"Compression ratio: {(float)binaryData.Length / EstimateTreeSize(root):P2}");
        }

        private static int EstimateTreeSize(IDependencyTreeNode node)
        {
            int size = node.Value.ToString().Length * 2; // Unicode chars
            
            foreach (var child in node.LeftChildren)
            {
                size += EstimateTreeSize(child);
            }
            
            foreach (var child in node.RightChildren)
            {
                size += EstimateTreeSize(child);
            }
            
            return size + 16; // Add overhead for object references
        }
    }
}
#endif