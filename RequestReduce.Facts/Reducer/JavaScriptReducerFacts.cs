using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Store;
using RequestReduce.Utilities;
using Xunit;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using RequestReduce.Module;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Facts.Reducer
{
    public class JavaScriptReducerFacts
    {
        private class TestableJavaScriptReducer : Testable<RequestReduce.Reducer.JavaScriptReducer>
        {
            public TestableJavaScriptReducer()
            {
                Mock<IMinifier>().Setup(x => x.Minify<JavaScriptResource>(It.IsAny<string>())).Returns("minified");
                Mock<ISpriteManager>().Setup(x => x.GetEnumerator()).Returns(new List<SpritedImage>().GetEnumerator());
                Inject<IUriBuilder>(new UriBuilder(Mock<IRRConfiguration>().Object));
            }

        }

        public class SupportedResourceType
        {
            [Fact]
            public void WillSupportJavaScript()
            {
                var testable = new TestableJavaScriptReducer();

                Assert.Equal(typeof(JavaScriptResource), testable.ClassUnderTest.SupportedResourceType);
            }
        }


        public class Process
        {
            [Fact]
            public void WillReturnProcessedJsUrlInCorrectConfigDirectory()
            {
                var testable = new TestableJavaScriptReducer();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");

                var result = testable.ClassUnderTest.Process("http://host/js1.js::http://host/js2.js");

                Assert.True(result.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillReturnProcessedJsUrlWithKeyInPath()
            {
                var testable = new TestableJavaScriptReducer();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");
                var guid = Guid.NewGuid();
                var builder = new UriBuilder(testable.Mock<IRRConfiguration>().Object);

                var result = testable.ClassUnderTest.Process(guid, "http://host/js1.js::http://host/js2.js");

                Assert.Equal(guid, builder.ParseKey(result));
            }

            [Fact]
            public void WillUseHashOfUrlsIfNoKeyIsGiven()
            {
                var testable = new TestableJavaScriptReducer();
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("spritedir");
                var guid = Hasher.Hash("http://host/js1.js::http://host/js2.js");
                var builder = new UriBuilder(testable.Mock<IRRConfiguration>().Object);

                var result = testable.ClassUnderTest.Process("http://host/js1.js::http://host/js2.js");

                Assert.Equal(guid, builder.ParseKey(result));
            }

            [Fact]
            public void WillReturnProcessedJsUrlWithARequestReducedFileName()
            {
                var testable = new TestableJavaScriptReducer();

                var result = testable.ClassUnderTest.Process("http://host/js1.js::http://host/js2.js");

                Assert.True(result.EndsWith("-" + new JavaScriptResource().FileName));
            }

            [Fact]
            public void WillDownloadContentOfEachOriginalJS()
            {
                var testable = new TestableJavaScriptReducer();

                var result = testable.ClassUnderTest.Process("http://host/js1.js::http://host/js2.js");

                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString<JavaScriptResource>("http://host/js1.js"), Times.Once());
                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString<JavaScriptResource>("http://host/js2.js"), Times.Once());
            }

            [Fact]
            public void WillSaveMinifiedAggregatedJS()
            {
                var testable = new TestableJavaScriptReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<JavaScriptResource>("http://host/js1.js")).Returns("js1");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString<JavaScriptResource>("http://host/js1.js")).Returns("js2");
                testable.Mock<IMinifier>().Setup(x => x.Minify<JavaScriptResource>("js1\njs2")).Returns("min");

                var result = testable.ClassUnderTest.Process("http://host/js1.js::http://host/js2.js");

                testable.Mock<IStore>().Verify(
                    x =>
                    x.Save(Encoding.UTF8.GetBytes("min").MatchEnumerable(), result,
                           "http://host/js1.js::http://host/js2.js"), Times.Once());
            }

        }
    }
}
