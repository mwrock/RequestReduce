using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using Xunit;
using RequestReduce.ResourceTypes;

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

        private static IReducer GetCssReducer()
        {
            return RRContainer.Current.GetAllInstances<IReducer>().Single(x => x.SupportedResourceType == typeof(CssResource));
        }

        private static IReducer GetJavaScriptReducer()
        {
            return RRContainer.Current.GetAllInstances<IReducer>().Single(x => x.SupportedResourceType == typeof(JavaScriptResource));
        }


        [OutputTraceOnFailFact]
        public void WillReturnSavedCSS()
        {
            var reducer = GetCssReducer();
            var urls = "http://localhost:8877/Styles/style1.css::http://localhost:8877/Styles/style2.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            var result = reducer.Process(urls);

            Assert.Equal(result,
                         Directory.GetFiles(config.SpritePhysicalPath, key + "*").Single(x => x.EndsWith(".css")).Replace(
                             config.SpritePhysicalPath, config.SpriteVirtualPath).Replace("\\", "/"), StringComparer.OrdinalIgnoreCase);
            reducer.Dispose();
        }

        [OutputTraceOnFailFact]
        public void WillSpriteWithForeignContentHost()
        {
            var reducer = GetCssReducer();
            var urls = "http://localhost:8877/Styles/style3.css";
            config.ContentHost = "http://localhost:8878";
            var tempVirtual = config.SpriteVirtualPath;
            config.SpriteVirtualPath = "/FactContent";
            var key = Hasher.Hash(urls).RemoveDashes();

            reducer.Process(urls);

            var files = Directory.GetFiles(config.SpritePhysicalPath, key + "*");
            Assert.NotNull(files);
            var png = files.SingleOrDefault(x => x.EndsWith(".png"));
            var css = files.SingleOrDefault(x => x.EndsWith(".css"));
            Assert.NotNull(png);
            Assert.True(File.ReadAllText(css).Contains(png.Replace(config.SpritePhysicalPath, config.SpriteVirtualPath).Replace("\\", "/")));
            reducer.Dispose();
            config.ContentHost = string.Empty;
            config.SpriteVirtualPath = tempVirtual;
        }

        [OutputTraceOnFailFact]
        public void WillReturnSavedJavaScript()
        {
            var reducer = GetJavaScriptReducer();
            var urls = "http://localhost:8877/Scripts/script1.js::http://localhost:8877/Scripts/script2.js";
            var key = Hasher.Hash(urls).RemoveDashes();

            var result = reducer.Process(urls);

            Assert.Equal(result,
                         Directory.GetFiles(config.SpritePhysicalPath, key + "*").Single(x => x.EndsWith(".js")).Replace(
                             config.SpritePhysicalPath, config.SpriteVirtualPath).Replace("\\", "/"), StringComparer.OrdinalIgnoreCase);
            reducer.Dispose();
        }

        [OutputTraceOnFailFact]
        public void WillSaveSprite()
        {
            var reducer = GetCssReducer();
            var urls = "http://localhost:8877/Styles/style1.css::http://localhost:8877/Styles/style2.css";
            var key = Hasher.Hash(urls).RemoveDashes();

            reducer.Process(urls);

            Assert.NotNull(Directory.GetFiles(config.SpritePhysicalPath,  key + "*").Single(x => x.EndsWith(".png")));
            reducer.Dispose();
        }

        [OutputTraceOnFailFact]
        public void WillContainImageWithPropperDimensions()
        {
            var reducer = GetCssReducer();
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
