using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigParser
{
    internal static class ConfigProcessor
    {
        public static void Run(string targetEnv = "DEV")
        {
            var webConfigEntries = ConfigManager.GetWebConfigValues();
            var existingSettings = ConfigManager.GetInputConfig();
            var settingsToRemove = ConfigManager.GetSettingsToRemove();

            var result = new Dictionary<string, string>(InvariantStringComparer.Instance);
            var notFound = new List<(string, string)>();
            foreach (var entry in existingSettings)
            {
                var key = entry.Key.Replace("_", ".");
                var existsInWebConfig = webConfigEntries.ContainsKey(key);
                if (!existsInWebConfig && key.HasEnv(out var env))
                {
                    if (env != targetEnv)
                    {
                        CWrapper.WriteCyan($"{key} not found in web.config, skipping because of invalid env {env}");
                        continue;
                    }

                    existsInWebConfig = ProcessKeyWithEnv(webConfigEntries, entry, ref key, env);
                }

                if (!existsInWebConfig)
                {
                    notFound.Add((key, entry.Value));
                    if(settingsToRemove.Contains(key))
                    {
                        CWrapper.WriteRed($"{key} will be removed");
                        continue;
                    }
                }

                Add(result, key, entry.Value);
            }

            File.WriteAllText(FilePath.ResultServerConfig, JsonSerializer.Serialize(result, options: Settings.JsonSettings));
        }

        private static bool ProcessKeyWithEnv(Dictionary<string, string> webConfigEntries, KeyValuePair<string, string> entry, ref string key,
            string env)
        {
            key = key.StripEnv(env);
            if (!webConfigEntries.ContainsKey(key))
            {
                CWrapper.WriteIndent($"Stripped {key} not found in web.config, will be kept as {entry.Key}");
                key = entry.Key;
                return false;
            }

            CWrapper.WriteGreen($"Stripped {key} found in web.config :+1, will be kept!");
            return true;
        }

        public static void Add(Dictionary<string, string> result, string key, string value)
        {
            if (!result.TryGetValue(key, out var existingValue))
            {
                result.Add(key, value);
                return;
            }


            var isExistingValueInvalid = existingValue.Contains("${");
            if (isExistingValueInvalid)
            {
                result[key] = value;
                return;
            }

            CWrapper.WriteYellow($"{key} was already added, skipping value: '{value}'");
        }
    }
}
