using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline.TreeStructure;

namespace Tests.AlgorithmImplementationTests;

[TestClass]
public class OriginalTreeRepairStrategyTests
{
    private IDependencyTreeNode CreateTestTree()
    {
        // Create a simple tree for testing
        var root = new DependencyTreeNode("root");
            
        // Add repeated patterns to ensure compression opportunities
        for (int i = 0; i < 5; i++)
        {
            var parent = new DependencyTreeNode("parent");
            var child1 = new DependencyTreeNode("child1");
            var child2 = new DependencyTreeNode("child2");
                
            parent.AddLeftChild(child1);
            parent.AddRightChild(child2);
                
            if (i % 2 == 0)
                root.AddLeftChild(parent);
            else
                root.AddRightChild(parent);
        }
            
        return root;
    }

    [TestMethod]
    public void OriginalTreeRepairStrategy_CompressAndDecompress_ReturnsEquivalentTree()
    {
        // Arrange
        var strategy = new OriginalTreeRepairStrategy(minFrequency: 2);
        var originalTree = CreateTestTree();
        var originalTreeString = originalTree.ToString();
            
        // Act
        var compressed = strategy.Compress(originalTree);
        var decompressed = strategy.Decompress(compressed);
            
        // Assert
        Assert.IsNotNull(compressed, "Compressed tree should not be null");
        Assert.IsNotNull(compressed.Structure, "Compressed tree structure should not be null");
        Assert.IsTrue(compressed.Structure.Length > 0, "Compressed tree structure should not be empty");
        Assert.IsNotNull(compressed.Metadata, "Compressed tree metadata should not be null");
            
        Assert.IsNotNull(decompressed, "Decompressed tree should not be null");
            
        // Convert trees to string for comparison
        var decompressedString = decompressed.ToString();
            
        Assert.AreEqual(originalTreeString, decompressedString, 
            "Decompressed tree should match original tree");
    }
        
    [TestMethod]
    public void OriginalTreeRepairStrategy_CompressNullTree_ThrowsArgumentNullException()
    {
        // Arrange
        var strategy = new OriginalTreeRepairStrategy();
            
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => strategy.Compress(null));
    }
        
    [TestMethod]
    public void OriginalTreeRepairStrategy_DecompressNullTree_ThrowsArgumentNullException()
    {
        // Arrange
        var strategy = new OriginalTreeRepairStrategy();
            
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => strategy.Decompress(null));
    }
}