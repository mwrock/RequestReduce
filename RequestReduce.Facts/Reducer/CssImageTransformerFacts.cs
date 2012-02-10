using Moq;
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

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

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
    width: 50px;
}

.TabOn {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
                Assert.True(result.Any(x => x.ImageUrl == "http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png"));
            }

            [Fact]
            public void WillPassCorrectOrderToBackgroundImage()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation {    
    background-image: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png');
    background-repeat: no-repeat;
    width: 50;
}

.TabOn {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-repeat: no-repeat;
    width: 50;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.First(x => x.Selector == ".LocalNavigation").ClassOrder);
                Assert.Equal(2, result.First(x => x.Selector == ".TabOn").ClassOrder);
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

                var result = testable.ClassUnderTest.ExtractImageUrls(formatedCss);

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

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

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

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
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
            public void WillReturnRightPositionedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat right 0px;
    width: 20px;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

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

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Bottom, result.First().YOffset.Direction);
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

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

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

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
                Assert.Equal(Direction.Center, result.First().YOffset.Direction);
                Assert.Equal(PositionMode.Direction, result.First().YOffset.PositionMode);
            }

            [Fact]
            public void WillReturnpositivelywidthedBackgroundImages()
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

                Assert.Equal(1, result.Count());
            }

            [Fact]
            public void WillReturnpositivelyHeightBackgroundImagesWithHeight()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-position: 0px 3px;
    width:20px;
    width:10px;
    background-repeat: no-repeat;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
            }

            [Fact]
            public void WillReturnPercentagewidthedBackgroundImages()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-position: 10% 0px;
    width:20;
    background-repeat: no-repeat;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
            }

            [Fact]
            public void WillReturnPercentageHeightBackgroundImagesWithHeight()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
    background-position: 0 10%;
    width:20px;
    height:30px;
    background-repeat: no-repeat;
}";

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(1, result.Count());
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

                testable.ClassUnderTest.ExtractImageUrls(css);

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

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(0, result.Count());
            }

            [Fact]
            public void WillNotAnalyzeSelectorsThatHaveNothingOfInterest()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.h1  {{color: blue;}}
.back  {{background-position: 10px 10px;}}";

                testable.ClassUnderTest.ExtractImageUrls(css);

                testable.Mock<ICssSelectorAnalyzer>().Verify(x => x.IsInScopeOfTarget(It.IsAny<string>(), ".h1"), Times.Never());
            }

            [Fact]
            public void WillNotAnalyzeSelectorsThatAreComplete()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.h1  {{width: 10px;}}
.LocalNavigation {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 0px center;
    width: 20px;
    height: 20px;
    padding: 0 0 0 0;
}";

                testable.ClassUnderTest.ExtractImageUrls(css);

                testable.Mock<ICssSelectorAnalyzer>().Verify(x => x.IsInScopeOfTarget(".LocalNavigation", It.IsAny<string>()), Times.Never());
            }

            [Theory]
            [InlineDataAttribute(@"background-image: url(""image.png"");")]
            [InlineDataAttribute("background-position: left;")]
            [InlineDataAttribute("background-position: top;")]
            public void WillAnalyzeSelectorsThatAreInCompleteAnHaveAndImageWithAnotherSelectorThatHasAnImageOrOffet(string propertyOfInterest)
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.h1  {{{0}}}
.LocalNavigation {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}}";

                testable.ClassUnderTest.ExtractImageUrls(string.Format(css, propertyOfInterest));

                testable.Mock<ICssSelectorAnalyzer>().Verify(x => x.IsInScopeOfTarget(".LocalNavigation", It.IsAny<string>()), Times.Once());
            }

            [Theory]
            [InlineDataAttribute(@"background-image: url(""image.png"");")]
            [InlineDataAttribute("background-position: left;")]
            [InlineDataAttribute("background-position: top;")]
            public void WillAnalyzeSelectorsThatAreInCompleteAndHaveAnXOffsetWithAnotherSelectorThatHasImageOrOffset(string propertyOfInterest)
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.h1  {{{0}}}
.LocalNavigation {{
    background-position: left;
}}";

                testable.ClassUnderTest.ExtractImageUrls(string.Format(css, propertyOfInterest));

                testable.Mock<ICssSelectorAnalyzer>().Verify(x => x.IsInScopeOfTarget(".LocalNavigation", It.IsAny<string>()), Times.Once());
            }

            [Theory]
            [InlineDataAttribute(@"background-image: url(""image.png"");")]
            [InlineDataAttribute("background-position: left;")]
            [InlineDataAttribute("background-position: top;")]
            public void WillAnalyzeSelectorsThatAreInCompleteAnHaveAYOffsetWithAnotherSelectorThatHasAnImageOrOffset(string propertyOfInterest)
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.h1  {{{0}}}
.LocalNavigation {{
    background-position: top;
}}";

                testable.ClassUnderTest.ExtractImageUrls(string.Format(css, propertyOfInterest));

                testable.Mock<ICssSelectorAnalyzer>().Verify(x => x.IsInScopeOfTarget(".LocalNavigation", It.IsAny<string>()), Times.Once());
            }

            [Fact]
            public void WillAddPropertiesToIncompleteWithoutPositionClassComplete()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
h1 {{
    background: url(""image"") no-repeat -10px -30px;
    width: 20px;
    height: 20px;
    padding: 40 50 60 70;
}}
h1.LocalNavigation {{
    background-image: url(""image2"");
}}";
                testable.Mock<ICssSelectorAnalyzer>().Setup(x => x.IsInScopeOfTarget("h1.LocalNavigation", "h1")).Returns(true);

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(2, result.Count());
                var image = result.FirstOrDefault(x => x.Selector == "h1.LocalNavigation");
                Assert.NotNull(image);
                Assert.Equal(image.ExplicitWidth, 20);
                Assert.Equal(image.ExplicitHeight, 20);
                Assert.Equal(image.Repeat, RepeatStyle.NoRepeat);
                Assert.Equal(image.XOffset.Offset, -10);
                Assert.Equal(image.YOffset.Offset, -30);
                Assert.Equal(image.PaddingTop, 40);
                Assert.Equal(image.PaddingRight, 50);
                Assert.Equal(image.PaddingBottom, 60);
                Assert.Equal(image.PaddingLeft, 70);
            }

            [Fact]
            public void WillAddPropertiesToIncompleteWithoutImageClassComplete()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
h1 {{
    background: url(""image"") no-repeat -10px -30px;
    width: 20px;
    height: 20px;
    padding: 40 50 60 70;
}}
h1.LocalNavigation {{
    background-position: -10px -30px;
}}";
                testable.Mock<ICssSelectorAnalyzer>().Setup(x => x.IsInScopeOfTarget("h1.LocalNavigation", "h1")).Returns(true);

                var result = testable.ClassUnderTest.ExtractImageUrls(css);

                Assert.Equal(2, result.Count());
                var image = result.FirstOrDefault(x => x.Selector == "h1.LocalNavigation");
                Assert.NotNull(image);
                Assert.Equal(image.ExplicitWidth, 20);
                Assert.Equal(image.ExplicitHeight, 20);
                Assert.Equal(image.Repeat, RepeatStyle.NoRepeat);
                Assert.Equal(image.XOffset.Offset, -10);
                Assert.Equal(image.YOffset.Offset, -30);
                Assert.Equal(image.PaddingTop, 40);
                Assert.Equal(image.PaddingRight, 50);
                Assert.Equal(image.PaddingBottom, 60);
                Assert.Equal(image.PaddingLeft, 70);
                Assert.Equal(image.ImageUrl, "image");
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
                var sprite = new SpritedImage(1, new BackgroundImageClass(css, 0), null) { Url = "spriteUrl", Position = 120 };

                var result = testable.ClassUnderTest.InjectSprite(css, sprite);

                Assert.Equal(expected, result);
            }

            [Fact]
            public void WillNotReplaceClassWithSameBodyAndDifferentSelector()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.localnavigation {    
    background: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png') no-repeat 0 -30px;
    width: 50;
}

.localnavigation2 {    
    background: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png') no-repeat 0 -30px;
    width: 50;
}";
                var imageCss = @".localnavigation {    
    background: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png') no-repeat 0 -30px;
    width: 50;
}";
                var expected =
    @"
.localnavigation {    
    background: url('spriteUrl') no-repeat 0 -30px;
    width: 50;
;background-position: -120px 0;}

.localnavigation2 {    
    background: url('http://i1.social.microsoft.com/contentservice/798d3f43-7d1e-41a1-9b09-9dad00d8a996/subnav_technet.png') no-repeat 0 -30px;
    width: 50;
}";
                var sprite = new SpritedImage(1, new BackgroundImageClass(imageCss, 0), null) { Url = "spriteUrl", Position = 120 };

                var result = testable.ClassUnderTest.InjectSprite(css, sprite);

                Assert.Equal(expected, result);
            }

            [Fact]
            public void WillAddUrlWithSpriteUrlAndIfItIsNotInCss()
            {
                var testable = new TestableCssImageTransformer();
                var css =
                    @"
.Localnavigation {    
    background-position: 0 -30px;
    width: 50;
}";
                var expected =
    @"
.Localnavigation {    
    background-position: 0 -30px;
    width: 50;
;background-image: url('spriteUrl');background-position: -120px 0;}";
                var sprite = new SpritedImage(1, new BackgroundImageClass(css, 0) { ImageUrl = "nonRRsprite"}, null) { Url = "spriteUrl", Position = 120 };

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
                var sprite = new SpritedImage(1, new BackgroundImageClass(css, 0), null) { Url = "spriteUrl", Position = 120 };


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
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""newUrl"");
;background-position: -0px 0;}";
                var testable = new TestableCssImageTransformer();
                var backgroundImage = new BackgroundImageClass(css, 0);
                var sprite = new SpritedImage(1, backgroundImage, null) { Url = "newUrl", Position = 0};

                var result = testable.ClassUnderTest.InjectSprite(css, sprite);

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
                var sprite = new SpritedImage(1, new BackgroundImageClass(css, 0) {Important = true }, null) { Url = "spriteUrl", Position = 120 };

                var result = testable.ClassUnderTest.InjectSprite(css, sprite);

                Assert.Equal(expected, result);
            }

        }
    }
}
