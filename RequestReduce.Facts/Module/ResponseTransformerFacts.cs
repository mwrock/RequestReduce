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
            public void WillTransformToSingleCssOnMatch()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");

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

                testable.ClassUnderTest.Transform(transform);

                testable.Mock<IReducingQueue>().Verify(x => x.Enqueue("http://server/Me.css::http://server/Me2.css::"), Times.Once());
            }

        }
    }
}
