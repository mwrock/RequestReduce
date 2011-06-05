using System;
using System.Data.Entity;
using RequestReduce.Configuration;
using RequestReduce.Store;
using Xunit;

namespace RequestReduce.Facts.Store
{
    public class RepositoryFacts
    {
        class TestableRepository : Testable<Repository<RequestReduceFile>>
        {
            public TestableRepository()
            {
                Database.SetInitializer<RequestReduceContext>(new DropCreateDatabaseAlways<RequestReduceContext>());
                Mock<IRRConfiguration>().Setup(x => x.ConnectionStringName).Returns("RequestReduce");
            }
        }

        public class Save
        {
            [Fact]
            public void WillSaveToDatabase()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                               {
                                   Content = new byte[] {1},
                                   FileName = "fileName",
                                   Key = Guid.NewGuid(),
                                   LastAccessed = DateTime.Now,
                                   LastUpdated = DateTime.Now,
                                   OriginalName = "originalName",
                                   RequestReduceFileId = id
                               };

                testable.ClassUnderTest.Save(file);

                var savedFile = testable.ClassUnderTest[id];
                Assert.Equal(file.Content.Length, savedFile.Content.Length);
                Assert.Equal(file.Content[0], savedFile.Content[0]);
                Assert.Equal(file.FileName, savedFile.FileName);
                Assert.Equal(file.Key, savedFile.Key);
                Assert.Equal(file.OriginalName, savedFile.OriginalName);
                Assert.Equal(file.RequestReduceFileId, savedFile.RequestReduceFileId);
                Assert.Equal(file.LastAccessed, savedFile.LastAccessed);
                Assert.Equal(file.LastUpdated, savedFile.LastUpdated);
            }
        }
    }
}
