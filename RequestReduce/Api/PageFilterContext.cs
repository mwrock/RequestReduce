using System.Web;

namespace RequestReduce.Api
{
    public class PageFilterContext : IFilterContext
    {
        public PageFilterContext(HttpRequestBase httpRequest)
        {
            HttpRequest = httpRequest;
        }
        public HttpRequestBase HttpRequest { private set; get; }
    }
}