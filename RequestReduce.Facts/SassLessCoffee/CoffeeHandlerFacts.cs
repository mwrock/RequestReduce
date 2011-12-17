using System;
using System.Web;
using Moq;
using RequestReduce.SassLessCoffee;
using SassAndCoffee.Core;
using SassAndCoffee.Core.Compilers;
using Xunit;

namespace RequestReduce.Facts.SassLessCoffee
{
    public class CoffeeHandlerFacts : IDisposable
    {
        private static int count;

        class TestableCoffeeHandler : Testable<CoffeeHandler>
        {
            public TestableCoffeeHandler()
            {
                Inject<ISimpleFileCompiler>(new CoffeeScriptFileCompiler());
                MockedContext = new Mock<HttpContextBase>();
                MockedContext.Setup(x => x.Request.Path)
                    .Returns("~/RRContent/script.coffee");
                MockedContext.Setup(x => x.Request.PhysicalApplicationPath)
                    .Returns(string.Format("{0}\\TestScripts", AppDomain.CurrentDomain.BaseDirectory));
                MockedResponse = new Mock<HttpResponseBase>();
                MockedContext.Setup(x => x.Response).Returns(MockedResponse.Object);
                MockedServer = new Mock<HttpServerUtilityBase>();
                MockedServer.Setup(x => x.MapPath("~/RRContent/script.coffee")).Returns(string.Format("{0}\\TestScripts\\test.coffee", AppDomain.CurrentDomain.BaseDirectory));
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
            var testable = new TestableCoffeeHandler();
            testable.MockedResponse.SetupProperty(x => x.ContentType);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal("text/javascript", testable.MockedResponse.Object.ContentType);
        }

        [Fact]
        public void WillWriteCompiledCoffee()
        {
            var testable = new TestableCoffeeHandler();
            const string expected = "var square;\nsquare = function(x) {\n  return x * x;\n};";

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(expected, testable.CompileResult);
        }

        [Fact]
        public void WillReturn404IfFileNotFound()
        {
            var testable = new TestableCoffeeHandler();
            testable.MockedContext.Setup(x => x.Request.Path).Returns("~/badaddress/bad.coffee");
            testable.MockedServer.Setup(x => x.MapPath("~/badaddress/bad.coffee")).Returns(string.Format("{0}\\TestScripts\\bad.coffee", AppDomain.CurrentDomain.BaseDirectory));
            testable.MockedResponse.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(404, testable.MockedResponse.Object.StatusCode);
        }

        [Fact]
        public void WillReturn500IfIOExceptionIsThrown()
        {
            var testable = new TestableCoffeeHandler();
            testable.MockedContext.Setup(x => x.Request.Path).Returns("~/badaddress/bad.coffee");
            testable.MockedServer.Setup(x => x.MapPath("~/badaddress/bad.coffee")).Returns("h:\\crazy\\crazy.coffee");
            testable.MockedResponse.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(500, testable.MockedResponse.Object.StatusCode);
        }

        public void Dispose()
        {
            if(++count == 4) 
                //we do tis because sassandcoffee javascript compiler provides no alternative
                //without causing the xunit test runner to hang waiting for the js compiler
                //thread to complete. It can only be called once per app domain lifetime.
                //when SassAndCoffee 2.0 releases, we can remove this
                JS.Shutdown();
        }
    }
}
