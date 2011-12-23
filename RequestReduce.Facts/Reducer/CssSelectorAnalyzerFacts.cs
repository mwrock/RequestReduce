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
        public void WillMatchElementAfterPartialMatch()
        {
            var target = "#icons .new .ne .warnings.red.small div.cls";
            var comparable = "#icons .ne";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchMultipleClassesWhenTargetHasMoreThenCamparable()
        {
            var target = "#icons .new .warnings.red.small div.cls";
            var comparable = "#icons .small.warnings";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchMultipleClassesWhenTargetHasMoreThenCamparableAndFirstMatchFailed()
        {
            var target = "#icons .new .small.blue.warnings .warnings.red.small div.cls";
            var comparable = "#icons .small.red.warnings";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchMultipleClassesWhenComparableHasMoreThanTarget()
        {
            var target = "#icons .new .warnings.red.small div.cls";
            var comparable = "#icons .small.warnings.blue";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillNotMatchElementsWithDifferentClasses()
        {
            var target = "#icons .new .warnings.red.small div.cls";
            var comparable = "#icons div.red";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillNotMatchStandAloneClassTiedToElementInTarget()
        {
            var target = "#icons .new .warnings.red.small div.cls";
            var comparable = "#icons .cls";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillNotMatchElementWithMoreClassesThanElementInTarget()
        {
            var target = "#icons .new .warnings.red.small div.cls";
            var comparable = "#icons div.cls.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatchElementWithClassesGettingMatchedByElementInTarget()
        {
            var target = "#icons .new .warnings.red.small div.cls.clp";
            var comparable = "#icons div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchElementWithTargeElementThatHasAnId()
        {
            var target = "#icons h1#myit .new .warnings.red.small div.cls.clp";
            var comparable = "h1 div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchElementWithTargeElementThatHasAMatchingId()
        {
            var target = "#icons h1#myit .new .warnings.red.small div.cls.clp";
            var comparable = "h1#myit div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchElementWithTargeElementThatHasAMisMatchingId()
        {
            var target = "#icons h1#myid .new .warnings.red.small div.cls.clp";
            var comparable = "h1#myit div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillNotMatchLoneIdIfTargetIsTiedToElement()
        {
            var target = "#icons h1#myid .new .warnings.red.small div.cls.clp";
            var comparable = "#myid div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

    }
}
