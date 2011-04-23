using System;
using System.Configuration;

namespace RequestReduce.Configuration
{
    public class RequestReduceConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("spriteDirectory")]
        public string SpriteDirectory
        {
            get
            {
                return base["spriteDirectory"].ToString();
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
