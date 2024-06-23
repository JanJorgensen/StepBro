using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.UI.WinForms.Controls.ChronoListViewPort;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace StepBro.UI.WinForms.Controls
{
    public class ChronoListViewPort : Control, IView
    {
        public interface IView
        {
            int HorizontalScrollPosition { get; }
            DynamicViewSettings ViewSettings { get; }
            Font NormalFont { get; }
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
            private int m_timestampWidth = 0;   // The width of the widest seen timestamp.
            private int m_lineHeaderEnd = 0;    // The right side of the widest line header (timestamp and type)

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

        private object m_sync = new object();
        private IChronoListViewer m_viewer = null;
        private IElementIndexer<ChronoListViewEntry> m_source = null;
        private ChronoListViewEntry[] m_viewEntries = new ChronoListViewEntry[200];
        private int m_viewEntryCount = 0;
        private int m_horizontalScrollPosition = 0;
        private DynamicViewSettings m_viewSettings = new DynamicViewSettings();

        private long m_topIndex = 0L;

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

        public void SetDataSource(IChronoListViewer viewer)
        {
            m_viewer = viewer;
            m_source = viewer.Source;

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

        public Font NormalFont { get { return this.Font; } }


        protected override void OnPaint(PaintEventArgs e)
        {
            m_viewSettings.ZeroTime = m_viewer.ZeroTime;
            //System.Drawing.Graphics g = e.Graphics;
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
            int y = 0;
            var sourceState = m_source.GetState();
            long lastIndex = sourceState.LastIndex;
            if (lastIndex < 0L)
            {
                m_viewEntryCount = 0;
                return;
            }

            var entryIndex = m_topIndex;
            int viewIndex = 0;

            Rectangle rect;

            try
            {
                while (entryIndex <= lastIndex)
                {
                    var entry = m_source.Get(entryIndex);
                    m_viewEntries[viewIndex] = entry;

                    bool isSelected = false;
                    rect = new Rectangle(m_horizontalScrollPosition, y, 10000, 20);
                    if (isSelected)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, rect);
                    }
                    entry.DoPaint(e, this, ref rect, isSelected);

                    entryIndex++;
                    y += entry.Height;
                    viewIndex++;
                }
                m_viewEntryCount = viewIndex;
            }
            catch
            {

            }
        }
    }
}
