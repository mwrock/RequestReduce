using Moq;
using RequestReduce.Configuration;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class DbDiskCacheFacts
    {
        class FakeDbDiskCache : DbDiskCache
        {
            public FakeDbDiskCache(IFileWrapper fileWrapper, IRRConfiguration configuration, IUriBuilder uriBuilder)
                : base(fileWrapper, configuration, uriBuilder)
            {
                timer.Dispose();
            }
        }

        class TestableDbDiskCache : Testable<FakeDbDiskCache>
        {
            public TestableDbDiskCache()
            {
                
            }
        }

        public class Ctor
        {
             [Fact]
             public void WillPurgePhysicalDirectory()
             {
                 var testable = new TestableDbDiskCache();
                 testable.Mock<IRRConfiguration>().Setup(x => x.SpritePhysicalPath).Returns("path");

                 var result = testable.ClassUnderTest;

                 testable.Mock<IFileWrapper>().Verify(x => x.DeleteDirectory("path"), Times.Once());
             }
        }
    }
}
