using System;
using SassAndCoffee.Core;
using System.Web;
using SassAndCoffee.Core.Compilers;

namespace RequestReduce.SassLessCoffee
{
    public abstract class SassAndCoffeeHandler : IHttpHandler
    {
        private readonly ISimpleFileCompiler simpleFileCompiler;

        protected SassAndCoffeeHandler(ISimpleFileCompiler simpleFileCompiler)
        {
            this.simpleFileCompiler = simpleFileCompiler;
        }

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            var request = context.Request;
            var response = context.Response;

            simpleFileCompiler.Init(new CompilerHost(context));
            try
            {
                var result = simpleFileCompiler.ProcessFileContent(context.Server.MapPath(request.Path));
                response.ContentType = simpleFileCompiler.OutputMimeType;
                response.Write(result);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                response.StatusCode = 404;
                response.Write("/* File Not Found while parsing: " + ex.Message + " */");
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Write("/* Error in compiling: " + ex.ToString() + " */");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        class CompilerHost : ICompilerHost
        {
            private readonly HttpContextBase context;

            public CompilerHost(HttpContextBase context)
            {
                this.context = context;
            }

            public string MapPath(string path)
            {
                return context.Server.MapPath(path);
            }

            public string ApplicationBasePath
            {
                get { return context.Request.PhysicalApplicationPath; }
            }
        }
    }
}
