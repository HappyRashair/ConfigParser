using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace ConfigParser
{
    internal static class ConfigManager
    {

        public static Dictionary<string, string> GetInputConfig()
        {
            string inputFile = File.ReadAllText(FilePath.InputServerConfig);
            var input= JsonSerializer.Deserialize<Dictionary<string, string>>(inputFile);
            var result = new Dictionary<string, string>(InvariantStringComparer.Instance);
            foreach (var entry in input)
            {
                result.Add(entry.Key.Replace("_", "."), entry.Value);
            }
            return result;
        }
        

        public static Dictionary<string, string> GetWebConfigValues(string section = "appSettings")
        {
            var xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(FilePath.WebConfig); // Load the XML document from the specified file

            // Get elements
            var path = $"/configuration/{section}/add";
            XmlNodeList sectionEntries = xmlDoc.DocumentElement.SelectNodes(path);

            var result = new Dictionary<string, string>(InvariantStringComparer.Instance);
            foreach (XmlNode entry in sectionEntries)
            {
                result.Add(entry.Attributes["key"].Value, entry.Attributes["value"].Value);
            }

            if (!result.Any())
            {
                throw new InvalidOperationException($"No entries in webconfig path '{path}' for file {FilePath.WebConfig}");
            }

            return result;
        }

        public static HashSet<string> GetSettingsToRemove()
        {
            string inputFile = File.ReadAllText(FilePath.SettingsToRemove);
            var input = JsonSerializer.Deserialize<List<string>>(inputFile);
            return new HashSet<string>(input, InvariantStringComparer.Instance);
        }
    }
}
