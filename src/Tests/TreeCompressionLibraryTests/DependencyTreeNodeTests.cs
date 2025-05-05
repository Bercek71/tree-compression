using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionPipeline.TreeStructure;

namespace Tests.TreeCompressionLibraryTests;

[TestClass]
public class DependencyTreeNodeTests
{
    [TestMethod]
    public void DependencyTreeNode_AddChildren_AddsCorrectly()
    {
        // Arrange
        var root = new DependencyTreeNode("root");
        var leftChild = new DependencyTreeNode("left");
        var rightChild = new DependencyTreeNode("right");
            
        // Act
        root.AddLeftChild(leftChild);
        root.AddRightChild(rightChild);
            
        // Assert
        Assert.AreEqual(1, root.LeftChildren.Count);
        Assert.AreEqual(1, root.RightChildren.Count);
        Assert.AreEqual(leftChild, root.LeftChildren[0]);
        Assert.AreEqual(rightChild, root.RightChildren[0]);
        Assert.AreEqual(root, leftChild.Parent);
        Assert.AreEqual(root, rightChild.Parent);
    }
        
    [TestMethod]
    public void DependencyTreeNode_ToString_FormatsTreeCorrectly()
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
        Assert.IsTrue(result.Contains("left root right"), "Tree formatting incorrect: " + result);
    }
        
    [TestMethod]
    public void DependencyTreeNode_ToString_HandlesMultiLevelTree()
    {
        // Arrange
        var root = new DependencyTreeNode("root");
        var child1 = new DependencyTreeNode("child1");
        var child2 = new DependencyTreeNode("child2");
        var grandchild = new DependencyTreeNode("grandchild");
            
        root.AddLeftChild(child1);
        root.AddRightChild(child2);
        child1.AddLeftChild(grandchild);
            
        // Act
        var result = root.ToString();
            
        // Assert
        Assert.IsTrue(result.Contains("grandchild"), "Grandchild not in result: " + result);
        // The exact format will depend on your implementation, so adjust as needed
    }
        
    [TestMethod]
    public void DependencyTreeNode_SerializeDeserialize_ComplexTree_MaintainsStructure()
    {
        // Arrange
        var root = new DependencyTreeNode("root");
        var level1Left = new DependencyTreeNode("1L");
        var level1Right = new DependencyTreeNode("1R");
        var level2LeftLeft = new DependencyTreeNode("2LL");
        var level2LeftRight = new DependencyTreeNode("2LR");
        var level2RightLeft = new DependencyTreeNode("2RL");
        var level2RightRight = new DependencyTreeNode("2RR");
            
        root.AddLeftChild(level1Left);
        root.AddRightChild(level1Right);
        level1Left.AddLeftChild(level2LeftLeft);
        level1Left.AddRightChild(level2LeftRight);
        level1Right.AddLeftChild(level2RightLeft);
        level1Right.AddRightChild(level2RightRight);
            
        // Act
        var serialized = DependencyTreeNode.SerializeToBytes(root);
        var deserialized = DependencyTreeNode.DeserializeFromBytes(serialized);
            
        // Assert
        Assert.AreEqual("root", deserialized.Value);
        Assert.AreEqual(1, deserialized.LeftChildren.Count);
        Assert.AreEqual(1, deserialized.RightChildren.Count);
        Assert.AreEqual("1L", deserialized.LeftChildren[0].Value);
        Assert.AreEqual("1R", deserialized.RightChildren[0].Value);
        Assert.AreEqual(2, deserialized.LeftChildren[0].LeftChildren.Count + 
                           deserialized.LeftChildren[0].RightChildren.Count);
        Assert.AreEqual(2, deserialized.RightChildren[0].LeftChildren.Count + 
                           deserialized.RightChildren[0].RightChildren.Count);
    }
        
    [TestMethod]
    public void DependencyTreeNode_GetNodeCount_CountsAllNodes()
    {
        // Arrange
        var root = new DependencyTreeNode("root");
        var leftChild = new DependencyTreeNode("left");
        var rightChild = new DependencyTreeNode("right");
        var leftGrandchild = new DependencyTreeNode("leftgrand");
        var rightGrandchild = new DependencyTreeNode("rightgrand");
            
        root.AddLeftChild(leftChild);
        root.AddRightChild(rightChild);
        leftChild.AddLeftChild(leftGrandchild);
        rightChild.AddRightChild(rightGrandchild);
            
        // Act
        var count = root.GetNodeCount();
            
        // Assert
        Assert.AreEqual(5, count);
    }
        
    [TestMethod]
    public void DependencyTreeNode_SerializeDeserialize_EmptyTree_WorksCorrectly()
    {
        // Arrange
        var emptyRoot = new DependencyTreeNode("<empty>");
            
        // Act
        var serialized = DependencyTreeNode.SerializeToBytes(emptyRoot);
        var deserialized = DependencyTreeNode.DeserializeFromBytes(serialized);
            
        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("<empty>", deserialized.Value);
        Assert.AreEqual(0, deserialized.LeftChildren.Count);
        Assert.AreEqual(0, deserialized.RightChildren.Count);
    }
        
    [TestMethod]
    public void DependencyTreeNode_SerializeDeserialize_LargeTree_HandlesCorrectly()
    {
        // Arrange
        var root = new DependencyTreeNode("root");
            
        // Create a tree with many nodes (breadth-focused)
        for (int i = 0; i < 100; i++)
        {
            var child = new DependencyTreeNode($"child{i}");
            if (i % 2 == 0)
                root.AddLeftChild(child);
            else
                root.AddRightChild(child);
        }
            
        // Act
        var originalCount = root.GetNodeCount();
        var serialized = DependencyTreeNode.SerializeToBytes(root);
        var deserialized = DependencyTreeNode.DeserializeFromBytes(serialized);
        var deserializedCount = deserialized.GetNodeCount();
            
        // Assert
        Assert.AreEqual(originalCount, deserializedCount);
        Assert.AreEqual(101, deserializedCount); // root + 100 children
    }
}