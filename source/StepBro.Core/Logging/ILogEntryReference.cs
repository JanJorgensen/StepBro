namespace StepBro.Core.Logging
{
    /// <summary>
    /// Interface for an object thar carries a single <see cref="ITimestampedData"/> object reference.
    /// </summary>
    public interface ILogEntryReference
    {
        /// <summary>
        /// Reference to <see cref="ITimestampedData"/> object.
        /// </summary>
        ITimestampedData Entry { get; }
    }
}
