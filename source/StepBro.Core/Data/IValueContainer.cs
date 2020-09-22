using System;
using StepBro.Core.Logging;

namespace StepBro.Core.Data
{
    /// <summary>
    /// General interface for an object containing a single value.
    /// </summary>
    public interface IValueContainer : IIdentifierInfo, IObjectContainer
    {
        /// <summary>
        /// An integer value identifying the container, used in storages for lookup identification.
        /// </summary>
        int UniqueID { get; }
        /// <summary>
        /// Gets the value of the container:
        /// </summary>
        /// <param name="logger">Interface for logging the container access.</param>
        /// <returns>Container value.</returns>
        object GetValue(ILogger logger = null);
        /// <summary>
        /// Sets the value of the container.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="logger">Interface for logging the container access.</param>
        void SetValue(object value, ILogger logger = null);
        /// <summary>
        /// Whether the value of the container is readonly.
        /// </summary>
        bool IsReadonly { get; }
        /// <summary>
        /// Indicates the general access level of the container.
        /// </summary>
        /// <remarks>The container does not check or use this value itself.</remarks>
        AccessModifier AccessProtection { get; }
    }

    /// <summary>
    /// General generic interface for an object containing a single value.
    /// </summary>
    /// <typeparam name="T">The type of the container value.</typeparam>
    public interface IValueContainer<T> : IValueContainer
    {
        /// <summary>
        /// Gets the value of the container.
        /// </summary>
        /// <param name="logger">Interface for logging the container access.</param>
        /// <returns>Container value.</returns>
        T GetTypedValue(ILogger logger = null);
        /// <summary>
        /// Modifies the value of the container with the specifed modifier function.
        /// </summary>
        /// <param name="modifier">Delegate to a modifier function.</param>
        /// <param name="logger">Interface for logging the container value modification.</param>
        /// <returns>The output value of the modifier, which can be different from the new value of the container.</returns>
        T Modify(ValueContainerModifier<T> modifier, ILogger logger = null);
    }

    /// <summary>
    /// Delegate type for modifying the value of a ValueContainer.
    /// </summary>
    /// <typeparam name="T">Value type of the ValueContainer.</typeparam>
    /// <param name="currentValue">The current container value.</param>
    /// <param name="newValue">The resulting new value for the container.</param>
    /// <returns>Value for the caller, which is typically either the current value or the new value of the container.</returns>
    public delegate T ValueContainerModifier<T>(T currentValue, out T newValue);

    public interface IValueContainerRich : IValueContainer
    {
        /// <summary>
        /// Event notifying when the value has been changed;
        /// </summary>
        event EventHandler ValueChanged;
        /// <summary>
        /// Index being incremented each time the value of the container has been changed.
        /// </summary>
        int ValueChangeIndex { get; }
        /// <summary>
        /// Synchronization object for locking the access to the container.
        /// </summary>
        object Sync { get; }
    }
}
