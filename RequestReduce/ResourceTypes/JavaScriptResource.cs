using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using System;

namespace RequestReduce.ResourceTypes
{
    public class JavaScriptResource : IResourceType
    {
        private enum ScriptBundle
        {
            Default = 0,
            Async = 1,
            Defer = 2
        }

        private readonly string[] ScriptFormats = new string[Enum.GetValues(typeof(ScriptBundle)).Length];
        private readonly Regex scriptPattern = new Regex(@"<script(.*?)(/>|</script>)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex ScriptFilterPattern = new Regex(@"^<script[^>]+src=[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public JavaScriptResource()
        {
            ScriptFormats[(int)ScriptBundle.Default] = @"<script src=""{0}"" type=""text/javascript"" ></script>";
            ScriptFormats[(int)ScriptBundle.Defer] = @"<script defer src=""{0}"" type=""text/javascript"" ></script>";
            ScriptFormats[(int)ScriptBundle.Async] = @"<script type=""text/javascript"">(function (d,s) {{
var b = d.createElement(s); b.type = ""text/javascript""; b.async = true; b.src = ""{0}"";
var t = d.getElementsByTagName(s)[0]; t.parentNode.insertBefore(b,t);
}}(document,'script'));</script>";
        }
        
        private Func<string, string, bool> tagValidator = ((tag, url) => 
                {
                    var match = ScriptFilterPattern.Match(tag);
                    if (!match.Success)
                        return false;
                    var urlsToIgnore = RRContainer.Current.GetInstance<IRRConfiguration>().JavaScriptUrlsToIgnore;
                    return urlsToIgnore.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).All(ignoredUrl => !url.ToLower().Contains(ignoredUrl.ToLower().Trim()));
                });

        public string FileName
        {
            get { return "RequestReducedScript.js"; }
        }

        public IEnumerable<string> SupportedMimeTypes
        {
            get { return new[] { "text/javascript", "application/javascript", "application/x-javascript" }; }
        }

        public int Bundle(string resource)
        {
            // Some real Regex needed here !!!
            // Some real Regex needed here !!!

            if (resource.Contains("async"))
            {
                return (int)ScriptBundle.Async;
            }

            if (resource.Contains("defer"))
            {
                return (int)ScriptBundle.Defer;
            }
            
            return (int)ScriptBundle.Default;
        }

        public string TransformedMarkupTag(string url, int bundle)
        {
            return string.Format(ScriptFormats[bundle], url);
        }

        public Regex ResourceRegex
        {
            get { return scriptPattern; }
        }


        public Func<string, string, bool> TagValidator
        {
            get 
            { 
                return tagValidator; 
            }
            set
            {
                tagValidator = value;
            }
        }

        public bool IsLoadDeferred(int bundle)
        {
            if (bundle == (int)ScriptBundle.Defer)
            {
                return true;
            }
            if(bundle == (int)ScriptBundle.Async)
            {
                return true;
            }
            return false;
        }

        public bool IsDynamicLoad(int bundle)
        {
            return (bundle == (int)ScriptBundle.Async);
        }
    }
}
