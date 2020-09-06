using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public static class StringExtensions
    {
        public static bool TryParse(this string s, ref DateTime time)
        {
            return false;   // TODO
        }

        public static DateTime ParseDateTime(this string s)
        {
            return DateTime.UtcNow; // TODO
        }
    }
}
