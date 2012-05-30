using System;
using System.Web;

namespace RequestReduce.Handlers
{
    public interface IHandlerFactory
    {
        void AddHandlerMap(Func<Uri, IHttpHandler> map);
        IHttpHandler ResolveHandler(Uri uri);
    }
}