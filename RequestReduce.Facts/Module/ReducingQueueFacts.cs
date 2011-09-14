using System;
using System.Collections;
using System.Collections.Concurrent;
using Moq;
using RequestReduce.Module;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;
using RequestReduce.Store;

namespace RequestReduce.Facts.Module
{
    public class ReducingQueueFacts : IDisposable
    {
        class FakeReducingQueue : ReducingQueue, IDisposable
        {
            public FakeReducingQueue(IReductionRepository reductionRepository, IStore store) : base(reductionRepository, store)
            {
                isRunning = false;
                ((FakeReductionRepository) reductionRepository).HasLoadedSavedEntries = true;
            }

            public ConcurrentQueue<QueueItem> BaseQueue
            {
                get { return queue; }
            }

            public new void ProcessQueuedItem()
            {
                base.ProcessQueuedItem();
            }

            public FakeReductionRepository ReductionRepository { get { return reductionRepository as FakeReductionRepository;  } }

            public void Dispose()
            {
                backgroundThread.Abort();
            }
        }

        class FakeReductionRepository : IReductionRepository
        {
            private Hashtable dict = new Hashtable();

            public string FindReduction(string urls)
            {
                var key = Hasher.Hash(urls);
                return dict[key] as string;
            }

            public bool HasLoadedSavedEntries { get; set; }

            public void RemoveReduction(Guid key)
            {
                throw new NotImplementedException();
            }

            public void AddReduction(Guid key, string reducedUrl)
            {
                dict[key] = reducedUrl;
            }
        }

        class TestableReducingQueue : Testable<FakeReducingQueue>, IDisposable
        {
            public TestableReducingQueue()
            {
                Inject<IReductionRepository>(new FakeReductionRepository());
                MockedReducer = new Mock<IReducer>();
                RRContainer.Current = new Container(x => x.For<IReducer>().Use(MockedReducer.Object));
                MockedReducer.Setup(x => x.Process(Hasher.Hash("url"), "url")).Returns("reducedUrl");
            }

            public Mock<IReducer> MockedReducer { get; set; }

            public void Dispose()
            {
                RRContainer.Current = null;
                ClassUnderTest.Dispose();
            }
        }

        public class EnqueueCss
        {
            [Fact]
            public void WillPutUrlsInTheQueue()
            {
                var testable = new TestableReducingQueue();
                QueueItem result;

                testable.ClassUnderTest.EnqueueCss("urls");

                testable.ClassUnderTest.BaseQueue.TryDequeue(out result);
                Assert.Equal("urls", result.Urls);
                testable.Dispose();
            }
        }

        public class EnqueueJavaScript
        {
            [Fact]
            public void WillPutUrlsInTheQueue()
            {
                var testable = new TestableReducingQueue();
                QueueItem result;

                testable.ClassUnderTest.EnqueueJavaScript("urls");

                testable.ClassUnderTest.BaseQueue.TryDequeue(out result);
                Assert.Equal("urls", result.Urls);
                testable.Dispose();
            }
        }

        public class ProcessQueuedItem
        {
            [Fact]
            public void WillReduceQueuedCSS()
            {
                var testable = new TestableReducingQueue();
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(ResourceType.Css);
                testable.ClassUnderTest.EnqueueCss("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url"), Times.Once());
                testable.Dispose();
            }

            [Fact]
            public void WillReduceQueuedJavaScript()
            {
                var testable = new TestableReducingQueue();
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(ResourceType.JavaScript);
                testable.ClassUnderTest.EnqueueJavaScript("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url"), Times.Once());
                testable.Dispose();
            }

            [Fact]
            public void WillNotReduceItemIfAlreadyReduced()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.ReductionRepository.AddReduction(Hasher.Hash("url"), "url");
                testable.ClassUnderTest.EnqueueCss("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url"), Times.Never());
                testable.Dispose();
            }

            [Fact]
            public void WillNotReduceItemIfAlreadyReducedOnAntherServer()
            {
                var testable = new TestableReducingQueue();
                testable.Mock<IStore>().Setup(x => x.GetUrlByKey(Hasher.Hash("url"))).Returns("newUrl");
                testable.ClassUnderTest.EnqueueCss("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url"), Times.Never());
                testable.Dispose();
            }

            [Fact]
            public void WillStopProcessingAfterExceedingFailureThreshold()
            {
                var testable = new TestableReducingQueue();
                var badUrl = "badUrl";
                var badKey = Hasher.Hash(badUrl);
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(ResourceType.Css);
                testable.MockedReducer.Setup(x => x.Process(badKey, badUrl)).Throws(new Exception());

                for (int i = 0; i < ReducingQueue.FailureThreshold + 1; i++)
                {
                    testable.ClassUnderTest.EnqueueCss(badUrl);
                    testable.ClassUnderTest.ProcessQueuedItem();
                }

                testable.MockedReducer.Verify(x => x.Process(badKey, badUrl), Times.Exactly(ReducingQueue.FailureThreshold));
                testable.Dispose();
            }

            [Fact]
            public void WillNotReduceQueuedCSSUntilRepositoryHasLoadedSavedItems()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.EnqueueCss("url");
                testable.ClassUnderTest.ReductionRepository.HasLoadedSavedEntries = false;

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url"), Times.Never());
                testable.Dispose();
            }
        }

        public class Count
        {
            [Fact]
            public void WillReturnTheCountOfTheBaseQueue()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.EnqueueCss("url");
                testable.ClassUnderTest.EnqueueCss("url");

                var result = testable.ClassUnderTest.Count;

                Assert.Equal(2, result);
                testable.Dispose();
            }
        }

        public class ClearFailures
        {
            [Fact]
            public void WillProcessFailedUrlAfterFailuresAreCleared()
            {
                RRContainer.Current = null;
                var testable = new TestableReducingQueue();
                var badUrl = "badUrl";
                var badKey = Hasher.Hash(badUrl);
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(ResourceType.Css);
                testable.MockedReducer.Setup(x => x.Process(badKey, badUrl)).Throws(new Exception());
                for (int i = 0; i < ReducingQueue.FailureThreshold; i++)
                {
                    testable.ClassUnderTest.EnqueueCss(badUrl);
                    testable.ClassUnderTest.ProcessQueuedItem();
                }

                testable.ClassUnderTest.ClearFailures();
                testable.ClassUnderTest.EnqueueCss(badUrl);
                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(badKey, badUrl), Times.Exactly(ReducingQueue.FailureThreshold + 1));
                testable.Dispose();
            }
        }

        public class CaptureErrors
        {
            [Fact]
            public void WillCaptureErrorsIfAnErrorActionIsRegistered()
            {
                var testable = new TestableReducingQueue();
                Exception error = null;
                var innerError = new ApplicationException();
                RequestReduceModule.CaptureErrorAction = (x => error= x);
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(ResourceType.Css);
                testable.MockedReducer.Setup(x => x.Process(It.IsAny<Guid>(), "url")).Throws(innerError);
                testable.ClassUnderTest.EnqueueCss("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                Assert.Equal(innerError, error.InnerException);
                Assert.Contains("url", error.Message);
                testable.Dispose();
            }
        }

        public void Dispose()
        {
            RRContainer.Current = null;
        }
    }
}
