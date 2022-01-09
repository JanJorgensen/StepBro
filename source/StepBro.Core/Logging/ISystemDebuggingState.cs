using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ISystemDebuggingState
    {
        void SetDebugState(bool isDebugging);
        bool IsDebugging { get; }
        event EventHandler DebuggingChanged;
    }
}
