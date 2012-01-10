using System;
using System.Net;
using System.Text;
using SassAndCoffee.Core;
using System.Web;

namespace RequestReduce.SassLessCoffee
{
    public abstract class SassAndCoffeeHandler : IHttpHandler
    {
        private readonly IContentPipeline pipeline;

        protected SassAndCoffeeHandler(IContentPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                var result = pipeline.ProcessRequest(request.PhysicalPath);
                if (result == null)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
                response.ContentEncoding = Encoding.UTF8;
                response.ContentType = result.MimeType;
                response.Write(result.Content);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Write("/* Error in compiling: " + ex + " */");
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }

    }
}
