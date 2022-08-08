using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConfigParser
{
    internal class Mapper
    {
        private readonly Dictionary<string, string> _mappings;

        public Mapper()
        {
            _mappings = ConfigManager.GetMappings();
        }

        public (string, string value) Map(string key, string value)
        {
            var resultKey = key;
            var resultValue = value;
            if (_mappings.TryGetValue(key, out var mappedKey))
            {
                CWrapper.WriteMagenta($"Renaming key '{resultKey}' to '{mappedKey}'");
                resultKey = mappedKey;
            }

            if (_mappings.TryGetValue($"{resultKey}.Value", out string replacementRegexPair))
            {
                var mappedValue = ProcessRegex(resultValue, replacementRegexPair);
                if (mappedValue != resultValue)
                {
                    CWrapper.WriteMagenta($"Renaming value '{resultValue}' to '{mappedValue}'");
                    resultValue = mappedValue;
                }
            }

            return (resultKey, resultValue);
        }

        private static string ProcessRegex(string value, string replacementRegexPair)
        {
            var pair = replacementRegexPair.Split("###");
            var regex = pair[0];
            var replacement = pair[1];
            return Regex.Replace(value, regex, replacement, RegexOptions.IgnoreCase);
        }
    }
}
