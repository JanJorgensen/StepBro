using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.StateMachine.Executor;

namespace StepBro.StateMachine
{
    public class EventData
    {
        private InstanceData m_instance;
        internal EventData(InstanceData instance, Event @event)
        {
            m_instance = instance;
            this.StateEvent = @event;
        }
        internal InstanceData Instance
        {
            get { return m_instance; }
        }
        public IProcedureReference Procedure { get; internal set; } = null;
        public IStateMachine Context { get; internal set; } = null;
        public Event StateEvent { get; private set; }
        public string MessageName { get; internal set; } = null;
        public string TimerName { get; internal set; } = null;
        public object Data { get; internal set; } = null;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\'');
            sb.Append(m_instance.Name);
            sb.Append("' ");
            sb.Append(this.StateEvent.ToString());

            switch (StateEvent)
            {
                case Event.Timer:
                    sb.Append(" \"");
                    sb.Append(this.TimerName);
                    sb.Append('"');
                    break;
                case Event.ExternalEvent:
                    sb.Append(" \"");
                    sb.Append(this.MessageName);
                    sb.Append('"');
                    break;
            }
            return sb.ToString();
        }
    }
}
