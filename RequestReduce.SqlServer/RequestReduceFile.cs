using System;
using PetaPoco;

namespace RequestReduce.SqlServer
{
    [TableName("RequestReduceFiles")]
    [PrimaryKey("RequestReduceFileId", autoIncrement = false)]
    [ExplicitColumns]
    public partial class RequestReduceFile : RequestReduceDB.Record<RequestReduceFile>
    {
        [Column]
        public Guid RequestReduceFileId
        {
            get
            {
                return _RequestReduceFileId;
            }
            set
            {
                _RequestReduceFileId = value;
                MarkColumnModified("RequestReduceFileId");
            }
        }
        Guid _RequestReduceFileId;

        [Column]
        public Guid Key
        {
            get
            {
                return _Key;
            }
            set
            {
                _Key = value;
                MarkColumnModified("Key");
            }
        }
        Guid _Key;

        [Column]
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value;
                MarkColumnModified("FileName");
            }
        }
        string _FileName;

        [Column]
        public byte[] Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
                MarkColumnModified("Content");
            }
        }
        byte[] _Content;

        [Column]
        public string OriginalName
        {
            get
            {
                return _OriginalName;
            }
            set
            {
                _OriginalName = value;
                MarkColumnModified("OriginalName");
            }
        }
        string _OriginalName;

        [Column]
        public bool IsExpired
        {
            get
            {
                return _IsExpired;
            }
            set
            {
                _IsExpired = value;
                MarkColumnModified("IsExpired");
            }
        }
        bool _IsExpired;

        [Column]
        public DateTime LastUpdated
        {
            get
            {
                return _LastUpdated;
            }
            set
            {
                _LastUpdated = value;
                MarkColumnModified("LastUpdated");
            }
        }
        DateTime _LastUpdated;
    }
}
