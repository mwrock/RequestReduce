using RequestReduce.Reducer;
using Xunit;
using Xunit.Extensions;

namespace RequestReduce.Facts.Reducer
{
    public class BackgroundImageClassFacts
    {
        public class Ctor
        {
            [Fact]
            public void WillSetOriginalClassStringToPassedString()
            {
                var testable = new BackgroundImageClass("original string", "http://server/content/style.css");

                Assert.Equal("original string", testable.OriginalClassString);
            }

            [Fact]
            public void WillSetImageUrlFromShortcutStyle()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: #fff url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") repeat;
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal("http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png", testable.ImageUrl);
            }

            [Fact]
            public void WillSetImageUrlFromBackgroundImageStyle()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal("http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png", testable.ImageUrl);
            }

            [Fact]
            public void WillSetImageAbsoluteUrlFromBackgroundImageStyle()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""subnav_on_technet.png"");
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal("http://server/content/subnav_on_technet.png", testable.ImageUrl);
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
                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                var result = testable.Render(new Sprite(0, 1) { Url = "newUrl" });

                Assert.Equal(expectedCss, result);
            }

            [Fact]
            public void WillSetRepeatIfARepeatDoesNotExist()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(RepeatStyle.Repeat, testable.Repeat);
            }

            [Fact]
            public void WillSetRepeatIfRepeatIsSpecifiedLast()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-repeat: repeat;
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(RepeatStyle.Repeat, testable.Repeat);
            }

            [Fact]
            public void WillSetNoRepeatIfNoRepeatIsSpecifiedLast()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") repeat;
    background-repeat: no-repeat;
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(RepeatStyle.NoRepeat, testable.Repeat);
            }

            [Fact]
            public void WillSetXRepeatIfXRepeatIsSpecifiedLast()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-repeat: x-repeat;
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(RepeatStyle.XRepeat, testable.Repeat);
            }

            [Fact]
            public void WillSetYRepeatIfYRepeatIsSpecifiedLast()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-repeat: y-repeat;
}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(RepeatStyle.YRepeat, testable.Repeat);
            }

            [Theory,
            InlineData("50px", 50),
            InlineData("50%", null),
            InlineData("50em", null)]
            public void WillSetWidthFromWidth(string statedWidth, int? expectedWidth)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    width: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), "http://server/content/style.css");

                Assert.Equal(expectedWidth, testable.Width);
            }

            [Theory,
            InlineData("50", 70),
            InlineData("50px", 70),
            InlineData("50%", 30),
            InlineData("50em", null)]
            public void WillAddLeftPaddingToWidth(string statedWidth, int? expectedWidth)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    width: 20px;
    padding-left: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), "http://server/content/style.css");

                Assert.Equal(expectedWidth, testable.Width);
            }

            [Theory,
            InlineData("50", 70),
            InlineData("50px", 70),
            InlineData("50%", 30),
            InlineData("50em", null)]
            public void WillAddRightPaddingToWidth(string statedWidth, int? expectedWidth)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    width: 20px;
    padding-right: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), "http://server/content/style.css");

                Assert.Equal(expectedWidth, testable.Width);
            }

            [Theory,
            InlineData("50", 70),
            InlineData("50px", 70),
            InlineData("50%", 30),
            InlineData("50em", null)]
            public void WillAddTopPaddingToHeight(string statedHeight, int? expectedHeight)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: 20px;
    padding-top: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), "http://server/content/style.css");

                Assert.Equal(expectedHeight, testable.Height);
            }

            [Theory,
            InlineData("50", 70),
            InlineData("50px", 70),
            InlineData("50%", 30),
            InlineData("50em", null)]
            public void WillAddBottomPaddingToHeight(string statedHeight, int? expectedHeight)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: 20px;
    padding-bottom: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), "http://server/content/style.css");

                Assert.Equal(expectedHeight, testable.Height);
            }

            [Theory,
            InlineData("10 20 30 40", 80, 60),
            InlineData("10px 20px 30px 40px", 80, 60),
            InlineData("10px 20px 30px", 60, 60),
            InlineData("10px 20px", 60, 40),
            InlineData("10px", 40, 40)]
            public void WillAddShortcutPaddingToWidthAndHeight(string statedPadding, int? expectedWidth, int? expectedHeight)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: 20px;
    width: 20px;
    padding: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedPadding), "http://server/content/style.css");

                Assert.Equal(expectedWidth, testable.Width);
                Assert.Equal(expectedHeight, testable.Height);
            }

            [Fact]
            public void WillNotAddShortcutPaddingToWidthIfNoWidthSpecified()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: 20px;
    padding-left: 10px;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Null(testable.Width);
            }

            [Fact]
            public void WillNotAddShortcutPaddingToHeightIfNoHeightSpecified()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    padding-top: 10px;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Null(testable.Height);
            }

            [Fact]
            public void WillAddLastLeftPaddingToWidth()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    width: 20px;
    padding-left: 10px;
    padding: 40px;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(100, testable.Width);
            }

            [Fact]
            public void WillAddLastRightPaddingToWidth()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    width: 20px;
    padding: 40px;
    padding-right: 10px;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(70, testable.Width);
            }

            [Fact]
            public void WillAddLastTopPaddingToHeight()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: 20px;
    padding: 40px;
    padding-top: 10px;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(70, testable.Height);
            }

            [Theory,
            InlineData("50", 50),
            InlineData("50px", 50),
            InlineData("50%", null),
            InlineData("50em", null)]
            public void WillSetWidthFromMaxWidth(string statedWidth, int? expectedWidth)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    max-width: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), "http://server/content/style.css");

                Assert.Equal(expectedWidth, testable.Width);
            }

            [Theory,
            InlineData("50", 50),
            InlineData("50px", 50),
            InlineData("50%", null),
            InlineData("50em", null)]
            public void WillSetHeightFromHeight(string statedHeight, int? expectedHeight)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), "http://server/content/style.css");

                Assert.Equal(expectedHeight, testable.Height);
            }

            [Theory,
            InlineData("50", 50),
            InlineData("50px", 50),
            InlineData("50%", null),
            InlineData("50em", null)]
            public void WillSetHeightFromMaxHeight(string statedHeight, int? expectedHeight)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    max-height: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), "http://server/content/style.css");

                Assert.Equal(expectedHeight, testable.Height);
            }

            [Fact]
            public void OffsetsWillDefaultTo0Percent()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(new Position {PositionMode = PositionMode.Percent, Offset = 0}, testable.XOffset);
            }

            [Fact]
            public void ShortcutOffsetsWillBePxIfNotTrailingWithValidUnitOrPercent()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 0 0;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(new Position { PositionMode = PositionMode.Unit, Offset = 0 }, testable.XOffset);
                Assert.Equal(new Position { PositionMode = PositionMode.Unit, Offset = 0 }, testable.YOffset);
            }

            [Fact]
            public void OffsetsWillBePxIfNotTrailingWithValidUnitOrPercent()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: 0 0;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(new Position { PositionMode = PositionMode.Unit, Offset = 0 }, testable.XOffset);
                Assert.Equal(new Position { PositionMode = PositionMode.Unit, Offset = 0 }, testable.YOffset);
            }

            [Fact]
            public void WillSetOffsetsFromLastOffsets()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 0px 0px;
    background-position: 10px -33px;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(PositionMode.Unit, testable.XOffset.PositionMode);
                Assert.Equal(10, testable.XOffset.Offset);
                Assert.Equal(PositionMode.Unit, testable.YOffset.PositionMode);
                Assert.Equal(-33, testable.YOffset.Offset);
            }

            [Fact]
            public void WillSetflippedOffsets()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 50 60;
    background-position: top;
}}";

                var testable = new BackgroundImageClass(css, "http://server/content/style.css");

                Assert.Equal(PositionMode.Unit, testable.XOffset.PositionMode);
                Assert.Equal(50, testable.XOffset.Offset);
                Assert.Equal(PositionMode.Direction, testable.YOffset.PositionMode);
                Assert.Equal(Direction.Top, testable.YOffset.Direction);
            }

        }

        public class ShortcutOffsets
        {
            [Theory,
            InlineData("left", PositionMode.Direction, Direction.Left),
            InlineData("right", PositionMode.Direction, Direction.Right),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetXOffsetFromShortcut(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.XOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.XOffset.Direction : testable.XOffset.Offset);
            }

            [Theory,
            InlineData("top", PositionMode.Direction, Direction.Top),
            InlineData("bottom", PositionMode.Direction, Direction.Bottom),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetYOffsetFromShortcut(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat center {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }

            [Theory,
            InlineData("left", PositionMode.Direction, Direction.Left),
            InlineData("right", PositionMode.Direction, Direction.Right),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetSecondOffsetAsXOffsetFromShortcutWhenFirstOffsetIsTop(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat top {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.XOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.XOffset.Direction : testable.XOffset.Offset);
            }

            [Theory,
            InlineData("left", PositionMode.Direction, Direction.Left),
            InlineData("right", PositionMode.Direction, Direction.Right),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetSecondOffsetAsXOffsetFromShortcutWhenFirstOffsetIsBottom(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat bottom {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.XOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.XOffset.Direction : testable.XOffset.Offset);
            }

            [Theory,
            InlineData("top", PositionMode.Direction, Direction.Top),
            InlineData("bottom", PositionMode.Direction, Direction.Bottom),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetFirstOffsetAsYOffsetFromShortcutWhenSecondOffsetIsLeft(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat {0} left;
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }

            [Theory,
            InlineData("top", PositionMode.Direction, Direction.Top),
            InlineData("bottom", PositionMode.Direction, Direction.Bottom),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetFirstOffsetAsYOffsetFromShortcutWhenSecondOffsetIsRight(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat {0} right;
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }
        }

        public class BackgroundPositionOffsets
        {
            [Theory,
            InlineData("left", PositionMode.Direction, Direction.Left),
            InlineData("right", PositionMode.Direction, Direction.Right),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetXOffset(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.XOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.XOffset.Direction : testable.XOffset.Offset);
            }

            [Theory,
            InlineData("top", PositionMode.Direction, Direction.Top),
            InlineData("bottom", PositionMode.Direction, Direction.Bottom),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetYOffset(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: center {0}
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }

            [Theory,
            InlineData("left", PositionMode.Direction, Direction.Left),
            InlineData("right", PositionMode.Direction, Direction.Right),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetSecondOffsetAsXOffsetWhenFirstOffsetIsTop(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: top {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.XOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.XOffset.Direction : testable.XOffset.Offset);
            }

            [Theory,
            InlineData("left", PositionMode.Direction, Direction.Left),
            InlineData("right", PositionMode.Direction, Direction.Right),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetSecondOffsetAsXOffsetWhenFirstOffsetIsBottom(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: bottom {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.XOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.XOffset.Direction : testable.XOffset.Offset);
            }

            [Theory,
            InlineData("top", PositionMode.Direction, Direction.Top),
            InlineData("bottom", PositionMode.Direction, Direction.Bottom),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetFirstOffsetAsYOffsetWhenSecondOffsetIsLeft(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: {0} left;
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }

            [Theory,
            InlineData("top", PositionMode.Direction, Direction.Top),
            InlineData("bottom", PositionMode.Direction, Direction.Bottom),
            InlineData("center", PositionMode.Direction, Direction.Center),
            InlineData("50%", PositionMode.Percent, 50),
            InlineData("50", PositionMode.Unit, 50),
            InlineData("-50", PositionMode.Unit, -50),
            InlineData("50px", PositionMode.Unit, 50),
            InlineData("50em", PositionMode.Percent, 0)]
            public void WillSetFirstOffsetAsYOffsetWhenSecondOffsetIsRight(string statedOffset, PositionMode expectedPositionMode, int expectedOffset)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: {0} right;
}}";

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), "http://server/content/style.css");

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }
        }
    }
}
