using RequestReduce.Reducer;
using System.Linq;
using Xunit;

namespace RequestReduce.Facts.Reducer
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

            [Fact]
            public void WillNotReturnNegativelyPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") norepeat 150px -10px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillNotReturnZeroPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") norepeat 0px 0px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillNotReturnExplicitelyZeroPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-position: 0px 0px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillNotReturnExplicitelyNegativePositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-position: -10px 5px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillNotReturnExplicitelyrepeatingBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-repeat: repeat-x;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillReturnExplicitelynonrepeatingBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-repeat: no-repeat;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
            }
        }

        public class InjectSprite
        {
            [Fact]
            public void WillReplaceFormerUrlWithSpriteUrl()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation {    
    background-image: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png');
}";
                var expected =
    @"
.LocalNavigation {    
    background-image: url('spriteUrl');
}";
                var sprite = new Sprite(120, "spriteUrl");

                var result = testable.ClassUnderTest.InjectSprite(css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png", sprite);

                Assert.Equal(expected, result);
            }
        }
    }
}
