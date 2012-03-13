using System;
using System.Linq;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.SqlServer;
using RequestReduce.SqlServer.ORM;
using RequestReduce.Utilities;
using Xunit;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Facts.Store
{
    public class RepositoryFacts
    {

        class FakeFileRepository : FileRepository
        {
            public FakeFileRepository(IRRConfiguration config)
                : base(config)
            {
                RequestReduceDB.DefaultProviderName = "System.Data.SQLite";
                var db = GetDatabase();
                db.KeepConnectionAlive = true;
                db.Execute(SqliteHelper.GetSqlLightSafeSql());
            }

            private static string GetSqlLightSafeSql(string sql)
            {
                var result = sql.Replace("[dbo].", string.Empty);
                result = result.Replace("(max)", "(1000)");
                result = result.Replace("CLUSTERED", string.Empty);
                result = result.Replace("GO", string.Empty);
                return result;
            }
        }

        class TestableRepository : Testable<FakeFileRepository>, IDisposable
        {
            public TestableRepository()
                : this("RRConnection")
            {
            }

            public TestableRepository(string connectionString)
            {
                Mock<IRRConfiguration>().Setup(x => x.ConnectionStringName).Returns(connectionString);
            }

            public void Dispose()
            {
                RRContainer.Current.Dispose();
                RRContainer.Current = null;
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
                                   LastUpdated = DateTime.Now,
                                   OriginalName = "originalName",
                                   RequestReduceFileId = id
                               };

                testable.ClassUnderTest.Save(file);

                var savedFile = testable.ClassUnderTest.SingleOrDefault<RequestReduceFile>(id);
                Assert.Equal(file.Content.Length, savedFile.Content.Length);
                Assert.Equal(file.Content[0], savedFile.Content[0]);
                Assert.Equal(file.FileName, savedFile.FileName);
                Assert.Equal(file.Key, savedFile.Key);
                Assert.Equal(file.OriginalName, savedFile.OriginalName);
                Assert.Equal(file.RequestReduceFileId, savedFile.RequestReduceFileId);
                Assert.True((file.LastUpdated - savedFile.LastUpdated) <= TimeSpan.FromMilliseconds(4));
            }

            [Fact]
            public void WillSaveToDatabaseUsingConnectionString()
            {
                var testable = new TestableRepository("Data Source=:memory:;Version=3;New=True");
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                testable.ClassUnderTest.Save(file);

                var savedFile = testable.ClassUnderTest.SingleOrDefault<RequestReduceFile>(id);
                Assert.Equal(file.Content.Length, savedFile.Content.Length);
                Assert.Equal(file.Content[0], savedFile.Content[0]);
                Assert.Equal(file.FileName, savedFile.FileName);
                Assert.Equal(file.Key, savedFile.Key);
                Assert.Equal(file.OriginalName, savedFile.OriginalName);
                Assert.Equal(file.RequestReduceFileId, savedFile.RequestReduceFileId);
                Assert.True((file.LastUpdated - savedFile.LastUpdated) <= TimeSpan.FromMilliseconds(4));
            }

            [Fact]
            public void WillUpdateContentAndLastUpdatedTime()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastUpdated = new DateTime(2010, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };
                testable.ClassUnderTest.Save(file);
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 2 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                testable.ClassUnderTest.Save(file2);

                var savedFile = testable.ClassUnderTest.SingleOrDefault<RequestReduceFile>(id);
                Assert.Equal(2, savedFile.Content[0]);
                Assert.True(savedFile.LastUpdated > new DateTime(2011, 1, 1));
            }

            [Fact]
            public void WillThrowIfFilenameIsTooLong()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-1",
                    Key = Guid.NewGuid(),
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                var ex = Record.Exception(() => testable.ClassUnderTest.Save(file));

                Assert.NotNull(ex);
            }

            [Fact]
            public void WillThrowIfFilenameIsNull()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = null,
                    Key = Guid.NewGuid(),
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                var ex = Record.Exception(() => testable.ClassUnderTest.Save(file));

                Assert.NotNull(ex);
            }

            [Fact]
            public void WillThrowIfContentIsNull()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = null,
                    FileName = "filename",
                    Key = Guid.NewGuid(),
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };

                var ex = Record.Exception(() => testable.ClassUnderTest.Save(file));

                Assert.NotNull(ex);
            }

        }

        public class Update
        {
            [Fact]
            public void WillUpdateContentAndLastUpdatedTime()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName",
                    Key = Guid.NewGuid(),
                    LastUpdated = new DateTime(2010, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = id
                };
                testable.ClassUnderTest.Save(file);
                file.Content = new byte[] {2};

                testable.ClassUnderTest.Update(file);

                var savedFile = testable.ClassUnderTest.SingleOrDefault<RequestReduceFile>(id);
                Assert.Equal(2, savedFile.Content[0]);
                Assert.True(savedFile.LastUpdated > new DateTime(2011, 1, 1));
            }
        }

        public class GetActiveFiles
        {
            [Fact]
            public void WillReturnListOfCssFiles()
            {
                var testable = new TestableRepository();
                var builder = new RequestReduce.Utilities.UriBuilder(testable.Mock<IRRConfiguration>().Object);
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 1 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 1 })
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildSpriteUrl(id, new byte[] { 2 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    RequestReduceFileId = Hasher.Hash(new byte[] { 2 })
                };
                var file3 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id2, new byte[] { 3 }),
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 3 })
                };

                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);
                testable.ClassUnderTest.Save(file3);

                var result = testable.ClassUnderTest.GetActiveFiles();

                Assert.Equal(2, result.Count());
                Assert.True(result.Contains(file.FileName));
                Assert.True(result.Contains(file3.FileName));
            }

            [Fact]
            public void WillNotReturnExpiredKeys()
            {
                var testable = new TestableRepository();
                var builder = new RequestReduce.Utilities.UriBuilder(testable.Mock<IRRConfiguration>().Object);
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 1 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 1 }),
                    IsExpired = true
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id2, new byte[] { 2 }),
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 2 })
                };
                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);

                var result = testable.ClassUnderTest.GetActiveFiles();

                Assert.Equal(1, result.Count());
                Assert.Equal(file2.FileName, result.First());
                Assert.True(result.Contains(file2.FileName));
            }

            [Fact]
            public void WillReturnMostRecentActiveEntryPerKey()
            {
                var testable = new TestableRepository();
                var builder = new RequestReduce.Utilities.UriBuilder(testable.Mock<IRRConfiguration>().Object);
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 1 }),
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 1 }),
                    IsExpired = true
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 2 }),
                    Key = id,
                    LastUpdated = DateTime.Now.Subtract(new TimeSpan(0, 0, 2)),
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 2 })
                };
                var file3 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id, new byte[] { 3 }),
                    Key = id,
                    LastUpdated = DateTime.Now.Subtract(new TimeSpan(0, 0, 3)),
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 3 })
                };
                var file4 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = builder.BuildResourceUrl<CssResource>(id2, new byte[] { 4 }),
                    Key = id2,
                    LastUpdated = DateTime.Now.Subtract(new TimeSpan(0, 0, 3)),
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 4 })
                };
                var file5 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "file.png",
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Hasher.Hash(new byte[] { 5 })
                };
                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);
                testable.ClassUnderTest.Save(file3);
                testable.ClassUnderTest.Save(file4);
                testable.ClassUnderTest.Save(file5);

                var result = testable.ClassUnderTest.GetActiveFiles();

                Assert.Equal(2, result.Count());
                Assert.True(result.Contains(file2.FileName));
                Assert.True(result.Contains(file4.FileName));
            }

        }

        public class GetFilesFromKey
        {
            [Fact]
            public void WillPullTheFilesWithTheSpecifiedKey()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var id2 = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = new CssResource().FileName,
                    Key = id,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName",
                    RequestReduceFileId = Guid.NewGuid()
                };
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = new CssResource().FileName,
                    Key = id,
                    LastUpdated = DateTime.Now,
                    RequestReduceFileId = Guid.NewGuid()
                };
                var file3 = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = new CssResource().FileName,
                    Key = id2,
                    LastUpdated = DateTime.Now,
                    OriginalName = "originalName2",
                    RequestReduceFileId = Guid.NewGuid()
                };
                testable.ClassUnderTest.Save(file);
                testable.ClassUnderTest.Save(file2);
                testable.ClassUnderTest.Save(file3);

                var result = testable.ClassUnderTest.GetFilesFromKey(id);

                Assert.Equal(2, result.Count());
                Assert.True(result.All(x => x.Key == id));

                Assert.NotNull(result.Single(f => f.RequestReduceFileId == file.RequestReduceFileId));
                Assert.NotNull(result.Single(f => f.RequestReduceFileId == file2.RequestReduceFileId));
            }
        }

        public class GetUrlByKey
        {
            [Fact]
            public void WillGetCssUrlFromDb()
            {
                var testable = new TestableRepository();
                var id = Guid.NewGuid();
                var file = new RequestReduceFile()
                {
                    Content = new byte[] { 1 },
                    FileName = "fileName1" + new CssResource().FileName,
                    Key = id,
                    LastUpdated = new DateTime(2010, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = Guid.NewGuid()
                };
                testable.ClassUnderTest.Save(file);
                var file2 = new RequestReduceFile()
                {
                    Content = new byte[] { 2 },
                    FileName = "fileName2" + new CssResource().FileName,
                    Key = Guid.NewGuid(),
                    LastUpdated = new DateTime(2011, 1, 1),
                    OriginalName = "originalName",
                    RequestReduceFileId = Guid.NewGuid()
                };
                testable.ClassUnderTest.Save(file2);

                var result = testable.ClassUnderTest.GetActiveUrlByKey(id, typeof(CssResource));

                Assert.Equal(file.FileName, result);
                testable.Dispose();
            }
        }

        [Fact]
        public void WillGetNonExpiredCssUrlFromDb()
        {
            var testable = new TestableRepository();
            var id = Guid.NewGuid();
            var file = new RequestReduceFile()
            {
                Content = new byte[] { 1 },
                FileName = "fileName1" + new CssResource().FileName,
                Key = id,
                LastUpdated = new DateTime(2010, 1, 1),
                OriginalName = "originalName",
                RequestReduceFileId = id,
                IsExpired = true
            };
            testable.ClassUnderTest.Save(file);
            var file2 = new RequestReduceFile()
            {
                Content = new byte[] { 2 },
                FileName = "fileName2" + new CssResource().FileName,
                Key = id,
                LastUpdated = new DateTime(2011, 1, 1),
                OriginalName = "originalName",
                RequestReduceFileId = Guid.NewGuid()
            };
            testable.ClassUnderTest.Save(file2);

            var result = testable.ClassUnderTest.GetActiveUrlByKey(id, typeof(CssResource));

            Assert.Equal(file2.FileName, result);
            testable.Dispose();
        }


    }
}
