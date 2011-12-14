using System.Web;
using RequestReduce.Utilities;
using dotless.Core;
using dotless.Core.Loggers;
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
            var physicalPath = context.Server.MapPath(context.Request.Path);
            var response = context.Response;

            try
            {
                var source = fileWrapper.GetFileString(physicalPath);
                var engine = new EngineFactory(new DotlessConfiguration
                                                   {
                                                       CacheEnabled = false,
                                                       Logger = typeof (LessLogger),
                                                       Web = HttpContext.Current != null
                                                   }
                    ).GetEngine();
                var lessEngine = (LessEngine) ((ParameterDecorator) engine).Underlying;
                ((LessLogger)lessEngine.Logger).Response = response;
                var result = engine.TransformToCss(source, physicalPath);
                response.ContentType = "text/css";
                if(!string.IsNullOrEmpty(result))
                    response.Write(result);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                response.StatusCode = 404;
                response.Write("/* File Not Found while parsing: " + ex.Message + " */");
            }
            catch (System.Exception ex)
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

    public class LessLogger : ILogger
    {
        public void Log(LogLevel level, string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Debug(string message)
        {
        }

        public void Warn(string message)
        {
        }

        public void Error(string message)
        {
            Response.Write(message);
        }

        public HttpResponseBase Response { get; set; }
    }

}
