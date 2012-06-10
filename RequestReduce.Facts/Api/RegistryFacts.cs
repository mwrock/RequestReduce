using System.IO;
using System.Linq;
using System.Web;
using Moq;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;

namespace RequestReduce.Facts.Api
{
    public class RegistryFacts
    {
        class TestFilter : CssFilter
        {
            public TestFilter() : base(null)
            {
            }

            public override bool IgnoreTarget(CssJsFilterContext context)
            {
                return false;
            }
        }

        public class CrazyMinifier : IMinifier
        {
            public string Minify<T>(string unMinifiedContent) where T : IResourceType
            {
                return "crazy";
            }
        }

        [Fact]
        public void WillPlugMinifierIntoContainer()
        {
            var minText = new CrazyMinifier().Minify<CssResource>("unminified");
            Registry.RegisterMinifier<CrazyMinifier>();

            var customMinifier = RRContainer.Current.GetInstance<IMinifier>();
            Assert.Equal(minText, customMinifier.Minify<CssResource>("unminified"));
            RRContainer.Current = null;
        }

        [Fact]
        public void WillPlugFilterIntoContainer()
        {
            Registry.AddFilter<TestFilter>();

            Assert.NotNull(RRContainer.Current.GetAllInstances<IFilter>().Where(x => x is CssFilter));
            RRContainer.Current = null;
        }

        [Fact]
        public void WillInstallFilter()
        {
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.ResourceVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(true);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                x.For<HttpContextBase>().Use(context.Object);
            });

            Registry.InstallResponseFilter();

            context.VerifySet((x => x.Response.Filter = It.IsAny<Stream>()), Times.Never());
            RRContainer.Current = null;
        }
    }
}
