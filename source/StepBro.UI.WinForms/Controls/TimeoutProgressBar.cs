using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.UI.WinForms.Controls
{
    public class TimeoutProgressBar : Control
    {
        private DateTime m_start = DateTime.MinValue;
        private TimeSpan m_time = TimeSpan.Zero;

        public TimeoutProgressBar()
        {
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, false);
        }

        public void Start(TimeSpan timeout)
        {
            m_time = timeout;
            m_start = DateTime.Now;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var br = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(br, this.ClientRectangle);
            }
            if (m_start != DateTime.MinValue)
            {
                var width = Math.Min(this.Width, (this.Width * (DateTime.Now.Ticks - m_start.Ticks)) / m_time.Ticks);
                var rc = new RectangleF(0, 0, (float)width, this.Height);
                using (var br = new SolidBrush(this.ForeColor))
                {
                    e.Graphics.FillRectangle(br, rc);
                }
            }
        }
    }
}
