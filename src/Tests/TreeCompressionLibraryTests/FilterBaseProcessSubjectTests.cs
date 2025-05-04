using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TreeCompressionPipeline;

namespace Tests.TreeCompressionLibraryTests;

[TestClass]
public class FilterBaseProcessSubjectTests
{
    private class TestFilter<T, TO> : FilterBase<T, TO>
    {
        public Func<T, TO> ProcessFunction { get; set; } = new Func<T, TO>( data => default!);

        protected override TO ProcessData(T data)
        {
            return ProcessFunction(data);
        }
            
        // Expose protected methods for testing
        public new void NotifyStart(string process) => base.NotifyStart(process);
        public new void NotifyProgress(string process, double percentComplete) => base.NotifyProgress(process, percentComplete);
        public new void NotifyComplete(string process, object result) => base.NotifyComplete(process, result);
        public new void NotifyError(string process, Exception error) => base.NotifyError(process, error);
    }

    private TestFilter<string, int> _filter = new TestFilter<string, int>();
    private Mock<IProcessObserver> _observerMock = new Mock<IProcessObserver>();

    [TestInitialize]
    public void Setup()
    {
        _filter = new TestFilter<string, int>
        {
            ProcessFunction = s => s.Length
        };
            
        _observerMock = new Mock<IProcessObserver>();
        _filter.AddObserver(_observerMock.Object);
    }

    [TestMethod]
    public void Process_CallsNotifyStartAndComplete()
    {
        // Arrange
        const string input = "test";
        const int expectedOutput = 4;
            
        // Act
        var result = _filter.Process(input);
            
        // Assert
        Assert.AreEqual(expectedOutput, result);
        _observerMock.Verify(o => o.OnStart(It.IsAny<string>()), Times.Once);
        _observerMock.Verify(o => o.OnComplete(It.IsAny<string>(), It.Is<object>(r => (int)r == expectedOutput)), Times.Once);
        _observerMock.Verify(o => o.OnError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
    }

    [TestMethod]
    public void Process_WithException_CallsNotifyError()
    {
        // Arrange
        _filter.ProcessFunction = _ => throw new InvalidOperationException("Test exception");
            
        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => _filter.Process("test"));
        _observerMock.Verify(o => o.OnStart(It.IsAny<string>()), Times.Once);
        _observerMock.Verify(o => o.OnError(It.IsAny<string>(), It.IsAny<InvalidOperationException>()), Times.Once);
        _observerMock.Verify(o => o.OnComplete(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public void Process_WithInvalidInput_CallsNotifyError()
    {
        // Arrange - input is not of type T
        object input = 123; // Integer instead of string
            
        // Act & Assert
        Assert.ThrowsException<InvalidCastException>(() => _filter.Process(input));
        _observerMock.Verify(o => o.OnStart(It.IsAny<string>()), Times.Once);
        _observerMock.Verify(o => o.OnError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
            
    }

    [TestMethod]
    public void Chain_ReturnsNextFilter()
    {
        // Arrange
        var nextFilter = new TestFilter<int, string>
        {
            ProcessFunction = i => i.ToString()
        };
            
        // Act
        var returnedFilter = _filter.Chain(nextFilter);
            
        // Assert
        Assert.AreEqual(nextFilter, returnedFilter);
    }

    [TestMethod]
    public void AddObserver_AddsObserver()
    {
        // Arrange
        var filter = new TestFilter<string, int>
        {
            ProcessFunction = s => s.Length
        };
        var observer = new Mock<IProcessObserver>();
            
        // Act
        filter.AddObserver(observer.Object);
        filter.NotifyStart("Test");
            
        // Assert
        observer.Verify(o => o.OnStart("Test"), Times.Once);
    }

    [TestMethod]
    public void RemoveObserver_RemovesObserver()
    {
        // Arrange
        var filter = new TestFilter<string, int>
        {
            ProcessFunction = s => s.Length
        };
        var observer = new Mock<IProcessObserver>();
        filter.AddObserver(observer.Object);
            
        // Verify added
        filter.NotifyStart("Test1");
        observer.Verify(o => o.OnStart("Test1"), Times.Once);
            
        // Act
        filter.RemoveObserver(observer.Object);
        filter.NotifyStart("Test2");
            
        // Assert
        observer.Verify(o => o.OnStart("Test2"), Times.Never);
    }
}