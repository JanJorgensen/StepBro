using FastColoredTextBoxNS;
using StepBro.Core.Data;
using StepBro.HostSupport;
using StepBro.HostSupport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static StepBro.UI.WinForms.Controls.ChronoListViewPort;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace StepBro.UI.WinForms.Controls
{
    public class ChronoListViewPort : Control, IChronoListView
    {
        private IChronoListViewer m_viewer = null;
        private IElementIndexer<ChronoListViewEntry> m_source = null;
        private int m_lineHeight = 20;
        private ChronoListViewEntry[] m_viewEntries = new ChronoListViewEntry[200];
        private int m_viewEntryCount = 0;
        private int m_horizontalScrollPosition = 0;
        private ChronoListViewDynamicSettings m_viewSettings = new ChronoListViewDynamicSettings();
        private Point m_mouseDownLocation = new Point();
        private DateTime m_lastViewScroll = DateTime.MinValue;

        private long m_topIndex = 0L;
        private long m_lastShown = -1L;

        public ChronoListViewPort() : base()
        {
            m_lineHeight = this.Font.Height;
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.StandardClick, true);
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            m_viewSettings.ZeroTime = DateTime.UtcNow;
        }

        public void SetDataSource(IChronoListViewer viewer, IElementIndexer<ChronoListViewEntry> source)
        {
            m_viewer = viewer;
            m_source = source;
        }

        public class MouseOnLineEventArgs : MouseEventArgs
        {
            private readonly int m_line;
            private readonly long m_index;

            public MouseOnLineEventArgs(MouseEventArgs args, int line, long index) : base(args.Button, args.Clicks, args.X, args.Y, args.Delta)
            {
                m_line = line;
                m_index = index;
            }

            public int Line { get { return m_line; } }
            public long Index { get { return m_index; } }
        }
        
        public delegate void MouseOnLineEventHandler(object sender, MouseOnLineEventArgs e);

        public event MouseOnLineEventHandler MouseDownOnLine;
        public event MouseOnLineEventHandler MouseUpOnLine;

        public int HorizontalScrollPosition
        {
            get { return m_horizontalScrollPosition; }
            set
            {
                m_horizontalScrollPosition = value;
            }
        }

        public ChronoListViewDynamicSettings DynamicSettings { get { return m_viewSettings; } }

        public Font NormalFont { get { return this.Font; } }

        public Brush NormalTextColor { get { return Brushes.White; } }

        public int MaxLinesVisible { get { return this.Height / m_lineHeight; } }
        public int MaxLinesPartlyVisible { get { return (this.Height + (m_lineHeight - 1)) / m_lineHeight; } }

        public long TopEntryIndex { get { return m_topIndex; } }
        public long LastShownEntryIndex { get { return m_lastShown; } }

        public DateTime LastViewScrollTime { get { return m_lastViewScroll; } }

        public bool ViewJustScrolled { get { return (DateTime.UtcNow - m_lastViewScroll) < TimeSpan.FromMilliseconds(500); } }

        public bool IsViewFilled()
        {
            return (m_viewEntryCount >= this.MaxLinesPartlyVisible);
        }

        public void RequestUpdate(long topEntry, int horizontalScrollPosition)
        {
            System.Diagnostics.Debug.Assert(!this.InvokeRequired);
            System.Diagnostics.Debug.WriteLine("ChronoListViewPort.RequestUpdate");
            if (topEntry != m_topIndex)
            {
                m_lastViewScroll = DateTime.UtcNow;
            }
            m_topIndex = topEntry;
            m_horizontalScrollPosition = horizontalScrollPosition;
            //this.Refresh();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect;
            if (m_source == null)
            {
                m_viewEntryCount = 0;
                return;
            }
            m_viewSettings.ZeroTime = m_viewer.ZeroTime;
            var sourceState = m_source.GetState();
            long lastIndex = sourceState.LastIndex;
            if (m_source == null || lastIndex < 0L)
            {
                m_viewEntryCount = 0;
                return;
            }

            bool first = true;
            while (first || m_viewSettings.ValueChanged())
            {
                first = false;
                int y = 0;
                e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
                var entryIndex = m_topIndex;
                int viewIndex = 0;
                long lastShown = 0;
                try
                {
                    while (entryIndex <= lastIndex && y < this.Height)
                    {
                        var entry = m_source.Get(entryIndex);
                        if (entry == null) break;
                        lastShown = entryIndex;
                        m_viewEntries[viewIndex] = entry;

                        var selectionState = m_viewer.GetEntryMarkState(entryIndex, entry);
                        rect = new Rectangle(m_horizontalScrollPosition, y, 10000, m_lineHeight);
                        if ((selectionState & EntryMarkState.Selected) != EntryMarkState.None)
                        {
                            e.Graphics.FillRectangle(Brushes.Blue, rect);
                            if ((selectionState & EntryMarkState.SearchMatch) != EntryMarkState.None)
                            {
                                var r = new Rectangle(m_horizontalScrollPosition, y + 1, this.DynamicSettings.TimeStampWidth + 2, m_lineHeight - 1);
                                e.Graphics.FillRectangle(Brushes.Purple, r);
                            }
                        }
                        else if ((selectionState & EntryMarkState.SearchMatch) != EntryMarkState.None)
                        {
                            var r = new Rectangle(m_horizontalScrollPosition, y + 1, 10000, m_lineHeight - 1);
                            e.Graphics.FillRectangle(Brushes.Purple, r);
                        }
                        if ((selectionState & EntryMarkState.Current) != EntryMarkState.None)
                        {
                            e.Graphics.DrawLine(Pens.White, 0, y - 1, this.ClientRectangle.Right, y - 1);
                            e.Graphics.DrawLine(Pens.White, 0, y + m_lineHeight, this.ClientRectangle.Right, y + m_lineHeight);
                        }
                        entry.DoPaint(e, this, ref rect, selectionState);

                        entryIndex++;
                        y += m_lineHeight;
                        viewIndex++;
                    }
                    m_lastShown = lastShown;
                    m_viewEntryCount = viewIndex;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("OnPaint exception!! - " + ex.GetType().Name + ", " + ex.Message);
                }
            }
        }


        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            m_lineHeight = this.Font.Height;
        }

        #region Mouse

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            m_mouseDownLocation = e.Location;
            var line = (e.Location.Y / m_lineHeight);
            var index = m_topIndex + line;
            if (line >= m_viewEntryCount || index > m_lastShown) index = -1L;
            this.MouseDownOnLine?.Invoke(this, new MouseOnLineEventArgs(e, line, index));
            if (!this.Focused)
            {
                this.Select();
            }


            //if ()

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            var line = (e.Location.Y / m_lineHeight);
            var index = m_topIndex + line;
            if (line >= m_viewEntryCount || index > m_lastShown) index = -1L;
            this.MouseUpOnLine?.Invoke(this, new MouseOnLineEventArgs(e, line, index));
        }

        #endregion
    }
}
