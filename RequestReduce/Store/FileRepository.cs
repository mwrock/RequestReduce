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
    }
    public class FileRepository : Repository<RequestReduceFile>, IFileRepository
    {
        public FileRepository(IRRConfiguration config) : base(config)
        {
            Database.SetInitializer<RequestReduceContext>(new DropCreateDatabaseIfModelChanges<RequestReduceContext>());
            Context.Database.Initialize(false);
        }

        public IEnumerable<Guid> GetKeys()
        {
            return AsQueryable().Where(x => x.FileName == Utilities.UriBuilder.CssFileName).Select(y => y.Key).Distinct().ToList();
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
                Context.SaveChanges();
            }
        }
    }
}
