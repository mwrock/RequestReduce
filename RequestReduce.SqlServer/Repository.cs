using System;
using System.Collections.Generic;
using RequestReduce.Configuration;
using System.Configuration;
using RequestReduce.SqlServer.ORM;

namespace RequestReduce.SqlServer
{
    public interface IRepository : IDisposable
    {
        T SingleOrDefault<T>(object primaryKey);
        T SingleOrDefault<T>(string sql, params object[] args);
        List<T> Fetch<T>(string sql, params object[] args);
        int Insert(object itemToAdd);
        int Update(object itemToUpdate);
        int Delete<T>(object primaryKeyValue);
    }
    public class Repository : IRepository
    {
        private readonly RequestReduceDB db;

        public Repository(IRRConfiguration config)
        {
            db = IsConnectionStringName(config.ConnectionStringName)
                     ? new RequestReduceDB(config.ConnectionStringName)
                     : new RequestReduceDB(config.ConnectionStringName, RequestReduceDB.DefaultProviderName);
        }

        public RequestReduceDB GetDatabase()
        {
            return db;
        }

        private bool IsConnectionStringName(string connectionStringName)
        {
            if (ConfigurationManager.ConnectionStrings == null)
            {
                return false;
            }

            return (ConfigurationManager.ConnectionStrings[connectionStringName] != null);
        }

        public TPassType SingleOrDefault<TPassType>(object primaryKey)
        {
            return db.SingleOrDefault<TPassType>(primaryKey);
        }
        public TPassType SingleOrDefault<TPassType>(string sql, params object[] args)
        {
            return db.SingleOrDefault<TPassType>(sql, args);
        }
        public List<TPassType> Fetch<TPassType>(string sql, params object[] args)
        {
            return db.Fetch<TPassType>(sql, args);
        }
        public int Insert(object poco)
        {
            return Convert.ToInt32(db.Insert(poco));
        }
        public int Update(object poco)
        {
            return db.Update(poco);
        }
        public int Delete<TPassType>(object pocoOrPrimaryKey)
        {
            return db.Delete<TPassType>(pocoOrPrimaryKey);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}