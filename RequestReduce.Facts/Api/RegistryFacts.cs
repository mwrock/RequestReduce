using System;
using System.Linq;
using RequestReduce.Api;
using RequestReduce.IOC;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Api
{
    public class RegistryFacts
    {
        class TestFilter : CssFilter
        {
            public override bool IgnoreTarget(CssJsFilterContext context)
            {
                return false;
            }
        }
        [Fact]
        public void WillThrowArgumentNullExceptionIfPassedNull()
        {
            var ex = Record.Exception(() => Registry.RegisterMinifier(null));

            Assert.NotNull(ex);
            Assert.IsType<ArgumentNullException>(ex);
        }

        [Fact]
        public void WillPlugMinifierIntoContainer()
        {
            var minifier = new Minifier();

            Registry.RegisterMinifier(minifier);

            Assert.Equal(minifier, RRContainer.Current.GetInstance<IMinifier>());
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
