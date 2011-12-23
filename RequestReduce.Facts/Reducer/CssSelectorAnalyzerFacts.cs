using RequestReduce.Reducer;
using Xunit;

namespace RequestReduce.Facts.Reducer
{
    public class CssSelectorAnalyzerFacts
    {
        [Fact]
        public void WillMatchOnSingleElements()
        {
            var target = "#icons .new .warnings.red div";
            var comparable = "div";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchOnMultipleElements()
        {
            var target = "#icons .new .warnings.red div";
            var comparable = ".new div";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchOnMultipleElementsInDifferentOrder()
        {
            var target = "#icons .new .warnings.red div";
            var comparable = "div .new";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillNotMatcWithForeignElement()
        {
            var target = "#icons .new .warnings.red div";
            var comparable = ".new div h1";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatcMatchingElementWithoutClassToElementWithClass()
        {
            var target = "#icons .new .warnings.red div.cls";
            var comparable = ".new div";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchPartialStringMatch()
        {
            var target = "#icons .new .warnings.red div.cls";
            var comparable = ".ne div";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatchelementWithClassToSameElementWithSameClass()
        {
            var target = "#icons .new .warnings.red div.cls";
            var comparable = "#icons div.cls";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatcMatchingElementWithClassToElementWithOutClass()
        {
            var target = "#icons .new .warnings.red div";
            var comparable = ".new div.cls";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatcSingleClassToMiltiClassContainingSingleClass()
        {
            var target = "#icons .new .warnings.red div.cls";
            var comparable = "#icons .red";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatcElementAfterPartialMatch()
        {
            var target = "#icons .new .ne .warnings.red.small div.cls";
            var comparable = "#icons .ne";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }
    }
}
