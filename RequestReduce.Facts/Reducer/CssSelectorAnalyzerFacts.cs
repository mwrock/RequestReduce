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
        public void WillMatchStandAloneClassTiedToElementInTarget()
        {
            var target = "#icons .new .warnings.red.small div.cls";
            var comparable = "#icons .cls";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
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
        public void WillNotMatchElementWithNoClassesIfComparableHasClass()
        {
            var target = "#icons .new .warnings.red.small div";
            var comparable = "#icons div.div";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
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

            Assert.True(result);
        }

        [Fact]
        public void WillMatchLoneIdWithIdWithClass()
        {
            var target = "#icons.blue h1#myit .new .warnings.red.small div.cls.clp";
            var comparable = "#icons div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchIdWithIdWithMatchingClass()
        {
            var target = "#icons.blue h1#myit .new .warnings.red.small div.cls.clp";
            var comparable = "#icons.blue div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchIdWithTargeIdThatHasAMisMatchingClass()
        {
            var target = "#icons.blue h1#myid .new .warnings.red.small div.cls.clp";
            var comparable = "#icons.red div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatchIdWithIdWithMatchingClassAnsExtraClass()
        {
            var target = "#icons.blue.red h1#myit .new .warnings.red.small div.cls.clp";
            var comparable = "#icons.blue div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchIdWithIdWithMatchingClassAndMisMachingClass()
        {
            var target = "#icons.blue.red h1#myit .new .warnings.red.small div.cls.clp";
            var comparable = "#icons.blue.green div.clp";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatchIdAndClassWithIdAndhMatchingClassOnAnElementWithExtraClasses()
        {
            var target = "#icons.blue.red h1#myit.myclass.other .new .warnings.red.small div.cls.clp";
            var comparable = "#icons.blue #myit.other";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchIdAndClassWithIdAndhMatchingClassOnAnElementWithExtraNonMatchingClasses()
        {
            var target = "#icons.blue.red h1#myit.myclass.other .new .warnings.red.small div.cls.clp";
            var comparable = "#icons.blue #myit.other.another";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatchUniversalSelectorAsOnlySelector()
        {
            var target = "#icons.blue.red h1#myit.myclass.other .new .warnings.red.small div.cls.clp";
            var comparable = "*";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchUniversalSelector()
        {
            var target = "#icons.blue.red h1#myit.myclass.other .new .warnings.red.small div.cls.clp";
            var comparable = "* #icons.blue #myit.other";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchUniversalSelectorAndMatchingIDAndClass()
        {
            var target = "#icons.blue.red h1#myit.myclass.other .new .warnings.red.small div.cls.clp";
            var comparable = "*#icons.blue #myit.other";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchUniversalSelectorAndNonMatchingIDAndClass()
        {
            var target = "#icons.blue.red h1#myit.myclass.other .new .warnings.red.small div.cls.clp";
            var comparable = "*#icons.blue.green #myit.other";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

        [Fact]
        public void WillMatchPseudoClasses()
        {
            var target = "#icons.blue.red a:link";
            var comparable = "* #icons.blue a:link";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillMatchNonPseudoClassesToPseudoClasses()
        {
            var target = "#icons.blue.red a:link";
            var comparable = "* #icons.blue a";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.True(result);
        }

        [Fact]
        public void WillNotMatchPseudoClassesToNonPseudoClasses()
        {
            var target = "#icons.blue.red a";
            var comparable = "* #icons.blue a:link";
            var testable = new CssSelectorAnalyzer();

            var result = testable.IsInScopeOfTarget(target, comparable);

            Assert.False(result);
        }

    }
}
