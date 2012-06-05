using Microsoft.Ajax.Utilities;
using System;
using RequestReduce.ResourceTypes;

namespace RequestReduce.Utilities
{
    public class Minifier : IMinifier
    {
        protected readonly Microsoft.Ajax.Utilities.Minifier minifier = new Microsoft.Ajax.Utilities.Minifier();
        protected readonly CodeSettings settings = new CodeSettings { EvalTreatment = EvalTreatment.MakeAllSafe };

        public virtual string Minify<T>(string unMinifiedContent) where T : IResourceType
        {
            if (typeof(T) == typeof(CssResource))
                return minifier.MinifyStyleSheet(unMinifiedContent);
            if (typeof(T) == typeof(JavaScriptResource))
                return  minifier.MinifyJavaScript(unMinifiedContent, settings);

            throw new ArgumentException("Cannot Minify ResourceStrings of unknown type", "unMinifiedContent");
        }

    }
}