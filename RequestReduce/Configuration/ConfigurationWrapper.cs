using System;
using System.Configuration;

namespace RequestReduce.Configuration
{
    public interface IConfigurationWrapper
    {
        string SpriteDirectory { get; }
    }

    public class ConfigurationWrapper : IConfigurationWrapper
    {
        private RequestReduceConfigSection config = ConfigurationManager.GetSection("requestReduce") as RequestReduceConfigSection;

        public string SpriteDirectory
        {
            get { return config.SpriteDirectory; }
        }
    }
}