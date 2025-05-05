using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline.TreeStructure;

namespace Tests.AlgorithmImplementationTests;

[TestClass]
public class TreeRePairOptimizedLinearStrategyTests
{
    [TestMethod]
    public void TreeRePairOptimizedLinear_RoundTrip_PreservesStructure()
    {
        // Arrange
        var strategy = new TreeRepairOptimizedLinearStrategy();
        var root = CreateTestTree();
        var originalString = root.ToString();
            
        // Act
        var compressed = strategy.Compress(root);
        var decompressed = strategy.Decompress(compressed);
        var decompressedString = decompressed.ToString();
            
        // Assert
        Assert.AreEqual(originalString, decompressedString);
    }
        
    [TestMethod]
    public void TreeRePairOptimizedLinear_CompressLargeTree_HandlesEfficiently()
    {
        // Arrange
        var strategy = new TreeRepairOptimizedLinearStrategy();
        var root = new DependencyTreeNode("root");
            
        // Create large test tree (200+ nodes)
        for (int i = 0; i < 50; i++)
        {
            var branch = new DependencyTreeNode($"branch{i%3}");
                
            for (int j = 0; j < 4; j++)
            {
                var leaf = new DependencyTreeNode($"leaf{j%2}");
                branch.AddRightChild(leaf);
            }
                
            root.AddRightChild(branch);
        }
            
        // Act - measure performance
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        var compressed = strategy.Compress(root);
        stopwatch.Stop();
        var compressionTime = stopwatch.ElapsedMilliseconds;
            
        stopwatch.Restart();
        var decompressed = strategy.Decompress(compressed);
        stopwatch.Stop();
        var decompressionTime = stopwatch.ElapsedMilliseconds;
            
        // Assert
        Assert.IsNotNull(decompressed);
        Console.WriteLine($"Compression time: {compressionTime}ms, Decompression time: {decompressionTime}ms");
        Assert.AreEqual(root.GetNodeCount(), decompressed.GetNodeCount());
    }
        
    [TestMethod]
    public void TreeRePairOptimizedLinear_MinMaxNParameters_AffectCompression()
    {
        // Arrange
        var tree = CreateTestTree();
        var strategy1 = new TreeRepairOptimizedLinearStrategy(minN: 2, maxN: 5);
        var strategy2 = new TreeRepairOptimizedLinearStrategy(minN: 3, maxN: 10);
            
        // Act
        var compressed1 = strategy1.Compress(tree);
        var compressed2 = strategy2.Compress(tree);
            
        // Assert - Different parameters should produce different compression
        Assert.IsTrue(
            compressed1.Structure.Length != compressed2.Structure.Length ||
            compressed1.Metadata.Count != compressed2.Metadata.Count,
            "Different parameters should produce different compression results"
        );
    }
        
    private IDependencyTreeNode CreateTestTree()
    {
        var root = new DependencyTreeNode("root");
            
        // Add repeated patterns
        for (int i = 0; i < 5; i++)
        {
            var pattern = new DependencyTreeNode("pattern");
            var child1 = new DependencyTreeNode("child1");
            var child2 = new DependencyTreeNode("child2");
                
            pattern.AddLeftChild(child1);
            pattern.AddRightChild(child2);
                
            root.AddRightChild(pattern);
        }
            
        return root;
    }
}