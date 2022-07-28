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

            var result = new Dictionary<string, string>(InvariantStringComparer.Instance);
            var notFound = new List<(string, string)>();
            foreach (var entry in existingSettings)
            {
                var key = entry.Key.Replace("_", ".");
                var existsInWebConfig = webConfigEntries.ContainsKey(key);
                if (existsInWebConfig)
                {
                    var env = key.GetEnv();
                    if (env == targetEnv)
                    {
                        key = key.StripEnv(targetEnv);
                    }
                    else if (env != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"{key} not found in web.config, skipping because of invalid env {env}");
                        Console.ResetColor();
                        continue;
                    }
                }

                existsInWebConfig = webConfigEntries.ContainsKey(key);
                if (existsInWebConfig)
                {
                    Console.WriteLine($"        {key} not found in web.config, will be kept");
                    notFound.Add((key, entry.Value));
                }

                result.Add(key, entry.Value);
            }

            File.WriteAllText(FilePath.ResultServerConfig, JsonSerializer.Serialize(result, options: Settings.JsonSettings));
        }
    }
}
