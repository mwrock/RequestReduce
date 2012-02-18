using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Api;

namespace RequestReduce.SqlServer
{
    public interface IPetaPocoFileRepository : IPetaPocoRepository
    {
        IEnumerable<string> GetActiveFiles();
        IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key);
        string GetActiveUrlByKey(Guid key, Type resourceType);
    }

    public class PetaPocoFileRepository : PetaPocoRepository, IPetaPocoFileRepository
    {
        public PetaPocoFileRepository(IRRConfiguration config) : base(config)
        {
        }

        public IEnumerable<string> GetActiveFiles()
        {
            return (from files in AsQueryable<RequestReduceFile>()
                    where !files.IsExpired && files.FileName.Contains("RequestReduce")
                    group files by files.Key
                        into filegroup
                        join files2 in AsQueryable<RequestReduceFile>() on new { k = filegroup.Key, u = filegroup.Max(m => m.LastUpdated) }
                            equals new { k = files2.Key, u = files2.LastUpdated }
                        where files2.FileName.Contains("RequestReduce")
                        select files2.FileName).ToList();
        }

        public IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key)
        {
            return AsQueryable<RequestReduceFile>().Where(x => x.Key == key).ToArray();
        }

        public int Update(RequestReduceFile entity)
        {
            try
            {
                return base.Update(entity);
            }
            catch (Exception dbe)
            {
                var exception = BuildFailedUpdateException(dbe, entity);

                var existingFile = Single<RequestReduceFile>(entity.RequestReduceFileId);
                if (existingFile == null)
                {
                    throw exception;
                }

                exception = null;
                existingFile.Content = entity.Content;
                existingFile.LastUpdated = entity.LastUpdated = DateTime.Now;
                existingFile.IsExpired = entity.IsExpired;
                try
                {
                    return base.Update(existingFile);
                }
                catch (Exception dbe2)
                {
                    exception = BuildFailedUpdateException(dbe2, existingFile);
                    throw exception;
                }
            }
        }

        private InvalidOperationException BuildFailedUpdateException(Exception e, RequestReduceFile attemptedEntity)
        {
            var message = new StringBuilder(string.Format("You were saving {0}. Context failed to save.", attemptedEntity.RequestReduceFileId));
            return new InvalidOperationException(message.ToString(), e);
        }

        public string GetActiveUrlByKey(Guid key, Type resourceType)
        {
            var fileName = RRContainer.GetAllResourceTypes().Single(x => x.GetType() == resourceType).FileName;
            return (from files in AsQueryable<RequestReduceFile>()
                    where files.Key == key && !files.IsExpired && files.FileName.Contains(fileName)
                    select files.FileName).FirstOrDefault();
        }
    }
}
