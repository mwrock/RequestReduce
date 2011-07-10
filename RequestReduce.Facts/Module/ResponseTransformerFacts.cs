using System;
using System.Web;
using Moq;
using RequestReduce.Module;
using Xunit;

namespace RequestReduce.Facts.Module
{
    public class ResponseTransformerFacts
    {
        private class TestableResponseTransformer : Testable<ResponseTransformer>
        {
            public TestableResponseTransformer()
            {
            }
        }

        public class Transform
        {
            [Fact]
            public void WillTransformToSingleCssAtBeginningOfHeadOnMatch()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
<meta name=""description"" content="""" />


<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformRelativeUrlToAbsolute()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"
<meta name=""description"" content="""" />
<link href=""/content/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
<meta name=""description"" content="""" />


<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/content/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillNotTransformIfRepoReturnsNull()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(
                    x => x.FindReduction("http://server/Me.css::http://server/Me2.css::"));
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transform, result);
            }

            [Fact]
            public void WillQueueUrlsIfRepoReturnsNull()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(
                    x => x.FindReduction("http://server/Me.css::http://server/Me2.css::"));
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testable.ClassUnderTest.Transform(transform);

                testable.Mock<IReducingQueue>().Verify(x => x.Enqueue("http://server/Me.css::http://server/Me2.css::"), Times.Once());
            }

        }
    }
}
