using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    internal class ExecutionStatusRoot : IExecutionScopeStatusUpdate
    {
        private ExecutionScopeStatusUpdater m_firstLevel = null;


        public IExecutionScopeStatusUpdate CreateProgressReporter(string text = "", TimeSpan expectedTime = default(TimeSpan), long progressMax = -1L, Func<long, string> progressFormatter = null)
        {
            if (m_firstLevel != null)
            {
                this.ClearSublevels();
            }
            m_firstLevel = new ExecutionScopeStatusUpdater(text, expectedTime, progressMax, progressFormatter);
            m_firstLevel.Disposed += M_firstLevel_Disposed;
            return m_firstLevel;
        }

        private void M_firstLevel_Disposed(object sender, EventArgs e)
        {
            if (m_firstLevel != null)
            {
                m_firstLevel.Disposed -= M_firstLevel_Disposed;
                m_firstLevel = null;
            }
        }

        public ExecutionScopeStatusUpdater FirstLevel { get { return m_firstLevel; } }


        public bool EnterPauseIfRequested(string state)
        {
            throw new NotImplementedException();
        }

        public void ProgressSetup(long start, long length, Func<long, string> toText)
        {
            throw new NotImplementedException();
        }

        public bool PauseRequested
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        #region Dead part of the ITaskStatusUpdate interface

        public AttentionColor ProgressColor
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

#pragma warning disable 67
        public event EventHandler Disposed;
        public event EventHandler ExpectedTimeExceeded;

        public void AddActionButton(string title, Func<bool,bool> activationAction)
        {
            throw new NotImplementedException();
        }

        public void ClearSublevels()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ProgressAliveSignal()
        {
            throw new NotImplementedException();
        }

        public void UpdateStatus(string text = null, long progress = -1)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
