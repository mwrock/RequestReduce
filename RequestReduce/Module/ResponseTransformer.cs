using System.Linq;
using System.Text;
using System.Web;
using RequestReduce.Api;
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
        private readonly IUriBuilder uriBuilder;
        private readonly IRelativeToAbsoluteUtility relativeToAbsoluteUtility;
        private static readonly RegexCache Regex = new RegexCache();
        private readonly IReducingQueue reducingQueue;
        private readonly HttpContextBase context;

        public ResponseTransformer(IReductionRepository reductionRepository, IReducingQueue reducingQueue, HttpContextBase context, IRRConfiguration config, IUriBuilder uriBuilder, IRelativeToAbsoluteUtility relativeToAbsoluteUtility)
        {
            this.reductionRepository = reductionRepository;
            this.reducingQueue = reducingQueue;
            this.context = context;
            this.config = config;
            this.uriBuilder = uriBuilder;
            this.relativeToAbsoluteUtility = relativeToAbsoluteUtility;
        }

        public string Transform(string preTransform)
        {
            if (!config.JavaScriptProcessingDisabled) preTransform = Transform<JavaScriptResource>(preTransform);
            if(!config.CssProcessingDisabled) preTransform = Transform<CssResource>(preTransform);

            return preTransform;
        }

        private string Transform<T>(string preTransform) where T : IResourceType
        {
            var noCommentTransform = Regex.HtmlCommentPattern.Replace(preTransform, string.Empty);
            var resource = RRContainer.Current.GetInstance<T>();
            var matches = resource.ResourceRegex.Matches(noCommentTransform);
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
                        var url = relativeToAbsoluteUtility.ToAbsolute(config.BaseAddress == null ? context.Request.Url : new Uri(config.BaseAddress), urlMatch.Groups["url"].Value);
                        if ((resource.TagValidator == null || resource.TagValidator(strMatch, url)) && (RRContainer.Current.GetAllInstances<IFilter>().Where(x => (x is CssFilter && typeof(T) == typeof(CssResource)) || (x is JavascriptFilter && typeof(T) == typeof(JavaScriptResource))).FirstOrDefault(y => y.IgnoreTarget(new CssJsFilterContext(context.Request, url, strMatch))) == null))
                        {
                            matched = true;
                            urls.Append(url);
                            urls.Append(GetMedia(strMatch));
                            urls.Append("::");
                            transformableMatches.Add(strMatch);
                        }
                    }
                    if (!matched && transformableMatches.Count > 0)
                    {
                        preTransform = DoTransform<T>(preTransform, urls, transformableMatches, noCommentTransform);
                        urls.Length = 0;
                        transformableMatches.Clear();
                    }
                }
                if (transformableMatches.Count > 0)
                {
                    preTransform = DoTransform<T>(preTransform, urls, transformableMatches, noCommentTransform);
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
            {
                var media = mediaMatch.Groups["media"].Value;
                if(!media.Trim().Equals("all", StringComparison.InvariantCultureIgnoreCase))
                    return "^" + media;
            }
            return null;
        }

        private string DoTransform<T>(string preTransform, StringBuilder urls, List<string> transformableMatches, string noCommentTransform) where T : IResourceType
        {
            var resource = RRContainer.Current.GetInstance<T>();
            RRTracer.Trace("Looking for reduction for {0}", urls);
            var transform = reductionRepository.FindReduction(urls.ToString());
            if (transform != null)
            {
                RRTracer.Trace("Reduction found for {0}", urls);
                if(uriBuilder.ParseSignature(transform) != Guid.Empty.RemoveDashes())
                {
                    var firstScript =
                        RRContainer.Current.GetInstance<JavaScriptResource>().ResourceRegex.Match(noCommentTransform);
                    var firstScriptIndex = firstScript.Success ? preTransform.IndexOf(firstScript.ToString(), StringComparison.Ordinal) : -1;
                    var insertionIdx = (firstScript.Success && firstScriptIndex <
                                        preTransform.IndexOf(transformableMatches[0], StringComparison.Ordinal) && resource is CssResource)
                                           ? firstScriptIndex - 1
                                           : preTransform.IndexOf(transformableMatches[0], StringComparison.Ordinal) - 1;
                    preTransform = preTransform.Insert(insertionIdx + 1, resource.TransformedMarkupTag(transform));
                }
                var result = preTransform;
                foreach (var match in transformableMatches)
                {
                    var idx = result.IndexOf(match, StringComparison.Ordinal);
                    result = result.Remove(idx, match.Length);
                    if(idx == result.Length)
                        continue;
                    if(result[idx] == '\r')
                        result = result.Remove(idx, 1);
                    if (result[idx] == '\n')
                        result = result.Remove(idx, 1);
                }
                return result;
            }
            var host = string.Empty;
            if(!string.IsNullOrEmpty(config.ContentHost))
            {
                var uri = context.Request.Url;
                if(uri != null)
                    host = string.Format("{0}://{1}/", uri.Scheme, uri.Host);
            }
            reducingQueue.Enqueue(new QueueItem<T> { Urls = urls.ToString(), Host = host});
            RRTracer.Trace("No reduction found for {0}. Enqueuing.", urls);
            return preTransform;
        }
    }
}
