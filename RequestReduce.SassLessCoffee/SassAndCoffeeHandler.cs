using SassAndCoffee.Core;
using System.Net;
using System.Text;
using System.Web;
using SassAndCoffee.Core.Compilers;

namespace RequestReduce.SassLessCoffee
{
    public abstract class SassAndCoffeeHandler : IHttpHandler, ICompilerHost
    {
        private readonly ISimpleFileCompiler simpleFileCompiler;

        protected SassAndCoffeeHandler(ISimpleFileCompiler simpleFileCompiler)
        {
            this.simpleFileCompiler = simpleFileCompiler;
        }

        public void ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            simpleFileCompiler.Init(this);
            var result = simpleFileCompiler.ProcessFileContent(MapPath(request.Path));

            if (result == null)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = simpleFileCompiler.OutputMimeType;
            response.Write(result);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public string MapPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }

        public string ApplicationBasePath
        {
            get
            {
                return HttpContext.Current.Request.PhysicalApplicationPath;
            }
        }
    }
}
