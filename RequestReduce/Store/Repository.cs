using System;
using System.Data.Entity;
using RequestReduce.Configuration;

namespace RequestReduce.Store
{
    public interface IRepository<T> where T : class
    {
        void Save(T entity);
        T this[object id] { get; }
        RequestReduceContext Context { get; }
    }

    public class Repository<T> : IDisposable, IRepository<T> where T : class
    {
        private readonly IRRConfiguration config;
        private RequestReduceContext context = null;

        public Repository(IRRConfiguration config)
        {
            this.config = config;
        }

        public RequestReduceContext Context 
        { 
            get { return context ?? (context = new RequestReduceContext(config.ConnectionStringName)); }
        }

        public void Save(T entity)
        {
            Context.Set<T>().Add(entity);
            Context.SaveChanges();
        }

        public T this[object id]
        {
            get 
            {
                return Context.Set<T>().Find(id);
            }
        }

        public void Dispose()
        {
            if(context != null)
                context.Dispose();
        }
    }
}