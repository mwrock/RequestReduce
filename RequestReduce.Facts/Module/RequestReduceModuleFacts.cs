using System;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Handlers;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;
using System.IO;
using Xunit.Extensions;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

namespace RequestReduce.Facts.Module
{
    public class RequestReduceModuleFacts : IDisposable
    {
        [Fact]
        public void WillGetAndSetResponseFilterIfHtmlContent()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
            context.Setup(x => x.Request.RawUrl).Returns("/content/blah");
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });

            module.InstallFilter(context.Object);

            context.VerifyGet(x => x.Response.Filter, Times.Once());
            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillGetAndSetResponseFilterIfXHtmlContent()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
            context.Setup(x => x.Request.RawUrl).Returns("/content/blah");
            context.Setup(x => x.Response.ContentType).Returns("application/xhtml+xml");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });

            module.InstallFilter(context.Object);

            context.VerifyGet(x => x.Response.Filter, Times.Once());
            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotSetResponseFilterIfFaviconIco()
        {
            RRContainer.Current = null;
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.RawUrl).Returns("/favicon.ico");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
            RRContainer.Current = null;
        }

        [Theory]
        [InlineData("/RRContent")]
        [InlineData("http://localhost/RRContent")]
        public void WillSetCachabilityIfInRRPathAndStoreSendsContent(string path)
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/css.css");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.css"));
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns(path);
            var store = new Mock<IStore>();
            store.Setup(
                x => x.SendContent(It.IsAny<string>(), context.Object.Response)).
                Returns(true);
            var builder = new Mock<IUriBuilder>();
            builder.Setup(x => x.ParseSignature("/RRContent/css.css")).Returns("sig");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(60*24*360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag(@"""sig"""), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillHonorEtagsandReturn304WhenTheyMatchIfNoneMatch()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection { { "If-None-Match", @"""match""" } });
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/key-match-css.css"));
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var store = new Mock<IStore>();
            var builder = new Mock<IUriBuilder>();
            builder.Setup(x => x.ParseSignature("/RRContent/key-match-css.css")).Returns("match");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(60 * 24 * 360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag(@"""match"""), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
            Assert.Equal(304, context.Object.Response.StatusCode);
            store.Verify(x => x.SendContent(It.IsAny<string>(), context.Object.Response), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillStripQueryStringDromGeneratedUrls()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css?somequerystring");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/key-match-css.css?somequerystring"));
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection { { "If-None-Match", @"""match""" } });
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var store = new Mock<IStore>();
            var builder = new Mock<IUriBuilder>();
            builder.Setup(x => x.ParseSignature("/RRContent/key-match-css.css")).Returns("match");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            store.Verify(x => x.SendContent("/RRContent/key-match-css.css", context.Object.Response), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillSetPhysicalPathToMappedVirtualPathWhenHandlingContent()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            var hostingEnvironment = new Mock<IHostingEnvironmentWrapper>();
            hostingEnvironment.Setup(x => x.MapPath("/Virtual")).Returns("physical");
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Request.RawUrl).Returns("/Virtual/blah");
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IStore>().Use(new Mock<IStore>().Object);
                x.For<IUriBuilder>().Use(new Mock<IUriBuilder>().Object);
                x.For<IHostingEnvironmentWrapper>().Use(hostingEnvironment.Object);
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            config.VerifySet(x => x.SpritePhysicalPath = "physical", Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotReturn304WhenSigDoesNotMatchIfNoneMatch()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection { { "If-None-Match", @"""notmatch""" } });
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/key-match-css.css"));
            context.Setup(x => x.Response.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var store = new Mock<IStore>();
            store.Setup(
                x => x.SendContent(It.IsAny<string>(), context.Object.Response)).
                Returns(true);
            var builder = new Mock<IUriBuilder>();
            builder.Setup(x => x.ParseSignature("/RRContent/key-match-css.css")).Returns("match");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(60 * 24 * 360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag(@"""match"""), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
            RRContainer.Current = null;
        }

        [Theory]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-sprite1.png", "image/png", true)]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-RequestReducedStyle.css", "text/css", true)]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-RequestReducedScript.js", "application/x-javascript", true)]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a-f5623565740657421d875131b8f5ce3a-RequestReducedStyle.css", null, false)]
        public void WillCorrectlySetContentType(string path, string contentType, bool contentInStore)
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.ContentType);
            response.Setup(x => x.Headers).Returns(new NameValueCollection());
            response.Setup(x => x.Cache).Returns(new Mock<HttpCachePolicyBase>().Object);
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Response).Returns(response.Object);
            context.Setup(x => x.Request.RawUrl).Returns(path);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var store = new Mock<IStore>();
            store.Setup(
                x => x.SendContent(It.IsAny<string>(), response.Object)).
                Returns(contentInStore);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(new UriBuilder(config.Object));
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(contentType, response.Object.ContentType);
            RRContainer.Current = null;
        }

        [Theory]
        [InlineData("/Content", true)]
        [InlineData("http://localhost/Content", true)]
        [InlineData("/RRContent", false)]
        [InlineData("http://localhost/RRContent", false)]
        public void WillNotSetCachabilityIfNotInRRPatOrStoreDoesNotSendContent(string path, bool contentSent)
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/css.css");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.css"));
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var cache = new Mock<HttpCachePolicyBase>();
            context.Setup(x => x.Response.Cache).Returns(cache.Object);
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns(path);
            var store = new Mock<IStore>();
            store.Setup(
                x => x.SendContent(It.IsAny<string>(), context.Object.Response)).
                Returns(contentSent);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(new UriBuilder(config.Object));
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotHandleRequestsNotInMyDirectory()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContents/someresource");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var store = new Mock<IStore>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            store.Verify(x => x.SendContent(It.IsAny<string>(), It.IsAny<HttpResponseBase>()), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotHandleRequestsInChildDirectory()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/child/someresource");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var store = new Mock<IStore>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
                x.For<IHandlerFactory>().Use<HandlerFactory>();
            });

            module.HandleRRContent(context.Object);

            store.Verify(x => x.SendContent(It.IsAny<string>(), It.IsAny<HttpResponseBase>()), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillDelegateContentMappedToHandler()
        {
            var module = new RequestReduceModule();
            var handler = new DefaultHttpHandler();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/content/someresource.less");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/content/someresource.less"));
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Items).Returns(new Hashtable());
            var handlerFactory = new HandlerFactory();
            handlerFactory.AddHandlerMap(x => x.AbsolutePath.EndsWith(".less") ? handler : null);
            handlerFactory.AddHandlerMap(x => x.AbsolutePath.EndsWith(".les") ? new DefaultHttpHandler() : null);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
                x.For<IHandlerFactory>().Use(handlerFactory);
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(handler, context.Object.Items["remapped handler"]);
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotFlushReductionsIfNotOnFlushUrl()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/notflush");
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var store = new Mock<IStore>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
            });

            module.HandleRRFlush(context.Object);

            store.Verify(x => x.Flush(It.IsAny<Guid>()), Times.Never());
            RRContainer.Current = null;
        }

        public void Dispose()
        {
            RRContainer.Current = null;
        }
    }
} 