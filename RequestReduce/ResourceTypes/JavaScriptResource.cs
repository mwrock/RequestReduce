using System.Collections.Generic;
using System.Text.RegularExpressions;
using RequestReduce.Configuration;
using RequestReduce.IOC;
using System;

namespace RequestReduce.ResourceTypes
{
    public class JavaScriptResource : IResourceType
    {
        private static readonly string scriptFormat = @"<script src=""{0}"" type=""text/javascript"" ></script>";
        private static readonly Regex ScriptPattern = new Regex(@"<script[^>]+src=['""]?.*?['""]?[^>]+>\s*?(</script>)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Func<string, string, bool> tagValidator = ((tag, url) => 
                {
                    var urlsToIgnore = RRContainer.Current.GetInstance<IRRConfiguration>().JavaScriptUrlsToIgnore;
                    foreach (var ignoredUrl in urlsToIgnore.Split(new char[]{','}, System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        if(url.ToLower().Contains(ignoredUrl.ToLower().Trim()))
                            return false;
                    }
                    return true;
                });

        public string FileName
        {
            get { return "RequestReducedScript.js"; }
        }

        public IEnumerable<string> SupportedMimeTypes
        {
            get { return new[] { "text/javascript", "application/javascript", "application/x-javascript" }; }
        }

        public string TransformedMarkupTag(string url)
        {
            return string.Format(scriptFormat, url);
        }

        public Regex ResourceRegex
        {
            get { return ScriptPattern; }
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
    }
}
