using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Tasks
{
    public interface ITaskContext : ITaskStateReporting
    {
        /// <summary>
        /// Indicates whether the task controller requests the task to start a pause.
        /// </summary>
        /// <seealso cref="EnterPauseIfRequested(string)"/>
        bool PauseRequested { get; }
        /// <summary>
        /// Tells the task controller that the task is ready to enter a pause, if it is requested.
        /// A task that runs through a list of sub asks, can call this method before each iteration.
        /// </summary>
        /// <param name="state">A description of the current task state.</param>
        /// <returns>Whether a pause was effectuated.</returns>
        bool EnterPauseIfRequested(string state);
        /// <summary>
        /// The logger to use in the task.
        /// </summary>
        ILoggerScope Logger { get; }
    }
}
