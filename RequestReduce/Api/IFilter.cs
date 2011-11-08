namespace RequestReduce.Api
{
    public interface IFilter
    {
        bool IgnoreTarget(IFilterContext context);
    }
}