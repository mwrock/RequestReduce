using System;
using System.Configuration;
using System.IO;
using System.Web;

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

        public RRConfiguration(HttpContextBase httpContextBase)
        {
            var val = config == null ? 0 : config.SpriteSizeLimit;
            spriteSizeLimit =  val == 0 ? 50000 : val;
            spriteVirtualPath = config == null ? null : config.SpriteVirtualPath;
            spritePhysicalPath = config == null ? null : config.SpritePhysicalPath;
            CreatePhysicalPath();
        }

        public string SpriteVirtualPath
        {
            get { return spriteVirtualPath; }
            set { spriteVirtualPath = value;  }
        }

        public string SpritePhysicalPath
        {
            get { return spritePhysicalPath; }
            set 
            { 
                spritePhysicalPath = value;
                CreatePhysicalPath();
            }
        }

        public int SpriteSizeLimit
        {
            get { return spriteSizeLimit; }
            set { spriteSizeLimit = value; }
        }

        private void CreatePhysicalPath()
        {
            if (!string.IsNullOrEmpty(spritePhysicalPath) && !Directory.Exists(spritePhysicalPath))
                Directory.CreateDirectory(spritePhysicalPath);
        }
    }
}