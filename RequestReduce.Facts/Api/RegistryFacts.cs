using System;
using System.Linq;
using RequestReduce.Api;
using RequestReduce.IOC;
using RequestReduce.ResourceTypes;
using RequestReduce.Utilities;
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

    }
}
