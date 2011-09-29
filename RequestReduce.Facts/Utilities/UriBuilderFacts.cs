using System;
using RequestReduce.Configuration;
using RequestReduce.Utilities;
using Xunit;
using Xunit.Extensions;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using RequestReduce.Reducer;
using RequestReduce.ResourceTypes;

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

        public class BuildUrl
        {
            [Fact]
            public void WillCreateCorrectCssUrl()
            {
                var testable = new TestableUriBuilder();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/vpath");
                var guid = Guid.NewGuid();
                var content = new byte[] {1};

                var result = testable.ClassUnderTest.BuildResourceUrl<CssResource>(guid, content);

                Assert.Equal(string.Format("http://host/vpath/{0}-{1}-{2}", guid.RemoveDashes(), Hasher.Hash(content).RemoveDashes(), new CssResource().FileName), result);
            }

            [Fact]
            public void WillCreateCorrectCssUrlFromString()
            {
                var testable = new TestableUriBuilder();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/vpath");
                var guid = Guid.NewGuid();
                var content = "abc";

                var result = testable.ClassUnderTest.BuildResourceUrl<CssResource>(guid, content);

                Assert.Equal(string.Format("http://host/vpath/{0}-{1}-{2}", guid.RemoveDashes(), content, new CssResource().FileName), result);
            }

            [Fact]
            public void WillCreateCorrectJavascriptUrl()
            {
                var testable = new TestableUriBuilder();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/vpath");
                var guid = Guid.NewGuid();
                var content = new byte[] { 1 };

                var result = testable.ClassUnderTest.BuildResourceUrl<JavaScriptResource>(guid, content);

                Assert.Equal(string.Format("http://host/vpath/{0}-{1}-{2}", guid.RemoveDashes(), Hasher.Hash(content).RemoveDashes(), new JavaScriptResource().FileName), result);
            }

            [Fact]
            public void WillCreateCorrectJavaScriptUrlFromString()
            {
                var testable = new TestableUriBuilder();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/vpath");
                var guid = Guid.NewGuid();
                var content = "abc";

                var result = testable.ClassUnderTest.BuildResourceUrl<JavaScriptResource>(guid, content);

                Assert.Equal(string.Format("http://host/vpath/{0}-{1}-{2}", guid.RemoveDashes(), content, new JavaScriptResource().FileName), result);
            }

            [Fact]
            public void WillThrowArgumentExceptionIfResourceTypeIsUnknown()
            {
                var testable = new TestableUriBuilder();
                testable.Mock<IRRConfiguration>().Setup(x => x.ContentHost).Returns("http://host");
                testable.Mock<IRRConfiguration>().Setup(x => x.SpriteVirtualPath).Returns("/vpath");
                var guid = Guid.NewGuid();
                var content = "abc";

                var result = Record.Exception(() => testable.ClassUnderTest.BuildResourceUrl(guid, content, typeof(Object))) as ArgumentException;

                Assert.NotNull(result);
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
                var content = new byte[] {1};

                var result = testable.ClassUnderTest.BuildSpriteUrl(guid, content);

                Assert.Equal(result, string.Format("http://host/vpath/{0}-{1}.png", guid.RemoveDashes(), Hasher.Hash(content).RemoveDashes()));
            }
        }

        public class ParseFileName
        {
            [Fact]
            public void WillParseFileName()
            {
                var testable = new TestableUriBuilder();

                var result = testable.ClassUnderTest.ParseFileName("http://host/path/key-file.css");

                Assert.Equal("key-file.css", result);
            }

            [Theory,
            InlineData(null),
            InlineData(" "),
            InlineData("")]
            public void WillReturnNullIfNameIsNullOrEmpty(string input)
            {
                var testable = new TestableUriBuilder();

                var result = testable.ClassUnderTest.ParseFileName(input);

                Assert.Null(result);
            }
        }

        public class ParseKey
        {
            [Fact]
            public void WillParseKey()
            {
                var testable = new TestableUriBuilder();
                var key = Guid.NewGuid();
                var sig = Guid.NewGuid().RemoveDashes();

                var result = testable.ClassUnderTest.ParseKey(string.Format("http://host/path/{0}-{1}-file.css", key.RemoveDashes(), sig));

                Assert.Equal(key, result);
            }

            [Fact]
            public void WillReturnEmptyGuidIfKeyIsNotGuid()
            {
                var testable = new TestableUriBuilder();
                var sig = Guid.NewGuid().RemoveDashes();

                var result = testable.ClassUnderTest.ParseKey(string.Format("http://host/path/{0}-{1}-file.css", "not a guid", sig));

                Assert.Equal(Guid.Empty, result);
            }

            [Theory,
            InlineData(null),
            InlineData(" "),
            InlineData("")]
            public void WillReturnEmptyGuidIfKeyIsNullOrWhiteSpace(string input)
            {
                var testable = new TestableUriBuilder();

                var result = testable.ClassUnderTest.ParseKey(input);

                Assert.Equal(Guid.Empty, result);
            }

        }

        public class ParseSignature
        {
            [Fact]
            public void WillParseSignature()
            {
                var testable = new TestableUriBuilder();
                var key = Guid.NewGuid().RemoveDashes();
                var sig = Guid.NewGuid().RemoveDashes();

                var result = testable.ClassUnderTest.ParseSignature(string.Format("http://host/path/{0}-{1}-file.css", key, sig));

                Assert.Equal(sig, result);
            }

            [Fact]
            public void WillReturnEmptyGuidIfSignatureIsNotGuid()
            {
                var testable = new TestableUriBuilder();
                var key = Guid.NewGuid().RemoveDashes();

                var result = testable.ClassUnderTest.ParseSignature(string.Format("http://host/path/{0}-{1}-file.css", key, "not a guid"));

                Assert.Equal(Guid.Empty.RemoveDashes(), result);
            }

            [Fact]
            public void WillReturnEmptyGuidIfkeyIsTooShort()
            {
                var testable = new TestableUriBuilder();

                var result = testable.ClassUnderTest.ParseSignature(string.Format("http://host/path/{0}-{1}-file.css", "uuu", "not a guid"));

                Assert.Equal(Guid.Empty.RemoveDashes(), result);
            }

            [Theory,
            InlineData(null),
            InlineData(" "),
            InlineData("")]
            public void WillReturnEmptyGuidIfkeyIsNullOrWhiteSpace(string input)
            {
                var testable = new TestableUriBuilder();

                var result = testable.ClassUnderTest.ParseSignature(input);

                Assert.Equal(Guid.Empty.RemoveDashes(), result);
            }

        }
    }

}
