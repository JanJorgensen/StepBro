using StepBro.Core.Api;
using StepBro.Core.Devices;
using StepBro.Core.Logging;

namespace StepBro.CAN
{
    [Public]
    public interface IAdapter : IDevice
    {
        int Channels { get; }
        IChannel GetChannel([Implicit] ILogger logger, int index);
    }
}
