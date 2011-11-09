using System;

namespace RequestReduce.Api
{
    public class PageFilter : Filter<PageFilterContext>
    {
        public PageFilter(Predicate<PageFilterContext> pageTest) : base(pageTest)
        {
        }
    }
}