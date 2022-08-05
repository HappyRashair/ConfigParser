using ConfigParser.Data.Model;
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
        private readonly TargetApp _targetApp = null;
        private readonly Dictionary<string, List<string>> _exclusions;
        private readonly Dictionary<string, string> _webConfigEntries;
        private readonly HashSet<string> _settingsToRemove;
        private readonly Dictionary<string, List<string>> _settingsToRemoveFromExclusions;
        private readonly bool _verboseLogging;

        public ConfigProcessor(string targetEnv = "DEV",
            string targetApp = "CND-API",
            bool verboseLogging = true)
        {
            _targetEnv = targetEnv;
            _targetApp = new TargetApp(targetApp);
            _exclusions = ConfigManager.GetExclusions();
            _webConfigEntries = ConfigManager.GetWebConfigValues();
            _settingsToRemove = new HashSet<string>(ConfigManager.GetSettingsToRemove(), InvariantStringComparer.Instance);
            _settingsToRemoveFromExclusions = _exclusions;


            _verboseLogging = verboseLogging;
        }

        public void Run()
        {
            var result = ProcessAllExistingEntries();

            ProcessNonExistingEntriesFromWebConfig(result);

            File.WriteAllText(FilePath.ResultServerConfig, JsonSerializer.Serialize(result, options: Settings.WriteJsonSettings));
        }

        private Dictionary<string, string> ProcessAllExistingEntries()
        {
            var result = new Dictionary<string, string>(InvariantStringComparer.Instance);
            var existingSettings = ConfigManager.GetInputConfig();
            var notFound = new List<(string, string)>();
            foreach (var entry in existingSettings)
            {
                var key = entry.Key.Replace("_", ".");
                var existsInWebConfig = _webConfigEntries.ContainsKey(key);
                if (!existsInWebConfig && key.HasEnv(out var env))
                {
                    if (env != _targetEnv)
                    {
                        CWrapper.WriteCyan($"{key} not found in web.config, skipping because of invalid env {env}", _verboseLogging);
                        continue;
                    }

                    existsInWebConfig = ProcessKeyWithEnv(_webConfigEntries, ref key, env);
                }

                if (!existsInWebConfig)
                {
                    notFound.Add((key, entry.Value));
                    var foundToRemove = _settingsToRemove.Contains(key);
                    if (foundToRemove)
                    {
                        CWrapper.WriteRed($"{key} will be removed");
                        continue;
                    }

                    foundToRemove = _settingsToRemoveFromExclusions.TryGetValue(key, out List<string> envs);
                    if (foundToRemove && (envs != null && envs.All(e => !_targetApp.CSEqual(e))))
                    {
                        CWrapper.WriteRed($"{key} will be removed as it doesn't match any exclusion env {envs.Delim()}");
                        continue;
                    }

                    if (!foundToRemove || envs != null) // not found or doesn't have a default value in the code
                    {
                        CWrapper.WriteIndent($"Stripped \"{key}\" not found in web.config for {_targetApp}, will be kept as {entry.Key}");
                        key = entry.Key;
                    }
                }

                Add(result, key, entry.Value);
            }

            return result;
        }

        private bool ProcessKeyWithEnv(Dictionary<string, string> webConfigEntries, ref string key,
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

        private void ProcessNonExistingEntriesFromWebConfig(Dictionary<string, string> result)
        {
            CWrapper.WriteCyan("\n\nNon-existing entries from web.config:");
            var toAdd = ConfigManager.GetEntriesToAdd();
            foreach (var entry in _webConfigEntries.Where(kvp => !result.ContainsKey(kvp.Key)))
            {
                var exclusionFound = _exclusions.TryGetValue(entry.Key, out List<string> envs);
                if (exclusionFound && (envs == null || envs.All(e => !_targetApp.CSEqual(e))))
                {
                    CWrapper.WriteCyan($"Skipping ({entry.Key}, because of exclusion for '{envs.Delim()}' app)");
                    continue;
                }

                var toAddFound = toAdd.TryGetValue(entry.Key, out List<string> appsToAdd);
                if (toAddFound && appsToAdd.Any(e => _targetApp.CSEqual(e)))
                {
                    CWrapper.WriteGreen($"-- Adding ({entry.Key}, for '{_targetApp}' with value: '{entry.Value}').\r\n\t"
                        + $"Please verify if the value is correct for {_targetEnv}".ToUpperInvariant());
                    result.Add(entry.Key, entry.Value);
                    continue;
                }

                //if (exclusionFound && _targetApp.Equals == _targetApp)
                //{
                //    CWrapper.WriteCyan($"Skipping ({entry.Key}, because of exclusion for '{value}' app)");
                //    continue;
                //}

                if (toAddFound && _targetApp.IsCsSpecific())
                {
                    CWrapper.WriteCyan($"Skipping ({entry.Key}, because of exclusion for '{_targetApp}' app, " +
                        $"would be added for {appsToAdd.Delim()}");
                    continue;
                }

                CWrapper.WriteYellow($"Found in web.config but not in input.config: ({entry.Key}: {entry.Value})");
            }
        }
    }
}
