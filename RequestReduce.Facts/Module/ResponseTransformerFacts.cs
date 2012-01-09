using System;
using System.Web;
using Moq;
using RequestReduce.Api;
using RequestReduce.Module;
using RequestReduce.Utilities;
using Xunit;
using RequestReduce.ResourceTypes;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using StructureMap;

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
            public void WillNotTransformCssIfDisabled()
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
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                testable.Mock<IRRConfiguration>().Setup(x => x.CssProcessingDisabled).Returns(true);

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillNotTransformJsIfDisabled()
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
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />

<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                testable.Mock<IRRConfiguration>().Setup(x => x.JavaScriptProcessingDisabled).Returns(true);

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformSigleQuotedStyles()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href='http://server/Me.css' rel='Stylesheet' type='text/css' />
<link href='http://server/Me2.css' rel='Stylesheet' type='text/css' />
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformAtFirstStyleIfNoScripts()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformAtFirstStyleIfBeforeScripts()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<script srd=""src.js"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
<script srd=""src.js"" />

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformAtFirstScriptIfBeforeStyles()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script srd=""src.js"" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" /><script srd=""src.js"" />


<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformUrlsInBothSingleAndDoubleQuotes()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<script src=""http://server/Me.js"" type=""text/javascript"" ></script><script src='http://server/Me2.js' type=""text/javascript"" ></script>";
                var transformed = @"<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformScriptsWithLineBreaksBetweenCloseAndEndTags()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<script src=""http://server/Me.js"" type=""text/javascript"" >

</script>";
                var transformed = @"<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::")).Returns("http://server/Me3.js");
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
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformScriptsInBody()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<script src=""http://server/Me.js"" type=""text/javascript"" ></script>";
                var transformed = @"<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillTransformEmptySignatureGuidToEmptyTag()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<script src=""http://server/Me.js"" type=""text/javascript"" ></script>";
                var transformed = string.Empty;
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::")).Returns(string.Format("http://host/vpath/{0}-{1}-{2}", Guid.NewGuid().RemoveDashes(), Guid.Empty.RemoveDashes(), new JavaScriptResource().FileName));
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                testable.Mock<IUriBuilder>().Setup(x => x.ParseSignature(It.IsAny<string>())).Returns(Guid.Empty.RemoveDashes());

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillMergeAjacenScriptsInBody()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>";
                var transformed = @"<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>
";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillIgnoreInlineScriptsInBody()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<script src=""http://server/Me.js"" type=""text/javascript"" ></script><script type=""text/javascript"" >document.write(""Hello"");</script>
<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>";
                var transformed = @"<script src=""http://server/Me4.js"" type=""text/javascript"" ></script><script type=""text/javascript"" >document.write(""Hello"");</script>
<script src=""http://server/Me5.js"" type=""text/javascript"" ></script>";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::")).Returns("http://server/Me4.js");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me3.js::")).Returns("http://server/Me5.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillNotMergeScriptsIntermingledWitIgnoredScriptsInorderToMaintainScriptOrder()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<script src=""http://server/Me.js"" type=""text/javascript"" ></script><script src=""http://server/Me2.js"" type=""text/javascript"" ></script><script src=""http://server/ignore/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me3.js"" type=""text/javascript"" ></script><script src=""http://server/Me4.js"" type=""text/javascript"" ></script>";
                var transformed = @"<script src=""http://server/Me5.js"" type=""text/javascript"" ></script><script src=""http://server/ignore/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me6.js"" type=""text/javascript"" ></script>";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me5.js");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me3.js::http://server/Me4.js::")).Returns("http://server/Me6.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.JavaScriptUrlsToIgnore).Returns("server/Ignore, server/alsoignore");
                RRContainer.Current = new Container(x => x.For<IRRConfiguration>().Use(config.Object));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
                RRContainer.Current = null;
            }

            [Fact]
            public void WillNotTransformScriptsContainingIgnoredUrls()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<script src=""http://server/ignore/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/alsoignore/Me.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>

<script src=""http://server/ignore/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/alsoignore/Me.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.JavaScriptUrlsToIgnore).Returns("server/Ignore, server/alsoignore");
                RRContainer.Current = new Container(x =>x.For<IRRConfiguration>().Use(config.Object));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
                RRContainer.Current = null;
            }

            [Fact]
            public void WillNotTransformScriptsIgnoredByFilter()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
<script src=""http://server/ignore/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/alsoignore/Me.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>

<script src=""http://server/ignore/Me.js"" type=""text/javascript"" ></script>
<script src=""http://server/alsoignore/Me.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                Registry.AddFilter(new JavascriptFilter(x => x.FilteredUrl.Contains("ignore")));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
                RRContainer.Current = null;
            }

            [Fact]
            public void WillNotTransformCssIgnoredByFilter()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" /></head>
                ";
                var transformed = @"<head id=""Head1"">
<link href=""http://server/Me4.css"" rel=""Stylesheet"" type=""text/css"" />

<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" /></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me4.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                Registry.AddFilter(new CssFilter(x => x.FilteredUrl.Contains("3")));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
                RRContainer.Current = null;
            }

            [Fact]
            public void WillTransformAllScriptsIfJsTagValidatorIsNull()
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
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />

<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                var jsResource = RRContainer.Current.GetInstance<JavaScriptResource>();
                jsResource.TagValidator = null;

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
                RRContainer.Current = null;
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
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />

<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>

<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me2.js::")).Returns("http://server/Me3.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillNotTransformIEConditionalCss()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head id=""Head1"">
<meta name=""description"" content="""" />
    <!--[if IE 6]>
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
    <![endif]-->
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                var transformed = @"<head id=""Head1"">
<meta name=""description"" content="""" />
    <!--[if IE 6]>
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
    <![endif]-->
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me2.css::")).Returns("http://server/Me3.css");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
            }

            [Fact]
            public void WillNotTransformIEConditionalJs()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"
<script src=""http://server/Me.js"" type=""text/javascript"" ></script>
    <!--[if IE 6]>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
    <![endif]-->
<script src=""http://server/Me3.js"" type=""text/javascript"" ></script>
";
                var transformed = @"
<script src=""http://server/Me4.js"" type=""text/javascript"" ></script>
    <!--[if IE 6]>
<script src=""http://server/Me2.js"" type=""text/javascript"" ></script>
    <![endif]-->

";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.js::http://server/Me3.js::")).Returns("http://server/Me4.js");
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
                var transformed = @"<head>
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />

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

                testable.Mock<IReducingQueue>().Verify(x => x.Enqueue(It.Is<QueueItem<CssResource>>(y => y.Urls == "http://server/Me.css::http://server/Me2.css::")), Times.Once());
            }

            [Fact]
            public void WillQueueCssUrlsAppendingMediaIfRepoReturnsNull()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head>
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" media=""print,screen""/>
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(
                    x => x.FindReduction("http://server/Me.css|print,screen::http://server/Me2.css::"));
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));

                testable.ClassUnderTest.Transform(transform);

                testable.Mock<IReducingQueue>().Verify(x => x.Enqueue(It.Is<QueueItem<CssResource>>(y => y.Urls == "http://server/Me.css|print,screen::http://server/Me2.css::")), Times.Once());
            }

            [Fact]
            public void WillTransformHeadWithDuplicateScripts()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head>
<script src=""http://server/script1.js"" type=""text/javascript"" ></script>
<script src=""http://server/script2.js"" type=""text/javascript"" ></script>
<script src=""http://server/script3.js"" type=""text/javascript"" ></script>
<script src=""http://server/script1.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                var transformed = @"<head>
<script src=""http://server/script4.js"" type=""text/javascript"" ></script>

<script src=""http://server/script3.js"" type=""text/javascript"" ></script>
<script src=""http://server/script5.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/script1.js::http://server/script2.js::")).Returns("http://server/script4.js");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/script1.js::")).Returns("http://server/script5.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.JavaScriptUrlsToIgnore).Returns("server/script3.js");
                RRContainer.Current = new Container(x => x.For<IRRConfiguration>().Use(config.Object));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
                RRContainer.Current = null;
            }

            [Fact]
            public void WillTransformHeadWithDuplicateScriptsAndInlineScript()
            {
                var testable = new TestableResponseTransformer();
                var transform = @"<head>
<script src=""http://server/script1.js"" type=""text/javascript"" ></script>
<script src=""http://server/script2.js"" type=""text/javascript"" ></script>
<script type=""text/javascript"" >here is some sphisticated javascript</script>
<script src=""http://server/script1.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                var transformed = @"<head>
<script src=""http://server/script4.js"" type=""text/javascript"" ></script>

<script type=""text/javascript"" >here is some sphisticated javascript</script>
<script src=""http://server/script5.js"" type=""text/javascript"" ></script>
<title>site</title></head>
                ";
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/script1.js::http://server/script2.js::")).Returns("http://server/script4.js");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/script1.js::")).Returns("http://server/script5.js");
                testable.Mock<HttpContextBase>().Setup(x => x.Request.Url).Returns(new Uri("http://server/megah"));
                var config = new Mock<IRRConfiguration>();
                config.Setup(x => x.JavaScriptUrlsToIgnore).Returns("server/script3.js");
                RRContainer.Current = new Container(x => x.For<IRRConfiguration>().Use(config.Object));

                var result = testable.ClassUnderTest.Transform(transform);

                Assert.Equal(transformed, result);
                RRContainer.Current = null;
            }

        }
    }
}
