using Moq;
using NUnit.Framework;
using TreeCompressionPipeline;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.TreeCompressionLibraryTests
{
    [TestFixture]
    public class ProcessSubjectTests
    {
        // Test implementation of IProcessSubject for testing
        private class TestProcessSubject : IProcessSubject
        {
            private readonly List<IProcessObserver> _observers = new List<IProcessObserver>();

            public void AddObserver(IProcessObserver observer)
            {
                _observers.Add(observer);
            }

            public void RemoveObserver(IProcessObserver observer)
            {
                _observers.Remove(observer);
            }

            public void NotifyStart(string process)
            {
                foreach (var observer in _observers)
                {
                    observer.OnStart(process);
                }
            }

            public void NotifyProgress(string process, double percentComplete)
            {
                foreach (var observer in _observers)
                {
                    observer.OnProgress(process, percentComplete);
                }
            }

            public void NotifyComplete(string process, object result)
            {
                foreach (var observer in _observers)
                {
                    observer.OnComplete(process, result);
                }
            }

            public void NotifyError(string process, Exception error)
            {
                foreach (var observer in _observers)
                {
                    observer.OnError(process, error);
                }
            }
        }

        // Simple observer implementation for verification
        private class TestObserver : IProcessObserver
        {
            public int StartCount { get; private set; }
            public int ProgressCount { get; private set; }
            public int CompleteCount { get; private set; }
            public int ErrorCount { get; private set; }
            
            public string LastProcess { get; private set; }
            public double LastProgress { get; private set; }
            public object LastResult { get; private set; }
            public Exception LastError { get; private set; }

            public void OnStart(string process)
            {
                StartCount++;
                LastProcess = process;
            }

            public void OnProgress(string process, double percentComplete)
            {
                ProgressCount++;
                LastProcess = process;
                LastProgress = percentComplete;
            }

            public void OnComplete(string process, object result)
            {
                CompleteCount++;
                LastProcess = process;
                LastResult = result;
            }

            public void OnError(string process, Exception error)
            {
                ErrorCount++;
                LastProcess = process;
                LastError = error;
            }

            public void Reset()
            {
                StartCount = 0;
                ProgressCount = 0;
                CompleteCount = 0;
                ErrorCount = 0;
                LastProcess = null;
                LastProgress = 0;
                LastResult = null;
                LastError = null;
            }
        }

        private TestProcessSubject _subject;
        private TestObserver _observer;

        [SetUp]
        public void Setup()
        {
            _subject = new TestProcessSubject();
            _observer = new TestObserver();
        }

        [Test]
        public void AddObserver_AddsObserverToNotificationList()
        {
            // Arrange
            // Observer is not yet added

            // Act
            _subject.AddObserver(_observer);
            _subject.NotifyStart("TestProcess");

            // Assert
            Assert.AreEqual(1, _observer.StartCount);
            Assert.AreEqual("TestProcess", _observer.LastProcess);
        }

        [Test]
        public void RemoveObserver_RemovesObserverFromNotificationList()
        {
            // Arrange
            _subject.AddObserver(_observer);
            
            // Verify observer is initially receiving notifications
            _subject.NotifyStart("TestProcess");
            Assert.AreEqual(1, _observer.StartCount);
            
            // Act
            _subject.RemoveObserver(_observer);
            _subject.NotifyStart("AnotherProcess");
            
            // Assert
            Assert.AreEqual(1, _observer.StartCount, "Observer should not receive notifications after being removed");
            Assert.AreEqual("TestProcess", _observer.LastProcess, "Last process should not have changed");
        }

        [Test]
        public void NotifyStart_NotifiesAllObservers()
        {
            // Arrange
            var secondObserver = new TestObserver();
            _subject.AddObserver(_observer);
            _subject.AddObserver(secondObserver);
            
            // Act
            _subject.NotifyStart("StartProcess");
            
            // Assert
            Assert.AreEqual(1, _observer.StartCount);
            Assert.AreEqual("StartProcess", _observer.LastProcess);
            Assert.AreEqual(1, secondObserver.StartCount);
            Assert.AreEqual("StartProcess", secondObserver.LastProcess);
        }

        [Test]
        public void NotifyProgress_NotifiesAllObservers()
        {
            // Arrange
            _subject.AddObserver(_observer);
            
            // Act
            _subject.NotifyProgress("ProgressProcess", 42.5);
            
            // Assert
            Assert.AreEqual(1, _observer.ProgressCount);
            Assert.AreEqual("ProgressProcess", _observer.LastProcess);
            Assert.AreEqual(42.5, _observer.LastProgress);
        }

        [Test]
        public void NotifyComplete_NotifiesAllObservers()
        {
            // Arrange
            _subject.AddObserver(_observer);
            var result = new { Value = "TestResult" };
            
            // Act
            _subject.NotifyComplete("CompleteProcess", result);
            
            // Assert
            Assert.AreEqual(1, _observer.CompleteCount);
            Assert.AreEqual("CompleteProcess", _observer.LastProcess);
            Assert.AreEqual(result, _observer.LastResult);
        }

        [Test]
        public void NotifyError_NotifiesAllObservers()
        {
            // Arrange
            _subject.AddObserver(_observer);
            var error = new InvalidOperationException("Test error");
            
            // Act
            _subject.NotifyError("ErrorProcess", error);
            
            // Assert
            Assert.AreEqual(1, _observer.ErrorCount);
            Assert.AreEqual("ErrorProcess", _observer.LastProcess);
            Assert.AreEqual(error, _observer.LastError);
        }

        [Test]
        public void MultipleObservers_AllReceiveNotifications()
        {
            // Arrange
            var observers = new List<TestObserver>();
            const int observerCount = 5;
            
            for (int i = 0; i < observerCount; i++)
            {
                var observer = new TestObserver();
                observers.Add(observer);
                _subject.AddObserver(observer);
            }
            
            // Act
            _subject.NotifyStart("MultiProcess");
            _subject.NotifyProgress("MultiProcess", 33.3);
            _subject.NotifyComplete("MultiProcess", "Done");
            
            // Assert
            foreach (var observer in observers)
            {
                Assert.AreEqual(1, observer.StartCount);
                Assert.AreEqual(1, observer.ProgressCount);
                Assert.AreEqual(1, observer.CompleteCount);
                Assert.AreEqual(0, observer.ErrorCount);
                
                Assert.AreEqual("MultiProcess", observer.LastProcess);
                Assert.AreEqual(33.3, observer.LastProgress);
                Assert.AreEqual("Done", observer.LastResult);
            }
        }
    }

    [TestFixture]
    public class FilterBaseProcessSubjectTests
    {
        // Create a concrete implementation of FilterBase for testing
        private class TestFilter<T, TO> : FilterBase<T, TO>
        {
            public Func<T, TO> ProcessFunction { get; set; }

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

        private TestFilter<string, int> _filter;
        private Mock<IProcessObserver> _observerMock;

        [SetUp]
        public void Setup()
        {
            _filter = new TestFilter<string, int>
            {
                ProcessFunction = s => s.Length
            };
            
            _observerMock = new Mock<IProcessObserver>();
            _filter.AddObserver(_observerMock.Object);
        }

        [Test]
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

        [Test]
        public void Process_WithException_CallsNotifyError()
        {
            // Arrange
            _filter.ProcessFunction = _ => throw new InvalidOperationException("Test exception");
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _filter.Process("test"));
            _observerMock.Verify(o => o.OnStart(It.IsAny<string>()), Times.Once);
            _observerMock.Verify(o => o.OnError(It.IsAny<string>(), It.IsAny<InvalidOperationException>()), Times.Once);
            _observerMock.Verify(o => o.OnComplete(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Process_WithInvalidInput_CallsNotifyError()
        {
            // Arrange - input is not of type T
            object input = 123; // Integer instead of string
            
            // Act & Assert
            Assert.Throws<InvalidCastException>(() => _filter.Process(input));
            _observerMock.Verify(o => o.OnStart(It.IsAny<string>()), Times.Once);
            _observerMock.Verify(o => o.OnError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
            _observerMock.Verify(o => o.OnComplete(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
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

        [Test]
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

        [Test]
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


    [TestFixture]
    public class PipelineWithProcessObserverTests
    {
        private Pipeline _pipeline;
        private Mock<IProcessObserver> _observerMock;
        private Mock<IFilter> _filterMock;

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
}