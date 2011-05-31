using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using RequestReduce.Module;
using RequestReduce.Reducer;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Filter
{
    public class ReductionRepositoryFacts
    {
        class FakeReductionRepository : ReductionRepository
        {
            public FakeReductionRepository(IStore store) : base(store)
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
                var testable = new TestableReductionRepository();
                var key = Hasher.Hash("url1");
                testable.Mock<IStore>().Setup(x => x.GetSavedUrls()).Returns(new Dictionary<Guid, string>()
                                                                                 {{key, "url1"}});

                var result = testable.ClassUnderTest;
                Thread.Sleep(200);

                Assert.True(result.Dictionary[key] as string == "url1");
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
