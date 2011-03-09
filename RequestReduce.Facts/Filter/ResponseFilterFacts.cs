using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
using RequestReduce.Filter;
using Xunit;

namespace RequestReduce.Facts.Filter
{
    public class ResponseFilterFacts
    {
        private class TestableResponseFilter : Testable<ResponseFilter>
        {
            public TestableResponseFilter()
            {
                Inject(Encoding.UTF8);
                Mock<Stream>().Setup(x => x.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).
                    Callback<byte[], int, int>((buf, off, len) =>
                    {
                        FilteredResult += Encoding.UTF8.GetString(buf, off, len);
                    });

            }

            public string FilteredResult { get; set; }

            public bool ByteArrayMatch(byte[] a, byte[] b)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i])
                        return false;
                }
                return true;
            }
        }

        public class Write
        {
            [Fact]
            public void WillTransformHeadInSingleWrite()
            {
                var testable = new TestableResponseFilter();
                var testBuffer = Encoding.UTF8.GetBytes("before<head>head</head>after");
                var testTransform = Encoding.UTF8.GetBytes("head</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(Match<byte[]>.Create(b => testable.ByteArrayMatch(b, testTransform)), Encoding.UTF8)).Returns(Encoding.UTF8.GetBytes("thead</head>"));

                testable.ClassUnderTest.Write(testBuffer, 0, testBuffer.Length);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadAtBeginningOfResponse()
            {
                var testable = new TestableResponseFilter();
                var testBuffer = Encoding.UTF8.GetBytes("<head>head</head>after");
                var testTransform = Encoding.UTF8.GetBytes("head</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(Match<byte[]>.Create(b => testable.ByteArrayMatch(b, testTransform)), Encoding.UTF8)).Returns(Encoding.UTF8.GetBytes("thead</head>"));

                testable.ClassUnderTest.Write(testBuffer, 0, testBuffer.Length);

                Assert.Equal("<head>thead</head>after", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadAtEndOfResponse()
            {
                var testable = new TestableResponseFilter();
                var testBuffer = Encoding.UTF8.GetBytes("before<head>head</head>");
                var testTransform = Encoding.UTF8.GetBytes("head</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(Match<byte[]>.Create(b => testable.ByteArrayMatch(b, testTransform)), Encoding.UTF8)).Returns(Encoding.UTF8.GetBytes("thead</head>"));

                testable.ClassUnderTest.Write(testBuffer, 0, testBuffer.Length);

                Assert.Equal("before<head>thead</head>", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadWhenAllResponse()
            {
                var testable = new TestableResponseFilter();
                var testBuffer = Encoding.UTF8.GetBytes("<head>head</head>");
                var testTransform = Encoding.UTF8.GetBytes("head</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(Match<byte[]>.Create(b => testable.ByteArrayMatch(b, testTransform)), Encoding.UTF8)).Returns(Encoding.UTF8.GetBytes("thead</head>"));

                testable.ClassUnderTest.Write(testBuffer, 0, testBuffer.Length);

                Assert.Equal("<head>thead</head>", testable.FilteredResult);
            }

            [Fact]
            public void WillTransformHeadInmultipleWrites()
            {
                var testable = new TestableResponseFilter();
                var testBuffer = Encoding.UTF8.GetBytes("before<head>head</head>after");
                var testTransform = Encoding.UTF8.GetBytes("head</head>");
                testable.Mock<IResponseTransformer>().Setup(x => x.Transform(Match<byte[]>.Create(b => testable.ByteArrayMatch(b, testTransform)), Encoding.UTF8)).Returns(Encoding.UTF8.GetBytes("thead</head>"));

                testable.ClassUnderTest.Write(testBuffer, 0, 9);
                testable.ClassUnderTest.Write(testBuffer, 9, testBuffer.Length-9);

                Assert.Equal("before<head>thead</head>after", testable.FilteredResult);
            }


        }
    }
}