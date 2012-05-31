using System.Web;

namespace RequestReduce.Handlers
{
    public class ReducedContentHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
