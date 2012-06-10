using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace RequestReduce.Configuration
{
    public enum Store
    {
        LocalDiskStore,
        SqlServerStore
    }
    public interface IRRConfiguration
    {
        [Obsolete("Use ResourceVirtualPath")]
        string SpriteVirtualPath { get; set; }
        [Obsolete("Use ResourcePhysicalPath")]
        string SpritePhysicalPath { get; set; }
        string ResourceVirtualPath { get; set; }
        string ResourceAbsolutePath { get; }
        string ResourcePhysicalPath { get; set; }
        string ContentHost { get; set; }
        string ConnectionStringName { get; set; }
        Store ContentStore { get; }
        int SpriteSizeLimit { get; set; }
        IEnumerable<string> AuthorizedUserList { get; set; }
        IEnumerable<string> IpFilterList { get; set; }
        IEnumerable<string> ProxyList { get; set; }
        bool CssProcessingDisabled { get; set; }
        bool JavaScriptProcessingDisabled { get; set; }
        bool ImageOptimizationDisabled { get; set; }
        int ImageOptimizationCompressionLevel { get; set; }
        bool ImageQuantizationDisabled { get; set; }
        int SpriteColorLimit { get; set; }
        int StorePollInterval { get; set; }
        bool IsFullTrust { get; }
        event Action PhysicalPathChange;
        string JavaScriptUrlsToIgnore { get; set; }
        bool ImageSpritingDisabled { get; set; }
        bool IgnoreNearFutureJavascriptDisabled { get; set; }
        string BaseAddress { get; set; }
    }

    public class RRConfiguration : IRRConfiguration
    {
        private readonly RequestReduceConfigSection config = ConfigurationManager.GetSection("RequestReduce") as RequestReduceConfigSection;
        private string spritePhysicalPath;
        private readonly Store contentStore = Store.LocalDiskStore;
        private string resourceVirtualPath;
        public static readonly IEnumerable<string> Anonymous = new[] { "Anonymous" };

        public string BaseAddress { get; set; }
        public bool CssProcessingDisabled { get; set; }
        public bool JavaScriptProcessingDisabled { get; set; }
        public bool ImageOptimizationDisabled { get; set; }
        public bool ImageQuantizationDisabled { get; set; }
        public bool ImageSpritingDisabled { get; set; }
        public bool IgnoreNearFutureJavascriptDisabled { get; set; }

        public int StorePollInterval { get; set; }

        public event Action PhysicalPathChange;

        public RRConfiguration()
        {
            IsFullTrust = GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted;
            ContentHost = config == null ? null : config.ContentHost;
            AuthorizedUserList = config == null || string.IsNullOrEmpty(config.AuthorizedUserList) ? Anonymous : config.AuthorizedUserList.Split(',').Length == 0 ? Anonymous : config.AuthorizedUserList.Split(',').ToList();
            IpFilterList = config == null || string.IsNullOrEmpty(config.IpFilterList) ? new List<string> { } : config.IpFilterList.Split(',').Length == 0 ? new List<string> { } : config.IpFilterList.Split(',').ToList();
            ProxyList = config == null || string.IsNullOrEmpty(config.ProxyList) ? new List<string> { } : config.ProxyList.Split(',').Length == 0 ? new List<string> { } : config.ProxyList.Split(',').ToList();
            var val = config == null ? 0 : config.SpriteSizeLimit;
            SpriteSizeLimit = val == 0 ? 50000 : val;
            val = config == null ? 0 : config.SpriteColorLimit;
            SpriteColorLimit = val == 0 ? 5000 : val;
            val = config == null ? 0 : config.StorePollInterval;
            StorePollInterval = val <= 0 ? Timeout.Infinite : val;
            val = config == null ? 0 : config.ImageOptimizationCompressionLevel;
            ImageOptimizationCompressionLevel = val == 0 ? 5 : val;
            CssProcessingDisabled = config != null && (config.CssProcesingDisabled || config.CssProcessingDisabled);
            JavaScriptProcessingDisabled = config != null && (config.JavaScriptProcesingDisabled || config.JavaScriptProcessingDisabled);
            ImageOptimizationDisabled = config != null && config.ImageOptimizationDisabled;
            ImageQuantizationDisabled = config != null && config.ImageQuantizationDisabled;
            ImageSpritingDisabled = config != null && config.ImageSpritingDisabled;
            IgnoreNearFutureJavascriptDisabled = config != null && config.IgnoreNearFutureJavascriptDisabled;
            ResourceVirtualPath = config == null || string.IsNullOrEmpty(config.SpriteVirtualPath) ? "~/RequestReduceContent" : config.SpriteVirtualPath;
            JavaScriptUrlsToIgnore = config == null || string.IsNullOrEmpty(config.JavaScriptUrlsToIgnore)
                                    ? "ajax.googleapis.com/ajax/libs/jquery/,ajax.aspnetcdn.com/ajax/jQuery/"
                                    : config.JavaScriptUrlsToIgnore;
            spritePhysicalPath = config == null ? null : string.IsNullOrEmpty(config.SpritePhysicalPath) ? null : config.SpritePhysicalPath;
            if (config != null && !string.IsNullOrEmpty(config.ContentStore))
            {
                try
                {
                    contentStore = (Store)Enum.Parse(typeof(Store), config.ContentStore, true);
                }
                catch (ArgumentException ex)
                {
                    throw new ConfigurationErrorsException(string.Format("{0} is not a valid Content Store.", config.ContentStore), ex);
                }
            }
            ConnectionStringName = config == null ? null : config.ConnectionStringName;
            if (ContentStore == Store.SqlServerStore && string.IsNullOrEmpty(ConnectionStringName))
                throw new ApplicationException("Your RequestReduce configuration is missing a connectionStringName. This is required with a SqlServerStore. This name should either correspond to one of your connectionStrings configs, a database name in sql express or a full sql connection string. The database must contain the RequestReduceFiles table which you can create using the script located either in the Tools directory of the Nuget package install or in the download zip.");

            CreatePhysicalPath();
        }

        private string GetAbsolutePath(string spriteVirtualPath)
        {
            if (HttpContext.Current != null)
                return VirtualPathUtility.ToAbsolute(spriteVirtualPath);
            return spriteVirtualPath.Replace("~", "");
        }

        public int SpriteColorLimit { get; set; }

        public IEnumerable<string> AuthorizedUserList { get; set; }

        public IEnumerable<string> IpFilterList { get; set; }
        public IEnumerable<string> ProxyList { get; set; }

        public string SpriteVirtualPath
        {
            get { return ResourceVirtualPath; }
            set { ResourceVirtualPath = value; }
        }
        public string SpritePhysicalPath
        {
            get { return ResourcePhysicalPath; }
            set { ResourcePhysicalPath = value; }
        }
        public string ResourceVirtualPath
        {
            get { return resourceVirtualPath; }
            set 
            { 
                resourceVirtualPath = value;
                ResourceAbsolutePath = GetAbsolutePath(value);
            }
        }

        public string ResourceAbsolutePath { get; private set; }

        public string ResourcePhysicalPath
        {
            get { return spritePhysicalPath; }
            set
            {
                spritePhysicalPath = value;
                CreatePhysicalPath();
                if (PhysicalPathChange != null)
                    PhysicalPathChange();
            }
        }

        public string ContentHost { get; set; }

        public string ConnectionStringName  { get; set; }

        public Store ContentStore
        {
            get { return contentStore; }
        }

        public int SpriteSizeLimit { get; set; }
        public int ImageOptimizationCompressionLevel { get; set; }
        public bool IsFullTrust { get; private set; }
        private void CreatePhysicalPath()
        {
            if (!string.IsNullOrEmpty(spritePhysicalPath) && !Directory.Exists(spritePhysicalPath))
            {
                Directory.CreateDirectory(spritePhysicalPath);
                while (!Directory.Exists(spritePhysicalPath))
                    Thread.Sleep(5000);
                if(!Directory.Exists(spritePhysicalPath))
                    throw new IOException(string.Format("unable to create {0}", spritePhysicalPath));
            }
        }

        // Based on 
        // http://blogs.msdn.com/b/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx
        public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            foreach (var trustLevel in
                    new[] {
                    AspNetHostingPermissionLevel.Unrestricted,
                    AspNetHostingPermissionLevel.High,
                    AspNetHostingPermissionLevel.Medium,
                    AspNetHostingPermissionLevel.Low,
                    AspNetHostingPermissionLevel.Minimal 
                })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                }
                catch (Exception)
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
        }


        public string JavaScriptUrlsToIgnore { get; set; }
    }

    public static class ConfigExtensions
    {
        public static bool AllowsAnonymous(this IEnumerable<string> list)
        {
            var isAnon = false;
            var count = 0;
            foreach (string s in list)
            {
                if (++count > 1)
                    return false;
                if (s.Equals(RRConfiguration.Anonymous.First(), StringComparison.OrdinalIgnoreCase))
                    isAnon = true;
            }
            return isAnon;
        }
    }
}