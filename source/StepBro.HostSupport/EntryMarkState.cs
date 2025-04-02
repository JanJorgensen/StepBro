using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport
{
    public enum EntryMarkState
    {
        None        = 0x00,
        Current     = 0x01,
        Selected    = 0x02,
        SearchMatch = 0x04,
        Parent      = 0x08,
        Sibling     = 0x10
    }
}
