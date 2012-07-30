using System;
using System.Web;

namespace RequestReduce.Handlers
{
    public interface IHandlerFactory
    {
        void AddHandlerMap(Func<Uri, bool, IHttpHandler> map);
        IHttpHandler ResolveHandler(Uri uri, bool postAuth);
    }
}