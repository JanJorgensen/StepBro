namespace StepBro.Core.Controls
{
    public enum CustomPanelBindingState
    {
        /// <summary>
        /// Unknown panel type.
        /// </summary>
        TypeUnknown,
        /// <summary>
        /// The panel is not bindable (it is a "static" panel).
        /// </summary>
        NotBindable,
        /// <summary>
        /// Set to be bound to a variable/container, but the target was not found.
        /// </summary>
        NotBound,
        /// <summary>
        /// Bound to a found variable/container, but the object is not set.
        /// </summary>
        BoundWithoutObject,
        /// <summary>
        /// Set to be bound, but something went wrong binding to the variable/container or the object.
        /// </summary>
        BindingFailed,
        /// <summary>
        /// Successfully bound.
        /// </summary>
        Bound,
        /// <summary>
        /// Was bound, but is now disconnected.
        /// </summary>
        Disconnected
    }
}
