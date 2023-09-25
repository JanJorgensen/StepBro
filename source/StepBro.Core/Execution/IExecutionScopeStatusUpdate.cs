using System;
using StepBro.Core.Tasks;

namespace StepBro.Core.Execution
{
    public enum AttentionColor
    {
        Normal,
        Warning,
        Error
    }

    /// <summary>
    /// Delegate type for state buttons.
    /// </summary>
    /// <param name="activated">Whether the button is being activated (true) or released (false).</param>
    /// <returns>Whether the button should be shown in pushed state.</returns>
    public delegate bool StateButtonAction(bool activated);

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

        void AddActionButton(string title, StateButtonAction activationAction);

        void SetProgressColor(AttentionColor color);
    }
}
