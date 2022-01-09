using StepBro.Core.Data;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Interface for an addon object class.
    /// </summary>
    /// <remarks>
    /// A single addon object of each addon type will typically be created, and reused during the lifetime of a StepBro host application execution.
    /// </remarks>
    public interface IAddon : INamedObject
    {
        string Description { get; }
    }
}
