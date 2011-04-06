using RequestReduce.Reducer;
using System.Linq;
using Xunit;

namespace RequestReduce.Facts.Filter
{
    public class CssImageTransformerFacts
    {
        class TestableCssImageTransformer : Testable<CssImageTransformer>
        {
            public TestableCssImageTransformer()
            {
                
            }
        }

        public class ExtractImageUrls
        {
            [Fact]
            public void WillReturnBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation {    
    background-image: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png');
}

.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(2, result.Count());
                Assert.True(result.Contains("http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png"));
                Assert.True(result.Contains("http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"));
            }

            [Fact]
            public void WillNotReturnReapeatingBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") repeat;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

        }
    }
}
