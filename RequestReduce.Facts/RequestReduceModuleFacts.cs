using System;
using System.Collections.Specialized;
using System.Web;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Filter;
using StructureMap;
using Xunit;
using System.IO;

namespace RequestReduce.Facts
{
    public class RequestReduceModuleFacts
    {
        [Fact]
        public void WillSetResponseFilterOnce()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(module.CONTEXT_KEY)).Returns(true);

            module.InstallFilter(context.Object);

            context.VerifySet((x => x.Response.Filter = It.IsAny<Stream>()), Times.Never());
        }

        [Fact]
        public void WillSetResponseFilterIfHtmlContent()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(module.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
        }

        [Fact]
        public void WillSetPhysicalPathToMappedVirtualPath()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            var config = new Mock<IRRConfiguration>();
            config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
            context.Setup(x => x.Items.Contains(module.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Server.MapPath("/Virtual")).Returns("physical");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
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
        public void WillNotSetResponseFilterIfRRFilterQSIsDisabled()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(module.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection() {{"RRFilter", "disabled"}});
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
        }

        [Fact]
        public void WillSetContextKeyIfNotSetBefore()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(module.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("type");
            context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
            context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            module.InstallFilter(context.Object);

            context.Verify(x => x.Items.Add(module.CONTEXT_KEY, It.IsAny<Object>()), Times.Once());
        }

    }
}
