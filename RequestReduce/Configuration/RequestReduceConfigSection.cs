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

        [ConfigurationProperty("spriteSizeLimit")]
        public int SpriteSizeLimit
        {
            get
            {
                var limit = 500*1024;
                Int32.TryParse(base["spriteSizeLimit"].ToString(), out limit);
                return limit;
            }
        }
    }
}
