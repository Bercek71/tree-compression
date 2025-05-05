using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TreeCompressionPipeline;
using TreeCompressionPipeline.CompressionStrategies;
using TreeCompressionPipeline.Filters;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace Tests.TreeCompressionLibraryTests;

[TestClass]
public class TreeNodeTests
{
    [TestMethod]
    public void TreeNode_Construction_SetsValueCorrectly()
    {
        // Arrange
        var creationStrategy = new Mock<ITreeCreationStrategy<IDependencyTreeNode>>();
        var compressionStrategy = new Mock<ICompressionStrategy<IDependencyTreeNode>>();
            
        // Act
        var textToTreeFilter = FilterFactory<IDependencyTreeNode>.CreateTextToTreeFilter(creationStrategy.Object);
        var compressionFilter = FilterFactory<IDependencyTreeNode>.CreateCompressionFilter(compressionStrategy.Object);
        var decompressionFilter = FilterFactory<IDependencyTreeNode>.CreateDecompressionFilter(compressionStrategy.Object);
            
        // Assert
        Assert.IsInstanceOfType(textToTreeFilter, typeof(TextToTreeFilter<IDependencyTreeNode>));
        Assert.IsInstanceOfType(compressionFilter, typeof(CompressionFilter<IDependencyTreeNode>));
        Assert.IsInstanceOfType(decompressionFilter, typeof(DecompressionFilter<IDependencyTreeNode>));
    }
}