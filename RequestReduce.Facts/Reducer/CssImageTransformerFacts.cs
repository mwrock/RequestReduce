using RequestReduce.Reducer;
using System.Linq;
using Xunit;
using Xunit.Extensions;

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
            public void WillReturnBackgroundImagesWithWidth()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation {    
    background-image: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png');
    background-repeat: no-repeat;
    width: 50;
}

.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
                Assert.True(result.Any(x => x.ImageUrl == "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png"));
            }

            [Theory,
            InlineDataAttribute("repeat"),
            InlineDataAttribute("x-repeat"),
            InlineDataAttribute("y-repeat")]
            public void WillNotReturnReapeatingBackgroundImages(string repeat)
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") {0};
    width: 20px;
}}";

                var result = testable.ClassUnderTest.ExtractImageUrls(string.Format(css, repeat));

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillReturnNegativelyXPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat -150px 0px;
    width: 20;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
            }

            [Theory,
            InlineDataAttribute(-20),
            InlineDataAttribute(0),
            InlineDataAttribute(20)]
            public void WillReturnUnitYPositionedBackgroundImages(int y)
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    string.Format(@"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat -150px {0}px;
    width: 20;
}}", y);

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
            }

            [Theory,
            InlineDataAttribute("10%"),
            InlineDataAttribute("center"),
            InlineDataAttribute("bottom")]
            public void WillNotReturnYPositionedBackgroundImagesNotTopPositionedAndOfPercentOrDirection(string y)
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    string.Format(@"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat -150px {0};
    width: 20;
}}", y);

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillReturnZeroPositionedXBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 0px 0px;
    width: 20;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
            }

            [Fact]
            public void WillNotReturnpositivelywidthedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-position: 10px 0px;
    width:20;
    background-repeat: no-repeat;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillNotReturnBackgroundImagesWithoutWidth()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillNotReturnBackgroundImagesWithWidthOfCenterOrRight()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") center;
    background-repeat: no-repeat;
    width: 50;
}

.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") right;
    background-repeat: no-repeat;
    width: 50;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }
        }

        public class InjectSprite
        {
            [Fact]
            public void WillReplaceFormerUrlWithSpriteUrlAndPositionOffset()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.localnavigation {    
    background: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png') no-repeat 0 -30px;
    width: 50;
}";
                var expected =
    @"
.localnavigation {    
    background: url('spriteUrl') no-repeat 0 -30px;
    width: 50;
;background-position: -120px 0;}";
                var sprite = new Sprite(120, "spriteUrl");


                var result = testable.ClassUnderTest.InjectSprite(css, new BackgroungImageClass(css), sprite);

                Assert.Equal(expected, result);
            }
        }
    }
}
