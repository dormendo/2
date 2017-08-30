using System.Configuration;
using Maxima.Server.Configuration;

namespace Maxima.Server.Tcp
{
    public sealed class BuffersConfiguration
    {
        public string Provider { get; private set; }
        
        private BuffersConfiguration()
        {
        }
        
        public static BuffersConfiguration LoadConfigurationFromAppConfig()
        {
            ServerConfigurationSection serverConfigurationSection = (ServerConfigurationSection)ConfigurationManager.GetSection("server");
            BuffersConfiguration buffersConfiguration = new BuffersConfiguration
            {
                Provider = serverConfigurationSection.Buffers.Provider
            };
            return buffersConfiguration;
        }
    }
}