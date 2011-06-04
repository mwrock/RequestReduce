using System;

namespace RequestReduce.Store
{
    public interface IRepository<in T>
    {
        void Save(T file);
    }

    public class Repository<T> : IRepository<T>
    {
        public void Save(T file)
        {
            
        }
    }
}