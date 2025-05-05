using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline;
using TreeCompressionPipeline.Filters;
using TreeCompressionPipeline.TreeCreationStrategies;
using TreeCompressionPipeline.TreeStructure;

namespace Tests.IntegrationTests;

[TestClass]
public class FullPipelineIntegrationTests
{
    // Test implementation of tree creation strategy that creates a simple deterministic tree
    private class TestTreeCreationStrategy : ITreeCreationStrategy<IDependencyTreeNode>
    {
        public IDependencyTreeNode CreateTree(string text)
        {
            var root = new DependencyTreeNode("root");

            var words = text.Split(' ');
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    root.AddRightChild(new DependencyTreeNode(word));
                }
            }

            return root;
        }
    }

    [TestMethod]
    public void FullPipeline_TextToTreeToCompressionToDecompression_Works()
    {
        // Arrange
        var originalText = "this is a test sentence for the full pipeline integration test";
        var treeCreationStrategy = new TestTreeCreationStrategy();
        var compressionStrategy = new TreeRepairEncodingStrategy();

        var pipeline = new Pipeline();

        // Add filters for text->tree->compressed
        var textToTreeFilter = new TextToTreeFilter<IDependencyTreeNode>(treeCreationStrategy);
        var compressionFilter = new CompressionFilter<IDependencyTreeNode>(compressionStrategy);

        pipeline.AddFilter(textToTreeFilter)
            .AddFilter(compressionFilter);

        // Create decompression pipeline
        var decompressionPipeline = new Pipeline();
        var decompressionFilter = new DecompressionFilter<IDependencyTreeNode>(compressionStrategy);
        decompressionPipeline.AddFilter(decompressionFilter);

        // Act - Run compression
        var compressedTree = pipeline.Process(originalText) as CompressedTree;

        // Run decompression
        var resultTree = decompressionPipeline.Process(compressedTree!) as IDependencyTreeNode;

        // Convert back to text (this depends on your actual implementation)
        var resultText = resultTree!.ToString();

        // Assert
        Assert.IsNotNull(compressedTree);
        Assert.IsNotNull(resultTree);

        // Check that the original words are all present in the result
        var originalWords = originalText.Split(' ');
        foreach (var word in originalWords)
        {
            if (!string.IsNullOrWhiteSpace(word))
            {
                Assert.IsTrue(resultText!.Contains(word),
                    $"Result should contain '{word}' but doesn't: {resultText}");
            }
        }
    }
            
    [TestMethod]
    public void FullPipeline_EmptyInput_HandledGracefully()
    {
        // Arrange
        var treeCreationStrategy = new TestTreeCreationStrategy();
        var compressionStrategy = new TreeRepairEncodingStrategy();

        var pipeline = new Pipeline();
        pipeline.AddFilter(new TextToTreeFilter<IDependencyTreeNode>(treeCreationStrategy))
            .AddFilter(new CompressionFilter<IDependencyTreeNode>(compressionStrategy));

        // Act
        var result = pipeline.Process(string.Empty) as CompressedTree;

        // Assert
        Assert.IsNotNull(result, "Should handle empty input");

        // Should be able to decompress
        var decompressionFilter = new DecompressionFilter<IDependencyTreeNode>(compressionStrategy);
        var decompressed = decompressionFilter.Process(result) as IDependencyTreeNode;

        Assert.IsNotNull(decompressed, "Should handle empty tree decompression");
    }

    [TestMethod]
    public void FullPipeline_SingleNode_CompressDecompress_Works()
    {
        // Arrange
        var strategy = new TreeRepairEncodingStrategy();
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

}