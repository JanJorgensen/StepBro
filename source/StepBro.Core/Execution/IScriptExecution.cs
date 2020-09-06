using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;

namespace StepBro.Core.Execution
{
    public interface IScriptExecution
    {
        IFileElement TargetElement { get; }
        void StartExecution();
        ITaskControl Task { get; }
        IExecutionResult Result { get; }
    }
}
