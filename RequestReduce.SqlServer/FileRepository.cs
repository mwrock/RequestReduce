using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RequestReduce.Configuration;
using RequestReduce.IOC;

namespace RequestReduce.SqlServer
{
    public interface IFileRepository : IRepository
    {
        IEnumerable<string> GetActiveFiles();
        IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key);
        string GetActiveUrlByKey(Guid key, Type resourceType);
        void Save(RequestReduceFile requestReduceFile);
    }

    public class FileRepository : Repository, IFileRepository
    {
        public FileRepository(IRRConfiguration config)
            : base(config)
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

        public void Save(RequestReduceFile requestReduceFile)
        {
            try
            {
                ValidateEntity(requestReduceFile);
                Insert(requestReduceFile);
            }
            catch (Exception dbe)
            {
                var exception = BuildFailedUpdateException(dbe, requestReduceFile);

                var existingFile = SingleOrDefault<RequestReduceFile>(requestReduceFile.RequestReduceFileId);
                if (existingFile == null)
                {
                    throw exception;
                }

                existingFile.Content = requestReduceFile.Content;
                existingFile.LastUpdated = requestReduceFile.LastUpdated = DateTime.Now;
                existingFile.IsExpired = requestReduceFile.IsExpired;
                try
                {
                    Update(existingFile);
                }
                catch (Exception dbe2)
                {
                    throw BuildFailedUpdateException(dbe2, existingFile);
                }
            }
        }

        private void ValidateEntity(RequestReduceFile requestReduceFile)
        {
            if (requestReduceFile == null)
            {
                throw new ArgumentNullException("requestReduceFile");
            }
            if (requestReduceFile.Content == null)
            {
                throw new ArgumentException("RequestReduceFile.Content is Null");
            }
            if (requestReduceFile.FileName == null)
            {
                throw new ArgumentException("RequestReduceFile.FileName is Null");
            }
            if (requestReduceFile.FileName.Length > 150)
            {
                throw new ArgumentOutOfRangeException("requestReduceFile", requestReduceFile.FileName.Length, "requestReduceFile.FileName.Length");
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
