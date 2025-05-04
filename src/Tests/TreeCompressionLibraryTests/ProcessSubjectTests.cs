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

            public void NotifyStart(string? process)
            {
                foreach (var observer in _observers)
                {
                    observer.OnStart(process);
                }
            }

            public void NotifyProgress(string? process, double percentComplete)
            {
                foreach (var observer in _observers)
                {
                    observer.OnProgress(process, percentComplete);
                }
            }

            public void NotifyComplete(string? process, object? result)
            {
                foreach (var observer in _observers)
                {
                    observer.OnComplete(process, result);
                }
            }

            public void NotifyError(string? process, Exception? error)
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
            
            public string? LastProcess { get; private set; }
            public double LastProgress { get; private set; }
            public object? LastResult { get; private set; }
            public Exception? LastError { get; private set; }

            public void OnStart(string? process)
            {
                StartCount++;
                LastProcess = process;
            }

            public void OnProgress(string? process, double percentComplete)
            {
                ProgressCount++;
                LastProcess = process;
                LastProgress = percentComplete;
            }

            public void OnComplete(string? process, object? result)
            {
                CompleteCount++;
                LastProcess = process;
                LastResult = result;
            }

            public void OnError(string? process, Exception? error)
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
            var observers = new List<TestObserver?>();
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
}