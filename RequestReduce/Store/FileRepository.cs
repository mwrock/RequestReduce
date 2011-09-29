using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using RequestReduce.Configuration;
using RequestReduce.Module;
using UriBuilder = RequestReduce.Utilities.UriBuilder;
using RequestReduce.ResourceTypes;
using RequestReduce.IOC;

namespace RequestReduce.Store
{
    public interface IFileRepository : IRepository<RequestReduceFile>
    {
        IEnumerable<string> GetActiveFiles();
        IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key);
        string GetActiveUrlByKey(Guid key, Type resourceType);
    }
    public class FileRepository : Repository<RequestReduceFile>, IFileRepository
    {
        public FileRepository(IRRConfiguration config) : base(config)
        {

            if (RequestReduceContext.SqlCeType == null || RequestReduceContext.SqlCeType != Context.Database.Connection.GetType())
                Database.SetInitializer<RequestReduceContext>(null);
            else
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<RequestReduceContext>());
            Context.Database.Initialize(false);
        }

        public IEnumerable<string> GetActiveFiles()
        {
            return (from files in AsQueryable()
                    where !files.IsExpired
                    group files by files.Key
                    into filegroup
                    join files2 in AsQueryable() on new {k = filegroup.Key, u = filegroup.Max(m => m.LastUpdated)}
                        equals new {k = files2.Key, u = files2.LastUpdated}
                    where files2.FileName.Contains("RequestReduce") select files2.FileName).ToList();
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
            catch (DbUpdateException dbe)
            {
                bool shouldUpdate;
                var exception = BuildFailedUpdateException(dbe, entity, out shouldUpdate);
                if(shouldUpdate)
                {
                    var existingFile = base[entity.RequestReduceFileId];
                    if (existingFile == null)
                        throw exception;
                    exception = null;
                    existingFile.Content = entity.Content;
                    existingFile.LastUpdated = entity.LastUpdated;
                    existingFile.IsExpired = entity.IsExpired;
                    try
                    {
                        Context.SaveChanges();
                    }
                    catch (DbUpdateException dbe2)
                    {
                        bool shouldFail;
                        exception = BuildFailedUpdateException(dbe2, existingFile, out shouldFail);
                        if (shouldFail)
                            throw exception;
                    }
                }
                if (RequestReduceModule.CaptureErrorAction != null && exception != null)
                    RequestReduceModule.CaptureErrorAction(exception);
            }
            catch(Exception)
            {
                Detach(entity);
                throw;
            }
        }

        private InvalidOperationException BuildFailedUpdateException(DbUpdateException dbe, RequestReduceFile attemptedEntity, out bool failedForThisEntity)
        {
            var failedUpdates = dbe.Entries;
            failedForThisEntity = false;
            var message = new StringBuilder(string.Format("You were saving {0}. Context failed to save : ", attemptedEntity.RequestReduceFileId));
            foreach (var dbEntityEntry in failedUpdates)
            {
                var badFile = dbEntityEntry.Cast<RequestReduceFile>().Entity;
                Detach(badFile);
                message.Append(badFile.RequestReduceFileId);
                message.Append(",");
                if (attemptedEntity.RequestReduceFileId == badFile.RequestReduceFileId)
                    failedForThisEntity = true;
            }

            return new InvalidOperationException(message.ToString(), dbe);
        }


        public string GetActiveUrlByKey(Guid key, Type resourceType)
        {
            var fileName = (RRContainer.Current.GetInstance(resourceType) as IResourceType).FileName;
            return (from files in AsQueryable()
                    where files.Key == key && !files.IsExpired && files.FileName.Contains(fileName)
                        select files.FileName).FirstOrDefault();
        }
    }
}
