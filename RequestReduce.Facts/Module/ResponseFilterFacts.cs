using System;
using System.IO;
using System.Text;
using Moq;
using RequestReduce.Api;
using RequestReduce.Module;
using Xunit;
using System.Web;

namespace RequestReduce.Facts.Module
{
    public class ResponseFilterFacts
    {
        private class TestableResponseFilter : Testable<ResponseFilter>
        {
            public TestableResponseFilter(Encoding encoding)
            {
                Inject(encoding);
                Mock<Stream>().Setup(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).
                    Callback<byte[], int, int>((buf, off, len) =>
                    {
                        FilteredResult += encoding.GetString(buf, off, len);
                    });

            }

            public string FilteredResult { get; set; }
        }

        public class Write
        {
            [Fact]
            public void WillTransformHeadInSingleWrite()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillSwallowAndLogTransformerErrors()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>head</head>after";
                var testTransform = "<head>head</head>";
                Exception error = null;
                var innerError = new ApplicationException();
                Registry.CaptureErrorAction = (x => error= x);
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Throws(innerError);

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(innerError, error.InnerException);
                Assert.Contains(testTransform, error.Message);
            }

            [Fact]
            public void WillTransformHeadAtBeginningOfResponse()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal("<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadAtEndOfResponse()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>head</head>";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);
                testable.ClassUnderTest.Flush();

                Assert.Equal("before<head>thead</head>", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadWhenAllResponse()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "<head>head</head>";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);
                testable.ClassUnderTest.Flush();

                Assert.Equal("<head>thead</head>", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadInmultipleWritesBrokenAtStartToken()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 9);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 9, testBuffer.Length-9);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillNotDoubleFlushUnmatchedStartsIfNoMatcheFound()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<heading>head</heading>after";

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal("before<heading>head</heading>after", testable.FilteredResult);
            }

            [Fact]
            public void WillNotLoseUntransformedMismatchFromPreviousWrite()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "this is the first write<hdiv>and this is the second";

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 24);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 24, testBuffer.Length-24);

                Assert.Equal("this is the first write<hdiv>and this is the second", testable.FilteredResult);
            }

            [Fact]
            public void WillNotLoseUntransformedMismatchFromPreviousWriteWhenMismatchIsOnStopStartMatch()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "this is the first write<heading>and this is the second";

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 25);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 25, testBuffer.Length - 25);

                Assert.Equal("this is the first write<heading>and this is the second", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadInmultipleWritesBrokenBeforeStartToken()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 3);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 3, testBuffer.Length - 3);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadInmultipleWritesBrokenBetweenToken()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 15);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 15, testBuffer.Length - 15);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadInmultipleWritesBrokenAtEndToken()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 26);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 26, testBuffer.Length - 26);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadInsingleWriteWithPartialMatchBeforeStart()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "be<h1>fo</h1>re<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal("be<h1>fo</h1>re<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadInsingleWriteWithPartialMatchBeforeEnd()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "before<head>h<h1>ea</h1>d</head>after";
                var testTransform = "<head>h<h1>ea</h1>d</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }

            public void WillTransformHeadInMultipleWritesWithPartialMatchBeforeStart()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = "be<h1>fo</h1>re<head>head</head>after";
                var testTransform = "<head>head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns("<head>thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 4);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 4, 10);
                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 14, testBuffer.Length - 14);

                Assert.Equal("be<h1>fo</h1>re<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadWithAttribute()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"before<head id=""Head1"">head</head>after";
                var testTransform = @"<head id=""Head1"">head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns(@"<head id=""Head1"">thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(@"before<head id=""Head1"">thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformUnicodeCorrectly()
            {
                var testable = new TestableResponseFilter(Encoding.Unicode);
                var testBuffer = @"before<head id=""Head1"">head</head>after";
                var testTransform = @"<head id=""Head1"">head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform)).Returns(@"<head id=""Head1"">thead</head>");

                testable.ClassUnderTest.Write(Encoding.Unicode.GetBytes(testBuffer), 0, Encoding.Unicode.GetBytes(testBuffer).Length);

                Assert.Equal(@"before<head id=""Head1"">thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformWhenBufferEndsBetweenTwoFilteredTags()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head><script src=""abc""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 28);
                testable.ClassUnderTest.Flush();

                Assert.Equal(@"<head id=""Head1"">thead</head>", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformMultipleSearchTags()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head>after<script src=""abc""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                var testTransform2 = @"<script src=""abc""></script>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform2)).Returns(@"<script src=""def""></script>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(@"<head id=""Head1"">thead</head>after<script src=""def""></script>end", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformAdjacntScritTagsInBodyTagsSeparatedByWhiteSpace()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head>after<script src=""abc""></script>
                    <script src=""def""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                var testTransform2 = @"<script src=""abc""></script>
                    <script src=""def""></script>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform2)).Returns(@"<script src=""ghi""></script>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(@"<head id=""Head1"">thead</head>after<script src=""ghi""></script>end", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformAdjacentHeadAndScriptTags()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head><script src=""abc""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                var testTransform2 = @"<script src=""abc""></script>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform2)).Returns(@"<script src=""def""></script>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(@"<head id=""Head1"">thead</head><script src=""def""></script>end", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformAdjacntScritTagsInBodyTagsSeparatedByNoScriptTags()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head>after<script src=""abc""></script><noscript>this is some stuff to do with js turned off.</noscript> 
                    <script src=""def""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                var testTransform2 = @"<script src=""abc""></script><noscript>this is some stuff to do with js turned off.</noscript> 
                    <script src=""def""></script>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform2)).Returns(@"<script src=""ghi""></script>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(@"<head id=""Head1"">thead</head>after<script src=""ghi""></script>end", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformAdjacntScritTagsInBodyTagsSeparatedByComments()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head>after<script src=""abc""></script><!--This is a coment <script> a script </script>  -->
                    <script src=""def""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                var testTransform2 = @"<script src=""abc""></script><!--This is a coment <script> a script </script>  -->
                    <script src=""def""></script>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform2)).Returns(@"<script src=""ghi""></script>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(@"<head id=""Head1"">thead</head>after<script src=""ghi""></script>end", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformAdjacntScritTagsInBodyTagsSeparatedByCommentsAndNoscript()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head>after<script src=""abc""></script><noscript>this is noscript</noscript><!--This is a coment <script> a script </script>  -->
                    <script src=""def""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                var testTransform2 = @"<script src=""abc""></script><noscript>this is noscript</noscript><!--This is a coment <script> a script </script>  -->
                    <script src=""def""></script>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform2)).Returns(@"<script src=""ghi""></script>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(@"<head id=""Head1"">thead</head>after<script src=""ghi""></script>end", testable.FilteredResult);
            }

            [Fact]
            public void WillBundleAsyncAndDeferScriptsBeforeBodyEnd()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<script async src=""http://server/Me3.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me4.js"" async type=""text/javascript"" ></script>
<script src=""http://server/Me5.js"" type=""text/javascript"" defer ></script>
<script defer src=""http://server/Me6.js"" type=""text/javascript"" ></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me7.js"" type=""text/javascript"" ></script>



<title>site</title></head><body><script type=""text/javascript"">(function (d,s) {
var b = d.createElement(s); b.type = ""text/javascript""; b.async = true; b.src = ""http://server/Me8.js"";
var t = d.getElementsByTagName(s)[0]; t.parentNode.insertBefore(b,t);
}(document,'script'));</script><script defer src=""http://server/Me9.js"" type=""text/javascript"" ></script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me7.js");
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me3.js::http://server/Me4.js::")).Returns("http://server/Me8.js");
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me5.js::http://server/Me6.js::")).Returns("http://server/Me9.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }

            [Fact]
            public void WillBundleMixedAsyncAndDeferScriptsBeforeBodyEnd()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<script async src=""http://server/Me3.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me5.js"" type=""text/javascript"" defer ></script>
<script src=""http://server/Me7.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me8.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me4.js"" async type=""text/javascript"" ></script>
<script defer src=""http://server/Me6.js"" type=""text/javascript"" ></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me9.js"" type=""text/javascript"" ></script>

<script src=""http://server/Me12.js"" type=""text/javascript"" ></script>

<title>site</title></head><body><script type=""text/javascript"">(function (d,s) {
var b = d.createElement(s); b.type = ""text/javascript""; b.async = true; b.src = ""http://server/Me10.js"";
var t = d.getElementsByTagName(s)[0]; t.parentNode.insertBefore(b,t);
}(document,'script'));</script><script defer src=""http://server/Me11.js"" type=""text/javascript"" ></script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me9.js");
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me3.js::http://server/Me4.js::")).Returns("http://server/Me10.js");
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me5.js::http://server/Me6.js::")).Returns("http://server/Me11.js");
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me7.js::http://server/Me8.js::")).Returns("http://server/Me12.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }

            
            
            [Fact]
            public void WillIgnoreAsyncInSrcAttribute()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/my%20async%20script.js"" type=""text/javascript"" ></script>
<script async src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/my%20async%20script.js"" type=""text/javascript"" ></script>

<title>site</title></head><body><script type=""text/javascript"">(function (d,s) {
var b = d.createElement(s); b.type = ""text/javascript""; b.async = true; b.src = ""http://server/Me2.js"";
var t = d.getElementsByTagName(s)[0]; t.parentNode.insertBefore(b,t);
}(document,'script'));</script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/my%20async%20script.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }

            [Fact]
            public void WillIgnoreAsyncInSingleQuoteSrcAttribute()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src='http://server/my%20async%20script.js' type=""text/javascript"" ></script>
<script async src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src='http://server/my%20async%20script.js' type=""text/javascript"" ></script>

<title>site</title></head><body><script type=""text/javascript"">(function (d,s) {
var b = d.createElement(s); b.type = ""text/javascript""; b.async = true; b.src = ""http://server/Me2.js"";
var t = d.getElementsByTagName(s)[0]; t.parentNode.insertBefore(b,t);
}(document,'script'));</script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/my%20async%20script.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }

            [Fact]
            public void WillIgnoreInlineScriptWithAsyncAndDeferContents()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script type=""text/javascript"" > async = defer </script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script type=""text/javascript"" > async = defer </script>
<title>site</title></head><body></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                
                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }

            [Fact]
            public void WillIgnoreDeferInSrcAttribute()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/my%20defer%20script.js"" type=""text/javascript"" ></script>
<script defer src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/my%20defer%20script.js"" type=""text/javascript"" ></script>

<title>site</title></head><body><script defer src=""http://server/Me2.js"" type=""text/javascript"" ></script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/my%20defer%20script.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }

            [Fact]
            public void WillIgnoreDeferInSingleQuoteSrcAttribute()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src='http://server/my%20defer%20script.js' type=""text/javascript"" ></script>
<script defer src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src='http://server/my%20defer%20script.js' type=""text/javascript"" ></script>

<title>site</title></head><body><script defer src=""http://server/Me2.js"" type=""text/javascript"" ></script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/my%20defer%20script.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }

            [Fact]
            public void WillBundleVariousDeferScriptsBeforeBodyEnd()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script defer src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" defer type=""text/javascript"" ></script>
<script src=""http://server/Me3.js"" type=""text/javascript"" defer></script>
<script defer=""defer"" src=""http://server/Me4.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me5.js"" defer=""defer"" type=""text/javascript"" ></script>
<script src=""http://server/Me6.js"" type=""text/javascript"" defer=""defer""></script>
<script defer='defer' src=""http://server/Me7.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me8.js"" defer='defer' type=""text/javascript"" ></script>
<script src=""http://server/Me9.js"" type=""text/javascript"" defer='defer'></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />









<title>site</title></head><body><script defer src=""http://server/Me10.js"" type=""text/javascript"" ></script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::http://server/Me3.js::http://server/Me4.js::http://server/Me5.js::http://server/Me6.js::http://server/Me7.js::http://server/Me8.js::http://server/Me9.js::")).Returns("http://server/Me10.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }


            [Fact]
            public void WillBundleVariousAsyncScriptsBeforeBodyEnd()
            {
                var testableFilter = new TestableResponseFilter(Encoding.UTF8);

                var testBuffer = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script async src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" async type=""text/javascript"" ></script>
<script src=""http://server/Me3.js"" type=""text/javascript"" async></script>
<script async=""async"" src=""http://server/Me4.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me5.js"" async=""async"" type=""text/javascript"" ></script>
<script src=""http://server/Me6.js"" type=""text/javascript"" async=""async""></script>
<script async='async' src=""http://server/Me7.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me8.js"" async='async' type=""text/javascript"" ></script>
<script src=""http://server/Me9.js"" type=""text/javascript"" async='async'></script>
<title>site</title></head><body></body>
                ";
                var expected = @"<head id=""Head1"">
<meta name=""description"" content="""" />









<title>site</title></head><body><script type=""text/javascript"">(function (d,s) {
var b = d.createElement(s); b.type = ""text/javascript""; b.async = true; b.src = ""http://server/Me10.js"";
var t = d.getElementsByTagName(s)[0]; t.parentNode.insertBefore(b,t);
}(document,'script'));</script></body>
                ";

                var testableTransformer = new RequestReduce.Facts.Module.ResponseTransformerFacts.TestableResponseTransformer();
                testableTransformer.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::http://server/Me3.js::http://server/Me4.js::http://server/Me5.js::http://server/Me6.js::http://server/Me7.js::http://server/Me8.js::http://server/Me9.js::")).Returns("http://server/Me10.js");
                testableTransformer.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testableFilter.Inject<IResponseTransformer>(testableTransformer.ClassUnderTest);
                testableFilter.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, testBuffer.Length);

                Assert.Equal(expected, testableFilter.FilteredResult);
            }
        }
    }
}