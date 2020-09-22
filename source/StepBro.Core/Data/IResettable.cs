using StepBro.Core.Logging;

namespace StepBro.Core.Data
{
    public interface IResettable
    {
        bool Reset(ILogger logger);
    }
}
