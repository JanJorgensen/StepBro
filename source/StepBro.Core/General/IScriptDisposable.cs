using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    public interface IScriptDisposable : IDisposing
    {
        void Dispose(ICallContext context);
    }
}
