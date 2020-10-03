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
    public interface IAdapter
    {
        IDriver Driver { get; }
        string Identification { get; }
        string DeviceType { get; }
        int Channels { get; }
        IChannel GetChannel([Implicit] ICallContext context, int index);
    }
}
