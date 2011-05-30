using System;
using System.Collections;
using System.Linq;
using Moq;
using RequestReduce.Module;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Filter
{
    public class ReductionRepositoryFacts
    {
        class FakeReductionRepository : ReductionRepository
        {
            public IDictionary Dictionary { get { return dictionary; } }
        }

        private class TestableReductionRepository : Testable<FakeReductionRepository>
        {
            public TestableReductionRepository()
            {

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
