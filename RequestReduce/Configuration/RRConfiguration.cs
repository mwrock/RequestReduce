using System;
using System.Configuration;
using System.IO;
using System.Security.AccessControl;
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
        string ConnectionStringName { get; }
        Store ContentStore { get; }
        int SpriteSizeLimit { get; set; }
        event Action PhysicalPathChange; 
    }

    public class RRConfiguration : IRRConfiguration
    {
        private readonly RequestReduceConfigSection config = ConfigurationManager.GetSection("RequestReduce") as RequestReduceConfigSection;
        private string spriteVirtualPath;
        private string spritePhysicalPath;
        private Store contentStore = Store.LocalDiskStore;
        private int spriteSizeLimit;
        public event Action PhysicalPathChange;  

        public RRConfiguration()
        {
            var val = config == null ? 0 : config.SpriteSizeLimit;
            spriteSizeLimit =  val == 0 ? 50000 : val;
            spriteVirtualPath = config == null ? "/RequestReduceContent" : config.SpriteVirtualPath;
            spritePhysicalPath = config == null ? null : config.SpritePhysicalPath;
            if(config != null && !string.IsNullOrEmpty(config.ContentStore))
            {
                var success = Enum.TryParse(config.ContentStore, true, out contentStore);
                if(!success)
                    throw new ConfigurationErrorsException(string.Format("{0} is not a valid Content Store.", config.ContentStore));
            }
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

        public string ConnectionStringName
        {
            get { return config.ConnectionStringName; }
        }

        public Store ContentStore
        {
            get { return contentStore; }
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