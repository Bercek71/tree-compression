using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline;
using TreeCompressionPipeline.Filters;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace Tests;

[TestClass]
public class EdgeCaseTests
{
    [TestMethod]
    public void EmptyInput_HandledGracefully()
    {
        // Arrange
        var textStrategy = new RePairTreeLinearization();
        var treeCreationStrategy = new TestTreeCreationStrategy();
            
        // Create a pipeline from text -> tree -> compressed
        var pipeline = new Pipeline();
        pipeline.AddFilter(new TextToTreeFilter<IDependencyTreeNode>(treeCreationStrategy))
            .AddFilter(new CompressionFilter<IDependencyTreeNode>(textStrategy));
            
        // Act
        var result = pipeline.Process(string.Empty) as CompressedTree;
            
        // Assert
        Assert.IsNotNull(result, "Should handle empty input");
            
        // Should be able to decompress
        var decompressionFilter = new DecompressionFilter<IDependencyTreeNode>(textStrategy);
        var decompressed = decompressionFilter.Process(result) as IDependencyTreeNode;
            
        Assert.IsNotNull(decompressed, "Should handle empty tree decompression");
    }
        
    [TestMethod]
    public void SingleNode_CompressDecompress_Works()
    {
        // Arrange
        var strategy = new RePairTreeLinearization();
        var singleNode = new DependencyTreeNode("single");
            
        // Act
        var compressed = strategy.Compress(singleNode);
        var decompressed = strategy.Decompress(compressed);
            
        // Assert
        Assert.IsNotNull(decompressed);
        Assert.AreEqual("single", decompressed.Value);
        Assert.AreEqual(0, decompressed.LeftChildren.Count);
        Assert.AreEqual(0, decompressed.RightChildren.Count);
    }
        
    [TestMethod]
    public void ExtremeLongNodeValues_Handled()
    {
        // Arrange
        var strategy = new RePairOptimizedLinearStrategy();
        var root = new DependencyTreeNode("root");
            
        // Create extremely long node values
        var longValue = new string('a', 10000); // 10KB string
        var child = new DependencyTreeNode(longValue);
        root.AddRightChild(child);
            
        // Act
        var compressed = strategy.Compress(root);
        var decompressed = strategy.Decompress(compressed);
            
        // Assert
        Assert.AreEqual(longValue, decompressed.RightChildren[0].Value);
    }
        
    [TestMethod]
    public void UnbalancedTree_DeepOnOneSide_HandledCorrectly()
    {
        // Arrange
        var strategy = new RePairOptimizedLinearStrategy();
        var root = new DependencyTreeNode("root");
            
        // Create a deeply unbalanced tree (all right children)
        var current = root;
        for (int i = 0; i < 100; i++)
        {
            var child = new DependencyTreeNode($"node{i}");
            current.AddRightChild(child);
            current = child;
        }
            
        var originalNodeCount = root.GetNodeCount();
            
        // Act
        var compressed = strategy.Compress(root);
        var decompressed = strategy.Decompress(compressed);
            
        // Assert
        Assert.AreEqual(originalNodeCount, decompressed.GetNodeCount());
            
        // Verify the deep path was preserved
        current = (DependencyTreeNode)decompressed;
        for (int i = 0; i < 100; i++)
        {
            Assert.AreEqual(1, current.RightChildren.Count, $"Node at depth {i} should have one right child");
            current = (DependencyTreeNode)current.RightChildren[0];
            Assert.AreEqual($"node{i}", current.Value);
        }
    }
        
    private class TestTreeCreationStrategy : ITreeCreationStrategy<IDependencyTreeNode>
    {
        public IDependencyTreeNode CreateTree(string text)
        {
            var root = new DependencyTreeNode("<root>");
                
            if (!string.IsNullOrEmpty(text))
            {
                var words = text.Split(' ');
                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word))
                        root.AddRightChild(new DependencyTreeNode(word));
                }
            }
                
            return root;
        }
    }
}