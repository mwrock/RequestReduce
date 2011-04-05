using System;
using Moq;
using RequestReduce.Configuration;
using RequestReduce.Reducer;
using RequestReduce.Utilities;
using Xunit;

namespace RequestReduce.Facts.Reducer
{
    public class ReducerFacts
    {
        private class TestableReducer : Testable<RequestReduce.Reducer.Reducer>
        {
            public TestableReducer()
            {
                
            }
        }

        public class Process
        {
            [Fact]
            public void WillReturnProcessedCssUrlInCorrectConfigDirectory()
            {
                var testable = new TestableReducer();
                testable.Mock<IConfigurationWrapper>().Setup(x => x.SpriteDirectory).Returns("spritedir");

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                Assert.True(result.StartsWith("spritedir/"));
            }

            [Fact]
            public void WillReturnProcessedCssUrlWithAGuidName()
            {
                var testable = new TestableReducer();
                testable.Mock<IConfigurationWrapper>().Setup(x => x.SpriteDirectory).Returns("spritedir");
                Guid guid;

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                Assert.True(Guid.TryParse(result.Substring("spritedir/".Length, result.Length - "spritedir/".Length - ".css".Length), out guid));
            }

            [Fact]
            public void WillReturnProcessedCssUrlWithAcssExtension()
            {
                var testable = new TestableReducer();

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                Assert.True(result.EndsWith(".css"));
            }

            [Fact]
            public void WillDownloadContentOfEachOriginalCSS()
            {
                var testable = new TestableReducer();

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString("http://host/css1.css"), Times.Once());
                testable.Mock<IWebClientWrapper>().Verify(x => x.DownloadString("http://host/css2.css"), Times.Once());
            }

            [Fact]
            public void WillSaveMinifiedAggregatedCSS()
            {
                var testable = new TestableReducer();
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString("http://host/css1.css")).Returns("css1");
                testable.Mock<IWebClientWrapper>().Setup(x => x.DownloadString("http://host/css2.css")).Returns("css2");
                testable.Mock<IMinifier>().Setup(x => x.Minify("css1css2")).Returns("min");

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                testable.Mock<IFileWrapper>().Verify(x => x.Save("min", result), Times.Once());
            }

            [Fact]
            public void WillFlushSpriteManager()
            {
                var testable = new TestableReducer();

                testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                testable.Mock<ISpriteManager>().Verify(x => x.Flush(), Times.AtLeastOnce());
            }

        }
    }
}
