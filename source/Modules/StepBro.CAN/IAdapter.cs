using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Api;
using StepBro.Core.Devices;
using StepBro.Core.Execution;

namespace StepBro.CAN
{
    [Public]
    public interface IAdapter : IDevice
    {
        int Channels { get; }
        IChannel GetChannel([Implicit] ICallContext context, int index);
    }
}
