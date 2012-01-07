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
            public void WillParseClassesWithEmptyComments()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
* html .RadInput a.riDown
{
	margin-top /**/:0;
}

/*label*/

.RadInput .riLabel
{
	margin:0 4px 0 0;
	white-space:nowrap;
}
";

                var expected =
    @"
* html .RadInput a.riDown
{
	margin-top :0;
}



.RadInput .riLabel
{
	margin:0 4px 0 0;
	white-space:nowrap;
}
";

                testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(expected, css);
            }

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
    width: 20px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Right, result.First().XOffset.Direction);
            }

            [Fact]
            public void WillReturnBottomPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat right bottom;
    width: 20px;
    height: 20px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Bottom, result.First().YOffset.Direction);
            }

            [Fact]
            public void WillNotReturnBottomPositionedBackgroundImagesWithNoHeight()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat right bottom;
    width: 20px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillReturnXCenterPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat center 0px;
    width: 20px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Center, result.First().XOffset.Direction);
                Assert.Equal(PositionMode.Direction, result.First().XOffset.PositionMode);
            }

            [Fact]
            public void WillReturnYCenterPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 0px center;
    width: 20px;
    height: 20px
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Center, result.First().YOffset.Direction);
                Assert.Equal(PositionMode.Direction, result.First().YOffset.PositionMode);
            }

            [Fact]
            public void WillNotReturnYCenterPositionedBackgroundImagesWithNoHeight()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 0px center;
    width: 20px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(0, result.Count());
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
            public void WillConvertRelativeUrlsToAbsoluteForFontFaces()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
@font-face
{
font-family: myFirstFont;
src: url('Sansation_Light.ttf'),
     url('Sansation_Light.eot');
}";
                var expectedcss =
                    @"
@font-face
{
font-family: myFirstFont;
src: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/Sansation_Light.ttf'),
     url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/Sansation_Light.eot');
}";

                testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(expectedcss, css);
            }

            [Fact]
            public void WillNotAlterDataURIs()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
@font-face
{
font-family: myFirstFont;
src: url('Sansation_Light.ttf'),
     url(data:font/woff;charset=utf-8;base64,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);
}";
                var expectedcss =
                    @"
@font-face
{
font-family: myFirstFont;
src: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/Sansation_Light.ttf'),
     url(data:font/woff;charset=utf-8;base64,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);
}";

                testable.ClassUnderTest.ExtractImageUrls(ref css, "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/style.css");

                Assert.Equal(expectedcss, css);
            }

            [Fact]
            public void WillConvertRelativeUrlsToAbsoluteForUnReturnedImagesWhenBracesInComments()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover { border: 1px solid #aaaaaa/*{borderColorContent}*/; background: #ffffff/*{bgColorContent}*/ url(images/ui-bg_flat_75_ffffff_40x100.png)/*{bgImgUrlContent}*/ 50%/*{bgContentXPos}*/ 50%/*{bgContentYPos}*/ repeat-x/*{bgContentRepeat}*/; color: #222222/*{fcContent}*/; }";
                var expectedcss =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover { border: 1px solid #aaaaaa; background: #ffffff url(http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/images/ui-bg_flat_75_ffffff_40x100.png) 50% 50% repeat-x; color: #222222; }";

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
