using System;

namespace RequestReduce.Api
{
    public class Filter<T> : IFilter where T : class, IFilterContext
    {
        protected readonly Predicate<T> Test;

        public Filter(){}

        public Filter(Predicate<T> test)
        {
            Test = test;
        }

        public virtual bool IgnoreTarget(T context)
        {
            return Test(context);
        }

        public bool IgnoreTarget(IFilterContext context)
        {
            return IgnoreTarget(context as T);
        }
    }
}