using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public static class UniqueInteger
    {
        internal static object m_sync = new object();
        private static ulong m_nextLong = 1UL;
        public static ulong GetLongDirectly()
        {
            return m_nextLong++;
        }
        public static ulong GetLongProtected()
        {
            lock (m_sync)
            {
                return m_nextLong++;
            }
        }
    }
}
