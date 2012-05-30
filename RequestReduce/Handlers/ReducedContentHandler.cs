using System.Web;

namespace RequestReduce.Handlers
{
    public class ReducedContentHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {

        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
