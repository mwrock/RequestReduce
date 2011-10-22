using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using RequestReduce.ResourceTypes;
using System.Net;
using System.IO;

namespace RequestReduce.Facts.Reducer
{
    public class CssReducerFacts
    {
        private class TestableCssReducer : Testable<CssReducer>
        {
            public TestableCssReducer()
            {
                Mock<IMinifier>().Setup(x => x.Minify<CssResource>(It.IsAny<string>())).Returns("minified");
                Mock<ISpriteManager>().Setup(x => x.GetEnumerator()).Returns(new List<SpritedImage>().GetEnumerator());
                Inject<IUriBuilder>(new UriBuilder(Mock<IRRConfiguration>().Object));
                Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>(It.IsAny<string>())).Returns(string.Empty);
            }

        }

        public class SupportedResourceType
        {
            [Fact]
            public void WillSupportCss()
            {
                var testable = new TestableCssReducer();

                Assert.Equal(typeof(CssResource), testable.ClassUnderTest.SupportedResourceType);
            }
        }

        public class Process
        {
            [Fact]
            public void WillReturnProcessedCssUrlInCorrectConfigDirectory()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                Assert.True(result.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillReturnProcessedCssUrlWithKeyInPath()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");
                var guid = Guid.NewGuid();
                var builder = new UriBuilder(testable.Mock<IRRConfiguration>().Object);

                var result = testable.ClassUnderTest.Process(guid, "http://host/css1.css::http://host/css2.css");

                Assert.Equal(guid, builder.ParseKey(result));
            }

            [Fact]
            public void WillSetSpriteManagerCssKey()
            {
                var testable = new TestableCssReducer();
                var guid = Guid.NewGuid();

                testable.ClassUnderTest.Process(guid, "http://host/css1.css::http://host/css2.css");

                testable.Mock<ISpriteManager>().VerifySet(x => x.SpritedCssKey = guid);
            }

            [Fact]
            public void WillUseHashOfUrlsIfNoKeyIsGiven()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");
                var guid = Hasher.Hash("http://host/css1.css::http://host/css2.css");
                var builder = new UriBuilder(testable.Mock<IRRConfiguration>().Object);

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                Assert.Equal(guid, builder.ParseKey(result));
            }

            [Fact]
            public void WillReturnProcessedCssUrlWithARequestReducedFileName()
            {
                var testable = new TestableCssReducer();

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                Assert.True(result.EndsWith("-" + new CssResource().FileName));
            }

            [Fact]
            public void WillDownloadContentOfEachOriginalCSS()
            {
                var testable = new TestableCssReducer();

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString<CssResource>("http://host/css1.css"), Times.Once());
                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString<CssResource>("http://host/css2.css"), Times.Once());
            }

            [Fact]
            public void WillSaveMinifiedAggregatedCSS()
            {
                var testable = new TestableCssReducer();
                var mockWebResponse = new Mock<WebResponse>();
                mockWebResponse.Setup(x => x.GetResponseStream()).Returns(new MemoryStream(new UTF8Encoding().GetBytes("css1")));
                var mockWebResponse2 = new Mock<WebResponse>();
                mockWebResponse2.Setup(x => x.GetResponseStream()).Returns(new MemoryStream(new UTF8Encoding().GetBytes("css2")));
                testable.Mock<IWebClientWrapper>().Setup(x => x.Download<CssResource>("http://host/css1.js")).Returns(mockWebResponse.Object);
                testable.Mock<IWebClientWrapper>().Setup(x => x.Download<CssResource>("http://host/css1.js")).Returns(mockWebResponse2.Object);
                testable.Mock<IMinifier>().Setup(x => x.Minify<CssResource>("css1css2")).Returns("min");

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                testable.Mock<IStore>().Verify(
                    x =>
                    x.Save(Encoding.UTF8.GetBytes("min").MatchEnumerable(), result,
                           "http://host/css1.css::http://host/css2.css"), Times.Once());
            }

            [Fact]
            public void WillAddSpriteToSpriteManager()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>(It.IsAny<string>())).Returns("css"); 
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "image1" };
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "image2" };
                var css = "css";
                testable.Mock<ICssImageTransformer>().Setup(x => x.ExtractImageUrls(ref css, It.IsAny<string>())).Returns(new BackgroundImageClass[] { image1, image2 });

                testable.ClassUnderTest.Process("http://host/css2.css");

                testable.Mock<ISpriteManager>().Verify(x => x.Add(image1), Times.Once());
            }

            [Fact]
            public void WillInjectSpritesToCssAfterFlush()
            {
                var testable = new TestableCssReducer();
                var image1 = new BackgroundImageClass("", "http://server/content/style.css") {ImageUrl = "image1"};
                var image2 = new BackgroundImageClass("", "http://server/content/style.css") { ImageUrl = "image2" };
                var css = "css";
                var mockWebResponse = new Mock<WebResponse>();
                mockWebResponse.Setup(x => x.GetResponseStream()).Returns(new MemoryStream(new UTF8Encoding().GetBytes(css)));
                testable.Mock<IWebClientWrapper>().Setup(x => x.Download<CssResource>(It.IsAny<string>())).Returns(mockWebResponse.Object);
                testable.Mock<ICssImageTransformer>().Setup(x => x.ExtractImageUrls(ref css, It.IsAny<string>())).Returns(new[] { image1, image2 });
                var sprite1 = new SpritedImage(1, null, null){Position = -100};
                var sprite2 = new SpritedImage(2, null, null) { Position = -100 };
                var sprites = new List<SpritedImage> { sprite1, sprite2 };
                testable.Mock<ISpriteManager>().Setup(x => x.GetEnumerator()).Returns(sprites.GetEnumerator());
                bool flushIsCalled = false;
                bool flushCalled = false;
                testable.Mock<ISpriteManager>().Setup(x => x.Flush()).Callback(() => flushIsCalled = true);
                testable.Mock<ICssImageTransformer>().Setup(x => x.InjectSprite(It.IsAny<string>(), It.IsAny<SpritedImage>())).Callback(() => flushCalled = flushIsCalled);

                testable.ClassUnderTest.Process("http://host/css2.css");

                testable.Mock<ICssImageTransformer>().Verify(x => x.InjectSprite(It.IsAny<string>(), sprite1), Times.Once());
                testable.Mock<ICssImageTransformer>().Verify(x => x.InjectSprite(It.IsAny<string>(), sprite2), Times.Once());
                Assert.True(flushCalled);
            }

            [Fact]
            public void WillFetchImportedCss()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css1.css")).Returns("@import url('css2.css');");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css2.css")).Returns("css2");
                
                testable.ClassUnderTest.Process("http://host/css1.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>("css2"), Times.Once());
            }

            [Fact]
            public void WillResolveImagePathsOfImportedCss()
            {
                var testable = new TestableCssReducer();
                var css = "css2";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/style1/css1.css")).Returns("@import url('../style2/css2.css');");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/style2/css2.css")).Returns(css);
                var anyStr = It.IsAny<string>();

                testable.ClassUnderTest.Process("http://host/style1/css1.css");

                testable.Mock<ICssImageTransformer>().Verify(
                    x =>
                    x.ExtractImageUrls(ref css, "http://host/style2/css2.css"), Times.Once());
            }

        }
    }
}
