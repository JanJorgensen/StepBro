using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Tasks
{
    /// <summary>
    /// The different available options for stopping a running task.
    /// </summary>
    [Flags]
    public enum BreakOption
    {
        /// <summary>
        /// No stop action available.
        /// </summary>
        None = 0x00,
        /// <summary>
        /// The task can be stopped.
        /// </summary>
        Stop = 0x01,
        /// <summary>
        /// The task can be paused.
        /// </summary>
        Pause = 0x02,
        /// <summary>
        /// The task can be brutally killed.
        /// </summary>
        Kill = 0x04
    }
}
