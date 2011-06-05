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

    public class Repository<T> : IRepository<T> where T : class
    {
        public void Save(T entity)
        {
            using(var db = new RequestReduceContext())
            {
                db.Set<T>().Add(entity);
                db.SaveChanges();
            }
        }

        public T this[object id]
        {
            get 
            {
                using (var db = new RequestReduceContext())
                {
                    return db.Set<T>().Find(id);
                }                
            }
        }
    }
}