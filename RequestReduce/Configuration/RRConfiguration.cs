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
        string SpriteVirtualPath { get; set; }
        string SpritePhysicalPath { get; set; }
        string ContentHost { get; }
        string ConnectionStringName { get; set; }
        Store ContentStore { get; }
        int SpriteSizeLimit { get; set; }
        IEnumerable<string> AuthorizedUserList { get; set; }
        bool CssProcesingDisabled { get; set; }
        bool JavaScriptProcesingDisabled { get; set; }
        bool ImageOptimizationDisabled { get; set; }
        int ImageOptimizationCompressionLevel { get; set; }
        bool ImageQuantizationDisabled { get; set; }
        int SpriteColorLimit { get; set; }
        int StorePollInterval { get; set; }
        bool IsFullTrust { get; }
        event Action PhysicalPathChange;
        string JavaScriptUrlsToIgnore { get; set; }
        bool ImageSpritingDisabled { get; set; }
    }

    public class RRConfiguration : IRRConfiguration
    {
        private readonly RequestReduceConfigSection config = ConfigurationManager.GetSection("RequestReduce") as RequestReduceConfigSection;
        private string spritePhysicalPath;
        private readonly Store contentStore = Store.LocalDiskStore;
        public static readonly IEnumerable<string> Anonymous = new[] { "Anonymous" };

        public bool CssProcesingDisabled { get; set; }
        public bool JavaScriptProcesingDisabled { get; set; }
        public bool ImageOptimizationDisabled { get; set; }
        public bool ImageQuantizationDisabled { get; set; }
        public bool ImageSpritingDisabled { get; set; }

        public int StorePollInterval { get; set; }

        public event Action PhysicalPathChange;

        public RRConfiguration()
        {
            IsFullTrust = GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted;
            AuthorizedUserList = config == null || string.IsNullOrEmpty(config.AuthorizedUserList) ? Anonymous : config.AuthorizedUserList.Split(',').Length == 0 ? Anonymous : config.AuthorizedUserList.Split(',');
            var val = config == null ? 0 : config.SpriteSizeLimit;
            SpriteSizeLimit = val == 0 ? 50000 : val;
            val = config == null ? 0 : config.SpriteColorLimit;
            SpriteColorLimit = val == 0 ? 5000 : val;
            val = config == null ? 0 : config.StorePollInterval;
            StorePollInterval = val <= 0 ? Timeout.Infinite : val;
            val = config == null ? 0 : config.ImageOptimizationCompressionLevel;
            ImageOptimizationCompressionLevel = val == 0 ? 5 : val;
            CssProcesingDisabled = config == null ? false : config.CssProcesingDisabled;
            JavaScriptProcesingDisabled = config == null ? false : config.JavaScriptProcesingDisabled;
            ImageOptimizationDisabled = config == null ? false : config.ImageOptimizationDisabled;
            ImageQuantizationDisabled = config == null ? false : config.ImageQuantizationDisabled;
            ImageSpritingDisabled = config == null ? false : config.ImageSpritingDisabled;
            SpriteVirtualPath = config == null || string.IsNullOrEmpty(config.SpriteVirtualPath)
                                    ? GetAbsolutePath("~/RequestReduceContent")
                                    : GetAbsolutePath(config.SpriteVirtualPath);
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
            else
                return spriteVirtualPath.Replace("~", "");
        }

        public int SpriteColorLimit { get; set; }

        public IEnumerable<string> AuthorizedUserList { get; set; }

        public string SpriteVirtualPath { get; set; }

        public string SpritePhysicalPath
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

        public string ContentHost
        {
            get { return config == null ? null : config.ContentHost; }
        }

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
                    Thread.Sleep(0);
            }
        }

        // Based on 
        // http://blogs.msdn.com/b/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx
        public static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            foreach (AspNetHostingPermissionLevel trustLevel in
                    new AspNetHostingPermissionLevel[] {
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
                catch (System.Security.SecurityException)
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
            if(list.Count()==1 && list.Contains(RRConfiguration.Anonymous.First(), StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}