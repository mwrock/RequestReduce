using System;
using System.Configuration;

namespace RequestReduce.Configuration
{
    public interface IConfigurationWrapper
    {
        string SpriteDirectory { get; }
        int SpriteSizeLimit { get; }
    }

    public class ConfigurationWrapper : IConfigurationWrapper
    {
        private RequestReduceConfigSection config = ConfigurationManager.GetSection("requestReduce") as RequestReduceConfigSection;

        public string SpriteDirectory
        {
            get { return config == null ? null : config.SpriteDirectory; }
        }

        public int SpriteSizeLimit
        {
            get { return config == null ? 0 : config.SpriteSizeLimit; }
        }
    }
}