using System;
using System.Configuration;

namespace RequestReduce.Configuration
{
    public interface IRRConfiguration
    {
        string SpriteVirtualPath { get; }
        string SpritePhysicalPath { get; }
        int SpriteSizeLimit { get; }
    }

    public class RRConfiguration : IRRConfiguration
    {
        private RequestReduceConfigSection config = ConfigurationManager.GetSection("RequestReduce") as RequestReduceConfigSection;

        public string SpriteVirtualPath
        {
            get { return config == null ? null : config.SpriteVirtualPath; }
        }

        public string SpritePhysicalPath
        {
            get { return config == null ? null : config.SpritePhysicalPath; }
        }

        public int SpriteSizeLimit
        {
            get { 
                var val =  config == null ? 0 : config.SpriteSizeLimit;
                return val == 0 ? 50000 : val;
            }
        }
    }
}