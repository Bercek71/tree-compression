using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionAlgorithms.Tests;

[TestClass]
public class CompressedTreeTests
{
        
    [TestMethod]
    public void CompressedTree_ToString_ContainsMetadataAndStructureInfo()
    {
        // Arrange
        var compressedTree = new CompressedTree
        {
            Structure = new byte[100],
            Metadata = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            }
        };
            
        // Act
        var result = compressedTree.ToString();
            
        // Assert
        Assert.IsNotNull(result, "ToString result should not be null");
        Assert.IsTrue(result.Contains("key1"), "ToString should contain metadata keys");
        Assert.IsTrue(result.Contains("value1"), "ToString should contain metadata values");
        Assert.IsTrue(result.Contains("100 bytes"), "ToString should contain structure size");
    }
}