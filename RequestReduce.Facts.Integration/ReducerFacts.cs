using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
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
        }

        [OutputTraceOnFailFact]
        public void WillReturnSavedCSS()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();
            var urls = "http://localhost:8877/Styles/style1.css::http://localhost:8877/Styles/style2.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            var result = reducer.Process(urls);

            Assert.Equal(result,
                         Directory.GetFiles(config.SpritePhysicalPath, key + "*").Single(x => x.EndsWith(".css")).Replace(
                             config.SpritePhysicalPath, config.SpriteVirtualPath).Replace("\\", "/"));
            reducer.Dispose();
        }

        [OutputTraceOnFailFact]
        public void WillSaveSprite()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();
            var urls = "http://localhost:8877/Styles/style1.css::http://localhost:8877/Styles/style2.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            var result = reducer.Process(urls);

            Assert.NotNull(Directory.GetFiles(config.SpritePhysicalPath,  key + "*").Single(x => x.EndsWith(".png")));
            reducer.Dispose();
        }

        [OutputTraceOnFailFact]
        public void WillContainImageWithPropperDimensions()
        {
            var reducer = RRContainer.Current.GetInstance<IReducer>();
            var urls = "http://localhost:8877/Styles/style3.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            reducer.Process(urls);

            reducer.Dispose();
            using(var image = new Bitmap(Directory.GetFiles(config.SpritePhysicalPath, key + "*").Single(x => x.EndsWith(".png"))))
            {
                Assert.Equal(1040, image.Width);
                Assert.Equal(33, image.Height);
                image.Dispose();
            }
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(config.SpritePhysicalPath, true);
            }
            catch(IOException)
            {
                Thread.Sleep(100);
                Directory.Delete(config.SpritePhysicalPath, true);
            }
            RRContainer.Current = null;
        }
    }
}
