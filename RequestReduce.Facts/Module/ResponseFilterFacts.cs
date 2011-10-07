using System.IO;
using System.Text;
using Moq;
using RequestReduce.Module;
using Xunit;

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

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 30);

                Assert.Equal(@"<head id=""Head1"">thead</head>", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformWhenBufferEndsBetweenTwoFilteredTags()
            {
                var testable = new TestableResponseFilter(Encoding.UTF8);
                var testBuffer = @"<head id=""Head1"">head</head><script src=""abc""></script>end";
                var testTransform1 = @"<head id=""Head1"">head</head>";
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(testTransform1)).Returns(@"<head id=""Head1"">thead</head>");

                testable.ClassUnderTest.Write(Encoding.UTF8.GetBytes(testBuffer), 0, 30);

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
            public void WillTransformAdjacntScritTagsInBodyTags()
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

        }
    }
}