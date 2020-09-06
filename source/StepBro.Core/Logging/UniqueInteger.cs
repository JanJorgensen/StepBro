using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public static class UniqueInteger
    {
        private static uint m_next = 1;
        public static uint Get()
        {
            return m_next++;
        }
    }
}
