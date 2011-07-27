using Microsoft.Ajax.Utilities;

namespace RequestReduce.Utilities
{
    public class Minifier : IMinifier
    {
        private Microsoft.Ajax.Utilities.Minifier minifier = new Microsoft.Ajax.Utilities.Minifier();

        public string Minify(string unMinifiedContent)
        {
            return minifier.MinifyStyleSheet(unMinifiedContent);
        }
    }
}