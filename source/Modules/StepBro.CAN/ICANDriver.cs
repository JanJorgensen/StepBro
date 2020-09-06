using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Api;
using StepBro.Core.Execution;

namespace StepBro.CAN
{
    [Public]
    public interface ICANDriver
    {
        IEnumerable<ICANAdapter> ListAdapters();
        ICANAdapter GetAdapter([Implicit] ICallContext context, string identification = "");
    }
}
