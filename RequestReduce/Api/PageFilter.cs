namespace RequestReduce.Api
{
    public abstract class PageFilter : IFilter
    {
        public abstract bool IgnoreTarget(PageFilterContext context);
        public bool IgnoreTarget(IFilterContext context)
        {
            return IgnoreTarget(context as PageFilterContext);
        }
    }
}