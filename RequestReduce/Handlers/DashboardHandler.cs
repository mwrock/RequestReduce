using System.Web;

namespace RequestReduce.Handlers
{
    public class DashboardHandler : IHttpHandler
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
