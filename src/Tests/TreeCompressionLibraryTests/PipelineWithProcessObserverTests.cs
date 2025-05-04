using Moq;
using NUnit.Framework;
using TreeCompressionPipeline;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.TreeCompressionLibraryTests;

[TestFixture]
public class PipelineWithProcessObserverTests
{
    private Pipeline? _pipeline;
    private Mock<IProcessObserver>? _observerMock;
    private Mock<IFilter>? _filterMock;

    [SetUp]
    public void Setup()
    {
        _observerMock = new Mock<IProcessObserver>();
        _filterMock = new Mock<IFilter>();
            
        _pipeline = new Pipeline
        {
            ProcessObserver = _observerMock.Object
        };
    }

    [Test]
    public void AddFilter_WithObserver_AddsObserverToFilter()
    {
        // Act
        _pipeline.AddFilter(_filterMock.Object);
            
        // Assert
        _filterMock.Verify(f => f.AddObserver(_observerMock.Object), Times.Once);
    }

    [Test]
    public void AddFilter_NoObserver_DoesNotAddObserverToFilter()
    {
        // Arrange
        _pipeline = new Pipeline(); // No observer set
            
        // Act
        _pipeline.AddFilter(_filterMock.Object);
            
        // Assert
        _filterMock.Verify(f => f.AddObserver(It.IsAny<IProcessObserver>()), Times.Never);
    }

    [Test]
    public void Process_CallsFilterProcessWithCorrectData()
    {
        // Arrange
        var inputData = "test input";
        var expectedOutput = "test output";
            
        _filterMock.Setup(f => f.Process(inputData)).Returns(expectedOutput);
        _pipeline.AddFilter(_filterMock.Object);
            
        // Act
        var result = _pipeline.Process(inputData);
            
        // Assert
        Assert.AreEqual(expectedOutput, result);
        _filterMock.Verify(f => f.Process(inputData), Times.Once);
    }
}