using StepBro.Core.Host;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.Core.Host.HostApplicationTaskHandler;

namespace StepBro.UI.WinForms
{
    public class FormWithHostApplicationTaskHandling : Form
    {
        private class MyTaskHandler : HostApplicationTaskHandler
        {
            FormWithHostApplicationTaskHandling m_parent;

            public MyTaskHandler(FormWithHostApplicationTaskHandling parent)
            {
                m_parent = parent;
            }

            protected override void RequestHostDomainHandling(Action action)
            {
                m_parent.BeginInvoke(action);   // Important! - do NOT wait here, therefore 'BeginInvoke'.
            }
        }

        private System.ComponentModel.IContainer components = null;
        private MyTaskHandler m_taskHandler;
        private System.Windows.Forms.Timer m_timer = null;
        private StateChange m_taskWorkingState = StateChange.Idle;
        private string m_taskWorkingText = "Ready";
        private int m_animationIndex = 0;
        private const int ANIMATION_STEPS = 4;

        public FormWithHostApplicationTaskHandling() : base()
        {
            components = new System.ComponentModel.Container();
            m_taskHandler = new MyTaskHandler(this);
            m_taskHandler.StateChangeEvent += TaskHandler_StateChangeEvent;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Starts a timer that enables <see cref="StateChange.StillWorking"/> updates every second in <see cref="OnTaskHandlingStateChanged"/>.
        /// </summary>
        public void StartUsingTaskHandlingTimer()
        {
            if (m_timer == null)
            {
                m_timer = new System.Windows.Forms.Timer(components);
                m_timer.Tick += Timer_Tick;
                m_timer.Interval = 1000;
                m_timer.Enabled = true;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (++m_animationIndex >= ANIMATION_STEPS)
            {
                m_animationIndex = 0;
            }
            m_taskWorkingState = StateChange.StillWorking;
            this.UpdateTaskWorkingState();
        }

        private void TaskHandler_StateChangeEvent(object sender, StateChangedEventArgs e)
        {
            switch (e.State)
            {
                case StateChange.Idle:
                    m_taskWorkingText = "Ready";
                    m_taskWorkingState = StateChange.Idle;
                    this.UpdateTaskWorkingState();
                    if (m_timer != null && m_timer.Enabled)
                    {
                        m_timer.Stop();
                    }
                    break;
                case StateChange.StartingNew:
                    m_taskWorkingText = e.WorkingText;
                    if (m_timer != null)
                    {
                        if (!m_timer.Enabled)
                        {
                            m_animationIndex = 0;
                            m_timer.Start();
                        }
                    }
                    if (m_taskWorkingState == StateChange.Idle)
                    {
                        m_taskWorkingState = StateChange.StartingNew;
                        this.UpdateTaskWorkingState();
                    }
                    break;
                case StateChange.StillWorking:
                    // Will never be sent from the HostApplicationTaskHandler.
                    break;
                default:
                    break;
            }
        }

        private void UpdateTaskWorkingState()
        {
            string text = m_taskWorkingText;
            if (m_timer != null && m_taskWorkingState != StateChange.Idle)
            {
                switch (m_animationIndex)
                {
                    case 0:
                        text += " \u25D0";
                        break;
                    case 1:
                        text += " \u25D3";
                        break;
                    case 2:
                        text += " \u25D1";
                        break;
                    case 3:
                        text += " \u25D2";
                        break;
                    default:
                        break;
                }
            }
            System.Diagnostics.Debug.WriteLine("TaskWorkingState: " + m_taskWorkingState.ToString() + " - " + text);
            this.OnTaskHandlingStateChanged(m_taskWorkingState, text);
        }

        /// <summary>
        /// Override this to get updates on the task handling state.
        /// </summary>
        /// <param name="change">The </param>
        /// <param name="workingText"></param>
        protected virtual void OnTaskHandlingStateChanged(StateChange change, string workingText)
        {
        }

        protected void AddTask<TState>(
            HostApplicationTaskHandler.Task<TState> task, 
            HostApplicationTaskHandler.Priority priority,
            string workingText, 
            string purposeText) where TState : struct, System.Enum
        {
            m_taskHandler.AddTask(task, priority, workingText, purposeText);
        }
    }
}
