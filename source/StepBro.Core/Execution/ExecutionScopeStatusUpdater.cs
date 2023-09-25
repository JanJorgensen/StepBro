using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace StepBro.Core.Execution
{
    internal class ExecutionScopeStatusUpdater : IExecutionScopeStatusUpdate, IExecutionScopeStatus
    {
        public enum LevelProgressType
        {
            NoProgress,
            Progress,
            ElapsedTime,
            Timeout
        }

        private class ExecutionStateButton : IExecutionStateButton
        {
            private bool m_activated = false;
            private string m_title;
            private StateButtonAction m_activationAction;

            public ExecutionStateButton(string title, StateButtonAction activationAction)
            {
                m_title = title;
                m_activationAction = activationAction;
            }

            public string Title
            {
                get { return m_title; }
            }

            public bool ShownActivated
            {
                get { return m_activated; }
                set
                {
                    if (value != m_activated)
                    {
                        m_activated = value;
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShownActivated)));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void SetButtonState(bool pushed)
            {
                bool activated = m_activationAction(pushed);
                this.ShownActivated = activated;
            }
        }

        private ExecutionScopeStatusUpdater m_parent;
        private ExecutionScopeStatusUpdater m_child = null;
        private ObservableCollection<IExecutionScopeStatus> m_stateStack;
        private int m_level = 0;
        private bool m_isDisposed = false;

        private readonly DateTime m_startTime;
        private string m_text;
        private string m_mainText;
        TimeSpan m_expectedTime;
        private LevelProgressType m_progressType = LevelProgressType.NoProgress;
        private string m_progressText = null;
        private long m_progress = -1L;
        private long m_progressMax;
        private readonly Func<long, string> m_progressFormatter;
        private AttentionColor m_progressColor = AttentionColor.Normal;

        private ObservableCollection<IExecutionStateButton> m_buttons = new ObservableCollection<IExecutionStateButton>();
        private ReadOnlyObservableCollection<IExecutionStateButton> m_buttonsReadOnly = null;


        public ExecutionScopeStatusUpdater Child { get { return m_child; } }

        public ExecutionScopeStatusUpdater(
            ExecutionScopeStatusUpdater parent,
            ObservableCollection<IExecutionScopeStatus> stateStack,
            int level,
            string text,
            TimeSpan expectedTime,
            long progressMax,
            Func<long, string> progressFormatter)
        {
            m_startTime = DateTime.Now;
            m_parent = parent;
            m_stateStack = stateStack;
            m_level = level;
            m_text = text;
            m_expectedTime = expectedTime;
            m_progressMax = progressMax;
            m_progressFormatter = progressFormatter;
            this.Update();
            stateStack.Add(this);
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public ExecutionScopeStatusUpdater GetUpmostChild()
        {
            if (m_child != null) return m_child.GetUpmostChild();
            else return this;
        }

        public void Dispose()
        {
            if (!m_isDisposed)
            {
                this.ClearSublevels();
                if (m_parent != null)
                {
                    m_parent.m_child = null;
                }
                m_isDisposed = true;
            }
        }

        public void ClearSublevels()
        {
            if (m_child != null)
            {
                m_child.Dispose();
                m_child = null;
            }
        }


        #region Update Interface

        public IExecutionScopeStatusUpdate CreateProgressReporter(
            string text = "",
            TimeSpan expectedTime = default(TimeSpan),
            long progressMax = -1,
            Func<long, string> progressFormatter = null)
        {
            if (m_child != null)
            {
                this.ClearSublevels();
            }
            m_child = new ExecutionScopeStatusUpdater(this, m_stateStack, m_level + 1, text, expectedTime, progressMax, progressFormatter);
            return m_child;
        }

        public void ProgressAliveSignal()
        {
            System.Diagnostics.Debug.WriteLine(String.Format("[{0}] ProgressAliveSignal", m_level));
        }

        public void UpdateStatus(string text = null, long progress = -1)
        {
            //if (progress >= 0)
            //{
            //    m_progressText = null;
            //}
            //System.Diagnostics.Debug.WriteLine(String.Format("[{0}] UpdateStatus: {1}, {2}", m_level, String.IsNullOrEmpty(text) ? "<no text>" : text, (progress >= 0) ? progress.ToString() : "<no progress>"));
        }

        public void AddActionButton(string title, StateButtonAction activationAction)
        {
            m_buttons.Add(new ExecutionStateButton(title, activationAction));
        }

        public void ProgressSetup(long start, long length, Func<long, string> toText)
        {
            throw new NotImplementedException();
        }

        public void SetProgressColor(AttentionColor color)
        {
            throw new NotImplementedException();
        }

        #endregion


        private void Update()
        {
            string main = null;
            string progress = null;
            if (GetText(ref main, ref progress))
            {
                this.MainText = main;
                this.ProgressText = progress;
            }
        }

        public bool GetText(ref string main, ref string progress)
        {
            if (!m_isDisposed)
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

        #region IExecutionScopeStatus interface

        public string MainText
        {
            get
            {
                return m_mainText;
            }
            set
            {
                if (((value == null) != (m_mainText == null)) || String.Compare(value, m_mainText, StringComparison.InvariantCulture) != 0)
                {
                    m_mainText = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IExecutionScopeStatus.MainText)));
                }
            }
        }
        public string ProgressText
        {
            get
            {
                return m_progressText;
            }
            set
            {
                if (((value == null) != (m_progressText == null)) || String.Compare(value, m_progressText, StringComparison.InvariantCulture) != 0)
                {
                    m_progressText = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IExecutionScopeStatus.ProgressText)));
                }
            }
        }

        AttentionColor IExecutionScopeStatus.ProgressColor => throw new NotImplementedException();

        DateTime IExecutionScopeStatus.StartTime { get { return m_startTime; } }

        TimeSpan? IExecutionScopeStatus.ExpectedExecutionTime => throw new NotImplementedException();

        ReadOnlyObservableCollection<IExecutionStateButton> IExecutionScopeStatus.Buttons
        {
            get
            {
                if (m_buttonsReadOnly == null)
                {
                    m_buttonsReadOnly = new ReadOnlyObservableCollection<IExecutionStateButton>(m_buttons);
                }
                return m_buttonsReadOnly;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
