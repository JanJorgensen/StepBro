using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using StepBro.Core.Tasks;

namespace StepBro.Core.Execution
{
    public interface IExecutionScopeStatusUpdate : IDisposable, ITaskStateReporting
    {
        /// <summary>
        /// Cleanup the current status level by recursively removing all sub-status levels.
        /// </summary>
        void ClearSublevels();


        /// <summary>
        /// Creates a sub-task in the status reporting.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxTime"></param>
        /// <param name="progressMax"></param>
        /// <param name="progressFormatter"></param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// using (ITaskStatusUpdate reporter = CreateProgressReporter("Sending data", TimeSpan.FromSeconds(20), dataSize))
        /// {
        ///     stream.SendNextChunk();
        ///     reporter.UpdateStatus(stream.TotalBytesSent);
        /// }
        /// </code>
        /// </example>
        IExecutionScopeStatusUpdate CreateProgressReporter(
            string text = "",
            TimeSpan expectedTime = default(TimeSpan),
            long progressMax = -1,
            Func<long, string> progressFormatter = null);

        void AddActionButton(string title, Controls.ButtonActivationType type, Action<bool> activationAction);

        /// <summary>
        /// Notifies exit of the subtask.
        /// </summary>
        event EventHandler Disposed;

        /// <summary>
        /// Notifies that the initial expected time now is exceeded.
        /// </summary>
        event EventHandler ExpectedTimeExceeded;

        Brush ProgressColor { get; set; }

        //void SetProgress(long progress);
        //void IndicateProgress();
        //void SetText(string text);
    }
}
