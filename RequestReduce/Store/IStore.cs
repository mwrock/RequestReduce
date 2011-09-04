using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace RequestReduce.Store
{
    public interface IStore : IDisposable
    {
        void Save(byte[] content, string url, string originalUrls);
        bool SendContent(string url, HttpResponseBase response);
        IDictionary<Guid, string> GetSavedUrls();
        void Flush(Guid keyGuid);
        string GetUrlByKey(Guid keyGuid);
    }
}