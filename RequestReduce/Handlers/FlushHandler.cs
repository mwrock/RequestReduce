using System.Web;

namespace RequestReduce.Handlers
{
    public class FlushHandler : IHttpHandler
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
