using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.Core.Tasks;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.SimpleWorkbench
{
    internal class ScriptExecutionData : IExecutionAccess
    {
        private MainForm m_parent;
        private IScriptExecution m_execution;
        private TaskExecutionState m_state = TaskExecutionState.StartRequested;

        public ScriptExecutionData(MainForm parent, string element, string partner, string @object, object[] arguments)
        {
            m_parent = parent;
            m_state = TaskExecutionState.Created;
            this.Element = element;
            this.Partner = partner;
            this.Object = @object;
            this.Arguments = arguments?.ToList();
        }
        public string Element { get; set; } = null;
        public string Partner { get; set; } = null;
        public string Object { get; set; } = null;
        public List<object> Arguments { get; set; } = null;
        public List<string> UnparsedArguments { get; set; } = null;
        public bool AddToHistory { get; set; } = false;
        public List<string> Errors { get; set; } = new List<string>();
        public IScriptExecution Execution { get { return m_execution; } }

        public void SetExecution(IScriptExecution execution)
        {
            m_execution = execution;
            this.NotifyState(execution.Task.CurrentState);
            execution.Task.CurrentStateChanged += Task_CurrentStateChanged;
        }

        private void Task_CurrentStateChanged(object sender, EventArgs e)
        {
            this.NotifyState(m_execution.Task.CurrentState);
        }

        public void NotifyState(TaskExecutionState state)
        {
            if (state != this.State)
            {
                this.State = state;
                if (state.HasEnded())
                {
                    m_execution.Task.CurrentStateChanged -= Task_CurrentStateChanged;
                }
                this.CurrentStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #region IExecutionAccess

        public TaskExecutionState State
        {
            get { return m_state; }
            private set
            {
                m_state = value;
            }
        }

        public object ReturnValue { get; set; }

        public event EventHandler CurrentStateChanged;

        public void RequestStopExecution()
        {
            if (!this.State.HasEnded())
            {
                m_execution.Task.RequestStop();
            }
        }

        #endregion
    }
}
