using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using RequestReduce.Filter;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Filter
{
    public class ReductionRepositoryFacts
    {
        private class TestableReductionRepository : Testable<ReductionRepository>
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
                var dic = new Hashtable();
                dic.Add(md5, "newUrl");
                testable.Inject<IDictionary>(dic);

                var result = testable.ClassUnderTest.FindReduction("urls");

                Assert.Equal("newUrl", result);
            }

            [Fact]
            public void WillRetrieveNullIfNotExists()
            {
                var testable = new TestableReductionRepository();
                var md5 = Hasher.Hash("urlssss");
                var dic = new Hashtable();
                dic.Add(md5, "newUrl");
                testable.Inject<IDictionary>(dic);

                var result = testable.ClassUnderTest.FindReduction("urls");

                Assert.Null(result);
            }

            [Fact]
            public void WillQueueUrlsIfNotInRepo()
            {
                var testable = new TestableReductionRepository();
                var md5 = Hasher.Hash("urlssss");
                var dic = new Hashtable();
                dic.Add(md5, "newUrl");
                testable.Inject<IDictionary>(dic);

                var result = testable.ClassUnderTest.FindReduction("urls");

                testable.Mock<IReducingQueue>().Verify(x => x.Enqueue("urls"), Times.Once());
            }
        }
    }
}
