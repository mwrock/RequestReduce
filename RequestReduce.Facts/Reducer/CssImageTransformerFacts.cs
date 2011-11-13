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
            public void WillParseClassesInMedia()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
@media screen {
    .LocalNavigation {    
        background-image: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png');
        background-repeat: no-repeat;
        width: 50;
    }
    .RemoteNavigation {    
        background-image: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technetr.png');
        background-repeat: no-repeat;
        width: 55;
    }
    .LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
        background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    }
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(2, result.Count());
            }

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

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

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
                var formatedCss = string.Format(css, repeat);

                var result = testable.ClassUnderTest.ExtractImageUrls(ref formatedCss, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

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

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
            }

            [Theory,
            InlineDataAttribute(0),
            InlineDataAttribute(20)]
            public void WillReturnPositivelyUnitYPositionedBackgroundImages(int y)
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    string.Format(@"
.DropDownArrow {{
    background: transparent url('http://i1.social.s-msft.com/contentservice/dcbd1ced-14f2-4c11-9ece-9d6e00f78d1c/arrow_dn_white.gif') no-repeat scroll 0 {0}px;
    width: 5px;
}}", y);

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
            }

            [Fact]
            public void WillReturnNegativelyUnitYPositionedBackgroundImagesWithHeight()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                   @"
.DropDownArrow {{
    background: transparent url('http://i1.social.s-msft.com/contentservice/dcbd1ced-14f2-4c11-9ece-9d6e00f78d1c/arrow_dn_white.gif') no-repeat scroll 0 -20px;
    width: 5px;
    height:24px;
}}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

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

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

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

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
            }

            [Fact]
            public void WillReturnRightPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat right 0px;
    width: 20;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Right, result.First().XOffset.Direction);
            }

            [Fact]
            public void WillReturnXCenterPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat center 0px;
    width: 20;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Center, result.First().XOffset.Direction);
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

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

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

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillConvertRelativeUrlsToAbsoluteForUnReturnedImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""subnav_on_technet.png"") no-repeat;
}";
                var expectedcss =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_on_technet.png"") no-repeat;
}";

                testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(expectedcss, css);
            }

            [Fact]
            public void WillNotTryToConvertUrlsOfClassesWithNoImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    style: val;
}";
                var expectedcss =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    style: val;
}";

                testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(expectedcss, css);
            }

            [Fact]
            public void WillNotReturnPreviouslyVerticallySpritedImagesWithoutHeight()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") 0 -30px;
    background-repeat: no-repeat;
    width: 50px;
}}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

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
                var sprite = new SpritedImage(1, new BackgroundImageClass(css, "http://server/content/style.css"), null) { Url = "spriteUrl", Position = 120 };

                var result = testable.ClassUnderTest.InjectSprite(css, sprite);

                Assert.Equal(expected, result);
            }

            [Fact]
            public void WillDefaultYOffsetToZero()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.localnavigation {    
    background: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png') no-repeat;
    width: 50;
}";
                var expected =
    @"
.localnavigation {    
    background: url('spriteUrl') no-repeat;
    width: 50;
;background-position: -120px 0;}";
                var sprite = new SpritedImage(1, new BackgroundImageClass(css, "http://server/content/style.css"), null) { Url = "spriteUrl", Position = 120 };


                var result = testable.ClassUnderTest.InjectSprite(css, sprite);

                Assert.Equal(expected, result);
            }

            [Fact]
            public void WillSetImageAbsoluteUrlFromBackgroundImageStyleAndReplaceRelativeUrl()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""subnav_on_technet.png"");
}";
                var expectedCss =
    @"
.localnavigation .tabon,.localnavigation .tabon:hover {
    background-image: url(""newUrl"");
;background-position: -0px 0;}";
                var testable = new TestableCssImageTransformer();
                var backgroundImage = new BackgroundImageClass(css, "http://server/content/style.css");
                var sprite = new SpritedImage(1, backgroundImage, null) { Url = "newUrl" };

                var result = testable.ClassUnderTest.InjectSprite(backgroundImage.OriginalClassString, sprite);

                Assert.Equal(expectedCss, result);
            }

            [Fact]
            public void WillAddImportanceDirectiveIfImportant()
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
;background-position: -120px 0 !important;}";
                var sprite = new SpritedImage(1, new BackgroundImageClass(css, "http://server/content/style.css") {Important = true }, null) { Url = "spriteUrl", Position = 120 };

                var result = testable.ClassUnderTest.InjectSprite(css, sprite);

                Assert.Equal(expected, result);
            }

        }
    }
}
