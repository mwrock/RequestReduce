using System;
using RequestReduce.SqlServer.ORM;

namespace RequestReduce.SqlServer
{
    [TableName("RequestReduceFiles")]
    [PrimaryKey("RequestReduceFileId", autoIncrement = false)]
    public class RequestReduceFile
    {
        public Guid RequestReduceFileId { get; set; }
        public Guid Key { get; set; }
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string OriginalName { get; set; }
        public bool IsExpired { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
