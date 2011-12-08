using System;
using System.Web;
using Moq;
using RequestReduce.SassLessCoffee;
using SassAndCoffee.Core.Compilers;
using Xunit;

namespace RequestReduce.Facts.SassLessCoffee
{
    public class SassHandlerFacts
    {
        class TestableSassHandler : Testable<SassHandler>
        {
            public TestableSassHandler()
            {
                Inject<ISimpleFileCompiler>(new SassFileCompiler());
                MockedContext = new Mock<HttpContextBase>();
                MockedContext.Setup(x => x.Request.Path)
                    .Returns("~/RRContent/css.sass");
                MockedContext.Setup(x => x.Request.PhysicalApplicationPath)
                    .Returns(string.Format("{0}\\TestScripts", AppDomain.CurrentDomain.BaseDirectory));
                MockedResponse = new Mock<HttpResponseBase>();
                MockedContext.Setup(x => x.Response).Returns(MockedResponse.Object);
                MockedServer = new Mock<HttpServerUtilityBase>();
                MockedServer.Setup(x => x.MapPath("~/RRContent/css.sass")).Returns(string.Format("{0}\\TestScripts\\test.sass", AppDomain.CurrentDomain.BaseDirectory));
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
            var testable = new TestableSassHandler();
            testable.MockedResponse.SetupProperty(x => x.ContentType);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal("text/css", testable.MockedResponse.Object.ContentType);
        }

        [Fact]
        public void WillWriteCompiledSass()
        {
            var testable = new TestableSassHandler();
            const string expected = ".content-navigation {\n  border-color: #3bbfce;\n  color: #2ca2af; }\n";

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(expected, testable.CompileResult);
        }

        [Fact]
        public void WillReturn404IfFileNotFound()
        {
            var testable = new TestableSassHandler();
            testable.MockedContext.Setup(x => x.Request.Path).Returns("~/badaddress/bad.sass");
            testable.MockedServer.Setup(x => x.MapPath("~/badaddress/bad.sass")).Returns(string.Format("{0}\\TestScripts\\bad.sass", AppDomain.CurrentDomain.BaseDirectory));
            testable.MockedResponse.SetupProperty(x => x.StatusCode);
            
            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(404, testable.MockedResponse.Object.StatusCode);
        }

        [Fact]
        public void WillReturn500IfIOExceptionIsThrown()
        {
            var testable = new TestableSassHandler();
            testable.MockedContext.Setup(x => x.Request.Path).Returns("~/badaddress/bad.sass");
            testable.MockedServer.Setup(x => x.MapPath("~/badaddress/bad.sass")).Returns("h:\\crazy\\crazy.sass");
            testable.MockedResponse.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(500, testable.MockedResponse.Object.StatusCode);
        }
    }
}
