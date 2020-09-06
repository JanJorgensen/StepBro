using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Interface to a named container for a dynamic object.
    /// </summary>
    public interface IObjectContainer : IAvailability
    {
        /// <summary>
        /// The fully qualified name of the container/object.
        /// </summary>
        string FullName { get; }
        /// <summary>
        /// Notifies when the container has been assigned another object.
        /// </summary>
        event EventHandler ObjectReplaced;
        /// <summary>
        /// Gets the container object reference.
        /// </summary>
        object Object { get; }
    }
}
