using System;
using System.IO;
using System.Web;
using Moq;
using RequestReduce.SassLessCoffee;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.SassLessCoffee
{
    public class LessHandlerFacts
    {
        class TestableLessHandler : Testable<LessHandler>
        {
            public TestableLessHandler()
            {
                
            }
        }

        [Fact]
        public void WillSetCorrectContentType()
        {
            var testable = new TestableLessHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.less"));
            var response = new Mock<HttpResponseBase>();
            response.SetupProperty(x => x.ContentType);
            context.Setup(x => x.Response).Returns(response.Object);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal("text/css", response.Object.ContentType);
        }

        [Fact]
        public void WillWriteCompiledLess()
        {
            var testable = new TestableLessHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.less"));
            var response = new Mock<HttpResponseBase>();
            context.Setup(x => x.Response).Returns(response.Object);
            testable.Mock<IFileWrapper>().Setup(x => x.GetFileString(It.IsAny<string>())).Returns("@brand_color: #4D926F;#header {color: @brand_color;}");
            var result = string.Empty;
            response.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(s => result = s);
            const string expected = "#header {\n  color: #4d926f;\n}\n";

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WillReturn404IfFileNotFound()
        {
            var testable = new TestableLessHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.less"));
            var response = new Mock<HttpResponseBase>();
            context.Setup(x => x.Response).Returns(response.Object);
            testable.Mock<IFileWrapper>().Setup(x => x.GetFileString(It.IsAny<string>())).Throws(new FileNotFoundException());
            response.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal(404, response.Object.StatusCode);
        }

        [Fact]
        public void WillReturn500IfIOExceptionIsThrown()
        {
            var testable = new TestableLessHandler();
            var context = new Mock<HttpContextBase>();
            context.Setup(x => x.Request.Url).Returns(new Uri("http://localhost/RRContent/css.less"));
            var response = new Mock<HttpResponseBase>();
            context.Setup(x => x.Response).Returns(response.Object);
            testable.Mock<IFileWrapper>().Setup(x => x.GetFileString(It.IsAny<string>())).Throws(new IOException());
            response.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(context.Object);

            Assert.Equal(500, response.Object.StatusCode);
        }
    }
}
