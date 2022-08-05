using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigParser
{
    internal static class Extensions
    {
        private static readonly List<string> _envs = "LOCAL,DEV,UAT,TEST,LIVE,PROD".Split(',').ToList();

        public static bool HasEnv(this string key, out string? env)
        {
            env = _envs.FirstOrDefault(e => key.EndsWith(e, StringComparison.InvariantCultureIgnoreCase));
            return env != null;
        }

        public static bool IsInvalidEnv(this string env)
        {
            return "TEST".Equals(env, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string StripEnv(this string key, string env)
        {
            return key.Replace($".{env}", "", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool Eql(this string str, string other)
        {
            if (str == null && other == null)
            {
                return true;
            }
            if (str == null)
            {
                return false;
            }

            return str.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string Delim(this List<string> strings)
        {
            return strings != null ? $"[{string.Join(", ", strings)}]" : "";
        }
    }

    internal static class FilePath
    {
        public const string WebConfig = @"Data\web-config-dump.xml";
        public const string InputServerConfig = @"Data\config-input.json";
        public const string ResultServerConfig = @"Data\config-result.json";
        public const string SettingsToRemove = @"Data\remove.json";
        public const string Exclusions = @"Data\exclusions.json";
        public const string EntriesToAdd = @"Data\to-add.json";
    }

    internal static class Settings
    {
        public static readonly JsonSerializerOptions WriteJsonSettings = new JsonSerializerOptions
        {
            // Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        public static readonly JsonSerializerOptions ReadJsonSettings = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };
    }
}
