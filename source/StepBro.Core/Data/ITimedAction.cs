using System;

namespace StepBro.Core.Data
{
    public interface ITimedAction
    {
        bool IsActive { get; }
        bool Periodic { get; }
        TimeSpan PeriodicTime { get; }
        DateTime NextActionTime { get; }
        /// <summary>
        /// Method to update the object when its time is due.
        /// </summary>
        /// <returns>Whether the action is still active and should be added to the queue of action to handle.</returns>
        bool UpdateWhenHandling(DateTime now);
        void Deactivate();
    }
}
