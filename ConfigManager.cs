using ConfigParser.Data.Model;
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

        public static InputConfig GetInputConfig()
        {
            string inputFile = File.ReadAllText(FilePath.InputServerConfig);
            var input = JsonSerializer.Deserialize<InputConfig>(inputFile);

            var result = new InputConfig();
            AddWithReplace(input.Default, result.Default);
            AddWithReplace(input.WestEurope, result.WestEurope);
            AddWithReplace(input.NorthEurope, result.NorthEurope);

            return result;
        }

        private static void AddWithReplace(Dictionary<string, string> input, Dictionary<string, string> result)
        {
            foreach (var entry in input)
            {
                result.Add(entry.Key.Replace("_", "."), entry.Value);
            }
        }

        public static Dictionary<string, string> GetWebConfigValues()
        {
            var xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(FilePath.WebConfig); // Load the XML document from the specified file

            // Get elements
            var appSettingsPath = $"/configuration/appSettings/add";
            var result = new Dictionary<string, string>(InvariantStringComparer.Instance);
            foreach (XmlNode entry in xmlDoc.DocumentElement.SelectNodes(appSettingsPath))
            {
                result.Add(entry.Attributes["key"].Value, entry.Attributes["value"].Value);
            }
            appSettingsPath = $"/configuration/connectionStrings/add";
            foreach (XmlNode entry in xmlDoc.DocumentElement.SelectNodes(appSettingsPath))
            {
                result.Add("ConnectionStrings:" + entry.Attributes["name"].Value, entry.Attributes["connectionString"].Value);
            }

            if (!result.Any())
            {
                throw new InvalidOperationException($"No entries in webconfig path '{appSettingsPath}' for file {FilePath.WebConfig}");
            }

            return result;
        }

        public static List<string> GetSettingsToRemove()
        {
            string inputFile = File.ReadAllText(FilePath.SettingsToRemove);
            var input = JsonSerializer.Deserialize<List<string>>(inputFile, Settings.ReadJsonSettings);
            return input;
        }

        public static Dictionary<string, List<string>> GetExclusions()
        {
            string inputFile = File.ReadAllText(FilePath.Exclusions);
            var input = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(inputFile, Settings.ReadJsonSettings);
            return new Dictionary<string, List<string>>(input, InvariantStringComparer.Instance);
        }

        internal static Dictionary<string, List<string>> GetEntriesToAdd()
        {
            string inputFile = File.ReadAllText(FilePath.EntriesToAdd);
            var input = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(inputFile, Settings.ReadJsonSettings);
            return new Dictionary<string, List<string>>(input, InvariantStringComparer.Instance);
        }

        internal static Dictionary<string, Dictionary<string, string>> GetEntriesToAddPerEnv()
        {
            var xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(FilePath.EntriesToAddPerEnv); // Load the XML document from the specified file

            var appSettingsPath = $"/configuration/appSettings/add";
            var result = new Dictionary<string, Dictionary<string, string>>(InvariantStringComparer.Instance);
            var validEnvs = Extensions.GetValidEnvs();
            foreach (XmlNode entry in xmlDoc.DocumentElement.SelectNodes(appSettingsPath))
            {
                var inputKey = entry.Attributes["key"].Value;
                var value = entry.Attributes["value"].Value;
                if (!inputKey.HasEnv(out string env)) // same for all envs
                {
                    var envsToValue = new Dictionary<string, string>(InvariantStringComparer.Instance);
                    foreach (var vEnv in validEnvs)
                    {
                        envsToValue.Add(vEnv, value);
                    }
                    result.Add(inputKey, envsToValue);
                    continue;
                }

                var key = inputKey.Replace($".{env}", "");
                if (result.ContainsKey(key))
                {
                    result[key].Add(env, value);
                }
                else
                {
                    result.Add(key, GetNewEntry(value, env));
                }
            }

            if (!result.Any())
            {
                throw new InvalidOperationException($"No entries in webconfig path '{appSettingsPath}' for file {FilePath.WebConfig}");
            }

            return result;
        }

        private static Dictionary<string, string> GetNewEntry(string value, string env)
        {
            return new Dictionary<string, string>(InvariantStringComparer.Instance) { { env, value } };
        }

        internal static Dictionary<string, string> GetMappings()
        {
            string inputFile = File.ReadAllText(FilePath.Mappings);
            var input = JsonSerializer.Deserialize<Dictionary<string, string>>(inputFile, Settings.ReadJsonSettings);
            return new Dictionary<string, string>(input, InvariantStringComparer.Instance);
        }

        internal static HashSet<string> GetRegexMappings()
        {
            string inputFile = File.ReadAllText(FilePath.RegexMappings);
            var input = JsonSerializer.Deserialize<List<string>>(inputFile, Settings.ReadJsonSettings);
            return new HashSet<string>(input, InvariantStringComparer.Instance);
        }
    }
}
