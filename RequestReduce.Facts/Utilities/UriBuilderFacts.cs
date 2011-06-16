using System;
using RequestReduce.Configuration;
using Xunit;
using UriBuilder = RequestReduce.Utilities.UriBuilder;

namespace RequestReduce.Facts.Utilities
{
    public class UriBuilderFacts
    {
        class TestableUriBuilder : Testable<UriBuilder>
        {
            public TestableUriBuilder()
            {
                
            }
        }

        public class BuildCssUrl
        {
            [Fact]
            public void WillCreateCorrectUrl()
            {
                var testable = new TestableUriBuilder();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/vpath");
                var guid = Guid.NewGuid();

                var result = testable.ClassUnderTest.BuildCssUrl(guid);

                Assert.Equal(result, string.Format("http://host/vpath/{0}/{1}", guid, UriBuilder.CssFileName));
            }
        }

        public class BuildSpriteUrl
        {
            [Fact]
            public void WillCreateCorrectUrl()
            {
                var testable = new TestableUriBuilder();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/vpath");
                var guid = Guid.NewGuid();

                var result = testable.ClassUnderTest.BuildSpriteUrl(guid, 5);

                Assert.Equal(result, string.Format("http://host/vpath/{0}/sprite5.png", guid));
            }
        }

        public class ParseFileName
        {
            [Fact]
            public void WillParseFileName()
            {
                var testable = new TestableUriBuilder();

                var result = testable.ClassUnderTest.ParseFileName("http://host/path/key/file.css");

                Assert.Equal("file.css", result);
            }
        }

        public class ParseKey
        {
            [Fact]
            public void WillParseKey()
            {
                var testable = new TestableUriBuilder();
                var key = Guid.NewGuid();

                var result = testable.ClassUnderTest.ParseKey("http://host/path/" + key + "/file.css");

                Assert.Equal(key, result);
            }
        }
    }

}
