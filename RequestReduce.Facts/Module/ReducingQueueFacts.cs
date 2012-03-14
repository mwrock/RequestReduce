using System;
using System.Collections;
using System.Collections.Generic;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;
using RequestReduce.Store;
using RequestReduce.ResourceTypes;
using RequestReduce.Api;

namespace RequestReduce.Facts.Module
{
    public class ReducingQueueFacts : IDisposable
    {
        class FakeReducingQueue : ReducingQueue, IDisposable
        {
            public FakeReducingQueue(IReductionRepository reductionRepository, IStore store) : base(reductionRepository, store)
            {
                IsRunning = false;
                ((FakeReductionRepository) reductionRepository).HasLoadedSavedEntries = true;
            }

            public Queue<IQueueItem> BaseQueue
            {
                get { return Queue; }
            }

            public new void ProcessQueuedItem()
            {
                base.ProcessQueuedItem();
            }

            public new FakeReductionRepository ReductionRepository { get { return base.ReductionRepository as FakeReductionRepository;  } }

            public IQueueItem LastProcessedItem { get; set; }

            public override IQueueItem ItemBeingProcessed
            {
                protected set { 
                    base.ItemBeingProcessed = value;
                    if(value != null) LastProcessedItem = value;
                }
            }

            public QueueItem<CssResource> ForceSetItemBeingProcessed
            {
                set { ItemBeingProcessed = value; }
            }

            public new void Dispose()
            {
                BackgroundThread.Abort();
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

            public string[] ToArray()
            {
                throw new NotImplementedException();
            }
        }

        class TestableReducingQueue : Testable<FakeReducingQueue>, IDisposable
        {
            public TestableReducingQueue()
            {
                Inject<IReductionRepository>(new FakeReductionRepository());
                MockedReducer = new Mock<IReducer>();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.IsFullTrust).Returns(true);
                RRContainer.Current = new Container(x =>
                                                        {
                                                            x.For<IReducer>().Use(MockedReducer.Object);
                                                            x.For<IRRConfiguration>().Use(config.Object);
                                                        });
                MockedReducer.Setup(x => x.Process(Hasher.Hash("url"), "url", string.Empty)).Returns("reducedUrl");
            }

            public Mock<IReducer> MockedReducer { get; set; }

            public void Dispose()
            {
                RRContainer.Current = null;
                ClassUnderTest.Dispose();
            }
        }

        public class Enqueue
        {
            [Fact]
            public void WillPutUrlsInTheQueue()
            {
                var testable = new TestableReducingQueue();
                IQueueItem result;
                var item = new QueueItem<CssResource>();

                testable.ClassUnderTest.Enqueue(item);

                result = testable.ClassUnderTest.BaseQueue.Dequeue();
                Assert.Equal(item, result);
                testable.Dispose();
            }

            [Fact]
            public void WillNotPutUrlsInTheQueueThatAreAlreadyQueued()
            {
                var testable = new TestableReducingQueue();
                var item = new QueueItem<CssResource>(){Urls = "urls"};
                testable.ClassUnderTest.Enqueue(item);
                var item2 = new QueueItem<CssResource>() { Urls = "urls" };
                
                testable.ClassUnderTest.Enqueue(item2);

                Assert.Equal(1, testable.ClassUnderTest.BaseQueue.Count);
                testable.Dispose();
            }

            [Fact]
            public void WillNotPutUrlsInTheQueueThatAreBeingProcessed()
            {
                var testable = new TestableReducingQueue();
                var item = new QueueItem<CssResource>() { Urls = "urls" };
                testable.ClassUnderTest.ForceSetItemBeingProcessed = new QueueItem<CssResource>() { Urls = "urls" };

                testable.ClassUnderTest.Enqueue(item);

                Assert.Equal(0, testable.ClassUnderTest.BaseQueue.Count);
                testable.Dispose();
            }

        }

        public class ProcessQueuedItem
        {
            [Fact]
            public void WillReduceQueuedCSS()
            {
                RRContainer.Current = null;
                var testable = new TestableReducingQueue();
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(typeof(CssResource));
                testable.ClassUnderTest.Enqueue(new QueueItem<CssResource> { Urls = "url", Host = "http://host/"});

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url", "http://host/"), Times.Once());
                testable.Dispose();
            }

            [Fact]
            public void WillReduceQueuedJavaScript()
            {
                RRContainer.Current = null;
                var testable = new TestableReducingQueue();
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(typeof(JavaScriptResource));
                testable.ClassUnderTest.Enqueue(new QueueItem<JavaScriptResource> { Urls = "url", Host = "http://host/" });

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url", "http://host/"), Times.Once());
                testable.Dispose();
            }

            [Fact]
            public void WillNotReduceItemIfAlreadyReduced()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.ReductionRepository.AddReduction(Hasher.Hash("url"), "url");
                testable.ClassUnderTest.Enqueue(new QueueItem<CssResource> { Urls = "url", Host = "http://host/" });

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url", "http://host/"), Times.Never());
                testable.Dispose();
            }

            [Fact]
            public void WillNotReduceItemIfAlreadyReducedOnAntherServer()
            {
                var testable = new TestableReducingQueue();
                testable.Mock<IStore>().Setup(x => x.GetUrlByKey(Hasher.Hash("url"), It.IsAny<Type>())).Returns("newUrl");
                testable.ClassUnderTest.Enqueue(new QueueItem<CssResource> { Urls = "url", Host = "http://host/" });

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url", "http://host/"), Times.Never());
                testable.Dispose();
            }

            [Fact]
            public void WillStopProcessingAfterExceedingFailureThreshold()
            {
                var testable = new TestableReducingQueue();
                var badUrl = new QueueItem<CssResource> { Urls = "url", Host = "http://host/" };
                var badKey = Hasher.Hash(badUrl.Urls);
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(typeof(CssResource));
                testable.MockedReducer.Setup(x => x.Process(badKey, badUrl.Urls, "http://host/")).Throws(new Exception());

                for (int i = 0; i < ReducingQueue.FailureThreshold + 1; i++)
                {
                    testable.ClassUnderTest.Enqueue(badUrl);
                    testable.ClassUnderTest.ProcessQueuedItem();
                }

                testable.MockedReducer.Verify(x => x.Process(badKey, badUrl.Urls, "http://host/"), Times.Exactly(ReducingQueue.FailureThreshold));
                testable.Dispose();
            }

            [Fact]
            public void WillNotReduceQueuedCSSUntilRepositoryHasLoadedSavedItems()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.Enqueue(new QueueItem<CssResource> { Urls = "url", Host = "http://host/" });
                testable.ClassUnderTest.ReductionRepository.HasLoadedSavedEntries = false;

                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(It.IsAny<Guid>(), "url", "http://host/"), Times.Never());
                testable.Dispose();
            }

            [Fact]
            public void WillSetCurrentProcessedItem()
            {
                RRContainer.Current = null;
                var testable = new TestableReducingQueue();
                var item = new QueueItem<CssResource> { Urls = "url" };
                testable.ClassUnderTest.Enqueue(item);

                testable.ClassUnderTest.ProcessQueuedItem();

                Assert.Equal(item, testable.ClassUnderTest.LastProcessedItem);
                testable.Dispose();
            }

            [Fact]
            public void WillReSetCurrentProcessedItemToNullAfterProcessing()
            {
                RRContainer.Current = null;
                var testable = new TestableReducingQueue();
                var item = new QueueItem<CssResource> { Urls = "url" };
                testable.ClassUnderTest.Enqueue(item);

                testable.ClassUnderTest.ProcessQueuedItem();

                Assert.Null(testable.ClassUnderTest.ItemBeingProcessed);
                testable.Dispose();
            }

        }

        public class Count
        {
            [Fact]
            public void WillReturnTheCountOfTheBaseQueue()
            {
                var testable = new TestableReducingQueue();
                testable.ClassUnderTest.Enqueue(new QueueItem<CssResource> { Urls = "url" });
                testable.ClassUnderTest.Enqueue(new QueueItem<CssResource> { Urls = "url2" });

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
                var badUrl = new QueueItem<CssResource> { Urls = "url" };
                var badKey = Hasher.Hash(badUrl.Urls);
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(typeof(CssResource));
                testable.MockedReducer.Setup(x => x.Process(badKey, badUrl.Urls, null)).Throws(new Exception());
                for (int i = 0; i < ReducingQueue.FailureThreshold; i++)
                {
                    testable.ClassUnderTest.Enqueue(badUrl);
                    testable.ClassUnderTest.ProcessQueuedItem();
                }

                testable.ClassUnderTest.ClearFailures();
                testable.ClassUnderTest.Enqueue(badUrl);
                testable.ClassUnderTest.ProcessQueuedItem();

                testable.MockedReducer.Verify(x => x.Process(badKey, badUrl.Urls, null), Times.Exactly(ReducingQueue.FailureThreshold + 1));
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
                Registry.CaptureErrorAction = (x => error= x);
                testable.MockedReducer.Setup(x => x.SupportedResourceType).Returns(typeof(CssResource));
                testable.MockedReducer.Setup(x => x.Process(It.IsAny<Guid>(), "url", null)).Throws(innerError);
                testable.ClassUnderTest.Enqueue(new QueueItem<CssResource> { Urls = "url" });

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
