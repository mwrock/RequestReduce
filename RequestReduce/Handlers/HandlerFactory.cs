using System;
using System.Collections.Generic;
using System.Web;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using RequestReduce.Utilities;

namespace RequestReduce.Handlers
{
    public class HandlerFactory : IHandlerFactory
    {
        private readonly IRRConfiguration config;
        private readonly IUriBuilder uriBuilder;
        readonly List<Func<Uri, bool, IHttpHandler>> handlerMaps = new List<Func<Uri, bool, IHttpHandler>>();

        public HandlerFactory(IRRConfiguration config, IUriBuilder uriBuilder)
        {
            this.config = config;
            this.uriBuilder = uriBuilder;
            AddHandlerMap(DefaultMap);
        }

        private IHttpHandler DefaultMap(Uri uri, bool postAuth)
        {
            if (!IsInRRContentDirectory(uri))
                return null;
            var url = uri.AbsoluteUri;
            var actionUrl = EnsurePath(url);
            var sig = uriBuilder.ParseSignature(url);
            if ((actionUrl.ToLowerInvariant().Contains("/flush/")
                || actionUrl.ToLowerInvariant().Contains("/flushfailures/")) && postAuth)
                return RRContainer.Current.GetInstance<FlushHandler>();
            if (actionUrl.ToLowerInvariant().Contains("/dashboard/") && postAuth)
                return RRContainer.Current.GetInstance<DashboardHandler>();
            return sig != Guid.Empty.RemoveDashes() && sig != null ? RRContainer.Current.GetInstance<ReducedContentHandler>() : null;
        }

        public void AddHandlerMap(Func<Uri, bool, IHttpHandler> map)
        {
            handlerMaps.Add(map);
        }

        public IHttpHandler ResolveHandler(Uri uri, bool postAuth)
        {
            foreach (var handlerMap in handlerMaps)
            {
                var handler = handlerMap(uri, postAuth);
                if (handler != null)
                    return handler;
            }
            return null;
        }

        private static string EnsurePath(string path)
        {
            if (path.EndsWith("/"))
                return path;
            return path + "/";
        }

        private bool IsInRRContentDirectory(Uri uri)
        {
            var rrPath = EnsurePath(config.ResourceAbsolutePath);
            var url = uri.PathAndQuery;
            if (rrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                url = uri.AbsoluteUri;
            return url.StartsWith(rrPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}
