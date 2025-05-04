using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.Tests;

[TestClass]
public class DependencyTreeNodeTests
{
    [TestMethod]
    public void DependencyTreeNode_SerializeAndDeserialize_MaintainsStructure()
    {
        // Arrange
        var originalTree = new DependencyTreeNode("root");
        var leftChild = new DependencyTreeNode("left");
        var rightChild = new DependencyTreeNode("right");
            
        originalTree.AddLeftChild(leftChild);
        originalTree.AddRightChild(rightChild);
            
        // Act
        var serialized = DependencyTreeNode.SerializeToBytes(originalTree);
        var deserialized = DependencyTreeNode.DeserializeFromBytes(serialized);
            
        // Assert
        Assert.IsNotNull(serialized, "Serialized data should not be null");
        Assert.IsTrue(serialized.Length > 0, "Serialized data should not be empty");
            
        Assert.IsNotNull(deserialized, "Deserialized tree should not be null");
        Assert.AreEqual("root", deserialized.Value, "Root node value should be preserved");
        Assert.AreEqual(1, deserialized.LeftChildren.Count, "Left children count should be preserved");
        Assert.AreEqual(1, deserialized.RightChildren.Count, "Right children count should be preserved");
        Assert.AreEqual("left", deserialized.LeftChildren[0].Value, "Left child value should be preserved");
        Assert.AreEqual("right", deserialized.RightChildren[0].Value, "Right child value should be preserved");
    }
        
    [TestMethod]
    public void DependencyTreeNode_ToString_CorrectlyFormatsTree()
    {
        // Arrange
        var root = new DependencyTreeNode("root");
        var leftChild = new DependencyTreeNode("left");
        var rightChild = new DependencyTreeNode("right");
            
        root.AddLeftChild(leftChild);
        root.AddRightChild(rightChild);
            
        // Act
        var result = root.ToString();
            
        // Assert
        Assert.IsNotNull(result, "ToString result should not be null");
        Assert.IsTrue(result.Contains("root"), "ToString should contain root node value");
        Assert.IsTrue(result.Contains("left"), "ToString should contain left child value");
        Assert.IsTrue(result.Contains("right"), "ToString should contain right child value");
    }
        
    [TestMethod]
    public void DependencyTreeNode_GetNodeCount_ReturnsCorrectCount()
    {
        // Arrange
        var root = new DependencyTreeNode("root");
        var leftChild = new DependencyTreeNode("left");
        var rightChild = new DependencyTreeNode("right");
        var leftGrandchild = new DependencyTreeNode("leftgrand");
            
        root.AddLeftChild(leftChild);
        root.AddRightChild(rightChild);
        leftChild.AddLeftChild(leftGrandchild);
            
        // Act
        var count = root.GetNodeCount();
            
        // Assert
        Assert.AreEqual(4, count, "Node count should include all nodes in the tree");
    }
}