using Newtonsoft.Json;
using System.IO;

namespace ProiectLicenta.Model
{
    public class ConfigLoader
    {
        public ConfigDataStructure LoadConfig()
        {
            // Load the configuration file from disk
            string configPath = "config.json";
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<ConfigDataStructure>(json);
            }
            else
            {
                // Create a default configuration object if the file does not exist
                ConfigDataStructure defaultConfig = new ConfigDataStructure
                {
                    RemoteIp = "192.168.0.1",
                    CommunityString = "public",
                    Port = "161",
                    Mail = "",
                    Username = "",
                    Password = "",
                };
                return defaultConfig;
            }
        }
    }
}
