using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using RequestReduce.Utilities;
using System;

namespace RequestReduce.Module
{
    public interface IResponseTransformer
    {
        string Transform(string preTransform);
    }

    public class ResponseTransformer : IResponseTransformer
    {
        private readonly IReductionRepository reductionRepository;
        private static readonly Regex CssPattern = new Regex(@"<link[^>]+type=""?text/css""?[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScriptPattern = new Regex(@"<script[^>]+src=['""]?.*?['""]?[^>]+>\s*?(</script>)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex UrlPattern = new Regex(@"(href|src)=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly string cssFormat = @"<link href=""{0}"" rel=""Stylesheet"" type=""text/css"" />";
        private static readonly string scriptFormat = @"<script src=""{0}"" type=""text/javascript"" ></script>";
        private readonly IReducingQueue reducingQueue;
        private readonly HttpContextBase context;

        public ResponseTransformer(IReductionRepository reductionRepository, IReducingQueue reducingQueue, HttpContextBase context)
        {
            this.reductionRepository = reductionRepository;
            this.reducingQueue = reducingQueue;
            this.context = context;
        }

        public string Transform(string preTransform)
        {
            preTransform = Transform(preTransform, CssPattern, cssFormat, reducingQueue.EnqueueCss);
            preTransform = Transform(preTransform, ScriptPattern, scriptFormat, reducingQueue.EnqueueJavaScript);

            return preTransform;
        }

        private string Transform(string preTransform, Regex markupPattern, string markupFormat, Action<string> enqueueFunc)
        {
            var matches = markupPattern.Matches(preTransform);
            if (matches.Count > 0)
            {
                var urls = new StringBuilder();
                foreach (var match in matches)
                {
                    var urlMatch = UrlPattern.Match(match.ToString());
                    if (urlMatch.Success)
                    {
                        urls.Append(RelativeToAbsoluteUtility.ToAbsolute(context.Request.Url, urlMatch.Groups["url"].Value));
                        urls.Append("::");
                    }
                }
                RRTracer.Trace("Looking for reduction for {0}", urls);
                var transform = reductionRepository.FindReduction(urls.ToString());
                if (transform != null)
                {
                    RRTracer.Trace("Reduction found for {0}", urls);
                    var closeHeadIdx = preTransform.IndexOf('>');
                    preTransform = preTransform.Insert(closeHeadIdx + 1, string.Format(markupFormat, transform));
                    foreach (var match in matches)
                        preTransform = preTransform.Replace(match.ToString(), "");
                    return preTransform;
                }
                enqueueFunc(urls.ToString());
                RRTracer.Trace("No reduction found for {0}. Enqueuing.", urls);
            }
            return preTransform;
        }
    }
}
