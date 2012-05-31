using System;
using System.Collections.Specialized;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Handlers;
using RequestReduce.IOC;
using RequestReduce.Store;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;
using Xunit.Extensions;

namespace RequestReduce.Facts.Handlers
{
    class ReducedContentHandlerTests
    {
        class TestableReducedContentHandler : Testable<ReducedContentHandler>
        {
            public TestableReducedContentHandler()
            {
                Inject(new HandlerFactory());
            }
        }

        [Theory]
        [InlineData("/RRContent")]
        [InlineData("http://localhost/RRContent")]
        public void WillSetCachabilityIfInRRPathAndStoreSendsContent(string path)
        {
            var testable = new TestableReducedContentHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/css.css");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.css"));
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns(path);
            testable.Mock<IStore>().Setup(
                x => x.SendContent(It.IsAny<string>(), context.Object.Response)).
                Returns(true);
            testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/RRContent/css.css")).Returns("sig");

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal(60 * 24 * 360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag(@"""sig"""), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
        }

        [Fact]
        public void WillHonorEtagsandReturn304WhenTheyMatchIfNoneMatch()
        {
            var testable = new TestableReducedContentHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection { { "If-None-Match", @"""match""" } });
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/key-match-css.css"));
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/RRContent/key-match-css.css")).Returns("match");

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal(60 * 24 * 360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag(@"""match"""), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
            Assert.Equal(304, context.Object.Response.StatusCode);
            testable.Mock<IStore>().Verify(x => x.SendContent(It.IsAny<string>(), context.Object.Response), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillStripQueryStringDromGeneratedUrls()
        {
            var testable = new TestableReducedContentHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css?somequerystring");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/key-match-css.css?somequerystring"));
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection { { "If-None-Match", @"""match""" } });
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            var config = new Mock<IRRConfiguration>();
            testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var store = new Mock<IStore>();
            var builder = new Mock<IUriBuilder>();
            testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/RRContent/key-match-css.css")).Returns("match");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.SendContent("/RRContent/key-match-css.css", context.Object.Response), Times.Never());
        }

        [Fact]
        public void WillSetPhysicalPathToMappedVirtualPathWhenHandlingContent()
        {
            var testable = new TestableReducedContentHandler();
            var context = new Mock<HttpContextBase>();
            testable.Mock<IHostingEnvironmentWrapper>().Setup(x => x.MapPath("/Virtual")).Returns("physical");
            testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Request.RawUrl).Returns("/Virtual/blah");
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IRRConfiguration>().VerifySet(x => x.SpritePhysicalPath = "physical", Times.Once());
        }

        [Fact]
        public void WillNotReturn304WhenSigDoesNotMatchIfNoneMatch()
        {
            var testable = new TestableReducedContentHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection { { "If-None-Match", @"""notmatch""" } });
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/key-match-css.css"));
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            testable.Mock<IStore>().Setup(
                x => x.SendContent(It.IsAny<string>(), context.Object.Response)).
                Returns(true);
            testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature("/RRContent/key-match-css.css")).Returns("match");

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal(60 * 24 * 360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag(@"""match"""), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
        }

        [Theory]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-sprite1.png", "image/png", true)]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-RequestReducedStyle.css", "text/css", true)]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-RequestReducedScript.js", "application/x-javascript", true)]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-RequestReducedStyle.css", null, false)]
        public void WillCorrectlySetContentType(string path, string contentType, bool contentInStore)
        {
            var testable = new TestableReducedContentHandler();
            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.ContentType);
            response.Setup(x => x.Headers).Returns(new NameValueCollection());
            response.Setup(x => x.Cache).Returns(new Mock<HttpCachePolicyBase>().Object);
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Response).Returns(response.Object);
            context.Setup(x => x.Request.RawUrl).Returns(path);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            testable.Mock<IStore>().Setup(
                x => x.SendContent(It.IsAny<string>(), response.Object)).
                Returns(contentInStore);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal(contentType, response.Object.ContentType);
        }

        [Theory]
        [InlineData("/RRContent", false)]
        [InlineData("http://localhost/RRContent", false)]
        public void WillNotSetCachabilityIfStoreDoesNotSendContent(string path, bool contentSent)
        {
            var testable = new TestableReducedContentHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/css.css");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.css"));
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns(path);
            testable.Mock<IStore>().Setup(
                x => x.SendContent(It.IsAny<string>(), context.Object.Response)).
                Returns(contentSent);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Never());
        }
    }
}
