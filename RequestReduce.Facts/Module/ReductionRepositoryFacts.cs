using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Module
{
    public class ReductionRepositoryFacts
    {
        class FakeLocalDiskStore : LocalDiskStore
        {
            public FakeLocalDiskStore() : base()
            {
            }

            public override event DeleeCsAction CssDeleted;
            public override event AddCssAction CssAded;

            protected override void SetupWatcher()
            {
                return;
            }

            public void TriggerChange(string change, Guid key)
            {
                if (change == "delete")
                    CssDeleted(key);
                if (change == "add")
                    CssAded(key, "url");
            }
        }

        class FakeReductionRepository : ReductionRepository
        {
            public FakeReductionRepository(IStore store) : base(store)
            {
            }

            public IDictionary Dictionary { get { return dictionary; } }

            public FakeLocalDiskStore LocalDiskStore { get { return store as FakeLocalDiskStore; } }
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

            [Fact]
            public void WillRegisterDeleteCssAction()
            {
                var testable = new TestableReductionRepository();
                testable.Inject<IStore>(new FakeLocalDiskStore());
                var key = Hasher.Hash("url1");
                testable.ClassUnderTest.Dictionary.Add(key, "val");

                testable.ClassUnderTest.LocalDiskStore.TriggerChange("delete", key);

                Assert.Null(testable.ClassUnderTest.Dictionary[key]);
            }

            [Fact]
            public void WillRegisterAddCssAction()
            {
                var testable = new TestableReductionRepository();
                testable.Inject<IStore>(new FakeLocalDiskStore());
                var key = Hasher.Hash("url1");

                testable.ClassUnderTest.LocalDiskStore.TriggerChange("add", key);

                Assert.NotNull(testable.ClassUnderTest.Dictionary[key]);
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
