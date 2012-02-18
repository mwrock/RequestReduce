using System;
using System.Collections.Generic;
using PetaPoco;
using RequestReduce.Configuration;
using System.Linq;

namespace RequestReduce.SqlServer
{
    public interface IPetaPocoRepository
    {
        T Single<T>(object primaryKey);

        IEnumerable<T> Query<T>();
        IQueryable<T> AsQueryable<T>();
        List<T> Fetch<T>();
        Page<T> PagedQuery<T>(long pageNumber, long itemsPerPage, string sql, params object[] args);
        int Insert(object itemToAdd);
        int Update(object itemToUpdate, object primaryKeyValue);
        int Delete<T>(object primaryKeyValue);
        RequestReduceDB Context { get; }
    }
    public class PetaPocoRepository : IPetaPocoRepository
    {
        private readonly IRRConfiguration config;
        private RequestReduceDB db;

        public PetaPocoRepository(IRRConfiguration config)
        {
            this.config = config;
            db = new RequestReduceDB(config.ConnectionStringName);
        }

        public RequestReduceDB Context
        {
            get { return db ?? (db = new RequestReduceDB(config.ConnectionStringName)); }
        }

        public TPassType Single<TPassType>(object primaryKey)
        {
            return db.Single<TPassType>(primaryKey);
        }
        public IEnumerable<TPassType> Query<TPassType>()
        {
            var pd = Database.PocoData.ForType(typeof(TPassType));
            var sql = "SELECT * FROM " + pd.TableInfo.TableName;
            return db.Query<TPassType>(sql);
        }
        public IEnumerable<TPassType> Query<TPassType>(string sql, params object[] args)
        {
            return db.Query<TPassType>(sql, args);
        }
        public IQueryable<TPassType> AsQueryable<TPassType>()
        {
            return Query<TPassType>().AsQueryable<TPassType>();
        }
        public List<TPassType> Fetch<TPassType>()
        {
            var pd = Database.PocoData.ForType(typeof(TPassType));
            var sql = "SELECT * FROM " + pd.TableInfo.TableName;
            return db.Fetch<TPassType>(sql);
        }
        public List<TPassType> Fetch<TPassType>(string sql, params object[] args)
        {
            return db.Fetch<TPassType>(sql, args);
        }
        public Page<TPassType> PagedQuery<TPassType>(long pageNumber, long itemsPerPage, string sql, params object[] args)
        {
            return db.Page<TPassType>(pageNumber, itemsPerPage, sql, args) as Page<TPassType>;
        }
        public Page<TPassType> PagedQuery<TPassType>(long pageNumber, long itemsPerPage, Sql sql)
        {
            return db.Page<TPassType>(pageNumber, itemsPerPage, sql) as Page<TPassType>;
        }
        public int Insert(object poco)
        {
            return Convert.ToInt32(db.Insert(poco));
        }
        public int Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
        {
            return Convert.ToInt32(db.Insert(tableName, primaryKeyName, autoIncrement, poco));
        }
        public int Insert(string tableName, string primaryKeyName, object poco)
        {
            return Convert.ToInt32(db.Insert(tableName, primaryKeyName, poco));
        }
        public int Update(object poco)
        {
            return db.Update(poco);
        }
        public int Update(object poco, object primaryKeyValue)
        {
            return db.Update(poco, primaryKeyValue);
        }
        public int Update(string tableName, string primaryKeyName, object poco)
        {
            return db.Update(tableName, primaryKeyName, poco);
        }
        public int Update(object poco, IEnumerable<string> columns)
        {
            return db.Update(poco, columns);
        }
        public int Delete<TPassType>(object pocoOrPrimaryKey)
        {
            return db.Delete<TPassType>(pocoOrPrimaryKey);
        }
    }
}