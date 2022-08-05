using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigParser
{
    internal class InvariantStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            if(x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.ToUpperInvariant().GetHashCode();
        }

        public static readonly IEqualityComparer<string> Instance = new InvariantStringComparer();
    }
}
