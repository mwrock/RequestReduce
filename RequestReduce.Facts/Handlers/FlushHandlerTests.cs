using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Module;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;
using Xunit.Extensions;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

namespace RequestReduce.Facts.Handlers
{
    class FlushHandlerTests
    {
        class TestableFlushHandler : Testable<RequestReduce.Handlers.FlushHandler>
        {
            public TestableFlushHandler()
            {
                Inject<IIpFilter>(new IpFilter(Mock<IRRConfiguration>().Object));
                Inject<IUriBuilder>(new UriBuilder(Mock<IRRConfiguration>().Object));
            }
        }

        [Fact]
        public void WillSetPhysicalPathToMappedVirtualPathOnFlush()
        {
            var testable = new TestableFlushHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock <IHostingEnvironmentWrapper>().Setup(x => x.MapPath("/RRContent")).Returns("physical");
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/FlushFailures/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IRRConfiguration>().VerifySet(x => x.ResourcePhysicalPath = "physical", Times.Once());
        }

        [Fact]
        public void WillFlushFailuresOnFlushFailureUrlWhenAndAuthorizedUsersIsAnonymous()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/FlushFailures/page.aspx"));
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IReducingQueue>().Verify(x => x.ClearFailures(), Times.Once());
        }

        [Fact]
        public void WillFlushFailuresOnFlushFailureUrlWhenAndAuthorizedUsersIsAnonymousAndNull()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/FlushFailures/page.aspx"));
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.User).Returns(null as IPrincipal);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IReducingQueue>().Verify(x => x.ClearFailures(), Times.Once());
        }

        [Fact]
        public void WillFlushFailuresOnFlushFailureUrlWithTrailingSlashWhenAndAuthorizedUsersIsAnonymous()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/FlushFailures/"));
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IReducingQueue>().Verify(x => x.ClearFailures(), Times.Once());
        }

        [Theory]
        [InlineData("http://host/RRContent/f5623565740657421d875131b8f5ce3a/flush", "f5623565-7406-5742-1d87-5131b8f5ce3a")]
        [InlineData("http://host/RRContent/flush", "00000000-0000-0000-0000-000000000000")]
        public void WillFlushReductionsOnFlushUrlWhenAndAuthorizedUsersIsAnonymous(string url, string key)
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri(url));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var keyGuid = Guid.Parse(key);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(keyGuid), Times.Once());
        }

        [Theory]
        [InlineData("http://host/RRContent/f5623565740657421d875131b8f5ce3a/flush/RRFlush.aspx", "f5623565-7406-5742-1d87-5131b8f5ce3a")]
        [InlineData("http://host/RRContent/flush/page.aspx", "00000000-0000-0000-0000-000000000000")]
        public void WillFlushReductionsOnFlushUrlWhenAuthorizedUsersIsAnonymousAndIpFilterIsEmpty(string url, string key)
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri(url));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var keyGuid = Guid.Parse(key);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(keyGuid), Times.Once());
        }

        [Theory]
        [InlineData("http://host/RRContent/f5623565740657421d875131b8f5ce3a/flush/RRFlush.aspx", "f5623565-7406-5742-1d87-5131b8f5ce3a")]
        [InlineData("http://host/RRContent/flush/page.aspx", "00000000-0000-0000-0000-000000000000")]
        public void WillFlushReductionsOnFlushUrlWhenAuthorizedUsersIsAnonymousAndIpFilterIsInvalid(string url, string key)
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "invalid" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri(url));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            var keyGuid = Guid.Parse(key);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(keyGuid), Times.Once());
        }

        [Fact]
        public void WillFlushReductionsOnFlushUrlWhenCurrentUserIsAuthorizedUser()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(Guid.Empty), Times.Once());
        }

        [Fact]
        public void WillFlushReductionsOnFlushUrlWhenCurrentUserIsAuthorizedUserAndIpFilterIsEmpty()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(Guid.Empty), Times.Once());
        }

        [Fact]
        public void WillFlushReductionsOnFlushUrlWhenCurrentUserIsAuthorizedUserAndUserIpIsInIpFilter()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "9.9.9.9" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.UserHostAddress).Returns("9.9.9.9");

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(Guid.Empty), Times.Once());
        }

        [Fact]
        public void WillFlushReductionsOnFlushUrlWhenCurrentUserIsAuthorizedUserAndEquivalentIpIsInIpFilter()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "001.002.003.004" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.UserHostAddress).Returns("1.2.3.4");

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(Guid.Empty), Times.Once());
        }

        [Fact]
        public void WillFlushReductionsOnFlushUrlWhenCurrentUserIsAuthorizedUserAndEquivalentIpv6IsInIpFilter()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "3780:0:c307:0:2c45:0:81c7:9273" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.UserHostAddress).Returns("3780:0000:c307:0000:2c45:0000:81c7:9273");

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(Guid.Empty), Times.Once());
        }

        [Fact]
        public void WillFlushReductionsOnFlushUrlWithTrailingSlashWhenCurrentUserIsAuthorizedUser()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(Guid.Empty), Times.Once());
        }

        [Fact]
        public void WillNotFlushReductionsOnFlushUrlWhenCurrentUserIsNotAuthorizedUserAndReturn401()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user3");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Response).Returns(response.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(It.IsAny<Guid>()), Times.Never());
            Assert.Equal(401, response.Object.StatusCode);
        }

        [Fact]
        public void WillNotFlushReductionsOnFlushUrlWhenCurrentUserIsAuthorizedUserButBlockedByIpFilterAndReturn401()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(new[] { "user1", "user2" });
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "1.2.3.4", " 3.4.5.6" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(true);
            identity.Setup(x => x.Name).Returns("user2");
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Response).Returns(response.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.UserHostAddress).Returns("9.9.9.9");

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(It.IsAny<Guid>()), Times.Never());
            Assert.Equal(401, response.Object.StatusCode);
        }

        [Fact]
        public void WillNotFlushReductionsOnFlushUrlWhenAuthorizedUsersIsAnonymousButBlockedByIpFilterAndReturn401()
        {
            var testable = new TestableFlushHandler();
            testable.Mock<IRRConfiguration>().Setup(x => x.AuthorizedUserList).Returns(RRConfiguration.Anonymous);
            testable.Mock<IRRConfiguration>().Setup(x => x.IpFilterList).Returns(new[] { "1.2.3.4", " 3.4.5.6" });
            testable.Mock<IRRConfiguration>().Setup(x => x.ResourceVirtualPath).Returns("/RRContent");
            var context = new Mock<HttpContextBase>();
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.StatusCode);
            context.Setup(x => x.Request.Url).Returns(new Uri("http://host/RRContent/flush/page.aspx"));
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.IsAuthenticated).Returns(false);
            context.Setup(x => x.User.Identity).Returns(identity.Object);
            context.Setup(x => x.Response).Returns(response.Object);
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
            context.Setup(x => x.Request.UserHostAddress).Returns("10.0.0.1");
            context.Setup(x => x.Request.ServerVariables).Returns(new NameValueCollection { { "HTTP_X_FORWARDED_FOR", "9.9.9.9" } });

            testable.ClassUnderTest.ProcessRequest(context.Object);

            testable.Mock<IStore>().Verify(x => x.Flush(It.IsAny<Guid>()), Times.Never());
            Assert.Equal(401, response.Object.StatusCode);
        }
    }
}
