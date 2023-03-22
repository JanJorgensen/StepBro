using System;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Interface used to get information about the high level inheritance of an object.
    /// </summary>
    public interface IInheritable
    {
        /// <summary>
        /// Gets reference to the object inherited from.
        /// </summary>
        IInheritable Base { get; }
    }
}
