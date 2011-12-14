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
                MockedContext = new Mock<HttpContextBase>();
                MockedContext.Setup(x => x.Request.Path)
                    .Returns("~/RRContent/css.less");
                MockedResponse = new Mock<HttpResponseBase>();
                MockedContext.Setup(x => x.Response).Returns(MockedResponse.Object);
                MockedServer = new Mock<HttpServerUtilityBase>();
                MockedServer.Setup(x => x.MapPath("~/RRContent/css.less")).Returns(string.Format("{0}\\TestScripts\\test.less", AppDomain.CurrentDomain.BaseDirectory));
                MockedContext.Setup(x => x.Server).Returns(MockedServer.Object);
                MockedResponse.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(s => CompileResult = s);
            }
            public Mock<HttpContextBase> MockedContext { get; set; }
            public Mock<HttpResponseBase> MockedResponse { get; set; }
            public Mock<HttpServerUtilityBase> MockedServer { get; set; }
            public string CompileResult { get; set; }
        }

        [Fact]
        public void WillSetCorrectContentType()
        {
            var testable = new TestableLessHandler();
            testable.MockedResponse.SetupProperty(x => x.ContentType);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal("text/css", testable.MockedResponse.Object.ContentType);
        }

        [Fact]
        public void WillWriteCompiledLess()
        {
            var testable = new TestableLessHandler();
            testable.Mock<IFileWrapper>().Setup(x => x.GetFileString(It.IsAny<string>()))
                .Returns("@brand_color: #4D926F;#header {color: @brand_color;}");
            const string expected = "#header {\n  color: #4d926f;\n}\n";

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(expected, testable.CompileResult);
        }

        [Fact]
        public void WillWriteLessParseErrorMessage()
        {
            var testable = new TestableLessHandler();
            testable.Mock<IFileWrapper>().Setup(x => x.GetFileString(It.IsAny<string>()))
                .Returns("@brand_color: #4D926F #header {color: @brand_color;}");

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Contains("Parse Error", testable.CompileResult);
        }

        [Fact]
        public void WillReturn404IfFileNotFound()
        {
            var testable = new TestableLessHandler();
            testable.Mock<IFileWrapper>().Setup(x => x.GetFileString(It.IsAny<string>()))
                .Throws(new FileNotFoundException());
            testable.MockedResponse.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(404, testable.MockedResponse.Object.StatusCode);
        }

        [Fact]
        public void WillReturn500IfIOExceptionIsThrown()
        {
            var testable = new TestableLessHandler();
            testable.Mock<IFileWrapper>().Setup(x => x.GetFileString(It.IsAny<string>()))
                .Throws(new IOException());
            testable.MockedResponse.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(500, testable.MockedResponse.Object.StatusCode);
        }
    }
}
