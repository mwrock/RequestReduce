﻿using System;
using RequestReduce.Configuration;

namespace RequestReduce.Utilities
{
    public interface IUriBuilder
    {
        string BuildCssUrl(Guid key, byte[] bytes);
        string BuildCssUrl(Guid key, string signature);
        string BuildJavaScriptUrl(Guid key, byte[] bytes);
        string BuildJavaScriptUrl(Guid key, string signature);
        string BuildSpriteUrl(Guid key, byte[] bytes);
        string ParseFileName(string url);
        Guid ParseKey(string url);
        string ParseSignature(string url);
    }

    public class UriBuilder : IUriBuilder
    {
        private readonly IRRConfiguration configuration;
        public const string CssFileName = "RequestReducedStyle.css";
        public const string JsFileName = "RequestReducedStyle.js";

        public UriBuilder(IRRConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string BuildCssUrl(Guid key, byte[] bytes)
        {
            return BuildCssUrl(key, Hasher.Hash(bytes).RemoveDashes());
        }

        public string BuildCssUrl(Guid key, string signature)
        {
            return string.Format("{0}{1}/{2}-{3}-{4}", configuration.ContentHost, configuration.SpriteVirtualPath, key.RemoveDashes(), signature, CssFileName);
        }

        public string BuildJavaScriptUrl(Guid key, byte[] bytes)
        {
            return BuildJavaScriptUrl(key, Hasher.Hash(bytes).RemoveDashes());
        }

        public string BuildJavaScriptUrl(Guid key, string signature)
        {
            return string.Format("{0}{1}/{2}-{3}-{4}", configuration.ContentHost, configuration.SpriteVirtualPath, key.RemoveDashes(), signature, JsFileName);
        }

        public string BuildSpriteUrl(Guid key, byte[] bytes)
        {
            return string.Format("{0}{1}/{2}-{3}.png", configuration.ContentHost, configuration.SpriteVirtualPath, key.RemoveDashes(), Hasher.Hash(bytes).RemoveDashes());
        }

        public string ParseFileName(string url)
        {
            return url.Substring(url.LastIndexOf('/') + 1);
        }

        public Guid ParseKey(string url)
        {
            var idx = url.LastIndexOf('/');
            string keyDir = idx > -1 ? url.Substring(idx + 1) : url;
            string strKey = string.Empty;
            idx = keyDir.IndexOf('-');
            if (idx > -1)
                strKey = keyDir.Substring(0, idx);
            Guid key;
            Guid.TryParse(strKey, out key);
            return key;
        }

        public string ParseSignature(string url)
        {
            var idx = url.LastIndexOf('/');
            var keyDir = idx > -1 ? url.Substring(idx + 1) : url;
            var strKey = string.Empty;
            try
            {
                strKey = keyDir.Substring(33, 32);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            Guid key;
            Guid.TryParse(strKey, out key);
            return key.RemoveDashes();
        }
    }
}
