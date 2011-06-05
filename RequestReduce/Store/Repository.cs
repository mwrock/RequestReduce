using System;
using System.Data.Entity;
using RequestReduce.Configuration;

namespace RequestReduce.Store
{
    public interface IRepository<T> where T : class
    {
        void Save(T entity);
        T this[object id] { get; }
    }

    public class Repository<T> : IDisposable, IRepository<T> where T : class
    {
        private readonly IRRConfiguration config;
        private readonly RequestReduceContext context = null;

        public Repository(IRRConfiguration config)
        {
            this.config = config;
            context = new RequestReduceContext(config.ConnectionStringName);
        }

        public void Save(T entity)
        {
            context.Set<T>().Add(entity);
            context.SaveChanges();
        }

        public T this[object id]
        {
            get 
            {
                return context.Set<T>().Find(id);
            }
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}