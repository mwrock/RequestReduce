namespace RequestReduce.Api
{
    public abstract class CssFilter : IFilter
    {
        public abstract bool IgnoreTarget(CssJsFilterContext context);
        public bool IgnoreTarget(IFilterContext context)
        {
            return IgnoreTarget(context as CssJsFilterContext);
        }
    }
}