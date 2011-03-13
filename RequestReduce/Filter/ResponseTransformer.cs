using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RequestReduce.Filter
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

        public ResponseTransformer(IReductionRepository reductionRepository)
        {
            this.reductionRepository = reductionRepository;
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
                        urls.Append(urlMatch.Groups["url"].Value);
                        urls.Append("::");
                    }
                }
                var transform = reductionRepository.FindReduction(urls.ToString());
                if(transform != null)
                {
                    var isTransformed = false;
                    foreach (var match in matches)
                    {
                        if(!isTransformed)
                        {
                            var urlMatch = UrlPattern.Match(match.ToString());
                            if (urlMatch.Success)
                            {
                                preTransform = preTransform.Replace(urlMatch.Groups["url"].Value, transform);
                                isTransformed = true;
                            }
                            else
                                preTransform = preTransform.Replace(match.ToString(), "");
                        }
                        else
                            preTransform = preTransform.Replace(match.ToString(), "");
                    }
                    return preTransform;
                }
            }
            return preTransform;
        }
    }
}
