using System;
using System.Configuration;

namespace RequestReduce.Configuration
{
    public interface IRRConfiguration
    {
        string SpriteVirtualPath { get; set; }
        string SpritePhysicalPath { get; set; }
        int SpriteSizeLimit { get; set; }
    }

    public class RRConfiguration : IRRConfiguration
    {
        private readonly RequestReduceConfigSection config = ConfigurationManager.GetSection("RequestReduce") as RequestReduceConfigSection;
        private string spriteVirtualPath;
        private string spritePhysicalPath;
        private int spriteSizeLimit;

        public RRConfiguration()
        {
            var val = config == null ? 0 : config.SpriteSizeLimit;
            spriteSizeLimit =  val == 0 ? 50000 : val;
            spritePhysicalPath = config == null ? null : config.SpritePhysicalPath;
            spriteVirtualPath = config == null ? null : config.SpriteVirtualPath;
        }

        public string SpriteVirtualPath
        {
            get { return spriteVirtualPath; }
            set { spriteVirtualPath = value;  }
        }

        public string SpritePhysicalPath
        {
            get { return spritePhysicalPath; }
            set { spritePhysicalPath = value; }
        }

        public int SpriteSizeLimit
        {
            get { return spriteSizeLimit; }
            set { spriteSizeLimit = value; }
        }
    }
}