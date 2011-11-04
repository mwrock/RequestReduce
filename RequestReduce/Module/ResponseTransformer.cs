using System.Linq;
using System.Text;
using System.Web;
using RequestReduce.Utilities;
using System;
using RequestReduce.ResourceTypes;
using RequestReduce.Configuration;
using System.Collections.Generic;
using RequestReduce.IOC;

namespace RequestReduce.Module
{
    public interface IResponseTransformer
    {
        string Transform(string preTransform);
    }

    public class ResponseTransformer : IResponseTransformer
    {
        private readonly IReductionRepository reductionRepository;
        private readonly IRRConfiguration config;
        private static readonly RegexCache Regex = new RegexCache();
        private readonly IReducingQueue reducingQueue;
        private readonly HttpContextBase context;

        public ResponseTransformer(IReductionRepository reductionRepository, IReducingQueue reducingQueue, HttpContextBase context, IRRConfiguration config)
        {
            this.reductionRepository = reductionRepository;
            this.reducingQueue = reducingQueue;
            this.context = context;
            this.config = config;
        }

        public string Transform(string preTransform)
        {
            if (!config.JavaScriptProcesingDisabled) preTransform = Transform<JavaScriptResource>(preTransform);
            if(!config.CssProcesingDisabled) preTransform = Transform<CssResource>(preTransform);

            return preTransform;
        }

        private string Transform<T>(string preTransform) where T : IResourceType
        {
            var resource = RRContainer.Current.GetInstance<T>();
            var matches = resource.ResourceRegex.Matches(preTransform);
            if (matches.Count > 0)
            {
                var urls = new StringBuilder();
                var transformableMatches = new List<string>();
                foreach (var match in matches)
                {
                    var strMatch = match.ToString();
                    var urlMatch = Regex.UrlPattern.Match(strMatch);
                    bool matched = false;
                    if (urlMatch.Success)
                    {
                        if (resource.TagValidator == null || resource.TagValidator(strMatch, urlMatch.Groups["url"].Value))
                        {
                            matched = true;
                            urls.Append(RelativeToAbsoluteUtility.ToAbsolute(context.Request.Url, urlMatch.Groups["url"].Value));
                            urls.Append(GetMedia(strMatch));
                            urls.Append("::");
                            transformableMatches.Add(strMatch);
                        }
                    }
                    if (!matched && transformableMatches.Count > 0)
                    {
                        preTransform = DoTransform<T>(preTransform, urls, transformableMatches);
                        urls.Length = 0;
                        transformableMatches.Clear();
                    }
                }
                if (transformableMatches.Count > 0)
                {
                    preTransform = DoTransform<T>(preTransform, urls, transformableMatches);
                    urls.Length = 0;
                    transformableMatches.Clear();
                }
            }
            return preTransform;
        }

        private string GetMedia(string strMatch)
        {
            var mediaMatch = Regex.MediaPattern.Match(strMatch);
            if (mediaMatch.Success)
                return "|" + mediaMatch.Groups["media"].Value;
            return null;
        }

        private string DoTransform<T>(string preTransform, StringBuilder urls, List<string> transformableMatches) where T : IResourceType
        {
            var resource = RRContainer.Current.GetInstance<T>();
            RRTracer.Trace("Looking for reduction for {0}", urls);
            var transform = reductionRepository.FindReduction(urls.ToString());
            if (transform != null)
            {
                RRTracer.Trace("Reduction found for {0}", urls);
                var closeHeadIdx = (preTransform.StartsWith("<head", StringComparison.OrdinalIgnoreCase) && resource is CssResource) ? preTransform.IndexOf('>') : preTransform.IndexOf(transformableMatches[0])-1;
                preTransform = preTransform.Insert(closeHeadIdx + 1, resource.TransformedMarkupTag(transform));
                return transformableMatches.Aggregate(preTransform, (current, match) => current.Replace(match, ""));
            }
            reducingQueue.Enqueue(new QueueItem<T> { Urls = urls.ToString() });
            RRTracer.Trace("No reduction found for {0}. Enqueuing.", urls);
            return preTransform;
        }
    }
}
