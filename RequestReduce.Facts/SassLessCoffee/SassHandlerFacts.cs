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
    public class SassHandlerFacts
    {
        class TestableSassHandler : Testable<SassHandler>
        {
            public TestableSassHandler()
            {
                Inject<IContentPipeline>(new ContentPipeline(new List<IContentTransform> { new SassCompilerContentTransform() }));
                MockedContext = new Mock<HttpContextBase>();
                MockedContext.Setup(x => x.Request.PhysicalPath)
                    .Returns(string.Format("{0}\\TestScripts\\test.sass", AppDomain.CurrentDomain.BaseDirectory));
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
            var testable = new TestableSassHandler();
            testable.MockedResponse.SetupProperty(x => x.ContentType);

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal("text/css", testable.MockedResponse.Object.ContentType);
        }

        [Fact]
        public void WillWriteCompiledSass()
        {
            var testable = new TestableSassHandler();
            const string expected = ".content-navigation {\n  border-color: #3bbfce;\n  color: #2ca2af; }\n\r\n";

            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(expected, testable.CompileResult);
        }

        [Fact]
        public void WillReturn404IfFileNotFound()
        {
            var testable = new TestableSassHandler();
            testable.MockedContext.Setup(x => x.Request.PhysicalPath).Returns(string.Format("{0}\\TestScripts\\bad.sass", AppDomain.CurrentDomain.BaseDirectory));
            testable.MockedResponse.SetupProperty(x => x.StatusCode);
            
            testable.ClassUnderTest.ProcessRequest(testable.MockedContext.Object);

            Assert.Equal(404, testable.MockedResponse.Object.StatusCode);
        }
    }
}
