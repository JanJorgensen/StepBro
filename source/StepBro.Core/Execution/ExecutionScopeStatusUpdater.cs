﻿using System;
using System.Collections.Generic;

namespace StepBro.Core.Execution
{
    internal class ExecutionScopeStatusUpdater : IExecutionScopeStatusUpdate
    {
        public enum LevelProgressType
        {
            NoProgress,
            Progress,
            ElapsedTime,
            Timeout
        }

        private int m_level = 0;
        private readonly string m_text = "";
        private readonly DateTime m_startTime;

        private readonly LevelProgressType m_progressType = LevelProgressType.NoProgress;
        private string m_progressText = null;
        private readonly long m_progress = -1L;
        private readonly Func<long, string> m_progressFormatter = null;
        private AttentionColor m_progressColor = AttentionColor.Normal;

        public List<Tuple<string, Func<bool,bool>>> m_buttons = null;

        private ExecutionScopeStatusUpdater m_child = null;

        public ExecutionScopeStatusUpdater Child { get { return m_child; } }

        public ExecutionScopeStatusUpdater(
            string text = "",
            TimeSpan expectedTime = default(TimeSpan),
            long progressMax = -1,
            Func<long, string> progressFormatter = null)
        {
            m_startTime = DateTime.Now;
        }

        private EventHandler m_expectedTimeExceeded;
        public event EventHandler ExpectedTimeExceeded
        {
            add
            {
                m_expectedTimeExceeded += value;
            }

            remove
            {
                m_expectedTimeExceeded -= value;
            }
        }

        public ExecutionScopeStatusUpdater GetUpmostChild()
        {
            if (m_child != null) return m_child.GetUpmostChild();
            else return this;
        }

        private void M_child_Disposed(object sender, EventArgs e)
        {
            if (m_child != null)
            {
                m_child.Disposed -= this.M_child_Disposed;
                m_child = null;
            }
        }

        public void Dispose()
        {
            if (m_level >= 0)
            {
                this.ClearSublevels();
                if (this.Disposed != null) this.Disposed(this, EventArgs.Empty);
                m_level = -1;
            }
        }

        public bool GetText(ref string main, ref string progress)
        {
            if (m_level >= 0)
            {
                if (!String.IsNullOrEmpty(m_text)) main = m_text;

                if (m_progressType == LevelProgressType.Progress)
                {
                    if (m_progressText == null)
                    {
                        if (m_progressFormatter != null)
                        {
                            m_progressText = m_progressFormatter(m_progress);
                        }
                    }
                    progress = m_progressText;
                }
                else if (m_progressType == LevelProgressType.ElapsedTime)
                {
                    progress = "<time>";
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Update Interface

        public event EventHandler Disposed;

        public void ClearSublevels()
        {
            if (m_child != null)
            {
                m_child.ClearSublevels();
                m_child = null;
            }
        }

        public IExecutionScopeStatusUpdate CreateProgressReporter(
            string text = "",
            TimeSpan expectedTime = default(TimeSpan),
            long progressMax = -1,
            //bool newStatusLevel = false,
            Func<long, string> progressFormatter = null)
        {
            if (m_child != null)
            {
                this.ClearSublevels();
            }
            m_child = new ExecutionScopeStatusUpdater(text, expectedTime, progressMax, progressFormatter);
            m_child.m_level = m_level + 1;
            m_child.Disposed += this.M_child_Disposed;
            return m_child;
        }

        public void ProgressAliveSignal()
        {
            System.Diagnostics.Debug.WriteLine(String.Format("[{0}] ProgressAliveSignal", m_level));
        }

        public void UpdateStatus(string text = null, long progress = -1)
        {
            if (progress >= 0)
            {
                m_progressText = null;
            }
            System.Diagnostics.Debug.WriteLine(String.Format("[{0}] UpdateStatus: {1}, {2}", m_level, String.IsNullOrEmpty(text) ? "<no text>" : text, (progress >= 0) ? progress.ToString() : "<no progress>"));
        }

        public void AddActionButton(string title, Func<bool, bool> activationAction)
        {
            if (m_buttons == null)
            {
                m_buttons = new List<Tuple<string, Func<bool,bool>>>();
            }
            m_buttons.Add(new Tuple<string, Func<bool,bool>>(title, activationAction));
        }

        public bool EnterPauseIfRequested(string state)
        {
            throw new NotImplementedException();
        }

        public void ProgressSetup(long start, long length, Func<long, string> toText)
        {
            throw new NotImplementedException();
        }

        public AttentionColor ProgressColor
        {
            get
            {
                return m_progressColor;
            }
            set
            {
                m_progressColor = value;
            }
        }

        public bool PauseRequested
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
