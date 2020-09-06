using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Controls;
using StepBro.Core.Execution;

using StepBroCoreTest.Utils;

namespace StepBroCoreTest.Mocks
{
    public class ExecutionScopeStatusUpdaterMock : StepBro.Core.Execution.IExecutionScopeStatusUpdate
    {
        private int m_level = 0;
        public int m_disposeCount = 0;
        public ExecutionScopeStatusUpdaterMock m_child = null;
        public string m_text = null;
        public TimeSpan m_expectedTime = default(TimeSpan);
        public long m_progressMax = -1;
        public long m_progress = -1;
        public long m_progressPokeCount = 0;
        public Func<long, string> m_progressFormatter = null;
        public List<Tuple<string, ButtonActivationType, Action<bool>>> m_buttons = new List<Tuple<string, ButtonActivationType, Action<bool>>>();

        public System.Windows.Media.Brush ProgressColor
        {
            get;
            set;
        }

        public bool PauseRequested
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler Disposed;

        public void AddActionButton(string title, ButtonActivationType type, Action<bool> activationAction)
        {
            throw new AccessViolationException();
            //MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").AddActionButton: " + title);
            //m_buttons.Add(new Tuple<string, ButtonActivationType, Action<bool>>(title, type, activationAction));
        }

        public void ClearSublevels()
        {
            if (m_child != null)
            {
                m_child.Dispose();
            }
        }

        public IExecutionScopeStatusUpdate CreateProgressReporter(string text = "", TimeSpan expectedTime = default(TimeSpan), long progressMax = -1L, Func<long, string> progressFormatter = null)
        {
            if (m_child != null) throw new Exception("Child status already active.");
            MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").CreateProgressReporter( " + text + " )");
            m_child = new ExecutionScopeStatusUpdaterMock();
            m_child.m_level = m_level + 1;
            m_child.m_text = text;
            m_child.m_expectedTime = expectedTime;
            m_child.m_progressMax = progressMax;
            m_child.m_progressFormatter = progressFormatter;
            m_child.Disposed += M_child_Disposed;
            return m_child;
        }

        private void M_child_Disposed(object sender, EventArgs e)
        {
            if (Object.ReferenceEquals(sender, m_child))
            {
                m_child.Disposed -= M_child_Disposed;
                m_child = null;
            }
        }

        public void Dispose()
        {
            m_disposeCount++;
            if (m_disposeCount == 1)
            {
                if (this.Disposed != null) this.Disposed(this, EventArgs.Empty);
            }
        }

        public void ProgressAliveSignal()
        {
            MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").ProgressAliveSignal");
            m_progressPokeCount++;
        }

        public void UpdateStatus(string text = null, long progress = -1)
        {
            if (text != null) m_text = text;
            if (progress >= 0) m_progress = progress;
            MiniLogger.Instance.Add(String.Format("TaskUpdate({0}).UpdateStatus: {1}, {2}", m_level, String.IsNullOrEmpty(text) ? "<no text>" : text, (progress >= 0) ? progress.ToString() : "<no progress>"));
        }

        public bool EnterPauseIfRequested(string state)
        {
            throw new NotImplementedException();
        }

        public void ProgressSetup(long start, long length, Func<long, string> toText)
        {
            throw new NotImplementedException();
        }
    }
}
