using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;

namespace RequestReduce.Facts.Module
{
    public class ReductionRepositoryFacts
    {
        class FakeReductionRepository : ReductionRepository
        {
            public FakeReductionRepository(IRRConfiguration configuration) : base(configuration)
            {
            }

            public IDictionary Dictionary { get { return dictionary; } }

        }

        private class TestableReductionRepository : Testable<FakeReductionRepository>
        {
            public TestableReductionRepository()
            {
            }

        }

        public class ctor
        {
            [Fact]
            public void WillGetPreviouslySavedEntriesFromStore()
            {
                RRContainer.Current = null;
                var testable = new TestableReductionRepository();
                var key = Hasher.Hash("url1");
                var mockStore = new Mock<IStore>();
                mockStore.Setup(x => x.GetSavedUrls()).Returns(new Dictionary<Guid, string>()
                                                                                 {{key, "url1"}});
                RRContainer.Current = new Container(x => { x.For<IStore>().Use(mockStore.Object);});

                var result = testable.ClassUnderTest;
                Thread.Sleep(200);

                Assert.True(result.Dictionary[key] as string == "url1");
                Assert.True(testable.ClassUnderTest.HasLoadedSavedEntries);
                RRContainer.Current = null;
            }

        }

        public class FindReduction
        {
            [Fact]
            public void WillRetrieveReductionIfExists()
            {
                var testable = new TestableReductionRepository();
                var md5 = Hasher.Hash("urls");
                testable.ClassUnderTest.Dictionary.Add(md5, "newUrl");

                var result = testable.ClassUnderTest.FindReduction("urls");

                Assert.Equal("newUrl", result);
            }

            [Fact]
            public void WillRetrieveNullIfNotExists()
            {
                var testable = new TestableReductionRepository();
                var md5 = Hasher.Hash("urlssss");
                testable.ClassUnderTest.Dictionary.Add(md5, "newUrl");

                var result = testable.ClassUnderTest.FindReduction("urls");

                Assert.Null(result);
            }
        }

        public class AddReduction
        {
            [Fact]
            public void WillAddHashedOriginalUrlsAsKey()
            {
                var testable = new TestableReductionRepository();
                var md5 = Hasher.Hash("urlssss");

                testable.ClassUnderTest.AddReduction(md5,"urls");

                Assert.Equal("urls", testable.ClassUnderTest.Dictionary[md5]);
            }

            [Fact]
            public void WillNotAddDuplicateKeys()
            {
                var testable = new TestableReductionRepository();
                var md5 = Hasher.Hash("urlssss");
                testable.ClassUnderTest.Dictionary.Add(md5, "urls");

                testable.ClassUnderTest.AddReduction(md5, "urls");

                Assert.Equal(1, testable.ClassUnderTest.Dictionary.Keys.Cast<Guid>().Where(x => x == md5).Count());
            }
        }
    }
}
