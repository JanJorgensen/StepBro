namespace StepBro.Core.Logging
{
    /// <summary>
    /// Enumerates the different logging options to set for a procedure and for a procedure call.
    /// </summary>
    public enum ContextLogOption
    {
        /// <summary>
        /// Force the <see cref="Normal"/> option even if the caller or the entity is set to <see cref="DebugOnly"/>.
        /// </summary>
        ForceAlways,
        /// <summary>
        /// Normal logging enabled and debug-logging is only done when the execution is in debug mode.
        /// </summary>
        Normal,
        /// <summary>
        /// The normal logging is only done when the execution is in debug mode.
        /// </summary>
        DebugOnly,
        /// <summary>
        /// No logging should be done; not even when debugging.
        /// </summary>
        Disabled
    }
}
