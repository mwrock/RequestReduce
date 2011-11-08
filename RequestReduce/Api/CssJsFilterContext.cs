using System.Web;

namespace RequestReduce.Api
{
    public class CssJsFilterContext : IFilterContext
    {
        public CssJsFilterContext(HttpRequestBase httpRequest, string url, string tag)
        {
            HttpRequest = httpRequest;
            FilteredUrl = url;
            FilteredTag = tag;
        }
        public HttpRequestBase HttpRequest { private set; get; }
        public string FilteredUrl { private set; get; }
        public string FilteredTag { private set; get; }
    }
}