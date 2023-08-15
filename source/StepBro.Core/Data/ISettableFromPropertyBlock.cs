using StepBro.Core.Logging;

namespace StepBro.Core.Data
{
    public interface ISettableFromPropertyBlock
    {
        void Setup(ILogger logger, PropertyBlock data);
    }
}
