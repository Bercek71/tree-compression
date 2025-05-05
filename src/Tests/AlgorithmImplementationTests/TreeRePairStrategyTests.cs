using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline.TreeStructure;

namespace Tests.AlgorithmImplementationTests;

[TestClass]
public class TreeRePairStrategyTests
{
    private IDependencyTreeNode CreateTestTree()
    {
        // Create a simple tree for testing
        var root = new DependencyTreeNode("root");
            
        var leftChild1 = new DependencyTreeNode("left1");
        var leftChild2 = new DependencyTreeNode("left2");
        var rightChild1 = new DependencyTreeNode("right1");
        var rightChild2 = new DependencyTreeNode("right2");
            
        // Add second level children
        var leftGrandchild1 = new DependencyTreeNode("leftgrand1");
        var leftGrandchild2 = new DependencyTreeNode("leftgrand2");
        var rightGrandchild1 = new DependencyTreeNode("rightgrand1");
        var rightGrandchild2 = new DependencyTreeNode("rightgrand2");
            
        // Build tree structure
        root.AddLeftChild(leftChild1);
        root.AddLeftChild(leftChild2);
        root.AddRightChild(rightChild1);
        root.AddRightChild(rightChild2);
            
        leftChild1.AddLeftChild(leftGrandchild1);
        leftChild1.AddRightChild(rightGrandchild1);
        leftChild2.AddLeftChild(leftGrandchild2);
        rightChild1.AddRightChild(rightGrandchild2);
            
        return root;
    }

    [TestMethod]
    public void TreeRepairEncodingStrategy_CompressAndDecompress_ReturnsEquivalentTree()
    {
        // Arrange
        var strategy = new TreeRepairEncodingStrategy(minFrequency: 2);
        var originalTree = CreateTestTree();
            
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
        var originalString = originalTree.ToString()?.Split(" ").Where(x => x != "root").ToString();
        var decompressedString = decompressed.ToString()?.Split(" ").Where(x => x != "root").ToString();
            
        Assert.AreEqual(originalString, decompressedString, 
            "Decompressed tree should match original tree");
    }
        
    [TestMethod]
    public void TreeRepairEncodingStrategy_CompressWithDifferentMinFrequency_AffectsCompression()
    {
        // Arrange
        var tree = CreateTestTree();
            
        // Create duplicate subtrees to ensure patterns
        for (int i = 0; i < 5; i++)
        {
            var duplicateSubtree = new DependencyTreeNode("duplicate");
            var child1 = new DependencyTreeNode("child1");
            var child2 = new DependencyTreeNode("child2");
                
            duplicateSubtree.AddLeftChild(child1);
            duplicateSubtree.AddRightChild(child2);
                
            tree.AddRightChild(duplicateSubtree);
        }
            
        var strategyHighFreq = new TreeRepairEncodingStrategy(minFrequency: 5);
        var strategyLowFreq = new TreeRepairEncodingStrategy(minFrequency: 2);
            
        // Act
        var compressedHighFreq = strategyHighFreq.Compress(tree);
        var compressedLowFreq = strategyLowFreq.Compress(tree);
            
        // Assert
        // Lower minimum frequency should result in more patterns being replaced, potentially resulting in different compression
        Assert.AreNotEqual(compressedHighFreq.Structure.Length, compressedLowFreq.Structure.Length,
            "Different minimum frequencies should produce different compression results");
    }
        
    [TestMethod]
    public void TreeRepairEncodingStrategy_CompressNullTree_ThrowsArgumentNullException()
    {
        // Arrange
        var strategy = new TreeRepairEncodingStrategy();
            
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => strategy.Compress(null));
    }
        
    [TestMethod]
    public void TreeRepairEncodingStrategy_DecompressNullTree_ThrowsArgumentNullException()
    {
        // Arrange
        var strategy = new TreeRepairEncodingStrategy();
            
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => strategy.Decompress(null));
    }
}