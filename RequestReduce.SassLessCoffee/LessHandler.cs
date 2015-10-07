using System.Reflection;
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
        private readonly ILessEngine engine;

        public LessHandler() : this(new FileWrapper())
        {
            
        }

        public LessHandler(IFileWrapper fileWrapper)
        {
            this.fileWrapper = fileWrapper;
            var config = new DotlessConfiguration
            {
                CacheEnabled = false,
                Logger = typeof(LessLogger),
                Web = HttpContext.Current != null,
            };
            var engineFactory = new EngineFactory(config);
            if (HttpContext.Current == null)
                engine = engineFactory.GetEngine();
            else
                engine = engineFactory.GetEngine(new AspNetContainerFactory());
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var physicalPath = context.Server.MapPath(context.Request.Path);
            var querystring = context.Request.QueryString.ToString();
            var response = context.Response;

            try
            {
                var source = fileWrapper.GetFileString(physicalPath);
                var lessEngine = (LessEngine) ((ParameterDecorator) engine).Underlying;
                ((LessLogger)lessEngine.Logger).Response = response;
                var result = engine.TransformToCss(source, physicalPath + (string.IsNullOrWhiteSpace(querystring) ? string.Empty : "?" + querystring));
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
                response.Write("/* Error in less parsing: " + ex + " */");
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

        public void Info(string message, params object[] args)
        {
            Info(string.Format(message, args));
        }

        public void Debug(string message)
        {
        }

        public void Debug(string message, params object[] args)
        {
            Debug(string.Format(message, args));
        }

        public void Warn(string message)
        {
        }

        public void Warn(string message, params object[] args)
        {
            Warn(string.Format(message, args));
        }

        public void Error(string message)
        {
            Response.Write("LESS Error: <br/>" + message);
        }

        public void Error(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        public HttpResponseBase Response { get; set; }
    }
}
