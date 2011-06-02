using System;
using System.Configuration;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Web;

namespace RequestReduce.Configuration
{
    public interface IRRConfiguration
    {
        string SpriteVirtualPath { get; set; }
        string SpritePhysicalPath { get; set; }
        string ContentHost { get; }
        int SpriteSizeLimit { get; set; }
        event Action PhysicalPathChange; 
    }

    public class RRConfiguration : IRRConfiguration
    {
        private readonly RequestReduceConfigSection config = ConfigurationManager.GetSection("RequestReduce") as RequestReduceConfigSection;
        private string spriteVirtualPath;
        private string spritePhysicalPath;
        private int spriteSizeLimit;
        public event Action PhysicalPathChange;  

        public RRConfiguration()
        {
            var val = config == null ? 0 : config.SpriteSizeLimit;
            spriteSizeLimit =  val == 0 ? 50000 : val;
            spriteVirtualPath = config == null ? "/RequestReduceContent" : config.SpriteVirtualPath;
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
                if (PhysicalPathChange != null)
                    PhysicalPathChange();
            }
        }

        public string ContentHost
        {
            get { return config.ContentHost; }
        }

        public int SpriteSizeLimit
        {
            get { return spriteSizeLimit; }
            set { spriteSizeLimit = value; }
        }

        private void CreatePhysicalPath()
        {
            if (!string.IsNullOrEmpty(spritePhysicalPath) && !Directory.Exists(spritePhysicalPath))
            {
                Directory.CreateDirectory(spritePhysicalPath);
                while (!Directory.Exists(spritePhysicalPath))
                    Thread.Sleep(0);
            }
        }
    }
}