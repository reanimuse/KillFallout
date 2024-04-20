using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KillFallout4.Utils
{
    internal static class StringExtensions
    {
        public static char? StartsWithAny(this string arg, params char[] charsToFind)
        {
            foreach (char c in charsToFind)
            {
                if (arg.Length > 0 && arg.StartsWith(c)) return c;
            }
            return null;
        }
    }
}
