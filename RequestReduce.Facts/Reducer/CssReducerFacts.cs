using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Moq;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Reducer;
using RequestReduce.Store;
using RequestReduce.Utilities;
using StructureMap;
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
                Inject<IRelativeToAbsoluteUtility>(new RelativeToAbsoluteUtility(Mock<HttpContextBase>().Object, Mock<IRRConfiguration>().Object));
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
            public void WillParseClassesWithEmptyComments()
            {
                var testable = new TestableCssReducer();
                var css =
                    @"
* html .RadInput a.riDown
{
	margin-top /**/:0;
}

/*label*/

.RadInput .riLabel
{
	margin:0 4px 0 0;
	white-space:nowrap;
}
";

                var expected =
    @"
* html .RadInput a.riDown
{
	margin-top :0;
}



.RadInput .riLabel
{
	margin:0 4px 0 0;
	white-space:nowrap;
}
";

                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/style/css2.css")).Returns(css);

                testable.ClassUnderTest.Process("http://host/style/css2.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expected), Times.Once());
            }

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

                var result = testable.ClassUnderTest.Process(guid, "http://host/css1.css::http://host/css2.css", string.Empty);

                Assert.Equal(guid, builder.ParseKey(result));
            }

            [Fact]
            public void WillSetSpriteManagerCssKey()
            {
                var testable = new TestableCssReducer();
                var guid = Guid.NewGuid();

                testable.ClassUnderTest.Process(guid, "http://host/css1.css::http://host/css2.css", string.Empty);

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
            public void WillRemoveMediaFromCSSUrl()
            {
                var testable = new TestableCssReducer();

                testable.ClassUnderTest.Process("http://host/css1.css^print,screen::http://host/css2.css");

                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString<CssResource>("http://host/css1.css"), Times.Once());
                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString<CssResource>("http://host/css2.css"), Times.Once());
            }

            [Fact]
            public void WillSaveMinifiedAggregatedCSS()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css1.css")).Returns("css1");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css2.css")).Returns("css2");
                testable.Mock<IMinifier>().Setup(x => x.Minify<CssResource>("css1css2")).Returns("min");

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                testable.Mock<IStore>().Verify(
                    x =>
                    x.Save(Encoding.UTF8.GetBytes("min").MatchEnumerable(), result,
                           "http://host/css1.css::http://host/css2.css"), Times.Once());
            }

            [Fact]
            public void WillSaveMinifiedAggregatedCSSWrappedInMedia()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css1.css")).Returns("css1");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css2.css")).Returns("css2");
                testable.Mock<IMinifier>().Setup(x => x.Minify<CssResource>("@media print,screen {css1}css2")).Returns("min");

                var result = testable.ClassUnderTest.Process("http://host/css1.css^print,screen::http://host/css2.css");

                testable.Mock<IStore>().Verify(
                    x =>
                    x.Save(Encoding.UTF8.GetBytes("min").MatchEnumerable(), result,
                           "http://host/css1.css^print,screen::http://host/css2.css"), Times.Once());
            }

            [Fact]
            public void WillAddSpriteToSpriteManager()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>(It.IsAny<string>())).Returns("css"); 
                var image1 = new BackgroundImageClass("", 0) { ImageUrl = "image1" };
                var image2 = new BackgroundImageClass("", 0) { ImageUrl = "image2" };
                var css = "css";
                testable.Mock<ICssImageTransformer>().Setup(x => x.ExtractImageUrls(css)).Returns(new BackgroundImageClass[] { image1, image2 });

                testable.ClassUnderTest.Process("http://host/css2.css");

                testable.Mock<ISpriteManager>().Verify(x => x.Add(image1), Times.Once());
            }

            [Fact]
            public void WillInjectSpritesToCssAfterDispose()
            {
                var testable = new TestableCssReducer();
                var image1 = new BackgroundImageClass("", 0) {ImageUrl = "image1"};
                var image2 = new BackgroundImageClass("", 0) { ImageUrl = "image2" };
                var css = "css";
                var mockWebResponse = new Mock<WebResponse>();
                mockWebResponse.Setup(x => x.GetResponseStream()).Returns(new MemoryStream(new UTF8Encoding().GetBytes(css)));
                testable.Mock<IWebClientWrapper>().Setup(x => x.Download<CssResource>(It.IsAny<string>())).Returns(mockWebResponse.Object);
                testable.Mock<ICssImageTransformer>().Setup(x => x.ExtractImageUrls(css)).Returns(new[] { image1, image2 });
                var sprite1 = new SpritedImage(1, null, null){Position = -100};
                var sprite2 = new SpritedImage(2, null, null) { Position = -100 };
                var sprites = new List<SpritedImage> { sprite1, sprite2 };
                testable.Mock<ISpriteManager>().Setup(x => x.GetEnumerator()).Returns(sprites.GetEnumerator());
                bool disposeIsCalled = false;
                bool disposeCalled = false;
                testable.Mock<ISpriteManager>().Setup(x => x.Dispose()).Callback(() => disposeIsCalled = true);
                testable.Mock<ICssImageTransformer>().Setup(x => x.InjectSprite(It.IsAny<string>(), It.IsAny<SpritedImage>())).Callback(() => disposeCalled = disposeIsCalled);

                testable.ClassUnderTest.Process("http://host/css2.css");

                testable.Mock<ICssImageTransformer>().Verify(x => x.InjectSprite(It.IsAny<string>(), sprite1), Times.Once());
                testable.Mock<ICssImageTransformer>().Verify(x => x.InjectSprite(It.IsAny<string>(), sprite2), Times.Once());
                Assert.True(disposeCalled);
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
            public void WillFetchImportedCssWhenUrlIsJustQuoted()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css1.css")).Returns(@"@import ""css2.css"";");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css2.css")).Returns("css2");

                testable.ClassUnderTest.Process("http://host/css1.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>("css2"), Times.Once());
            }

            [Fact]
            public void WillIgnoreFilteredImportedCss()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css1.css")).Returns("@import url('css2.css');");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css2.css")).Returns("css2");
                Registry.AddFilter(new CssFilter(x => x.FilteredUrl.Contains("2")));

                testable.ClassUnderTest.Process("http://host/css1.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>("@import url('http://host/css2.css');"), Times.Once());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillWrapImportedCssInMediaIfAMediaIsSpecified()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css1.css")).Returns("@import url('css2.css') print,screen;");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/css2.css")).Returns("css2");

                testable.ClassUnderTest.Process("http://host/css1.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>("@media print,screen {css2}"), Times.Once());
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
                    x.ExtractImageUrls(css), Times.Once());
            }

            [Fact]
            public void WillNotSpriteImagesWhenSpritingIsDisabled()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>(It.IsAny<string>())).Returns("css");
                testable.Mock<IRRConfiguration>().Setup(x => x.ImageSpritingDisabled).Returns(true);
                var image1 = new BackgroundImageClass("", 0) { ImageUrl = "image1" };
                var image2 = new BackgroundImageClass("", 0) { ImageUrl = "image2" };
                var css = "css";
                testable.Mock<ICssImageTransformer>().Setup(x => x.ExtractImageUrls(css)).Returns(new BackgroundImageClass[] { image1, image2 });

                testable.ClassUnderTest.Process("http://host/css2.css");

                testable.Mock<ISpriteManager>().Verify(x => x.Add(image1), Times.Never());
            }

            [Fact]
            public void WillConvertRelativeUrlsToAbsoluteForUnReturnedImages()
            {
                var testable = new TestableCssReducer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""subnav_on_technet.png"") no-repeat;
}";
                var expectedcss =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://host/style/subnav_on_technet.png"") no-repeat;
}";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/style/css2.css")).Returns(css);

                testable.ClassUnderTest.Process("http://host/style/css2.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expectedcss), Times.Once());
            }

            [Fact]
            public void WillInjectContentHostinUnReturnedImagesThatAreLocalToHost()
            {
                var testable = new TestableCssReducer();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://contentHost");
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""subnav_on_technet.png"") no-repeat;
}";
                var expectedcss =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://contentHost/style/subnav_on_technet.png"") no-repeat;
}";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/style/css2.css")).Returns(css);

                testable.ClassUnderTest.Process(Hasher.Hash("mykey"), "http://host/style/css2.css", "http://host/");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expectedcss), Times.Once());
            }

            [Fact]
            public void WillNotInjectContentHostinUnReturnedImagesThatAreNotLocalToHost()
            {
                var testable = new TestableCssReducer();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.ContentHost).Returns("http://contentHost");
                testable.Inject(config.Object);
                RRContainer.Current = new Container(x => x.For<IRRConfiguration>().Use(config.Object));
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""subnav_on_technet.png"") no-repeat;
}";
                var expectedcss =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://hostyosty/style/subnav_on_technet.png"") no-repeat;
}";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://hostyosty/style/css2.css")).Returns(css);

                testable.ClassUnderTest.Process(Hasher.Hash("mykey"), "http://hostyosty/style/css2.css", "http://host/");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expectedcss), Times.Once());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillConvertRelativeUrlsToAbsoluteForUnReturnedImagesWhenBracesInComments()
            {
                var testable = new TestableCssReducer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover { border: 1px solid #aaaaaa/*{borderColorContent}*/; background: #ffffff/*{bgColorContent}*/ url(images/ui-bg_flat_75_ffffff_40x100.png)/*{bgImgUrlContent}*/ 50%/*{bgContentXPos}*/ 50%/*{bgContentYPos}*/ repeat-x/*{bgContentRepeat}*/; color: #222222/*{fcContent}*/; }";
                var expectedcss =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover { border: 1px solid #aaaaaa; background: #ffffff url(http://host/style/images/ui-bg_flat_75_ffffff_40x100.png) 50% 50% repeat-x; color: #222222; }";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://host/style/css2.css")).Returns(css);

                testable.ClassUnderTest.Process("http://host/style/css2.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expectedcss), Times.Once());
            }

            [Fact]
            public void WillConvertRelativeUrlsToAbsoluteForFontFaces()
            {
                var testable = new TestableCssReducer();
                var css =
                    @"
@font-face
{
font-family: myFirstFont;
src: url('Sansation_Light.ttf'),
     url('Sansation_Light.eot');
}";
                var expectedcss =
                    @"
@font-face
{
font-family: myFirstFont;
src: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/Sansation_Light.ttf'),
     url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/Sansation_Light.eot');
}";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css")).Returns(css);

                testable.ClassUnderTest.Process("http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expectedcss), Times.Once());
            }

            [Fact]
            public void WillHandleMultipleFontFacesInMedia()
            {
                var testable = new TestableCssReducer();
                var css =
                    @"
@media screen {
@font-face {
  font-family: 'Open Sans';
  font-style: normal;
  font-weight: 700;
  src: local('Open Sans Bold'), local('OpenSans-Bold'), url('http://themes.googleusercontent.com/static/fonts/opensans/v6/k3k702ZOKiLJc3WVjuplzHhCUOGz7vYGh680lGh-uXM.woff') format('woff');
}
}
@media screen {
@font-face {
  font-family: 'Ubuntu Condensed';
  font-style: normal;
  font-weight: 400;
  src: local('Ubuntu Condensed'), local('UbuntuCondensed-Regular'), url('http://themes.googleusercontent.com/static/fonts/ubuntucondensed/v3/DBCt-NXN57MTAFjitYxdrFzqCfRpIA3W6ypxnPISCPA.woff') format('woff');
}
}";
                var expectedcss =
                    @"
@media screen {
@font-face {
  font-family: 'Open Sans';
  font-style: normal;
  font-weight: 700;
  src: local('Open Sans Bold'), local('OpenSans-Bold'), url('http://themes.googleusercontent.com/static/fonts/opensans/v6/k3k702ZOKiLJc3WVjuplzHhCUOGz7vYGh680lGh-uXM.woff') format('woff');
}
}
@media screen {
@font-face {
  font-family: 'Ubuntu Condensed';
  font-style: normal;
  font-weight: 400;
  src: local('Ubuntu Condensed'), local('UbuntuCondensed-Regular'), url('http://themes.googleusercontent.com/static/fonts/ubuntucondensed/v3/DBCt-NXN57MTAFjitYxdrFzqCfRpIA3W6ypxnPISCPA.woff') format('woff');
}
}";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css")).Returns(css);

                testable.ClassUnderTest.Process("http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expectedcss), Times.Once());
            }

            [Fact]
            public void WillNotAlterDataURIs()
            {
                var testable = new TestableCssReducer();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.ContentHost).Returns("http://host");
                RRContainer.Current = new Container(x => x.For<IRRConfiguration>().Use(config.Object));
                var css =
                    @"
.cls
{
     background: url(data:image/png;base64,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);
}";
                var expectedcss =
                    @"
.cls
{
     background: url(data:image/png;base64,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);
}";
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<CssResource>("http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css")).Returns(css);

                testable.ClassUnderTest.Process("http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                testable.Mock<IMinifier>().Verify(
                    x =>
                    x.Minify<CssResource>(expectedcss), Times.Once());
                RRContainer.Current = null;
            }
        }
    }
}
