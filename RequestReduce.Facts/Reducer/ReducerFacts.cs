using RequestReduce.Reducer;
using Xunit;

namespace RequestReduce.Facts.Reducer
{
    public class ReducerFacts
    {
        private class TestableReducer : Testable<RequestReduce.Reducer.Reducer>
        {
            public TestableReducer()
            {
                
            }
        }

        public class Process
        {
            [Fact]
            public void WillReturnProcessedCssUrl()
            {
                var testable = new TestableReducer();

                var result = testable.ClassUnderTest.Process("http://host/css1.css::http://host/css2.css");

                Assert.Equal("http://host/processed.css", result);
            }
        }
    }
}
