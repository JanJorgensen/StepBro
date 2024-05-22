using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;

namespace StepBro.StateMachine
{
    /// <summary>
    /// Interface to a state machine instance.
    /// This interface is a <see cref="IDynamicStepBroObject"/>, to make it possible to get and set defined statemachine variables as properties on this interface.
    /// </summary>
    public interface IStateMachine : IDynamicStepBroObject
    {
        string Name { get; }
        void ChangeState([Implicit] ICallContext context, Identifier state);
        void StartPollTimer([Implicit] ICallContext context, TimeSpan time, bool currentStateOnly);
        void StartTimer([Implicit] ICallContext context, string name, TimeSpan time, bool repeating, bool currentStateOnly);
        void StartTimer([Implicit] ICallContext context, string name, DateTime time, bool currentStateOnly);
        void StopTimer([Implicit] ICallContext context, string name);
        /// <summary>
        /// Stop the state machime instance.
        /// </summary>
        /// <param name="context">Standard call context object.</param>
        void Stop([Implicit] ICallContext context);

        DateTime StateEntryTime { get; }
    }
}
