using System;
using System.Collections.Generic;
using System.Web;
using Moq;
using RequestReduce.SassLessCoffee;
using SassAndCoffee.Core;
using SassAndCoffee.Ruby.Sass;
using Xunit;

namespace RequestReduce.Facts.SassLessCoffee
{
    public class CoffeeHandlerFacts
    {

        class TestableCoffeeHandler : Testable<CoffeeHandler>
        {
            public TestableCoffeeHandler()
            {
                Inject<IContentPipeline>(new ContentPipeline(new List<IContentTransform> { new SassCompilerContentTransform() }));
                MockedContext = new Mock<HttpContextBase>();
                MockedContext.Setup(x => x.Request.PhysicalPath)
                    .Returns(string.Format("{0}\\TestScripts\\test.coffee", AppDomain.CurrentDomain.BaseDirectory));
                MockedResponse = new Mock<HttpResponseBase>();
                MockedContext.Setup(x => x.Response).Returns(MockedResponse.Object);
                MockedResponse.Setup(x => x.Write(It.IsAny<string>())).Callback<string>(s => CompileResult = s);
            }

            public Mock<HttpContextBase> MockedContext { get; set; }
            public Mock<HttpResponseBase> MockedResponse { get; set; }
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
            const string expected = "(function() {\n  var square;\n\n  square = function(x) {\n    return x * x;\n  };\n\n}).call(this);\n\r\n";

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(expected, testable.CompileResult);
        }

        [Fact]
        public void WillReturn404IfFileNotFound()
        {
            var testable = new TestableCoffeeHandler();
            testable.MockedContext.Setup(x => x.Request.PhysicalPath).Returns(string.Format("{0}\\TestScripts\\bad.coffee", AppDomain.CurrentDomain.BaseDirectory));
            testable.MockedResponse.SetupProperty(x => x.StatusCode);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(404, testable.MockedResponse.Object.StatusCode);
        }
    }
}
