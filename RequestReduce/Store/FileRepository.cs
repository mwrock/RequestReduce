using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using RequestReduce.Configuration;

namespace RequestReduce.Store
{
    public interface IFileRepository : IRepository<RequestReduceFile>
    {
        IEnumerable<Guid> GetKeys();
        IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key);
    }
    public class FileRepository : Repository<RequestReduceFile>, IFileRepository
    {
        public FileRepository(IRRConfiguration config) : base(config)
        {
            if (!(Context.Database.Connection is System.Data.SqlServerCe.SqlCeConnection))
            {
                Database.SetInitializer<RequestReduceContext>(null);
                Context.Database.Initialize(false);
            }
            else
            {
                Database.SetInitializer<RequestReduceContext>(new DropCreateDatabaseIfModelChanges<RequestReduceContext>());
                Context.Database.Initialize(false);
            }
        }

        public IEnumerable<Guid> GetKeys()
        {
            return AsQueryable().Where(x => x.FileName == Utilities.UriBuilder.CssFileName && !x.IsExpired).Select(y => y.Key).Distinct().ToList();
        }

        public IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key)
        {
            return AsQueryable().Where(x => x.Key == key).ToArray();
        }

        public override void Save(RequestReduceFile entity)
        {
            try
            {
                base.Save(entity);
            }
            catch (DbUpdateException)
            {
                Context.Files.Remove(entity);
                var existingFile = base[entity.RequestReduceFileId];
                existingFile.Content = entity.Content;
                existingFile.LastUpdated = entity.LastUpdated;
                existingFile.IsExpired = entity.IsExpired;
                Context.SaveChanges();
            }
        }
    }
}
