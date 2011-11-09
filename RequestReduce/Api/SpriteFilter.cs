using System;

namespace RequestReduce.Api
{
    public class SpriteFilter : Filter<SpriteFilterContext>
    {
        public SpriteFilter(Predicate<SpriteFilterContext> test) : base(test) { }
    }
}