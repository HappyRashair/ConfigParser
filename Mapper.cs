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
        private const string RegexReplaceSeparator = "###";
        private readonly Dictionary<string, string> _mappings;

        public Mapper()
        {
            _mappings = ConfigManager.GetMappings();
        }

        public (string, string value) Map(string key, string value)
        {
            var resultKey = key;
            var resultValue = value;
            if (_mappings.TryGetValue(key, out var mappingStr))
            {
                var mappedKey = IsRegex(mappingStr) ? ProcessRegex(resultValue, mappingStr) : mappingStr;
                if (mappedKey != resultKey)
                {
                    CWrapper.WriteMagenta($"Renaming key '{resultKey}' to '{mappedKey}'");
                    resultKey = mappedKey;
                }

            }

            if (_mappings.TryGetValue($"{resultKey}.Value", out mappingStr))
            {
                var mappedValue = IsRegex(mappingStr) ? ProcessRegex(resultValue, mappingStr) : mappingStr;
                if (mappedValue != resultValue)
                {
                    CWrapper.WriteMagenta($"Renaming value '{resultValue}' to '{mappedValue}'");
                    resultValue = mappedValue;
                }
            }

            return (resultKey, resultValue);
        }

        public bool IsRegex(string reg)
        {
            return reg.Contains(RegexReplaceSeparator);
        }

        private static string ProcessRegex(string value, string replacementRegexPair)
        {
            var pair = replacementRegexPair.Split(RegexReplaceSeparator);
            var regex = pair[0];
            var replacement = pair[1];
            return Regex.Replace(value, regex, replacement, RegexOptions.IgnoreCase);
        }
    }
}
