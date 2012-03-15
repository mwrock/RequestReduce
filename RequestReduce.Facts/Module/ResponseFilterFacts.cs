using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using Moq;
using RequestReduce.Api;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Module;
using RequestReduce.Utilities;
using StructureMap;
using Xunit;
using Xunit.Extensions;

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
        }

        public class InstallFilter
        {
            public void WillSetResponseFilterOnce()
            {
                var context = new Mock<HttpContextBase>();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(true);
                RRContainer.Current = new Container(x =>
                                                        {
                                                            x.For<IRRConfiguration>().Use(config.Object);
                                                            x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                                                        });

                ResponseFilter.InstallFilter(context.Object);

                context.VerifySet((x => x.Response.Filter = It.IsAny<Stream>()), Times.Never());
                RRContainer.Current = null;
            }

            [Theory]
            [InlineData(301)]
            [InlineData(302)]
            public void WillNotSetResponseFilterIfStatusIs302Or301(int status)
            {
                RRContainer.Current = null;
                var context = new Mock<HttpContextBase>();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
                context.Setup(x => x.Request.RawUrl).Returns("/content/blah");
                context.Setup(x => x.Response.StatusCode).Returns(status);
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                });

                ResponseFilter.InstallFilter(context.Object);

                context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillSetPhysicalPathToMappedVirtualPath()
            {
                var context = new Mock<HttpContextBase>();
                var config = new Mock<IRRConfiguration>();
                var hostingEnvironmentWrapper = new Mock<IHostingEnvironmentWrapper>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                hostingEnvironmentWrapper.Setup(x => x.MapPath("/Virtual")).Returns("physical");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Request.RawUrl).Returns("/content/blah");
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                    x.For<IHostingEnvironmentWrapper>().Use(hostingEnvironmentWrapper.Object);
                });

                ResponseFilter.InstallFilter(context.Object);

                config.VerifySet(x => x.SpritePhysicalPath = "physical", Times.Once());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillNotSetResponseFilterIfRRFilterQSIsDisabled()
            {
                var context = new Mock<HttpContextBase>();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection { { "RRFilter", "disabled" } });
                context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                });

                ResponseFilter.InstallFilter(context.Object);

                context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillNotSetResponseFilterIfPageFilterIgnoresTarget()
            {
                var context = new Mock<HttpContextBase>();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                });
                Registry.AddFilter(new PageFilter(x => x.HttpRequest.RawUrl.Contains("blah")));

                ResponseFilter.InstallFilter(context.Object);

                context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillNotSetResponseFilterIfCssAndJsProcessingIsDisabledFromConfig()
            {
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                config.Setup(x => x.CssProcessingDisabled).Returns(true);
                config.Setup(x => x.JavaScriptProcessingDisabled).Returns(true);
                var context = new Mock<HttpContextBase>();
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
                context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                });

                ResponseFilter.InstallFilter(context.Object);

                context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Never());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillSetResponseFilterIfJustJsProcessingIsDisabledFromConfig()
            {
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                config.Setup(x => x.JavaScriptProcessingDisabled).Returns(true);
                var context = new Mock<HttpContextBase>();
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
                context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                });

                ResponseFilter.InstallFilter(context.Object);

                context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillSetResponseFilterIfJustCssProcessingIsDisabledFromConfig()
            {
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                config.Setup(x => x.CssProcessingDisabled).Returns(true);
                var context = new Mock<HttpContextBase>();
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
                context.Setup(x => x.Request.RawUrl).Returns("/NotVirtual/blah");
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                    x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                });

                ResponseFilter.InstallFilter(context.Object);

                context.VerifySet(x => x.Response.Filter = It.IsAny<Stream>(), Times.Once());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillSetContextKeyIfNotSetBefore()
            {
                RRContainer.Current = null;
                var context = new Mock<HttpContextBase>();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpriteVirtualPath).Returns("/Virtual");
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Response.ContentType).Returns("type");
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.RawUrl).Returns("/content/blah");
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                });

                ResponseFilter.InstallFilter(context.Object);

                context.Verify(x => x.Items.Add(ResponseFilter.ContextKey, It.IsAny<Object>()), Times.Once());
                RRContainer.Current = null;
            }

            [Fact]
            public void WillNotSetPhysicalPathToMappedPathOfVirtualPathIfPhysicalPathIsNotEmpty()
            {
                var context = new Mock<HttpContextBase>();
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.SpritePhysicalPath).Returns("physicalPath");
                config.Setup(x => x.SpriteVirtualPath).Returns("/RRContent");
                RRContainer.Current = new Container(x =>
                {
                    x.For<IRRConfiguration>().Use(config.Object);
                    x.For<IHostingEnvironmentWrapper>().Use(new Mock<IHostingEnvironmentWrapper>().Object);
                    x.For<AbstractFilter>().Use(new Mock<AbstractFilter>().Object);
                });
                context.Setup(x => x.Items.Contains(ResponseFilter.ContextKey)).Returns(false);
                context.Setup(x => x.Request.QueryString).Returns(new NameValueCollection());
                context.Setup(x => x.Response.ContentType).Returns("text/html");
                context.Setup(x => x.Request.RawUrl).Returns("/content/blah");

                ResponseFilter.InstallFilter(context.Object);

                config.VerifySet(x => x.SpritePhysicalPath = It.IsAny<string>(), Times.Never());
                RRContainer.Current = null;
            }
        }
    }
}