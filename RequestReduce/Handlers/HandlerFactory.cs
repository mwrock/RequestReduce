using System;
using System.Collections.Generic;
using System.Web;

namespace RequestReduce.Handlers
{
    public class HandlerFactory : IHandlerFactory
    {
        readonly List<Func<Uri, IHttpHandler>> handlerMaps = new List<Func<Uri, IHttpHandler>>();

        public void AddHandlerMap(Func<Uri, IHttpHandler> map)
        {
            handlerMaps.Add(map);
        }

        public IHttpHandler ResolveHandler(Uri uri)
        {
            foreach (var handlerMap in handlerMaps)
            {
                var handler = handlerMap(uri);
                if (handler != null)
                    return handler;
            }
            return null;
        }
    }
}
