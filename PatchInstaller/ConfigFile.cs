using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchInstaller
{
    public class ConfigFile
    {
        string _ConfigFilePath = "";

        public ConfigFile(string configFilePath)
        {
            _ConfigFilePath = configFilePath;
        }

        public string GetValue(string Key)
        {
            foreach (string line in File.ReadAllLines(_ConfigFilePath))
            {
                if (line.StartsWith(Key + "="))
                {
                    int splitIndex = line.IndexOf('=');
                    return line.Substring(splitIndex + 1);
                }
            }
            throw new Exception("Value not found in Config File. Missing key: " + Key);
        }

    }
}
