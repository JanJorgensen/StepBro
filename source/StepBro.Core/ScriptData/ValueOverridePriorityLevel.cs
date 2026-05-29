using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    /// <summary>
    /// Logical named override priority levels for each of the logical sources of override values. 
    /// </summary>
    public enum ValueOverridePriorityLevel : int
    {
        /// <summary>
        /// The highest priority level; overrides all other priority level values defined.
        /// </summary>
        Highest = 0,
        /// <summary>
        /// The user has chosen a value. The value is set in the host application and may be persisted in a user file associated with the main script file opened.
        /// </summary>
        UserValue = Highest,
        /// <summary>
        /// The Station Properties file overrides the value.
        /// </summary>
        Station,
        /// <summary>
        /// The value is overridden in another script file higher up the dependency hierarchy.
        /// </summary>
        FileOverride
    }
}
