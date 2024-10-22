namespace StepBro.Core.Logging
{
    /// <summary>
    /// Interface for an object thar carries a single <see cref="ILogEntry"/> object reference.
    /// </summary>
    public interface ILogEntryReference
    {
        /// <summary>
        /// Reference to <see cref="ILogEntry"/> object.
        /// </summary>
        ILogEntry Entry { get; }
    }
}
