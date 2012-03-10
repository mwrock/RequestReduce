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
        void Update(RequestReduceFile requestReduceFile);
    }

    public class FileRepository : Repository, IFileRepository
    {
        public FileRepository(IRRConfiguration config)
            : base(config)
        {
        }

        public IEnumerable<string> GetActiveFiles()
        {
            return Fetch<string>(@"
                SELECT f2.FileName
                FROM RequestReduceFiles f2
                INNER JOIN (
	                SELECT f.[key]
		                ,max(f.Lastupdated) AS maxUpdated
	                FROM requestreducefiles f
	                WHERE f.isexpired = 0
		                AND filename LIKE '%RequestReduce%'
	                GROUP BY f.[key]
	                ) AS innerF ON innerF.[key] = f2.[key]
	                AND innerF.maxUpdated = f2.lastupdated
                WHERE filename LIKE '%RequestReduce%'
               ");
        }

        public IEnumerable<RequestReduceFile> GetFilesFromKey(Guid key)
        {
            return Fetch<RequestReduceFile>("select * from RequestReduceFiles where [key]=@0", key);
        }

        public void Update(RequestReduceFile requestReduceFile)
        {
            requestReduceFile.LastUpdated = requestReduceFile.LastUpdated = DateTime.Now;
            base.Update(requestReduceFile);
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
            return SingleOrDefault<string>(@"SELECT FileName from RequestReduceFiles
                    where [Key] = @0 and IsExpired=0 and FileName like @1", key, string.Format("{0}{1}", '%', fileName));
        }
    }
}
