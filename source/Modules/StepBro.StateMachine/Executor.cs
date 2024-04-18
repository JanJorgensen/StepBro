using Antlr4.Runtime.Dfa;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.StateMachine
{
    public class Executor : INameable
    {
        internal class InstanceData
        {
            private class Variable
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
            private List<NamedData<object>> m_variables;
            private List<Tuple<int, string, IProcedureReference>> m_states;
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
                m_variables = new List<NamedData<object>>();
                foreach (var variable in definition.ListVariables())
                {
                    m_variables.Add(new NamedData<object>(variable.Name, variable.Value));
                }
            }

            public string Name { get { return m_name; } }

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
        }

        public class EventData
        {
            private InstanceData m_instance;
            internal EventData(InstanceData instance, Event @event, object data)
            {
                m_instance = instance;
                this.StateEvent = @event;
                this.Data = data;
            }
            internal InstanceData Instance
            {
                get { return m_instance; }
            }
            public IProcedureReference Procedure { get; internal set; } = null;
            public IStateMachine Context { get; internal set; } = null;
            public Event StateEvent { get; private set; }
            public object Data { get; private set; }
        }

        private class Context : IStateMachine
        {
            private Executor m_executor;
            InstanceData m_instance = null;
            Event m_event = Event.ExternalEvent;

            public Context(Executor parent)
            {
                m_executor = parent;
            }

            public void Setup(InstanceData instance, Event @event)
            {
                m_instance = instance;
                StateEntryTime = DateTime.Now;
                m_event = @event;
            }

            public string Name { get { return m_instance.Name; } }

            public DateTime StateEntryTime { get; private set; }

            public void ChangeState([Implicit] ICallContext context, Identifier state)
            {
                if (m_event == Event.Exit)
                {
                    context.Logger.LogError("Trying to change state in Exit event.");
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

            public void StartPollTimer([Implicit] ICallContext context, TimeSpan time)
            {
                throw new NotImplementedException();
            }

            public void StartTimer([Implicit] ICallContext context, string name, TimeSpan time, bool repeating, bool currentStateOnly)
            {
                throw new NotImplementedException();
            }

            public void StartTimer([Implicit] ICallContext context, string name, DateTime time, bool currentStateOnly)
            {
                throw new NotImplementedException();
            }

            public void StopTimer([Implicit] ICallContext context, string name)
            {
                throw new NotImplementedException();
            }

            #region IDynamicStepBroObject

            public object GetProperty([Implicit] ICallContext context, string name)
            {
                context.Logger.LogDetail("Get '" + name + "' property.");
                return 5L;
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

            public void SetProperty([Implicit] ICallContext context, string name, object value)
            {
                context.Logger.LogDetail("Set '" + name + "' property.");
            }

            #endregion
        }

        private string m_name = "StateMachine";
        private List<InstanceData> m_instances = new List<InstanceData>();
        private Queue<EventData> m_highPriorityEvents = new Queue<EventData>();
        private Queue<EventData> m_lowPriorityEvents = new Queue<EventData>();
        private Context m_context;
        private EventData m_currentEvent = null;

        public Executor()
        {
            m_context = new Context(this);
        }

        public string Name { get { return m_name; } set { m_name = value; } }

        public void CreateStateMachine([Implicit] ICallContext context, StateMachineDefinition type, Identifier name, ArgumentList arguments)
        {
            context.Logger.Log("Name: " + name.Name);
            var instance = new InstanceData(type, name.Name);
            m_instances.Add(instance);

            m_highPriorityEvents.Enqueue(new EventData(instance, Event.Enter, null));
        }

        public EventData AwaitNextEvent([Implicit] ICallContext context, TimeSpan timeout)
        {
            // When we enter here, we might just have called a state procedure,
            // so we might have some post-actions to do.

            if (m_currentEvent != null) // Have a state procedure just been called?
            {
                if (m_currentEvent.StateEvent == Event.Exit)
                {
                    var enterEvent = new EventData(m_currentEvent.Instance, Event.Enter, null);
                    m_highPriorityEvents.Enqueue(enterEvent);
                }
            }

            m_currentEvent = null;

            if (m_highPriorityEvents.Count > 0)
            {
                m_currentEvent = m_highPriorityEvents.Dequeue();
            }
            else if (m_lowPriorityEvents.Count > 0)
            {
                m_currentEvent = m_lowPriorityEvents.Dequeue();
            }
            else
            {
                // Check the timers.
            }

            if (m_currentEvent != null)
            {
                m_currentEvent.Context = m_context;
                if (m_currentEvent.StateEvent == Event.Enter)
                {
                    m_currentEvent.Instance.SetNewState();  // Do the actual state change in the state machine.
                }
                m_currentEvent.Procedure = m_currentEvent.Instance.CurrentProcedure();
                m_context.Setup(m_currentEvent.Instance, m_currentEvent.StateEvent);
            }

            return m_currentEvent;
        }

        internal void MakeStateChange(InstanceData instance)
        {
            var exitEvent = new EventData(instance, Event.Exit, null);
            m_highPriorityEvents.Enqueue(exitEvent);
        }
    }
}
