using System;
using System.Collections.Generic;
using System.IO;

namespace RequestReduce.Store
{
    public delegate void AddCssAction(Guid key, string url);

    public delegate void DeleeCsAction(Guid key);

    public interface IStore : IDisposable
    {
        void Save(byte[] content, string url, string originalUrls);
        byte[] GetContent(string url);
        Stream OpenStream(string url);
        IDictionary<Guid, string> GetSavedUrls();
        event DeleeCsAction CssDeleted;
        event AddCssAction CssAded;
    }
}