using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;

namespace StepBro.Core.Api
{
    /// <summary>
    /// Enumerates the different results for the <seealso cref="IDynamicStepBroObject.HasProperty"/> and <seealso cref="IDynamicStepBroObject.HasMethod"/> methods.
    /// </summary>
    public enum DynamicSupport
    {
        /// <summary>
        /// The specified method or property is supported.
        /// </summary>
        Yes,
        /// <summary>
        /// The specified method or property is not supported.
        /// </summary>
        No,
        /// <summary>
        /// The dynamic object is currently not in a state where it can determine if the specified method or property is supported.
        /// </summary>
        NotReady,
        /// <summary>
        /// The dynamic object can not determine whether a specified method or property is supported until runtime.
        /// </summary>
        KnownAtRuntimeOnly
    }

    public interface IDynamicStepBroObject
    {
        DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly);
        object GetProperty([Implicit] ICallContext context, string name);
        void SetProperty([Implicit] ICallContext context, string name, object value);
        DynamicSupport HasMethod(string name, out NamedData<Type>[] parameters, out Type returnType);
        object InvokeMethod([Implicit] ICallContext context, string name, object[] args);
    }
}
