using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using Xunit;

namespace RequestReduce.Facts.Integration
{
    public class ReducerFacts : IDisposable
    {
        private readonly IRRConfiguration config;

        public ReducerFacts()
        {
            config = RRContainer.Current.GetInstance<IRRConfiguration>();
            var rrFolder = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\RequestReduce.SampleWeb\\FactContent";
            config.SpritePhysicalPath = rrFolder;
            Directory.CreateDirectory(rrFolder);
        }

        [Fact]
        public void WillReturnSavedCSS()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();

            var result = reducer.Process("http://localhost:8888/Styles/style1.css::http://localhost:8888/Styles/style2.css");

            Assert.Equal(result,
                         Directory.GetFiles(config.SpritePhysicalPath).Single(x => x.EndsWith(".css")).Replace(
                             config.SpritePhysicalPath, config.SpriteVirtualPath).Replace("\\", "/"));
        }

        [Fact]
        public void WillSaveSprite()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();

            var result = reducer.Process("http://localhost:8888/Styles/style1.css::http://localhost:8888/Styles/style2.css");

            Assert.NotNull(Directory.GetFiles(config.SpritePhysicalPath).Single(x => x.EndsWith(".png")));
        }

        public void Dispose()
        {
            Directory.Delete(config.SpritePhysicalPath, true);
        }
    }
}
