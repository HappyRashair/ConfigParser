using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigParser
{
    internal class ConfigProcessor
    {
        private readonly string _targetEnv = null;
        private readonly string _targetApp = null;
        private readonly Dictionary<string, string> _exclusions;
        private readonly Dictionary<string, string> _settingsToRemove;
        private readonly bool _verboseLogging;

        public ConfigProcessor(string targetEnv = "DEV",
            string targetApp = "API",
            bool verboseLogging = true)
        {
            _targetEnv = targetEnv;
            _targetApp = targetApp;
            _exclusions = ConfigManager.GetExclusions();
            _settingsToRemove = ConfigManager.GetSettingsToRemove().Select(k => KeyValuePair.Create(k, "")).Concat(_exclusions)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, InvariantStringComparer.Instance);
            _verboseLogging = verboseLogging;
        }

        public void Run()
        {
            var webConfigEntries = ConfigManager.GetWebConfigValues();

            var result = ProcessAllExistingEntries(webConfigEntries);

            LogAllNonExistingEntriesFromWebConfig(webConfigEntries, result);

            File.WriteAllText(FilePath.ResultServerConfig, JsonSerializer.Serialize(result, options: Settings.JsonSettings));
        }

        private Dictionary<string, string> ProcessAllExistingEntries(Dictionary<string, string> webConfigEntries)
        {
            var result = new Dictionary<string, string>(InvariantStringComparer.Instance);
            var existingSettings = ConfigManager.GetInputConfig();
            var notFound = new List<(string, string)>();
            foreach (var entry in existingSettings)
            {
                var key = entry.Key.Replace("_", ".");
                var existsInWebConfig = webConfigEntries.ContainsKey(key);
                if (!existsInWebConfig && key.HasEnv(out var env))
                {
                    if (env != _targetEnv)
                    {
                        CWrapper.WriteCyan($"{key} not found in web.config, skipping because of invalid env {env}", _verboseLogging);
                        continue;
                    }

                    existsInWebConfig = ProcessKeyWithEnv(webConfigEntries, entry, ref key, env);
                }

                if (!existsInWebConfig)
                {
                    notFound.Add((key, entry.Value));
                    var foundToRemove = _settingsToRemove.TryGetValue(key, out string value);
                    if (foundToRemove && (value == "" || value != _targetApp))
                    {
                        CWrapper.WriteRed($"{key} will be removed");
                        continue;
                    }
                    else if(foundToRemove && value == _targetApp)
                    {
                        CWrapper.WriteIndent($"Stripped {key} not found in web.config for {_targetApp}, will be kept as {entry.Key}");
                        key = entry.Key;
                    }
                }

                Add(result, key, entry.Value);
            }

            return result;
        }

        private bool ProcessKeyWithEnv(Dictionary<string, string> webConfigEntries, KeyValuePair<string, string> entry, ref string key,
            string env)
        {
            key = key.StripEnv(env);
            if (!webConfigEntries.ContainsKey(key))
            {
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

        private void LogAllNonExistingEntriesFromWebConfig(Dictionary<string, string> webConfigEntries, Dictionary<string, string> result)
        {
            CWrapper.WriteCyan("\n\nNon-existing entries from web.config:");
            foreach (var entry in webConfigEntries.Where(kvp => !result.ContainsKey(kvp.Key)))
            {
                if (_exclusions.TryGetValue(entry.Key, out string? value) && !_targetApp.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    CWrapper.WriteCyan($"Skipping ({entry.Key}, because of exclusion for '{value}' app)");
                    continue;
                }

                CWrapper.WriteYellow($"Found in web.config but not in input.config: ({entry.Key}: {entry.Value})");
            }
        }
    }
}
