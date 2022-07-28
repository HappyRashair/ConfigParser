using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigParser
{
    internal static class Extensions
    {
        private static readonly List<string> _envs = "LOCAL,DEV,UAT,LIVE,PROD".Split(',').ToList();

        public static string? GetEnv(this string key)
        {
            return _envs.FirstOrDefault(e => key.EndsWith(e, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string StripEnv(this string key, string env)
        {
            return key.Replace($".{env}", "", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    internal static class FilePath
    {
        public const string WebConfig = @"Data\web-config-dump.xml";
        public const string InputServerConfig = @"Data\config-input.json";
        public const string OutputServerConfig = @"Data\config-output.json";
    }
}
