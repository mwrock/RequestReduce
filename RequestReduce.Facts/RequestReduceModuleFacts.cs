using System;
using System.Web;
using System.Linq;
using System.Text;
using Moq;
using StructureMap;
using Xunit;

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

            context.VerifySet(x => x.Response.Filter, Times.Never());
        }

        [Fact]
        public void WillSetResponseFilterIfHtmlContent()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(module.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("text/html");

            module.InstallFilter(context.Object);

            context.VerifySet(x => x.Response.Filter, Times.Once());
        }

        [Fact]
        public void WillSetContextKeyIfNotSetBefore()
        {
            var module = new RequestReduceModule();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Items.Contains(module.CONTEXT_KEY)).Returns(false);
            context.Setup(x => x.Response.ContentType).Returns("type");

            module.InstallFilter(context.Object);

            context.Verify(x => x.Items.Add(module.CONTEXT_KEY, It.IsAny<Object>()), Times.Once());
        }

    }
}
