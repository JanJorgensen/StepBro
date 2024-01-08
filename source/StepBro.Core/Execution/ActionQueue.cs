using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    public class ActionQueue
    {
        public enum Priority : uint { Low, BelowNormal, Normal, AboveNormal, High }

        public class Base
        {
            private enum Flags { None = 0, IsInHandling = 0x01, IsTimer = 0x02, IsActive = 0x04, IsRepeating = 0x08, IsDelayedRecall = 0x10 }

            internal Base m_next = null;
            private string m_name;
            private Priority m_priority;
            private Flags m_flags = Flags.None;

            protected Base(string name, Priority priority = Priority.Normal)
            {
                m_name = name;
                m_priority = priority;
            }

            public bool IsTimer { get { return m_flags.HasFlag(Flags.IsTimer); } }
            public bool IsRepeating { get { return m_flags.HasFlag(Flags.IsRepeating); } }
            public Priority Priority { get { return m_priority; } }


            internal void SetInHandling(bool inHandling)
            {
                if (inHandling) { m_flags |= Flags.IsInHandling; }
                else { m_flags &= ~Flags.IsInHandling; }
            }
            public bool IsInHandling { get { return false; } }

            protected virtual void OnQueueEntry() { }
            protected virtual void OnRelease() { }
        }

        public interface IActionContext
        {

        }

        public abstract class Action : Base
        {
            protected Action(string name, Priority priority = Priority.Normal) : base(name, priority)
            {
            }

            protected abstract void Invoke(IActionContext context);
        }

        public interface ITimernContext
        {

        }
    }
}
