using ConfigParser.Data.Model;
using System.Text.Json;

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
        private readonly Mapper _mapper;
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
            _mapper = new Mapper();

            _verboseLogging = verboseLogging;
        }

        public void Run()
        {
            var result = ProcessAllExistingEntries();

            ProcessNonExistingEntriesFromWebConfig(result);

            WriteToFiles(result);

        }

        private InputConfig ProcessAllExistingEntries()
        {
            var result = new InputConfig();
            var existingSettings = ConfigManager.GetInputConfig();
            ProcessSettings(result.Default, existingSettings.Default);
            ProcessSettings(result.WestEurope, existingSettings.WestEurope);
            ProcessSettings(result.NorthEurope, existingSettings.NorthEurope);

            return result;
        }

        private void ProcessSettings(Dictionary<string, string> result, Dictionary<string, string> existingSettings)
        {
            foreach (var entry in existingSettings)
            {
                var key = entry.Key;
                var value = entry.Value;
                (key, value) = _mapper.Map(key, value);

                var existsInWebConfig = _webConfigEntries.ContainsKey(key);
                if (!existsInWebConfig && key.HasEnv(out var env))
                {
                    if (env != _targetEnv)
                    {
                        CWrapper.WriteCyan($"{key} not found in web.config, skipping because of invalid env {env}", _verboseLogging);
                        continue;
                    }
                    existsInWebConfig = ProcessKeyWithEnv(ref key, ref value, env);
                }

                if (!existsInWebConfig)
                {
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
                        CWrapper.WriteIndent($"Stripped \"{key}\" not found in web.config for {_targetApp}, will be kept as {key}");
                    }
                }

                Add(result, key, value);
            }
        }

        private bool ProcessKeyWithEnv(ref string key, ref string value,
            string env)
        {
            key = key.StripEnv(env);
            (key, value) = _mapper.Map(key, value);
            if (!_webConfigEntries.ContainsKey(key))
            {
                return false;
            }

            CWrapper.WriteGreen($"Stripped {key} found in web.config :+1, will be kept!");
            return true;
        }

        public void Add(Dictionary<string, string> result, string key, string value)
        {
            if (!result.TryGetValue(key, out var existingValue))
            {
                result.Add(key, value);
                return;
            }

            var isExistingValueInvalid = existingValue.Contains("${");
            if (isExistingValueInvalid)
            {
                CWrapper.WriteYellow($"{key} had invalid value, skipping value: '{existingValue}'", _verboseLogging);
                result[key] = value;
                return;
            }

            CWrapper.WriteYellow($"{key} was already added, skipping value: '{value}'", _verboseLogging);
        }

        private void ProcessNonExistingEntriesFromWebConfig(InputConfig result)
        {
            CWrapper.WriteCyan("\n\nNon-existing entries from web.config:");
            var toAdd = ConfigManager.GetEntriesToAdd();
            var toAddPerEnv = ConfigManager.GetEntriesToAddPerEnv();
            var allEntries = result.AllEntries;
            var resultDefault = result.Default;
            foreach (var entry in _webConfigEntries.Where(kvp => !allEntries.ContainsKey(kvp.Key)))
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
                    resultDefault.Add(entry.Key, entry.Value);
                    continue;
                }

                if (toAddFound && _targetApp.IsCsSpecific())
                {
                    CWrapper.WriteCyan($"Skipping ({entry.Key}, because of exclusion for '{_targetApp}' app, " +
                        $"would be added for {appsToAdd.Delim()}");
                    continue;
                }

                if (toAddPerEnv.TryGetValue(entry.Key, out var valuesPerEnv))
                {
                    var value = valuesPerEnv[_targetEnv];
                    CWrapper.WriteGreen($"-- Adding ({entry.Key}, for '{_targetApp}' with value: '{value}').\r\n\t"
                        + $"Please verify if the value is correct!".ToUpperInvariant());
                    resultDefault.Add(entry.Key, value);
                    continue;
                }

                CWrapper.WriteYellow($"Found in web.config but not in input.config: ({entry.Key}: {entry.Value})");
            }
        }

        private static void WriteToFiles(InputConfig result)
        {
            var serializedResult = JsonSerializer.Serialize(result, options: Settings.WriteJsonSettings);
            File.WriteAllText(FilePath.ResultServerConfig, serializedResult);
            
            var solutionFilePath = Path.Combine("..", "..", "..", FilePath.ResultServerConfig);
            if (File.Exists(solutionFilePath))
            {
                File.WriteAllText(solutionFilePath, serializedResult);
            }
        }
    }
}
