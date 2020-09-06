using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Tasks
{
    public interface ITaskControl
    {
        TaskExecutionState CurrentState { get; }
        DateTime StartTime { get; }
        DateTime EndTime { get; }
        event EventHandler CurrentStateChanged;
        BreakOption BreakOptions { get; }

        bool RequestPause();
        bool RequestContinue();
        bool RequestStop();
        bool Kill();
    }
}
