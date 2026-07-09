using System.Collections.Generic;
using StepBro.Core.Api;
using StepBro.Core.Logging;

namespace StepBro.CAN
{
    [Public]
    public interface IDriver : StepBro.Core.Devices.IDriver
    {
        IEnumerable<IAdapter> ListAdapters();
        IAdapter GetAdapter([Implicit] ILogger logger, string identification = "");
    }
}
