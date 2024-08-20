using Antlr4.Runtime.Dfa;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.StateMachine
{
    public class Executor : INameable
    {
        internal const string PollTimerName = "poll";

        internal class InstanceData
        {
            public class Variable
            {
                public Variable(string name, object value)
                {
                    this.Name = name;
                    this.Value = value;
                }

                public string Name { get; private set; }
                public object Value { get; set; }
            }

            private StateMachineDefinition m_definition;
            private string m_name;
            private List<Variable> m_variables;
            private List<Tuple<int, string, IProcedureReference>> m_states;

            private bool m_isActive = true;
            private bool m_requestedStop = false;
            private bool m_requestedStopSeen = false;
            private uint m_stateTransitionNumber = 0;
            private int m_currentState = 0;
            private int m_requestedState = 0;

            public InstanceData(StateMachineDefinition definition, string name)
            {
                m_definition = definition;
                m_name = name;
                m_states = new List<Tuple<int, string, IProcedureReference>>();
                int i = 0;
                foreach (var s in definition.ListStates())
                {
                    m_states.Add(new Tuple<int, string, IProcedureReference>(i++, s.Name, s.Value));
                }
                m_variables = new List<Variable>();
                foreach (var variable in definition.ListVariables())
                {
                    m_variables.Add(new Variable(variable.Name, variable.Value));
                }
            }

            public string Name { get { return m_name; } }
            public string TypeName { get { return m_definition.Name; } }
            public bool IsActive { get { return m_isActive; } set { m_isActive = value; } }
            public bool StopRequested
            {
                get
                {
                    if (m_requestedStop)
                    {
                        m_requestedStopSeen = true;
                        return true;
                    }
                    else return false;
                }
            }
            public bool StopRequestedSeen
            {
                get { return m_requestedStopSeen; }
                set { m_requestedStopSeen = value; }
            }

            public uint StateTransitionNumber { get { return m_stateTransitionNumber; } }

            public bool RequestStateChange(string state)
            {
                var stateData = m_states.FirstOrDefault(s => String.Equals(state, s.Item2, StringComparison.InvariantCulture));
                if (stateData.Item2 == null || stateData.Item1 == m_currentState) return false;
                else
                {
                    m_requestedState = stateData.Item1;
                    return true;
                }
            }

            public IProcedureReference CurrentProcedure()
            {
                return m_states[m_currentState].Item3;
            }

            public string CurrentState()
            {
                return m_states[m_currentState].Item2;
            }

            public void SetNewState()
            {
                m_currentState = m_requestedState;
                m_requestedState = -1;
                m_stateTransitionNumber++;
            }

            public Variable GetVariable(string name)
            {
                return m_variables.FirstOrDefault(v => v.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }

            public void SetStopRequest()
            {
                m_requestedStop = true;
            }
        }

        internal class Timer : IDisposable
        {
            public Timer(InstanceData instance, string name, DateTime time)
            {
                Debug.Assert(time > DateTime.MinValue);
                this.Instance = instance;
                this.Name = name;
                this.Time = time;
            }
            public Timer(InstanceData instance, string name, DateTime time, uint stateIndex)
            {
                this.Instance = instance;
                this.Name = name;
                this.Time = time;
                this.CurrentStateOnly = true;
                this.StateTransitionIndex = stateIndex;
            }
            public bool IsActive()
            {
                return this.Instance != null;
            }
            public Timer Next { get; set; }     // As linked list.
            public InstanceData Instance { get; private set; }
            public string Name { get; private set; }
            public DateTime Time { get; set; }  // In UTC time
            public TimeSpan IntervalTime { get; set; } = TimeSpan.Zero;
            public bool CurrentStateOnly { get; set; } = false;
            public uint StateTransitionIndex { get; set; } = 0;

            public void Dispose()
            {
                this.Instance = null;
                this.Next = null;
            }
        }

        private class Context : IStateMachine, INameable, INamedObject
        {
            private Executor m_executor;
            InstanceData m_instance = null;
            Event m_event = Event.ExternalEvent;

            public Context(Executor parent)
            {
                m_executor = parent;
            }

            public override string ToString()
            {
                return this.Name;
            }

            public void Setup(InstanceData instance, Event @event)
            {
                m_instance = instance;
                StateEntryTime = DateTime.Now;
                m_event = @event;
            }

            public string Name { get { return m_instance.Name; } set { throw new NotSupportedException("Name change on 'Context' is not supported."); } }
            public string ShortName { get { return this.Name; } }
            public string FullName { get { return this.Name; } }

            public DateTime StateEntryTime { get; private set; }

            public void ChangeState([Implicit] ICallContext context, Identifier state)
            {
                if (m_event == Event.Exit)
                {
                    context.Logger.LogError("Trying to change state in Exit event; that's not allowed.");
                    return;
                }
                if (m_instance.RequestStateChange(state.Name))
                {
                    m_executor.MakeStateChange(m_instance);
                }
                else
                {
                    context.Logger.LogError("State change rejected. Current: " + m_instance.CurrentState());
                }
            }

            public void StartPollTimer([Implicit] ICallContext context, TimeSpan time, bool currentStateOnly)
            {
                m_executor.StopTimer(m_instance, PollTimerName);    // Just in case it is already started.
                var timer = new Timer(m_instance, PollTimerName, DateTime.UtcNow + time);
                timer.IntervalTime = time;
                if (currentStateOnly)
                {
                    timer.CurrentStateOnly = true;
                    timer.StateTransitionIndex = m_instance.StateTransitionNumber;
                }
                m_executor.AddTimer(timer);
            }

            public void StartTimer([Implicit] ICallContext context, string name, TimeSpan time, bool repeating, bool currentStateOnly)
            {
                m_executor.StopTimer(m_instance, name);    // Just in case it is already started.
                var timer = new Timer(m_instance, name, DateTime.UtcNow + time);
                if (repeating)
                {
                    timer.IntervalTime = time;
                }
                if (currentStateOnly)
                {
                    timer.CurrentStateOnly = true;
                    timer.StateTransitionIndex = m_instance.StateTransitionNumber;
                }
                context.Logger.Log(
                    "Start timer '" + name + "' in " + ((long)(time.TotalMilliseconds)).ToString() + "ms from now (at " + (DateTime.Now + time).ToString() + ")");
                m_executor.AddTimer(timer);
            }

            public void StartTimer([Implicit] ICallContext context, string name, DateTime time, bool currentStateOnly)
            {
                m_executor.StopTimer(m_instance, name);    // Just in case it is already started.
                var now = DateTime.UtcNow;
                var t = time.ToUniversalTime();
                if (time == DateTime.MinValue || (now - t) > TimeSpan.FromSeconds(60))
                {
                    context.Logger.LogError("Wrong time specified.");
                    return;
                }
                var timer = new Timer(m_instance, name, t);
                if (currentStateOnly)
                {
                    timer.CurrentStateOnly = true;
                    timer.StateTransitionIndex = m_instance.StateTransitionNumber;
                }
                context.Logger.Log(
                    "Start timer '" + name + "' at " + t.ToLocalTime().ToString() + " (" + ((long)((t - now).TotalMilliseconds)).ToString() + "ms from now)");
                m_executor.AddTimer(timer);
            }

            public void StopTimer([Implicit] ICallContext context, string name)
            {
                m_executor.StopTimer(m_instance, name);
            }

            public void Stop([Implicit] ICallContext context)
            {
                m_instance.IsActive = false;
            }

            public bool StopRequested { get { return m_instance.StopRequested; } }

            #region IDynamicStepBroObject

            public object GetProperty([Implicit] ICallContext context, string name)
            {
                var variable = m_instance.GetVariable(name);
                if (variable != null)
                {
                    if (context.LoggingEnabled)
                    {
                        context.Logger.LogDetail("Get '" + this.Name + "." + name + "': " + StringUtils.ObjectToString(variable.Value));
                    }
                    return variable.Value;
                }
                else
                {
                    context.Logger.LogError("No variable named '" + name + "'.");
                    return null;
                }
            }

            public void SetProperty([Implicit] ICallContext context, string name, object value)
            {
                if (context.LoggingEnabled)
                {
                    context.Logger.LogDetail("Set '" + this.Name + "." + name + "': " + StringUtils.ObjectToString(value));
                }
                var variable = m_instance.GetVariable(name);
                if (variable != null)
                {
                    if (variable.Value.GetType().IsAssignableFrom(value.GetType()))
                    {
                        variable.Value = value;
                    }
                    else
                    {
                        context.Logger.LogError("value for '" + name + "' is not a '" + variable.Value.GetType().Name + "'.");
                    }
                }
                else
                {
                    context.Logger.LogError("No variable named '" + name + "'.");
                }
            }

            public DynamicSupport HasMethod(string name, out NamedData<Type>[] parameters, out Type returnType)
            {
                throw new NotImplementedException();
            }

            public DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly)
            {
                throw new NotImplementedException();
            }

            public object InvokeMethod([Implicit] ICallContext context, string name, object[] args)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        private string m_name = "StateMachine";
        private List<InstanceData> m_instances = new List<InstanceData>();
        private Queue<EventData> m_highPriorityEvents = new Queue<EventData>();
        private Queue<EventData> m_lowPriorityEvents = new Queue<EventData>();
        private Context m_context;
        private EventData m_currentEvent = null;
        private Timer m_nextTimer = null;
        private bool m_stopRequested = false;

        public Executor()
        {
            m_context = new Context(this);
        }

        public string Name { get { return m_name; } set { m_name = value; } }

        public void Reset([Implicit] ICallContext context)
        {
            m_currentEvent = null;
            m_instances.Clear();
            m_highPriorityEvents.Clear();
            m_lowPriorityEvents.Clear();
            m_stopRequested = false;
            while (m_nextTimer != null)
            {
                var next = m_nextTimer.Next;
                m_nextTimer.Dispose();
                m_nextTimer = next;
            }
        }

        public void CreateStateMachine([Implicit] ICallContext context, StateMachineDefinition type, Identifier name, ArgumentList arguments)
        {
            context.Logger.Log("Name: " + name.Name);
            var instance = new InstanceData(type, name.Name);
            m_instances.Add(instance);

            m_highPriorityEvents.Enqueue(new EventData(instance, Event.Enter));

            foreach (var arg in arguments)
            {
                var v = instance.GetVariable(arg.Name);
                if (v != null)
                {
                    if (v.Value != null)
                    {
                        if (v.Value.GetType().IsAssignableFrom(arg.Value.GetType()))
                        {
                            v.Value = arg.Value;
                        }
                        else
                        {
                            context.ReportError("The statemachine variable '" + arg.Name + "' is not compatible with the specified value.");
                        }
                    }
                    else
                    {
                        context.ReportError("The statemachine variable '" + arg.Name + "' does not have value; the type is therefore unknown.");
                    }
                }
                else
                {
                    context.ReportError("The statemachine does not have a variable named '" + arg.Name + "'.");
                }
            }
        }

        public EventData AwaitNextEvent([Implicit] ICallContext context, TimeSpan timeout)
        {
            //System.Diagnostics.Debug.WriteLine("AwaitNextEvent enter");
            /////////////////// FIRST HANDLE THE CURRENT EVENT /////////////////// 
            // When we enter here, we might just have called a state procedure,
            // so we might have some post-actions to do.

            if (m_currentEvent != null) // Have a state procedure just been called?
            {
                if (m_currentEvent.StateEvent == Event.Exit)
                {
                    var enterEvent = new EventData(m_currentEvent.Instance, Event.Enter);
                    m_highPriorityEvents.Enqueue(enterEvent);
                }
                if (m_currentEvent.StateEvent == Event.StopRequested)
                {
                    m_currentEvent.Instance.StopRequestedSeen = true;
                }
            }

            m_currentEvent = null;

            /////////////////// NOW HANDLE THE NEXT EVENT /////////////////// 

            var entryTime = DateTime.UtcNow;
            //System.Diagnostics.Debug.WriteLine("AwaitNextEvent at " + entryTime.ToLongTimeString());

            while (true)
            { // We might be looping, waiting for a timer.
                //System.Diagnostics.Debug.WriteLine("AwaitNextEvent loop");

                var now = DateTime.UtcNow;
                // Check timers. Note that these timers use UTC time, to stay out of trouble with daylight saving.
                while (m_nextTimer != null && m_nextTimer.Time <= now)
                {
                    //System.Diagnostics.Debug.WriteLine("AwaitNextEvent timer expiry! - " + m_nextTimer.Instance.Name + " timer " + m_nextTimer.Name);
                    var t = m_nextTimer;
                    m_nextTimer = m_nextTimer.Next;

                    // Check if timer was bound to a state.
                    if (t.Instance.IsActive && (!t.CurrentStateOnly || t.StateTransitionIndex == t.Instance.StateTransitionNumber))
                    {
                        //System.Diagnostics.Debug.WriteLine("AwaitNextEvent do add timer event");
                        var e = new EventData(
                            t.Instance,
                            (t.Name.Equals(PollTimerName, StringComparison.InvariantCulture)) ? Event.PollTimer : Event.Timer);
                        e.Timer = t;

                        // Re-insert if repeating timer.
                        if (t.IntervalTime != TimeSpan.Zero)
                        {
                            t.Time += t.IntervalTime;
                            this.AddTimer(t);
                        }

                        m_lowPriorityEvents.Enqueue(e);
                    }
                }



                if (m_highPriorityEvents.Count > 0)
                {
                    m_currentEvent = m_highPriorityEvents.Dequeue();
                }
                else if (m_lowPriorityEvents.Count > 0)
                {
                    m_currentEvent = m_lowPriorityEvents.Dequeue();
                }

                if (m_currentEvent != null)
                {
                    if (!m_currentEvent.Instance.IsActive ||
                        (m_currentEvent.StateEvent == Event.StopRequested && m_currentEvent.Instance.StopRequestedSeen))    // Don't send if statemachine already knows about the stop request.
                    {
                        m_currentEvent = null;
                        continue;
                    }
                    //System.Diagnostics.Debug.WriteLine("AwaitNextEvent event!");
                    m_currentEvent.Context = m_context;
                    if (m_currentEvent.StateEvent == Event.Enter)
                    {
                        m_currentEvent.Instance.SetNewState();  // Do the actual state change in the state machine.
                    }
                    m_currentEvent.Procedure = m_currentEvent.Instance.CurrentProcedure();
                    m_context.Setup(m_currentEvent.Instance, m_currentEvent.StateEvent);

                    //context.Logger.LogDetail("Event: " + m_currentEvent.Instance.Name + "." + m_currentEvent.StateEvent.ToString() + ", " + m_currentEvent.Instance.StateTransitionNumber.ToString());
                    break;
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine("AwaitNextEvent no event; sleep?");
                    bool exitAfterSleep = true;
                    var sleepUntil = entryTime + timeout;    // Time for the specified timeout in method argument.
                    if (m_nextTimer != null && m_nextTimer.Time < sleepUntil)
                    {
                        sleepUntil = m_nextTimer.Time;
                        exitAfterSleep = false;
                    }
                    bool justStartedWaiting = true;
                    var timeLeft = sleepUntil - DateTime.UtcNow;
                    while (!m_stopRequested && timeLeft > TimeSpan.Zero)
                    {
                        //System.Diagnostics.Debug.WriteLine("AwaitNextEvent sleep");
                        if (justStartedWaiting)
                        {
                            context.Logger.LogDetail("Waiting " + ((long)timeLeft.TotalMilliseconds).ToString() + "ms");
                            justStartedWaiting = false;
                        }
                        if (!m_stopRequested && context.StopRequested())
                        {
                            context.Logger.Log("User requested execution stop.");
                            m_stopRequested = true;
                            foreach (var inst in m_instances)
                            {
                                m_lowPriorityEvents.Enqueue(new EventData(inst, Event.StopRequested));
                                inst.SetStopRequest();
                            }
                        }
                        var sleepTime = TimeSpan.FromMilliseconds(1000);
                        if (timeLeft < sleepTime) sleepTime = timeLeft;
                        //System.Diagnostics.Debug.WriteLine("Sleep " + ((long)sleepTime.TotalMilliseconds).ToString() + "ms");
                        Thread.Sleep(sleepTime);
                        timeLeft = sleepUntil - DateTime.UtcNow;
                    }
                    if (exitAfterSleep) break;
                }
            }

            //System.Diagnostics.Debug.WriteLine("AwaitNextEvent exit");
            return m_currentEvent;
        }

        public bool AnyActiveStateMachines
        {
            get
            {
                return m_instances.Any(inst => inst.IsActive);
            }
        }

        public void RequestStop([Implicit] ICallContext context)
        {
            foreach (var inst in m_instances)
            {
                inst.SetStopRequest();
            }
        }

        internal void MakeStateChange(InstanceData instance)
        {
            var exitEvent = new EventData(instance, Event.Exit);
            m_highPriorityEvents.Enqueue(exitEvent);
        }

        internal Timer TryGetTimer(InstanceData instance, string name)
        {
            var t = m_nextTimer;
            while (t != null)
            {
                if (Object.ReferenceEquals(t.Instance, instance) && t.Name.Equals(name, StringComparison.InvariantCulture))
                {
                    return t;
                }
                t = t.Next;
            }
            return null;
        }

        internal void AddTimer(Timer timer)
        {
            if (m_nextTimer == null || m_nextTimer.Time > timer.Time)
            {
                timer.Next = m_nextTimer;
                m_nextTimer = timer;
            }
            else
            {
                var current = m_nextTimer;
                while (current.Next != null && current.Next.Time < timer.Time)
                {
                    current = current.Next;
                }
                timer.Next = current.Next;
                current.Next = timer;
            }
        }

        internal bool StopTimer(InstanceData instance, string name)
        {
            Timer last = null;
            Timer timer = m_nextTimer;
            while (timer != null)
            {
                if (Object.ReferenceEquals(timer.Instance, instance) && timer.Name == name)
                {
                    timer.Dispose();
                    if (last != null)
                    {
                        last.Next = timer.Next;
                        timer.Next = null;
                    }
                    else
                    {
                        m_nextTimer = timer.Next;
                        timer.Next = null;
                    }
                    return true;
                }
                last = timer;
                timer = timer.Next;
            }
            return false;
        }
    }
}
