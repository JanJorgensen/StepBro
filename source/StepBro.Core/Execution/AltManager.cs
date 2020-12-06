using System;
using System.Collections.Generic;

namespace StepBro.Core.Execution
{
    public class AltManager
    {
        public enum AltType { Alt, Interleave }

        private class AltEntry : IAwaitAction
        {
            private readonly AltManager m_manager;
            public Action m_action;
            private bool m_trigged = false;
            private bool m_cancelled = false;
            public AltEntry(AltManager manager, Action action)
            {
                m_manager = manager;
                m_action = action;
            }

            public bool Cancelled
            {
                get { return m_cancelled; }
            }

            public event EventHandler CancelEvent;

            public void Cancel()
            {
                if (!m_trigged && !m_cancelled)
                {
                    m_cancelled = true;
                    this.CancelEvent?.Invoke(null, EventArgs.Empty);
                }
            }

            public bool ReportEvent()
            {
                if (!m_cancelled && !m_trigged)
                {
                    m_trigged = true;
                    lock (m_manager.m_sync)
                    {
                        return m_manager.ReportEvent(this);
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private readonly AltType m_type;
        private readonly List<AltEntry> m_alternatives = new List<AltEntry>();
        private Queue<AltEntry> m_trigged = new Queue<AltEntry>();
        private readonly object m_sync = new object();

        public AltManager(AltType type)
        {
            m_type = type;
        }

        public IAwaitAction CreateAlternative(Action action, string Name)
        {
            var entry = new AltEntry(this, action);
            m_alternatives.Add(entry);
            return entry;
        }

        private bool ReportEvent(AltEntry alt)
        {
            if (m_type == AltType.Interleave || m_trigged.Count == 0)
            {
                m_trigged.Enqueue(alt);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Await(TimeSpan timeoutTime, Action timeoutAction)
        {

        }
    }
}
