using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConfigParser.Data.Model
{
    internal class InputConfig
    {
        public InputConfig()
        {
            Default = new Dictionary<string, string>(InvariantStringComparer.Instance);
            NorthEurope = new Dictionary<string, string>(InvariantStringComparer.Instance);
            WestEurope = new Dictionary<string, string>(InvariantStringComparer.Instance);
        }

        [JsonIgnore]
        public Dictionary<string, string> AllEntries
        {
            get
            {
                var result = new Dictionary<string, string>(Default, InvariantStringComparer.Instance);
                foreach (var entry in WestEurope)
                {
                    result.TryAdd(entry.Key, entry.Value);
                }
                foreach (var entry in NorthEurope)
                {
                    result.TryAdd(entry.Key, entry.Value);
                }
                return result;
            }
        }


        public Dictionary<string, string> Default { get; set; }
        public Dictionary<string, string> NorthEurope { get; set; }
        public Dictionary<string, string> WestEurope { get; set; }
    }
}
