using System;
using Microsoft.Ajax.Utilities;

namespace RequestReduce.Reducer
{
    public class Minifier : IMinifier
    {
        private Microsoft.Ajax.Utilities.Minifier minifier = new Microsoft.Ajax.Utilities.Minifier();
        private CssSettings settings = new CssSettings();

        public Minifier()
        {
            settings.CommentMode = CssComment.Hacks;
        }

        public string Minify(string unMinifiedContent)
        {
            return minifier.MinifyStyleSheet(unMinifiedContent);
        }
    }
}