using StepBro.Core.Api;
using StepBro.Core.Data;
using System;

namespace StepBro.StateMachine
{
    /// <summary>
    /// Interface to a state machine instance.
    /// This interface is a <see cref="IDynamicStepBroObject"/>, to make it possible to get and set variables as properties on this interface.
    /// </summary>
    public interface IStateMachine : IDynamicStepBroObject
    {
        string Name { get; }
        void ChangeState(Identifier state);
        void StartPollTimer(TimeSpan time);
        void StartTimer(string name, TimeSpan time, bool repeating);
        void StartTimer(string name, DateTime time);
        void StopTimer(string name);
        DateTime StateEntryTime { get; }
    }
}
