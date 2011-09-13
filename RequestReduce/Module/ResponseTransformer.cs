using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using RequestReduce.Utilities;

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
        private static readonly Regex UrlPattern = new Regex(@"href=""?(?<url>[^"" ]+)""?[^ />]+[ />]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly string cssFormat = @"<link href=""{0}"" rel=""Stylesheet"" type=""text/css"" />";
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
            var matches = CssPattern.Matches(preTransform);
            if(matches.Count > 0)
            {
                var urls = new StringBuilder();
                foreach (var match in matches)
                {
                    var urlMatch = UrlPattern.Match(match.ToString());
                    if(urlMatch.Success)
                    {
                        urls.Append(RelativeToAbsoluteUtility.ToAbsolute(context.Request.Url, urlMatch.Groups["url"].Value));
                        urls.Append("::");
                    }
                }
                RRTracer.Trace("Looking for reduction for {0}", urls);
                var transform = reductionRepository.FindReduction(urls.ToString());
                if(transform != null)
                {
                    RRTracer.Trace("Reduction found for {0}", urls);
                    var closeHeadIdx = preTransform.IndexOf('>');
                    preTransform = preTransform.Insert(closeHeadIdx+1, string.Format(cssFormat, transform));
                    foreach (var match in matches)
                        preTransform = preTransform.Replace(match.ToString(), "");
                    return preTransform;
                }
                reducingQueue.EnqueueCss(urls.ToString());
                RRTracer.Trace("No reduction found for {0}. Enqueuing.", urls);
            }
            return preTransform;
        }
    }
}
