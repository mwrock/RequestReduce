using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using Moq;
using RequestReduce.Configuration;
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
        public void WillSetResponseFilterOnce()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(true);

            module.InstallFilter(context.Object);

            context.VerifySet((x => x.Response.Filter = It.IsAny<Stream>()), Times.Never());
        }

        [Fact]
        public void WillSetResponseFilterIfHtmlContent()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Request.RawUrl).Returns("/content/blah");
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
        }

        [Fact]
        public void WillNotSetResponseFilterIfFaviconIco()
        {
            RRContainer.Current = null;
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.RawUrl).Returns("/favicon.ico");

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
        }

        [Fact]
        public void WillSetPhysicalPathToMappedVirtualPath()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Server.MapPath("/Virtual")).Returns("physical");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Request.RawUrl).Returns("/content/blah");
            RRContainer.Current = new Container(x =>
                                                    {
                                                        x.For<IRRConfiguration>().Use(config.Object);
                                                        x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                                                    });

            module.InstallFilter(context.Object);

            config.VerifySet(x => x.SpritePhysicalPath = "physical", Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillSetPhysicalPathToMappedVirtualPathWhenHandlingContent()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Server.MapPath("/Virtual")).Returns("physical");
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
            });

            module.HandleRRContent(context.Object);

            config.VerifySet(x => x.SpritePhysicalPath = "physical", Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotSetResponseFilterIfRRFilterQSIsDisabled()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection() {{"RRFilter", "disabled"}});
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotSetResponseFilterIfCssAndJsProcessingIsDisabledFromConfig()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            config.Setup(x => x.CssProcesingDisabled).Returns(true);
            config.Setup(x => x.JavaScriptProcesingDisabled).Returns(true);
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillSetResponseFilterIfJustJsProcessingIsDisabledFromConfig()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            config.Setup(x => x.JavaScriptProcesingDisabled).Returns(true);
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillSetResponseFilterIfJustCssProcessingIsDisabledFromConfig()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            config.Setup(x => x.CssProcesingDisabled).Returns(true);
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillSetContextKeyIfNotSetBefore()
        {
            RRContainer.Current = null;
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("type");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.RawUrl).Returns("/content/blah");

            module.InstallFilter(context.Object);

            context.Verify(x => x.Items.Add(RequestReduceModule.CONTEXT_KEY, It.IsAny<Object>()), Times.Once());
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
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(60*24*360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag("sig"), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillHonorEtagsandReturn304WhenTheyMatchIfNoneMatch()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection() { { "If-None-Match", "match" } });
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
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(60 * 24 * 360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag("match"), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
            Assert.Equal(304, context.Object.Response.StatusCode);
            store.Verify(x => x.SendContent(It.IsAny<string>(), context.Object.Response), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotReturn304WhenSigDoesNotMatchIfNoneMatch()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/key-match-css.css");
            context.Setup(x => x.Request.Headers).Returns(new NameValueCollection() { { "If-None-Match", "notmatch" } });
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
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(builder.Object);
            });

            module.HandleRRContent(context.Object);

            Assert.Equal(60 * 24 * 360, context.Object.Response.Expires);
            cache.Verify(x => x.SetETag("match"), Times.Once());
            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Once());
            RRContainer.Current = null;
        }

        [Theory]
        [InlineData("/RRContent/f5623565-7406-5742-1d87-5131b8f5ce3a/sprite1.png", "image/png", true)]
        [InlineData("/RRContent/f5623565-7406-5742-1d87-5131b8f5ce3a/RequestReducedStyle.css", "text/css", true)]
        [InlineData("/RRContent/f5623565-7406-5742-1d87-5131b8f5ce3a/RequestReducedScript.js", "application/x-javascript", true)]
        [InlineData("/RRContent/f5623565-7406-5742-1d87-5131b8f5ce3a/RequestReducedStyle.css", null, false)]
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
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(new UriBuilder(config.Object));
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
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use(new UriBuilder(config.Object));
            });

            module.HandleRRContent(context.Object);

            cache.Verify(x => x.SetCacheability(HttpCacheability.Public), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotSetPhysicalPathToMappedPathOfVirtualPathIfPhysicalPathIsNotEmpty()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpritePhysicalPath).Returns("physicalPath");
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
            });
            context.Setup(x => x.Items.Contains(RequestReduceModule.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.RawUrl).Returns("/content/blah");

            module.InstallFilter(context.Object);

            config.VerifySet(x => x.SpritePhysicalPath = It.IsAny<string>(), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillSetPhysicalPathToMappedVirtualPathOnFlush()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            config.Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            context.Setup(x => x.Server.MapPath("/RRContent")).Returns("physical");
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/FlushFailures");
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            var queue = new Mock<IReducingQueue>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IReducingQueue>().Use(queue.Object);
            });

            module.HandleRRFlush(context.Object);

            config.VerifySet(x => x.SpritePhysicalPath = "physical", Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillFlushFailuresOnFlushFailureUrlWhenAndAuthorizedUsersIsAnonymous()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/FlushFailures");
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            var queue = new Mock<IReducingQueue>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IReducingQueue>().Use(queue.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
            });

            module.HandleRRFlush(context.Object);

            queue.Verify(x => x.ClearFailures(), Times.Once());
            RRContainer.Current = null;
        }

        [Theory]
        [InlineData("/RRContent/f5623565740657421d875131b8f5ce3a/flush", "f5623565-7406-5742-1d87-5131b8f5ce3a")]
        [InlineData("/RRContent/flush", "00000000-0000-0000-0000-000000000000")]
        public void WillFlushReductionsOnFlushUrlWhenAndAuthorizedUsersIsAnonymous(string url, string key)
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns(url);
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var store = new Mock<IStore>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
            });
            var keyGuid = Guid.Parse(key);

            module.HandleRRFlush(context.Object);

            store.Verify(x => x.Flush(keyGuid), Times.Once());
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
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
            });

            module.HandleRRFlush(context.Object);

            store.Verify(x => x.Flush(It.IsAny<Guid>()), Times.Never());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillFlushReductionsOnFlushUrlWhenCurrentUserIsAuthorizedUser()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.AuthorizedUserList).Returns(new string[]{"user1", "user2"});
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/flush");
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var store = new Mock<IStore>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
            });

            module.HandleRRFlush(context.Object);

            store.Verify(x => x.Flush(Guid.Empty), Times.Once());
            RRContainer.Current = null;
        }

        [Fact]
        public void WillNotFlushReductionsOnFlushUrlWhenCurrentUserIsNotAuthorizedUserAndReturn401()
        {
            var module = new RequestReduceModule();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.AuthorizedUserList).Returns(new string[] { "user1", "user2" });
            config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            context.Setup(x => x.Request.RawUrl).Returns("/RRContent/flush");
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user3");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Response).Returns(response.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var store = new Mock<IStore>();
            RRContainer.Current = new Container(x =>
            {
                x.For<IRRConfiguration>().Use(config.Object);
                x.For<IStore>().Use(store.Object);
                x.For<IUriBuilder>().Use<UriBuilder>();
            });

            module.HandleRRFlush(context.Object);

            store.Verify(x => x.Flush(It.IsAny<Guid>()), Times.Never());
            Assert.Equal(401, response.Object.StatusCode);
            RRContainer.Current = null;
        }

        public void Dispose()
        {
            RRContainer.Current = null;
        }
    }
} 