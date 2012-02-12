using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

namespace RequestReduce.ResourceTypes
{
    public interface  IResourceType
    {
        string FileName { get; }
        IEnumerable<string> SupportedMimeTypes { get; }
        int BundleId(string resource);
        bool IsLoadDeferred(int bundle);
        bool IsDynamicLoad(int bundle);
        string TransformedMarkupTag(string url, int bundle);
        Regex ResourceRegex { get; }
        Func<string, string, bool> TagValidator { get; set; }
    }
}
