using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline.TreeStructure;

namespace Tests;

[TestClass]
public class StressTests
{
    [TestMethod]
    public void MultipleCompressDecompress_EnsureStability()
    {
        // Arrange
        var strategy = new RePairOptimizedLinearStrategy();
        var tree = CreateTestTree();
        var originalString = tree.ToString();
            
        // Act - Multiple compression-decompression cycles
        CompressedTree compressed = null;
        IDependencyTreeNode decompressed = null;
            
        for (int i = 0; i < 10; i++)
        {
            compressed = strategy.Compress(tree);
            decompressed = strategy.Decompress(compressed);
                
            // Use the decompressed tree as input for next iteration
            tree = decompressed;
        }
            
        // Assert - After multiple cycles, structure should be preserved
        var finalString = decompressed.ToString();
        Assert.AreEqual(originalString, finalString, "Tree structure should be preserved after multiple cycles");
    }
        
    [TestMethod]
    public void RandomTrees_CompressDecompress_Stability()
    {
        // Arrange
        var strategy = new RePairOptimizedLinearStrategy();
        var random = new Random(42); // Fixed seed for reproducibility
            
        // Create 20 random trees of varying sizes
        for (int i = 0; i < 20; i++)
        {
            var tree = CreateRandomTree(random, 5 + i, 3); // Varying sizes
            var originalString = tree.ToString();
                
            // Act
            var compressed = strategy.Compress(tree);
            var decompressed = strategy.Decompress(compressed);
            var resultString = decompressed.ToString();
                
            // Assert - for each random tree
            Assert.AreEqual(originalString, resultString, $"Random tree {i} failed to preserve structure");
        }
    }
        
    private IDependencyTreeNode CreateTestTree()
    {
        var root = new DependencyTreeNode("root");
            
        // Create a tree with specific patterns
        for (int i = 0; i < 3; i++)
        {
            var branch = new DependencyTreeNode($"branch{i}");
                
            for (int j = 0; j < 3; j++)
            {
                var leaf = new DependencyTreeNode($"leaf{i}{j}");
                branch.AddRightChild(leaf);
            }
                
            root.AddRightChild(branch);
        }
            
        return root;
    }
        
    private IDependencyTreeNode CreateRandomTree(Random random, int maxNodes, int maxChildren)
    {
        var root = new DependencyTreeNode("root");
        var nodesToProcess = new Queue<IDependencyTreeNode>();
        nodesToProcess.Enqueue(root);
            
        int totalNodes = 1; // Start with root
            
        while (nodesToProcess.Count > 0 && totalNodes < maxNodes)
        {
            var currentNode = nodesToProcess.Dequeue();
                
            // Randomly decide how many children to add to this node
            var childrenCount = random.Next(maxChildren + 1);
            childrenCount = Math.Min(childrenCount, maxNodes - totalNodes);
                
            for (int i = 0; i < childrenCount; i++)
            {
                var child = new DependencyTreeNode($"node{totalNodes}");
                totalNodes++;
                    
                // Randomly add as left or right child
                if (random.Next(2) == 0)
                    currentNode.AddLeftChild(child);
                else
                    currentNode.AddRightChild(child);
                    
                // Add to processing queue
                nodesToProcess.Enqueue(child);
            }
        }
            
        return root;
    }
}