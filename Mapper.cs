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
        private readonly Dictionary<Regex, string> _regexMappings;

        public Mapper()
        {
            _mappings = ConfigManager.GetMappings();

            var regexMappings = ConfigManager.GetRegexMappings();
            if (regexMappings.FirstOrDefault(r => !IsReplaceRegex(r)) is string str)
            {
                throw new InvalidOperationException($"'{str}' is not a valid regex");
            }
            _regexMappings = regexMappings.Select(r => GetRegexWithReplacement(r))
                .ToDictionary(p => p.regex, p => p.replacement);
        }

        public static bool IsReplaceRegex(string reg)
        {
            return reg.Contains(RegexReplaceSeparator);
        }

        public (string, string value) Map(string key, string value)
        {
            (key, value) = MapSimple(key, value);
            (key) = MapRegex(key);

            return (key, value);
        }

        private (string key, string value) MapSimple(string key, string value)
        {
            if (_mappings.TryGetValue(key, out var mappedKey))
            {
                WriteMapped(key, mappedKey, "key");
                key = mappedKey;
            }

            if (_mappings.TryGetValue($"{key}.Value", out var valueMappingStr))
            {
                value = MapValue(value, valueMappingStr);
            }

            return (key, value);
        }

        private string MapValue(string value, string valueMappingStr)
        {
            // We may want to replace the whole value or just a part of it
            var mappedValue = IsReplaceRegex(valueMappingStr) ? ProcessRegex(value, valueMappingStr) : valueMappingStr;
            if (mappedValue != value)
            {
                WriteMapped(value, mappedValue, "value");
                return mappedValue;
            }

            return value;
        }

        private static void WriteMapped(string key, string mappedKey, string what)
        {
            CWrapper.WriteMagenta($"Mapping {what} '{key}' to '{mappedKey}'");
        }

        private string MapRegex(string input)
        {
            foreach (var regex in _regexMappings)
            {
                var replacedStr = regex.Key.Replace(input, regex.Value);
                if (replacedStr != input)
                {
                    WriteMapped(input, replacedStr, "key");
                    input = replacedStr;
                }
            }

            return input;
        }


        private static string ProcessRegex(string value, string replacementRegexPair)
        {
            var (regex, replacement) = GetRegexWithReplacement(replacementRegexPair);
            return regex.Replace(value, replacement);
        }

        private static (Regex regex, string replacement) GetRegexWithReplacement(string replacementRegexPair)
        {
            var pair = replacementRegexPair.Split(RegexReplaceSeparator);
            return (new Regex(pair[0], RegexOptions.Compiled | RegexOptions.IgnoreCase), pair[1]);
        }
    }
}
