using System;

namespace RequestReduce.Api
{
    public class JavascriptFilter : Filter<CssJsFilterContext>
    {
        public JavascriptFilter(Predicate<CssJsFilterContext> test) : base(test) { }
    }
}