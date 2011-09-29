using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace RequestReduce.ResourceTypes
{
    public class CssResource : IResourceType
    {
        private static readonly string cssFormat = @"<link href=""{0}"" rel=""Stylesheet"" type=""text/css"" />";
        private static readonly Regex CssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>(?![\s]*<!\[endif]-->)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string FileName
        {
            get { return "RequestReducedStyle.css"; }
        }

        public IEnumerable<string> SupportedMimeTypes
        {
            get { return new[] { "text/css" }; }
        }

        public string TransformedMarkupTag(string url)
        {
            return string.Format(cssFormat, url);
        }

        public Regex ResourceRegex
        {
            get { return CssPattern; }
        }


        public Func<string, string, bool> TagValidator { get; set; }
    }
}
