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

            Assert.Equal(expectedHandler, Registry.HandlerMaps[0](new Uri("http://host/path/file" + extension)) == null ? null : Registry.HandlerMaps[0](new Uri("http://host/path/file" + extension)).GetType());
            Registry.HandlerMaps.Clear();
        }
    }
}
