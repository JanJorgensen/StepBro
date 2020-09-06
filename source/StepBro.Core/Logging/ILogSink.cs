using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ILogSink
    {
        void Start(LogEntry entry);
        void Stop();
        void Add(LogEntry entry);
    }
}
