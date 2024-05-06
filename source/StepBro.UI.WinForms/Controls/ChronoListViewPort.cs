using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.UI.WinForms.Controls.ChronoListViewPort;

namespace StepBro.UI.WinForms.Controls
{
    public class ChronoListViewPort : Control, IView
    {
        public interface IView
        {
            int HorizontalScrollPosition { get; }
            DynamicViewSettings ViewSettings { get; }
        }

        public enum TimestampFormat
        {
            Seconds,
            SecondsDelta,
            HoursMinutesSeconds,
            LocalTime,
            LocalDateTime
        }

        public class DynamicViewSettings
        {
            private bool m_valueChanged = false;
            private DateTime m_zeroTime;
            private TimestampFormat m_timeFormat = TimestampFormat.Seconds;
            private int m_timestampWidth = 0;
            private int m_lineHeaderEnd = 0;

            public bool ValueChanged()
            {
                if (m_valueChanged)
                {
                    m_valueChanged = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                m_timestampWidth = 0;
                m_lineHeaderEnd = 0;
            }

            public DateTime ZeroTime
            {
                get { return m_zeroTime; }
                set
                {
                    if (value != m_zeroTime)
                    {
                        m_zeroTime = value;
                        m_valueChanged = true;
                    }
                }
            }

            public TimestampFormat TimeFormat
            {
                get { return m_timeFormat; }
                set
                {
                    if (value != m_timeFormat)
                    {
                        m_timeFormat = value;
                        m_valueChanged = true;
                    }
                }
            }

            public int TimeStampWidth
            {
                get { return m_timestampWidth; }
                set
                {
                    if (value != m_timestampWidth)
                    {
                        m_timestampWidth = value;
                        m_valueChanged = true;
                    }
                }
            }
            public int LineHeaderEnd
            {
                get { return m_lineHeaderEnd; }
                set
                {
                    if (value != m_lineHeaderEnd)
                    {
                        m_lineHeaderEnd = value;
                        m_valueChanged = true;
                    }
                }
            }
        }

        private int m_horizontalScrollPosition = 0;
        private DynamicViewSettings m_viewSettings = new DynamicViewSettings();
        private ChronoListViewEntry m_first = null;

        public ChronoListViewPort()
        {
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.StandardClick, true);
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            m_viewSettings.ZeroTime = DateTime.UtcNow;
        }

        public int HorizontalScrollPosition
        {
            get { return m_horizontalScrollPosition; }
            set
            {
                m_horizontalScrollPosition = value;
            }
        }

        public DynamicViewSettings ViewSettings { get { return m_viewSettings; } }

        protected override void OnPaint(PaintEventArgs e)
        {
            System.Drawing.Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Black, e.ClipRectangle);

        }
    }
}
