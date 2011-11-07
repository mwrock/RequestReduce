using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace RequestReduce.ResourceTypes
{
    public class CssResource : IResourceType
    {
        private const string CssFormat = @"<link href=""{0}"" rel=""Stylesheet"" type=""text/css"" />";
        private readonly Regex cssPattern = new Regex(@"<link[^>]+rel=""?stylesheet""?[^>]+>(?![\s]*<!\[endif]-->)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            return string.Format(CssFormat, url);
        }

        public Regex ResourceRegex
        {
            get { return cssPattern; }
        }


        public Func<string, string, bool> TagValidator { get; set; }
    }
}
