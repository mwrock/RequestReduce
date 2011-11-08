namespace RequestReduce.Api
{
    public abstract class SpriteFilter : IFilter
    {
        public abstract bool IgnoreTarget(SpriteFilterContext context);
        public bool IgnoreTarget(IFilterContext context)
        {
            return IgnoreTarget(context as SpriteFilterContext);
        }
    }
}