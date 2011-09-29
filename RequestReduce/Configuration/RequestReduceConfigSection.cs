using System;
using System.Configuration;

namespace RequestReduce.Configuration
{
    public class RequestReduceConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("spriteVirtualPath")]
        public string SpriteVirtualPath
        {
            get
            {
                return base["spriteVirtualPath"].ToString();
            }
        }

        [ConfigurationProperty("spritePhysicalPath")]
        public string SpritePhysicalPath
        {
            get
            {
                return base["spritePhysicalPath"].ToString();
            }
        }

        [ConfigurationProperty("contentHost")]
        public string ContentHost
        {
            get
            {
                return base["contentHost"].ToString();
            }
        }

        [ConfigurationProperty("contentStore")]
        public string ContentStore
        {
            get
            {
                return base["contentStore"].ToString();
            }
        }

        [ConfigurationProperty("connectionStringName")]
        public string ConnectionStringName
        {
            get
            {
                return base["connectionStringName"].ToString();
            }
        }

        [ConfigurationProperty("spriteSizeLimit")]
        public int SpriteSizeLimit
        {
            get
            {
                int limit;
                Int32.TryParse(base["spriteSizeLimit"].ToString(), out limit);
                return limit;
            }
        }

        [ConfigurationProperty("imageOptimizationCompressionLevel")]
        public int ImageOptimizationCompressionLevel
        {
            get
            {
                int limit;
                Int32.TryParse(base["imageOptimizationCompressionLevel"].ToString(), out limit);
                return limit;
            }
        }

        [ConfigurationProperty("authorizedUserList")]
        public string AuthorizedUserList
        {
            get { return base["authorizedUserList"].ToString(); }
        }

        [ConfigurationProperty("cssProcesingDisabled")]
        public bool CssProcesingDisabled
        {
            get
            {
                bool result;
                bool.TryParse(base["cssProcesingDisabled"].ToString(), out result);
                return result;
            }
        }

        [ConfigurationProperty("javascriptProcesingDisabled")]
        public bool JavaScriptProcesingDisabled
        {
            get
            {
                bool result;
                bool.TryParse(base["javascriptProcesingDisabled"].ToString(), out result);
                return result;
            }
        }

        [ConfigurationProperty("imageOptimizationDisabled")]
        public bool ImageOptimizationDisabled
        {
            get
            {
                bool result;
                bool.TryParse(base["imageOptimizationDisabled"].ToString(), out result);
                return result;
            }
        }

        [ConfigurationProperty("imageQuantizationDisabled")]
        public bool ImageQuantizationDisabled
        {
            get
            {
                bool result;
                bool.TryParse(base["imageQuantizationDisabled"].ToString(), out result);
                return result;
            }
        }

        [ConfigurationProperty("spriteColorLimit")]
        public int SpriteColorLimit
        {
            get
            {
                int limit;
                Int32.TryParse(base["spriteColorLimit"].ToString(), out limit);
                return limit;
            }
        }

        [ConfigurationProperty("storePollInterval")]
        public int StorePollInterval
        {
            get
            {
                int limit;
                Int32.TryParse(base["storePollInterval"].ToString(), out limit);
                return limit;
            }
        }

        [ConfigurationProperty("javascriptUrlsToIgnore")]
        public string JavaScriptUrlsToIgnore
        {
            get
            {
                return base["javascriptUrlsToIgnore"].ToString();
            }
        }

    }
}
