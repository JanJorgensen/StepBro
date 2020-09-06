using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Tasks
{
    public enum TaskResultType { Success, Timeout, Failed }

    public interface ITaskStateReporting
    {
        void ProgressSetup(long start, long length, Func<long, string> toText);

        /// <summary>
        /// Sets the status text for the context.
        /// </summary>
        /// <remarks>If progress is set, the progress will be closed.</remarks>
        /// <param name="text">State text to show to the user.</param>
        /// <param name="progress">Current progress value.</param>
        void UpdateStatus(string text = null, long progress = -1);

        /// <summary>
        /// Indicates the current running task is still alive.
        /// </summary>
        ///<remarks>The status text is not changed.</remarks>
        void ProgressAliveSignal();
    }
}
