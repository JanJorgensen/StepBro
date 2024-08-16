using StepBro.Core.Api;
using StepBro.ToolBarCreator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.Controls
{
    public class ToolStripTextBoxWithAutoWidth : ToolStripTextBox, IResizeable
    {
        private int m_maxWidth = -1;
        private string m_widthGroup = null;
        private string m_changedText = null;
        private System.Windows.Forms.Timer m_timer = null;
        private object m_lock = new object();

        public ToolStripTextBoxWithAutoWidth()
        {
            m_timer = new System.Windows.Forms.Timer();
            m_timer.Tick += UpdateTimer_Tick;
            m_timer.Interval = 200;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                if (m_changedText != null)
                {
                    this.SetText(m_changedText);
                    m_changedText = null;
                }
                else
                {
                    m_timer.Stop();
                }
            }
        }

        public new string Text
        {
            get
            {
                lock (m_lock)
                {
                    return (m_changedText == null) ? base.Text : m_changedText;
                }
            }
            set
            {
                lock (m_lock)
                {
                    if (m_changedText == null)
                    {
                        if (m_timer.Enabled)
                        {
                            m_changedText = value;  // Let the running timer handle the change.
                        }
                        else
                        {
                            this.SetText(value);
                            m_timer.Start();        // Start the timer, to postpone the next change time.
                        }
                    }
                    else
                    {
                        m_changedText = value;  // Let the running timer handle the change.
                    }
                }
            }
        }

        private void SetText(string text)
        {
            base.Text = text;
        }

        public new int Width
        {
            get { return base.Width; }
            set
            {
                this.SetWidth(value);
            }
        }

        #region IResizeable members

        public int MaxWidth
        {
            get { return m_maxWidth; }
            set
            {
                m_maxWidth = value;
                //this.SetWidth(value);   // Temporary!! To be deleted.
            }
        }

        public void SetWidth(int width)
        {
            this.AutoSize = false;
            if (m_maxWidth > 0) width = Math.Min(m_maxWidth, width);
            this.Size = new Size(width, this.Height);
            this.Parent.Invalidate();
        }

        public int GetPreferredWidth()
        {
            return this.GetPreferredSize(new Size(m_maxWidth, this.Height)).Width;
        }

        public string WidthGroup { get { return m_widthGroup; } set { m_widthGroup = value; } }

        #endregion
    }
}
