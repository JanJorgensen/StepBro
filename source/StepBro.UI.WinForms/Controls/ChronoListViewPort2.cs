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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace StepBro.UI.WinForms.Controls
{
    public class ChronoListViewPort2 : Control, IChronoListView
    {
        //public interface IView
        //{
        //    int HorizontalScrollPosition { get; }
        //    ChronoListViewDynamicSettings DynamicSettings { get; }
        //    Font NormalFont { get; }
        //    Brush NormalTextColor { get; }
        //}

        private IChronoListViewer m_viewer = null;
        private ChronoListViewModel<ChronoListViewEntry>.ViewPortModel m_model = null;
        private Point m_mouseDownLocation = new Point();

        public ChronoListViewPort2() : base()
        {
            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.StandardClick, true);
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //m_viewSettings.ZeroTime = DateTime.UtcNow;
        }

        public void Setup(ChronoListViewModel<ChronoListViewEntry>.ViewPortModel model)
        {
            this.DataContext = model;
            m_model = model;
            model.Invalidated += Model_Invalidated;
            model.LineHeight = this.FontHeight;
        }

        private void Model_Invalidated(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private ChronoListViewModel<ChronoListViewEntry>.ViewPortModel Model { get { return m_model; } }

        public int HorizontalScrollPosition { get { return m_model.HorizontalScrollPosition; } }


        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (m_model != null)
            {
                m_model.LineHeight = this.Height;
            }
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

        public ChronoListViewDynamicSettings DynamicSettings { get { return m_model.DynamicSettings; } }

        public Font NormalFont { get { return this.Font; } }

        public Brush NormalTextColor { get { return Brushes.White; } }

        //public void RequestUpdate(long topEntry, int horizontalScrollPosition)
        //{
        //    System.Diagnostics.Debug.Assert(!this.InvokeRequired);
        //    System.Diagnostics.Debug.WriteLine("ChronoListViewPort.RequestUpdate");
        //    if (topEntry != m_topIndex)
        //    {
        //        m_lastViewScroll = DateTime.UtcNow;
        //    }
        //    m_topIndex = topEntry;
        //    m_horizontalScrollPosition = horizontalScrollPosition;
        //    //this.Refresh();
        //    this.Invalidate();
        //}

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect;
            if (m_model == null)
            {
                //m_viewEntryCount = 0;
                return;
            }

            var entries = m_model.Refresh();

            //m_viewSettings.ZeroTime = m_viewer.ZeroTime;
            //var sourceState = m_source.GetState();
            //long lastIndex = sourceState.LastIndex;
            //if (m_source == null || lastIndex < 0L)
            //{
            //    m_viewEntryCount = 0;
            //    return;
            //}

            var dynamicSettings = m_model.DynamicSettings;
            var horizontalScrollPosition = m_model.HorizontalScrollPosition;
            var lineHeight = m_model.LineHeight;
            bool first = true;
            while (first || dynamicSettings.ValueChanged())
            {
                first = false;
                int y = 0;
                e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
                var entryIndex = m_model.TopEntryIndex;
                int viewIndex = 0;
                //long lastShown = 0;
                try
                {
                    foreach (var entry in entries)
                    {
                        //lastShown = entryIndex;

                        var selectionState = m_viewer.GetEntryMarkState(entryIndex, entry);
                        rect = new Rectangle(horizontalScrollPosition, y, 10000, lineHeight);
                        if ((selectionState & EntryMarkState.Selected) != EntryMarkState.None)
                        {
                            e.Graphics.FillRectangle(Brushes.Blue, rect);
                            if ((selectionState & EntryMarkState.SearchMatch) != EntryMarkState.None)
                            {
                                var r = new Rectangle(horizontalScrollPosition, y + 1, dynamicSettings.TimeStampWidth + 2, lineHeight - 1);
                                e.Graphics.FillRectangle(Brushes.Purple, r);
                            }
                        }
                        else if ((selectionState & EntryMarkState.SearchMatch) != EntryMarkState.None)
                        {
                            var r = new Rectangle(horizontalScrollPosition, y + 1, 10000, lineHeight - 1);
                            e.Graphics.FillRectangle(Brushes.Purple, r);
                        }
                        if ((selectionState & EntryMarkState.Current) != EntryMarkState.None)
                        {
                            e.Graphics.DrawLine(Pens.White, 0, y - 1, this.ClientRectangle.Right, y - 1);
                            e.Graphics.DrawLine(Pens.White, 0, y + lineHeight, this.ClientRectangle.Right, y + lineHeight);
                        }
                        entry.DoPaint(e, this, ref rect, selectionState);

                        entryIndex++;
                        y += lineHeight;
                        viewIndex++;
                    }
                    //m_lastShown = lastShown;
                    //m_viewEntryCount = viewIndex;
                }
                catch
                {

                }
            }
        }


        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.Model.LineHeight = this.Font.Height;
        }

        #region Mouse

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            m_mouseDownLocation = e.Location;
            var line = (e.Location.Y / m_model.LineHeight);
            var index = m_model.TopEntryIndex + line;
            if (index > m_model.LastShownEntryIndex) index = -1L;
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
            var line = (e.Location.Y / m_model.LineHeight);
            var index = m_model.TopEntryIndex + line;
            if (index > m_model.LastShownEntryIndex) index = -1L;
            this.MouseUpOnLine?.Invoke(this, new MouseOnLineEventArgs(e, line, index));
        }

        #endregion
    }
}
