using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Streams
{
    internal class SerialPortDebugLogEntry : DebugLogEntry
    {
        public enum Type { CountState, ErrorReceived }

        private Type m_type;

        public SerialPortDebugLogEntry(Type type) : base()
        {
            m_type = type;
        }

        public override string ToString()
        {
            return $"SP {m_type}";
        }
    }
}
