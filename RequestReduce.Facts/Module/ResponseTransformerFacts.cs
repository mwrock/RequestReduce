﻿using System;
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
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1""><link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
<meta name=""description"" content="""" />


<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformToSingleScriptAtBeginningOfHeadOnMatch()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1""><script src=""http://server/Me3.js"" type=""text/javascript"" ></script>
<meta name=""description"" content="""" />


<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }


            [Fact]
            public void WillTransformCombinationOfScriptsAndStylesheets()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1""><script src=""http://server/Me3.js"" type=""text/javascript"" ></script><link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
<meta name=""description"" content="""" />




<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformRelativeUrlToAbsolute()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head>
<meta name=""description"" content="""" />
<link href=""/content/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<head><link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
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
                var transform = @"<head>
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
                var transform = @"<head>
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(
                    x => x.FindReduction("http://server/Me.css::http://server/Me2.css::"));
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testable.ClassUnderTest.Transform(transform);

                testable.Mock<IReducingQueue>().Verify(x => x.EnqueueCss("http://server/Me.css::http://server/Me2.css::"), Times.Once());
            }

        }
    }
}
