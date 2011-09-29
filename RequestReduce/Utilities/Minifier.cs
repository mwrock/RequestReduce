using Microsoft.Ajax.Utilities;
using RequestReduce.Reducer;
using System;
using System.Security.AccessControl;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Utilities
{
    public class Minifier : IMinifier
    {
        private Microsoft.Ajax.Utilities.Minifier minifier = new Microsoft.Ajax.Utilities.Minifier();

        public string Minify<T>(string unMinifiedContent) where T : IResourceType
        {
            if(typeof(T) == typeof(CssResource))
                return minifier.MinifyStyleSheet(unMinifiedContent);
            if(typeof(T) == typeof(JavaScriptResource))
                return minifier.MinifyJavaScript(unMinifiedContent);

            throw new ArgumentException("Cannot Minify Resources of unknown type", "resourceType");
        }

    }
}