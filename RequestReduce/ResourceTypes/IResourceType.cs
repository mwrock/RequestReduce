using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace RequestReduce.ResourceTypes
{
    public interface  IResourceType
    {
        string FileName { get; }
        IEnumerable<string> SupportedMimeTypes { get; }
        string TransformedMarkupTag(string url);
        Regex ResourceRegex { get; }
        Func<string, string, bool> TagValidator { get; set; }
    }
}
