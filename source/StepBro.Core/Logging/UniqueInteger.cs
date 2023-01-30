using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public static class UniqueInteger
    {
        private static ulong m_nextLong = 1UL;
        public static ulong GetLong()
        {
            return m_nextLong++;
        }
    }
}
