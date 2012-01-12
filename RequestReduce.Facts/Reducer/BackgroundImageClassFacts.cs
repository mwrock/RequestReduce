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
                var testable = new BackgroundImageClass("original string", 0);

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

                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal("http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png", testable.ImageUrl);
            }

            [Fact]
            public void WillSetImageUrlFromShortcutStyleWhwenLastPropertyWithNoSemicolon()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: #fff url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") repeat
}";

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal("http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png", testable.ImageUrl);
            }

            [Fact]
            public void WillLeaveImageUrlNullIfBackgroundImageUrlIsEmpty()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url("""");
}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.Null(testable.ImageUrl);
            }

            [Fact]
            public void WillLeaveImageUrlNullIfBackgroundImageUrlIsDataUri()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""data:image/png;base64,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"");
}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.Null(testable.ImageUrl);
            }

            [Fact]
            public void WillSetRepeatIfARepeatDoesNotExist()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}";

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), 0);

                Assert.Equal(expectedWidth, testable.Width);
            }

            [Fact]
            public void WillSetWidthFromWidthWhenLastPropertyWithNoSemicolon()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    width: 50px}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal(50, testable.Width);
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

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedPadding), 0);

                Assert.Equal(expectedWidth, testable.Width);
                Assert.Equal(expectedHeight, testable.Height);
            }

            [Fact]
            public void WillSetWidthToNullIfNoWidthSpecified()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: 20px;
    padding-left: 10px;
}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.Null(testable.Width);
            }

            [Fact]
            public void WillSetHeightToNullIfNoHeightIsFound()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    padding-top: 10px;
}}";

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedWidth), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), 0);

                Assert.Equal(expectedHeight, testable.Height);
            }

            [Fact]
            public void WillSetHeightFromHeightWhenLastPropertyNotEndingWithSemicolon()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    height: 50}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal(50, testable.Height);
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

                var testable = new BackgroundImageClass(string.Format(css, statedHeight), 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

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

                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal(PositionMode.Unit, testable.XOffset.PositionMode);
                Assert.Equal(50, testable.XOffset.Offset);
                Assert.Equal(PositionMode.Direction, testable.YOffset.PositionMode);
                Assert.Equal(Direction.Top, testable.YOffset.Direction);
            }

            [Fact]
            public void WillSetImportantIfLonghandpositionIsImportant()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position:  50 60 !important
}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.True(testable.Important);
            }

            [Fact]
            public void WillSetImportantIfShorthandpositionIsImportant()
            {
                var css =
    @"
img.icon {
    background: url(""http://galchameleon.redmond.corp.microsoft.com/contentservice/d046de6b-2d8e-43ec-9b37-9f5d010e51dd/icons_windows.png"") no-repeat 0 0 !important;width: 20px;
}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.True(testable.Important);
            }

            [Fact]
            public void WillNotSetImportantIfNotImportant()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 50 60;
}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.False(testable.Important);
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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }

            [Theory,
            InlineData("center"),
            InlineData("left"),
            InlineData("right")]
            public void WillSetYOffsetAsCenterWhenXOffsetIsDirectionAndYOffsetIsNotGiven(string xDirection)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, xDirection), 0);

                Assert.Equal(PositionMode.Direction, testable.YOffset.PositionMode);
                Assert.Equal(Direction.Center, testable.YOffset.Direction);
            }

            [Theory,
            InlineData("top"),
            InlineData("bottom")]
            public void WillSetXOffsetAsCenterWhenYOffsetIsDirectionAndXOffsetIsNotGiven(string xDirection)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, xDirection), 0);

                Assert.Equal(PositionMode.Direction, testable.XOffset.PositionMode);
                Assert.Equal(Direction.Center, testable.XOffset.Direction);
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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

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

                var testable = new BackgroundImageClass(string.Format(css, statedOffset), 0);

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }

            [Theory,
            InlineData("center"),
            InlineData("left"),
            InlineData("right")]
            public void WillSetYOffsetAsCenterWhenXOffsetIsDirectionAndYOffsetIsNotGiven(string xDirection)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, xDirection), 0);

                Assert.Equal(PositionMode.Direction, testable.YOffset.PositionMode);
                Assert.Equal(Direction.Center, testable.YOffset.Direction);
            }

            [Theory,
            InlineData("top"),
            InlineData("bottom")]
            public void WillSetXOffsetAsCenterWhenYOffsetIsDirectionAndXOffsetIsNotGiven(string xDirection)
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat;
    background-position: {0};
}}";

                var testable = new BackgroundImageClass(string.Format(css, xDirection), 0);

                Assert.Equal(PositionMode.Direction, testable.XOffset.PositionMode);
                Assert.Equal(Direction.Center, testable.XOffset.Direction);
            }

            [Fact]
            public void SetInitialSpecificityScore()
            {
                var testable = new BackgroundImageClass("h1 {{color:blue}}", 0);

                Assert.Equal(-1, testable.SpecificityScore);
            }
        }

        public class PropertyCompletion
        {
            [Theory]
            [InlineData("h1 {{float:left;}}", RequestReduce.Reducer.PropertyCompletion.HasNothing)]
            [InlineData(@"h1 {{background-image: url(""http://i3.social.microsoft.com/contentservice/technet.png"");}}", RequestReduce.Reducer.PropertyCompletion.HasImage)]
            [InlineData("h1 {{background-repeat: none;;}}", RequestReduce.Reducer.PropertyCompletion.HasRepeat)]
            [InlineData("h1 {{background-position: 0;}}", RequestReduce.Reducer.PropertyCompletion.HasXOffset)]
            [InlineData("h1 {{background-position: top;}}", RequestReduce.Reducer.PropertyCompletion.HasYOffset)]
            [InlineData("h1 {{width:5px;}}", RequestReduce.Reducer.PropertyCompletion.HasWidth)]
            [InlineData("h1 {{height:5px;}}", RequestReduce.Reducer.PropertyCompletion.HasHeight)]
            [InlineData("h1 {{padding-left: 5px;}}", RequestReduce.Reducer.PropertyCompletion.HasPaddingLeft)]
            [InlineData("h1 {{padding-right: 5px;}}", RequestReduce.Reducer.PropertyCompletion.HasPaddingRight)]
            [InlineData("h1 {{padding-top: 5px;}}", RequestReduce.Reducer.PropertyCompletion.HasPaddingTop)]
            [InlineData("h1 {{padding-bottom: 5px;}}", RequestReduce.Reducer.PropertyCompletion.HasPaddingBottom)]
            public void WillIndicateIfAPropertyIsFilledIn(string css, RequestReduce.Reducer.PropertyCompletion expectedCompletion)
            {
                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal(expectedCompletion, testable.PropertyCompletion);
            }
        }

        public class Dimensions
        {
            [Fact]
            public void WillSetDimensionalPropertiesCorrectly()
            {
                var css =
    @"
.LocalNavigation{{
width: 5px;
height: 10px;    
padding: 15px 20px 25px 30px;
}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal(5, testable.ExplicitWidth);
                Assert.Equal(10, testable.ExplicitHeight);
                Assert.Equal(15, testable.PaddingTop);
                Assert.Equal(20, testable.PaddingRight);
                Assert.Equal(25, testable.PaddingBottom);
                Assert.Equal(30, testable.PaddingLeft);
                Assert.Equal(55, testable.Width);
                Assert.Equal(50, testable.Height);
            }
        }

        public class Selector
        {
            [Fact]
            public void WillPopulateSelectorWithSelectorPortionOfPassedCss()
            {
                var css = @".LocalNavigation{{width: 5px;}}";

                var testable = new BackgroundImageClass(css, 0);

                Assert.Equal(".LocalNavigation", testable.Selector);
            }
        }

        public class ScoreSpecificity
        {
            [Fact]
            public void WillScore0ForUniversalSelector()
            {
                var testable = new BackgroundImageClass("*{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(0, score);
            }

            [Fact]
            public void WillScore100ForIDs()
            {
                var testable = new BackgroundImageClass("#id1 #id2 *#id3{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(300, score);
            }

            [Fact]
            public void WillScore10Forclasses()
            {
                var testable = new BackgroundImageClass(".cls1.cls4 .cls2 *.cls3{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(40, score);
            }

            [Fact]
            public void WillScore10Forattributes()
            {
                var testable = new BackgroundImageClass(@"*[title=""title""]{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(10, score);
            }

            [Fact]
            public void WillScore1ForElements()
            {
                var testable = new BackgroundImageClass(@"h1 + h2 span > a * p{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(5, score);
            }

            [Fact]
            public void WillScore10ForPseudoClasses()
            {
                var testable = new BackgroundImageClass(@"li:first-child a:link{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(22, score);
            }

            [Fact]
            public void WillScore1ForPseudoElements()
            {
                var testable = new BackgroundImageClass(@"li:first-letter p:first-line p:before p:after{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(8, score);
            }

            [Fact]
            public void WillAccuratelyScoreCombination()
            {
                var testable = new BackgroundImageClass(@"h1#myid.myclass a.special:visited span#myid[title=""title""]{{color:blue}}", 0);

                var score = testable.ScoreSpecificity();

                Assert.Equal(243, score);
            }
        }
    }
}
