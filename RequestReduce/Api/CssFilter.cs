using System;

namespace RequestReduce.Api
{
    public class CssFilter : Filter<CssJsFilterContext>
    {
        public CssFilter(Predicate<CssJsFilterContext> test) : base(test) {}
    }
}