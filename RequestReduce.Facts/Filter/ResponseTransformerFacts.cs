using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.Filter;
using Xunit;

namespace RequestReduce.Facts.Filter
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
                var transformBytes = Encoding.UTF8.GetBytes(@"
<meta name=""description"" content="""" />
<link href=""http://server/Me.css"" rel=""Stylesheet"" type=""text/css"" />
<link href=""http://server/Me2.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ");
                var transformedBytes = Encoding.UTF8.GetBytes(@"
<meta name=""description"" content="""" />
<link href=""http://server/Me3.css"" rel=""Stylesheet"" type=""text/css"" />
<title>site</title></head>
                ");
                testable.Mock<IReductionRepository>().Setup(x => x.FindReduction("http://server/Me.css::http://server/Me2.css")).Returns("http://server/Me3.css");

                var result = testable.ClassUnderTest.Transform(transformBytes, Encoding.UTF8);

                Assert.Equal(transformedBytes, result);
            }
        }
    }
}
