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
        private const string ScriptFormat = @"<script src=""{0}"" type=""text/javascript"" ></script>";
        private readonly Regex scriptPattern = new Regex(@"<script(.*?)(/>|</script>)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex ScriptFilterPattern = new Regex(@"^<script[^>]+src=[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
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

        public string TransformedMarkupTag(string url)
        {
            return string.Format(ScriptFormat, url);
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
    }
}
