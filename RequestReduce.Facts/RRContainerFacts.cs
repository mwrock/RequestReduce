using System;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;
using RequestReduce.SqlServer;
using RequestReduce.Store;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;

namespace RequestReduce.Facts
{
    public class RRContainerFacts : IDisposable
    {
        public class BadResource : IResourceType
        {
            public string FileName
            {
                get { return "badfilename"; }
            }

            public System.Collections.Generic.IEnumerable<string> SupportedMimeTypes
            {
                get { throw new System.NotImplementedException(); }
            }

            public string TransformedMarkupTag(string url)
            {
                throw new System.NotImplementedException();
            }

            public System.Text.RegularExpressions.Regex ResourceRegex
            {
                get { return null; }
            }


            public System.Func<string, string, bool> TagValidator { get; set; }
        }

        [Fact]
        public void ContainerIsValid()
        {
            RRContainer.Current.AssertConfigurationIsValid();
        }

        [Fact]
        public void WillThowIfResourceTypeMissingRequestReduceInFileName()
        {
            RRContainer.Current.Configure(x => x.Scan( y => {
                y.Assembly("RequestReduce.Facts");
                y.AddAllTypesOf<IResourceType>();
            }));

            var ex = Record.Exception(() => RRContainer.Current.AssertConfigurationIsValid());

            Assert.NotNull(ex);
        }

        [Fact]
        public void StoreIsLocalDiskWhenConfigurationIsLocalDisk()
        {
            var container = new Container();
            var config = Mock.Of<IRRConfiguration>(c => c.ContentStore == Configuration.Store.LocalDiskStore);
            container.Configure(x =>
            {
                x.For<IRRConfiguration>().Use(config);
                x.For<IFileWrapper>().Use(Mock.Of<IFileWrapper>());
                x.For<IUriBuilder>().Use(Mock.Of<IUriBuilder>());
                x.For<IReductionRepository>().Use(Mock.Of<IReductionRepository>());
            });
            RRContainer.LoadAppropriateStoreRegistry(container);

            var result = container.GetInstance<IStore>();
            
            Assert.IsType<LocalDiskStore>(result);
        }

        [Fact]
        public void StoreIsSqlServerWhenConfigurationIsSqlServerAndAssemblyExists()
        {
            var container = new Container();
            var config = new Mock<IRRConfiguration>();
            config.Setup(c => c.ContentStore).Returns(Configuration.Store.SqlServerStore);
            config.Setup(x => x.ConnectionStringName).Returns("RRConnection");
            container.Configure(x => x.For<IRRConfiguration>().Use(config.Object));
            container.Configure(x =>
            {
                x.For<IUriBuilder>().Use(Mock.Of<IUriBuilder>());
                x.For<IFileRepository>().Use(Mock.Of<IFileRepository>());
                x.For<IReductionRepository>().Use(Mock.Of<IReductionRepository>());
                x.For<IFileWrapper>().Use(Mock.Of<IFileWrapper>());
            });
            RRContainer.LoadAppropriateStoreRegistry(container);

            var result = container.GetInstance<IStore>();

            Assert.IsType<SqlServerStore>(result);
        }

        public void Dispose()
        {
            RRContainer.Current.Dispose();
            RRContainer.Current = null;
        }
    }
}
