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
    }
}
