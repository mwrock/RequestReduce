using System;
using RequestReduce.Api;
using RequestReduce.SassLessCoffee;
using Xunit;
using Xunit.Extensions;

namespace RequestReduce.Facts.SassLessCoffee
{
    public class BootstraperFacts
    {
        [Theory]
        [InlineData(".less", typeof(LessHandler))]
        [InlineData(".sass", typeof(SassHandler))]
        [InlineData(".scss", typeof(SassHandler))]
        [InlineData(".coffee", typeof(CoffeeHandler))]
        [InlineData(".tre", null)]
        public void WillMapExtensionsToCorrectHandlers(string extension, Type expectedHandler)
        {
            Bootstrapper.Start();

            Assert.Equal(expectedHandler, Registry.HandlerFactory.ResolveHandler(new Uri("http://host/path/file" + extension), false) == null ? null : Registry.HandlerFactory.ResolveHandler(new Uri("http://host/path/file" + extension), false).GetType());
        }
    }
}
