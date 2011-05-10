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
                var testable = new BackgroungImageClass("original string");

                Assert.Equal("original string", testable.OriginalClassString);
            }

            [Fact]
            public void WillSetImageUrlFromShortcutStyle()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") repeat;
}";

                var testable = new BackgroungImageClass(css);

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

                var testable = new BackgroungImageClass(css);

                Assert.Equal("http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png", testable.ImageUrl);
            }

            [Fact]
            public void WillSetRepeatIfARepeatDoesNotExist()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {
    background-image: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"");
}";

                var testable = new BackgroungImageClass(css);

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

                var testable = new BackgroungImageClass(css);

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

                var testable = new BackgroungImageClass(css);

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

                var testable = new BackgroungImageClass(css);

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

                var testable = new BackgroungImageClass(css);

                Assert.Equal(RepeatStyle.YRepeat, testable.Repeat);
            }

            [Theory,
            InlineData("50", 50),
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

                var testable = new BackgroungImageClass(string.Format(css, statedWidth));

                Assert.Equal(expectedWidth, testable.Width);
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

                var testable = new BackgroungImageClass(string.Format(css, statedWidth));

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

                var testable = new BackgroungImageClass(string.Format(css, statedHeight));

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

                var testable = new BackgroungImageClass(string.Format(css, statedHeight));

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

                var testable = new BackgroungImageClass(css);

                Assert.Equal(new Position(){PositionMode = PositionMode.Percent, Offset = 0}, testable.XOffset);
            }

            [Fact]
            public void WillSetOffsetsFromLastOffsets()
            {
                var css =
    @"
.LocalNavigation .TabOn,.LocalNavigation .TabOn:hover {{
    background: url(""http://i3.social.microsoft.com/contentservice/1f22465a-498c-46f1-83d3-9dad00d8a950/subnav_on_technet.png"") no-repeat 50 60;
    background-position: 75 100;
}}";

                var testable = new BackgroungImageClass(css);

                Assert.Equal(PositionMode.Unit, testable.XOffset.PositionMode);
                Assert.Equal(75, testable.XOffset.Offset);
                Assert.Equal(PositionMode.Unit, testable.YOffset.PositionMode);
                Assert.Equal(100, testable.YOffset.Offset);
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

                var testable = new BackgroungImageClass(css);

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

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

                var testable = new BackgroungImageClass(string.Format(css, statedOffset));

                Assert.Equal(expectedPositionMode, testable.YOffset.PositionMode);
                Assert.Equal(expectedOffset, expectedPositionMode == PositionMode.Direction ? (int)testable.YOffset.Direction : testable.YOffset.Offset);
            }
        }
    }
}
