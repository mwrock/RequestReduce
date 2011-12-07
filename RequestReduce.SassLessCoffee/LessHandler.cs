using System.Web;
using RequestReduce.Utilities;
using dotless.Core;
using dotless.Core.configuration;

namespace RequestReduce.SassLessCoffee
{
    public class LessHandler : IHttpHandler
    {
        private readonly IFileWrapper fileWrapper;

        public LessHandler(IFileWrapper fileWrapper)
        {
            this.fileWrapper = fileWrapper;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var localPath = context.Request.Url.LocalPath;
            var response = context.Response;

            try
            {
                var source = fileWrapper.GetFileString(localPath);

                response.ContentType = "text/css";
                response.Write(new EngineFactory(new DotlessConfiguration
                                                     {
                                                         CacheEnabled = false
                                                     }
                                   ).GetEngine().TransformToCss(source, localPath));
            }
            catch (System.IO.FileNotFoundException ex)
            {
                response.StatusCode = 404;
                response.Write("/* File Not Found while parsing: " + ex.Message + " */");
            }
            catch (System.IO.IOException ex)
            {
                response.StatusCode = 500;
                response.Write("/* Error in less parsing: " + ex.Message + " */");
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
