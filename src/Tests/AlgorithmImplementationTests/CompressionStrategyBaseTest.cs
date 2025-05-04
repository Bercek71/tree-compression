using Microsoft.VisualStudio.TestTools.UnitTesting;
using TreeCompressionPipeline;
using TreeCompressionPipeline.TreeStructure;
using TreeCompressionPipeline.CompressionStrategies;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace TreeCompressionAlgorithms.Tests
{
    [TestClass]
    public class CompressionStrategyBaseTests
    {
        // Test implementation of a compression strategy
        private class TestCompressionStrategy : ICompressionStrategy<IDependencyTreeNode>
        {
            public bool CompressWasCalled { get; private set; }
            public bool DecompressWasCalled { get; private set; }
            public IDependencyTreeNode InputTree { get; private set; }
            public CompressedTree InputCompressedTree { get; private set; }

            public CompressedTree Compress(IDependencyTreeNode tree)
            {
                CompressWasCalled = true;
                InputTree = tree;
                
                // Create a simple compressed representation
                var compressedTree = new CompressedTree
                {
                    Structure = Encoding.UTF8.GetBytes("compressed data"),
                    Metadata = new Dictionary<string, string> { { "test", "metadata" } }
                };
                
                return compressedTree;
            }

            public IDependencyTreeNode Decompress(CompressedTree compressedTree)
            {
                DecompressWasCalled = true;
                InputCompressedTree = compressedTree;
                
                // Return a simple tree
                return new DependencyTreeNode("decompressed");
            }
        }

        [TestMethod]
        public void ICompressionStrategy_ImplementationTest()
        {
            // Arrange
            var strategy = new TestCompressionStrategy();
            var tree = new DependencyTreeNode("root");
            var compressedTree = new CompressedTree();
            
            // Act
            var result1 = strategy.Compress(tree);
            var result2 = strategy.Decompress(compressedTree);
            
            // Assert
            Assert.IsTrue(strategy.CompressWasCalled, "Compress method was not called");
            Assert.IsTrue(strategy.DecompressWasCalled, "Decompress method was not called");
            Assert.AreEqual(tree, strategy.InputTree, "Input tree not passed correctly");
            Assert.AreEqual(compressedTree, strategy.InputCompressedTree, "Input compressed tree not passed correctly");
        }
    }
}