using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Moq;
using RequestReduce.Module;
using RequestReduce.Reducer;
using Xunit;

namespace RequestReduce.Facts.Reducer
{
    public class ReducingQueueFacts
    {
        class FakeReducingQueue : ReducingQueue, IDisposable
        {
            public FakeReducingQueue(IReducer reducer, IReductionRepository reductionRepository) : base(reducer, reductionRepository)
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

            public IReductionRepository ReductionRepository { get { return reductionRepository;  } }

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
                return dict[urls] as string;
            }

            public void AddReduction(string originalUrlList, string reducedUrl)
            {
                dict[originalUrlList] = reducedUrl;
            }
        }

        class TestableReducingQueue : Testable<FakeReducingQueue>, IDisposable
        {
            public TestableReducingQueue()
            {
                Inject<IReductionRepository>(new FakeReductionRepository());
            }

            public void Dispose()
            {
                ClassUnderTest.Dispose();
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
            public void WillPlaceReducedCSSInRepo()
            {
                var testable = new TestableReducingQueue();
                testable.Mock<IReducer>().Setup(x => x.Process("url")).Returns("reducedUrl");
                testable.ClassUnderTest.Enqueue("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                Assert.Equal("reducedUrl", testable.ClassUnderTest.ReductionRepository.FindReduction("url"));
            }

            [Fact]
            public void WillNotReduceItemIfAlreadyReduced()
            {
                var testable = new TestableReducingQueue();
                testable.Mock<IReducer>().Setup(x => x.Process("url")).Returns("reducedUrl");
                testable.ClassUnderTest.Enqueue("url");
                testable.ClassUnderTest.Enqueue("url");
                testable.ClassUnderTest.ProcessQueuedItem();

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.Mock<IReducer>().Verify(x => x.Process("url"), Times.Once());
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

        public class CaptureErrors
        {
            [Fact]
            public void WillCaptureErrorsIfAnErrorActionIsRegistered()
            {
                var testable = new TestableReducingQueue();
                Exception error = null;
                testable.ClassUnderTest.CaptureError(x => error= x);
                testable.Mock<IReducer>().Setup(x => x.Process("url")).Throws(new ApplicationException());
                testable.ClassUnderTest.Enqueue("url");

                testable.ClassUnderTest.ProcessQueuedItem();

                Assert.True(error is ApplicationException);
            }
        }
    }
}
