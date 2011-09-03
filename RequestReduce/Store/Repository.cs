using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using RequestReduce.Configuration;

namespace RequestReduce.Store
{
    public interface IRepository<T> where T : class
    {
        void Save(T entity);
        void Detach(T entity);
        T this[object id] { get; }
        RequestReduceContext Context { get; }
        IQueryable<T> AsQueryable();
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

        public virtual void Save(T entity)
        {
            Context.Set<T>().Add(entity);
            Context.SaveChanges();
        }

        public void Detach(T entity)
        {
            ((IObjectContextAdapter)Context).ObjectContext.Detach(entity);
        }

        public T this[object id]
        {
            get 
            {
                return Context.Set<T>().Find(id);
            }
        }

        public IQueryable<T> AsQueryable()
        {
            return Context.Set<T>();
        }

        public void Dispose()
        {
            if(context != null)
                context.Dispose();
        }
    }
}