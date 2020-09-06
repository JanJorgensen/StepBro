using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    /// <summary>
    /// Enumerates different states of the script executing the current method call.
    /// </summary>
    public enum CallEntry
    {
        /// <summary>
        /// The first call after definition or after opening the host application.
        /// </summary>
        FirstCall,
        /// <summary>
        /// First call after script execution start.
        /// </summary>
        FirstInExecution,
        /// <summary>
        /// Called more than once.
        /// </summary>
        Subsequent
    }
}
