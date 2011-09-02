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
        IEnumerable<string> GetActiveCssFiles();
        IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key);
    }
    public class FileRepository : Repository<RequestReduceFile>, IFileRepository
    {
        public FileRepository(IRRConfiguration config) : base(config)
        {

            if (RequestReduceContext.SqlCeType == null || RequestReduceContext.SqlCeType != Context.Database.Connection.GetType())
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

        public IEnumerable<string> GetActiveCssFiles()
        {
            return (from files in AsQueryable()
                    where !files.IsExpired
                    group files by files.Key
                    into filegroup
                    join files2 in AsQueryable() on new {k = filegroup.Key, u = filegroup.Max(m => m.LastUpdated)}
                        equals new {k = files2.Key, u = files2.LastUpdated}
                    where files2.FileName.Contains(Utilities.UriBuilder.CssFileName) select files2.FileName).ToList();
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
                if (existingFile == null)
                    throw;
                existingFile.Content = entity.Content;
                existingFile.LastUpdated = entity.LastUpdated;
                existingFile.IsExpired = entity.IsExpired;
                Context.SaveChanges();
            }
        }
    }
}
