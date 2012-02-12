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
        string EmptyDeferredBundles();
    }

    public class ResponseTransformer : IResponseTransformer
    {
        private readonly IReductionRepository reductionRepository;
        private readonly IRRConfiguration config;
        private readonly IUriBuilder uriBuilder;
        private static readonly RegexCache Regex = new RegexCache();
        private readonly IReducingQueue reducingQueue;
        private readonly HttpContextBase context;
        private readonly Dictionary<IResourceType, Dictionary<int, IList<KeyValuePair<string, string>>>> deferredResources = new Dictionary<IResourceType, Dictionary<int, IList<KeyValuePair<string, string>>>>();

        public ResponseTransformer(IReductionRepository reductionRepository, IReducingQueue reducingQueue, HttpContextBase context, IRRConfiguration config, IUriBuilder uriBuilder)
        {
            this.reductionRepository = reductionRepository;
            this.reducingQueue = reducingQueue;
            this.context = context;
            this.config = config;
            this.uriBuilder = uriBuilder;
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
                int bundle = 0;
                for (int cursor = 0; cursor < matches.Count; cursor++)
                {
                    var match = matches[cursor];
                    var strMatch = match.ToString();
                    var urlMatch = Regex.UrlPattern.Match(strMatch);
                    bool matched = false;
                    if (urlMatch.Success)
                    {
                        var url = RelativeToAbsoluteUtility.ToAbsolute(config.BaseAddress == null ? context.Request.Url : new Uri(config.BaseAddress), urlMatch.Groups["url"].Value);
                        if ((resource.TagValidator == null || resource.TagValidator(strMatch, url)) && (RRContainer.Current.GetAllInstances<IFilter>().Where(x => (x is CssFilter && typeof(T) == typeof(CssResource)) || (x is JavascriptFilter && typeof(T) == typeof(JavaScriptResource))).FirstOrDefault(y => y.IgnoreTarget(new CssJsFilterContext(context.Request, url, strMatch))) == null))
                        {
                            int matchBundle = resource.Bundle(strMatch);
                            if(!resource.IsDeferred(matchBundle))
                            {
                                if ((transformableMatches.Count == 0) || (matchBundle == bundle))
                                {
                                    matched = true;
                                    bundle = resource.Bundle(strMatch);
                                    urls.Append(url);
                                    urls.Append(GetMedia(strMatch));
                                    urls.Append("::");
                                    transformableMatches.Add(strMatch);
                                }
                                else
                                {
                                    cursor--; // This resource into next bundle
                                }
                            }
                            else
                            {
                                // Removed deferred resource
                                var idx = preTransform.IndexOf(strMatch, StringComparison.Ordinal);
                                preTransform = preTransform.Remove(idx, strMatch.Length);

                                // Add to deferred resource bundle
                                if(!deferredResources.ContainsKey(resource))
                                {
                                    deferredResources[resource] = new Dictionary<int, IList<KeyValuePair<string, string>>>();
                                }
                                var deferredResourceBundles = deferredResources[resource];
                                if(!deferredResourceBundles.ContainsKey(matchBundle))
                                {
                                    deferredResourceBundles[matchBundle] = new List<KeyValuePair<string, string>>();
                                }
                                var deferredBundle = deferredResourceBundles[matchBundle];
                                deferredBundle.Add(new KeyValuePair<string, string>(url, strMatch));
                            }
                        }
                    }
                    if (!matched && transformableMatches.Count > 0)
                    {
                        preTransform = DoTransform<T>(preTransform, urls, transformableMatches, noCommentTransform, bundle);
                        urls.Length = 0;
                        transformableMatches.Clear();
                    }
                }
                if (transformableMatches.Count > 0)
                {
                    preTransform = DoTransform<T>(preTransform, urls, transformableMatches, noCommentTransform, bundle);
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
                return "^" + mediaMatch.Groups["media"].Value;
            return null;
        }

        private string DoTransform<T>(string preTransform, StringBuilder urls, List<string> transformableMatches, string noCommentTransform, int bundle) where T : IResourceType
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
                    var firstScriptIndex = firstScript.Success ? preTransform.IndexOf(firstScript.ToString(), System.StringComparison.Ordinal) : -1;
                    var insertionIdx = (firstScript.Success && firstScriptIndex <
                                        preTransform.IndexOf(transformableMatches[0], StringComparison.Ordinal) && resource is CssResource)
                                           ? firstScriptIndex - 1
                                           : preTransform.IndexOf(transformableMatches[0], StringComparison.Ordinal) - 1;
                    preTransform = preTransform.Insert(insertionIdx + 1, resource.TransformedMarkupTag(transform, bundle));
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
            reducingQueue.Enqueue(new QueueItem<T> { Urls = urls.ToString() });
            RRTracer.Trace("No reduction found for {0}. Enqueuing.", urls);
            return preTransform;
        }

        // Last chance to empty deferred buckets
        public string EmptyDeferredBundles()
        {
            return EmptyDeferredResourceBundle<JavaScriptResource>();
        }

        public string EmptyDeferredResourceBundle<T>() where T : IResourceType
        {
            var resource = RRContainer.Current.GetInstance<T>();
            StringBuilder completeTransform = new StringBuilder();

            if (deferredResources.ContainsKey(resource))
            {
                var bundles = deferredResources[resource];
                foreach (int bundle in bundles.Keys)
                {
                    if (bundles.ContainsKey(bundle))
                    {
                        var urls = new StringBuilder();
                        var transformableMatches = new List<string>();
                        StringBuilder transformBuilder = new StringBuilder();

                        var matches = bundles[bundle];
                        foreach (var match in matches)
                        {
                            string url = match.Key;
                            string strMatch = match.Value;
                            urls.Append(url);
                            urls.Append(GetMedia(strMatch));
                            urls.Append("::");
                            transformableMatches.Add(strMatch);
                            transformBuilder.Append(strMatch);
                        }

                        string transform = transformBuilder.ToString();
                        var noCommentTransform = Regex.HtmlCommentPattern.Replace(transform, string.Empty);
                        transform = DoTransform<T>(transform, urls, transformableMatches, noCommentTransform, bundle);
                        completeTransform.Append(transform);
                    } 
                }
            }
            return completeTransform.ToString();
        }
        
    }
}
