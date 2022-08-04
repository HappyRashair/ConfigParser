using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigParser.Data.Model
{
    internal class InputConfig
    {
        public Dictionary<string, string> AllEntries => Default.Concat(NorthEurope).ToDictionary(p => p.Key, p => p.Value);

        public Dictionary<string, string> Default { get; set; }
        public Dictionary<string, string> NorthEurope { get; set; }
        public Dictionary<string, string> WestEurope { get; set; }
    }
}
