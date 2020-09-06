using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface ILogLineSource
    {
        LogLineData FirstEntry { get; }
        event LogLineAddEventHandler LinesAdded;
    }
}
