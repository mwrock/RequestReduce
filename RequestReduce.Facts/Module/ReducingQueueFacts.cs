using System;
using System.Collections;
using System.Collections.Concurrent;
using Moq;
using RequestReduce.Module;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;

namespace RequestReduce.Facts.Module
{
    public class ReducingQueueFacts : IDisposable
    {
        class FakeReducingQueue : ReducingQueue, IDisposable
        {
            public FakeReducingQueue(IReductionRepository reductionRepository) : base(reductionRepository)
            {
                isRunning = false;
            }

            public ConcurrentQueue<string> BaseQueue
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
                ClassUnderTest.Dispose();
                RRContainer.Current = null;
            }
        }

        public class Enqueue
        {
            [Fact]
            public void WillPutUrlsInTheQueue()
            {
                var testable = new TestableReducingQueue();
                string result;

                testable.ClassUnderTest.Enqueue("urls");

                testable.ClassUnderTest.BaseQueue.TryDequeue(out result);
                Assert.Equal("urls", result);
            }
        }

        public class ProcessQueuedItem
        {
            [Fact]
            public void WillReduceQueuedCSS()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.Enqueue("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url"), Times.Once());
            }

            [Fact]
            public void WillNotReduceItemIfAlreadyReduced()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.ReductionRepository.AddReduction(Hasher.Hash("url"), "url");
                testable.ClassUnderTest.Enqueue("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url"), Times.Never());
            }

            [Fact]
            public void WillStopProcessingAfterExceedingFailureThreshold()
            {
                var testable = new TestableReducingQueue();
                var badUrl = "badUrl";
                var badKey = Hasher.Hash(badUrl);
                testable.MockedReducer.Setup(x => x.Process(badKey, badUrl)).Throws(new Exception());

                for (int i = 0; i < ReducingQueue.FailureThreshold + 1; i++)
                {
                    testable.ClassUnderTest.Enqueue(badUrl);
                    testable.ClassUnderTest.ProcessQueuedItem();
                }

                testable.MockedReducer.Verify(x => x.Process(badKey, badUrl), Times.Exactly(ReducingQueue.FailureThreshold));
            }
        }

        public class Count
        {
            [Fact]
            public void WillReturnTheCountOfTheBaseQueue()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.Enqueue("url");
                testable.ClassUnderTest.Enqueue("url");

                var result = testable.ClassUnderTest.Count;

                Assert.Equal(2, result);
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
                testable.MockedReducer.Setup(x => x.Process(badKey, badUrl)).Throws(new Exception());
                for (int i = 0; i < ReducingQueue.FailureThreshold; i++)
                {
                    testable.ClassUnderTest.Enqueue(badUrl);
                    testable.ClassUnderTest.ProcessQueuedItem();
                }

                testable.ClassUnderTest.ClearFailures();
                testable.ClassUnderTest.Enqueue(badUrl);
                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(badKey, badUrl), Times.Exactly(ReducingQueue.FailureThreshold + 1));
            }
        }

        public class CaptureErrors
        {
            [Fact]
            public void WillCaptureErrorsIfAnErrorActionIsRegistered()
            {
                var testable = new TestableReducingQueue();
                Exception error = null;
                testable.ClassUnderTest.CaptureError(x => error= x);
                testable.MockedReducer.Setup(x => x.Process(It.IsAny<Guid>(), "url")).Throws(new ApplicationException());
                testable.ClassUnderTest.Enqueue("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                Assert.True(error is ApplicationException);
            }
        }

        public void Dispose()
        {
            RRContainer.Current = null;
        }
    }
}
