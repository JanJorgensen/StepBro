using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using StepBro.Core.Data;
using StepBro.HostSupport;
using StepBro.HostSupport.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.UI.Controls
{
    public class ChronoListViewPort : Control, INotifyPropertyChanged, ChronoListViewPort.IView
    {
        public interface IView
        {
            int HorizontalScrollPosition { get; }
            ChronoListViewDynamicSettings ViewSettings { get; }
            Typeface NormalFont { get; }
            double FontSize { get; }
            IBrush NormalTextColor { get; }
        }

        ChronoListViewModel.ViewPortModel m_model = null;
        private bool m_dataInvalidated = true;
        IList<ITimestampedViewEntry> m_entries = null;
        private ChronoListViewDynamicSettings m_viewSettings = null;
        private Avalonia.Point m_mouseDownLocation = new Avalonia.Point();
        private Typeface m_normalFont = Typeface.Default;
        private double m_fontSize = 1.0;
        public const int TicksPerSecond = 60;
        private readonly DispatcherTimer m_timer = new() { Interval = new TimeSpan(0, 0, 0, 0, 100) };
        private int m_horizontalScrollPosition = 0;


        public ChronoListViewPort()
        {
            this.Focusable = true;
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            if (m_model != null)
            {
                m_model.Height = (int)this.Bounds.Height;
            }
            m_dataInvalidated = true;
            this.InvalidateVisual();
        }

        public void Setup(ChronoListViewModel.ViewPortModel model)
        {
            this.DataContext = m_model = model;
            m_viewSettings = model.ViewSettings;
            model.PropertyChanged += Model_PropertyChanged;
            model.Invalidated += Model_Invalidated;
            m_timer.Tick += TimerTick;
            m_timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            m_model.View.RequestUpdate();
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(m_model.HorizontalScrollPosition))
            {
                m_horizontalScrollPosition = m_model.HorizontalScrollPosition;
            }
        }

        private void Model_Invalidated(object sender, EventArgs e)
        {
            m_dataInvalidated = true;
            this.InvalidateVisual();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            m_normalFont = new Typeface(new FontFamily("Consolas"));
            m_fontSize = 12.0;
            m_model.LineHeight = (int)(m_fontSize + 2.0);
            this.InvalidateVisual();
        }

        public int HorizontalScrollPosition
        {
            get { return m_horizontalScrollPosition; }
            set
            {
                m_horizontalScrollPosition = value;
            }
        }

        public ChronoListViewDynamicSettings ViewSettings { get { return m_viewSettings; } }

        public Typeface NormalFont { get { return m_normalFont; } }

        public double FontSize { get { return m_fontSize; } }

        public IBrush NormalTextColor { get { return Brushes.White; } }

        //protected override void OnFontChanged(EventArgs e)
        //{
        //    base.OnFontChanged(e);
        //    m_lineHeight = this.Font.Height;
        //}

        public override void Render(DrawingContext context)
        {
            Rect windowRect = this.Bounds;
            context.FillRectangle(Brushes.White, this.Bounds);
            var emSize = TextElement.GetFontSize(this);
            var penWhite = new Pen(Brushes.White, 1, lineCap: PenLineCap.Square);
            var penBlack = new Pen(Brushes.Black);

            Rect rect;
            
            if (m_model != null && (m_entries == null || m_dataInvalidated))
            {
                m_entries = m_model.Refresh();
                m_dataInvalidated = false;
            }

            if (m_entries == null || m_entries.Count == 0)
            {
                var formattedText = new FormattedText("So empty!", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, this.NormalFont, this.FontSize, Brushes.LightGray);
                context.DrawText(formattedText, windowRect.TopLeft);
                return;
            }

            bool first = true;
            while (first || m_viewSettings.ValueChanged())
            {
                first = false;
                int y = 0;
                context.FillRectangle(Brushes.Black, windowRect);
                var entryIndex = m_model.TopEntryIndex;
                var lineHeight = m_model.LineHeight;
                try
                {
                    foreach (ChronoListViewEntry entry in m_entries)
                    {
                        if (entry == null) break;

                        var selectionState = m_model.View.GetEntryMarkState(entryIndex, entry);
                        rect = new Rect(m_horizontalScrollPosition, y, windowRect.Width, lineHeight);
                        if ((selectionState & EntryMarkState.Selected) != EntryMarkState.None)
                        {
                            context.FillRectangle(Brushes.Blue, rect);
                            if ((selectionState & EntryMarkState.SearchMatch) != EntryMarkState.None)
                            {
                                var r = new Rect(m_horizontalScrollPosition, y + 1, this.ViewSettings.TimeStampWidth + 2, lineHeight - 1);
                                context.FillRectangle(Brushes.Purple, r);
                            }
                        }
                        else if ((selectionState & EntryMarkState.SearchMatch) != EntryMarkState.None)
                        {
                            var r = new Rect(m_horizontalScrollPosition, y + 1, 10000, lineHeight - 1);
                            context.FillRectangle(Brushes.Purple, r);
                        }
                        if ((selectionState & EntryMarkState.Current) != EntryMarkState.None)
                        {
                            context.DrawLine(penWhite, new Avalonia.Point(0, y), new Avalonia.Point(windowRect.Right, y));
                            context.DrawLine(penWhite, new Avalonia.Point(0, y + lineHeight + 1.0), new Avalonia.Point(windowRect.Right, y + lineHeight + 1.0));
                        }
                        entry.DoPaint(context, this, ref rect, selectionState);

                        entryIndex++;
                        y += lineHeight;
                    }
                }
                catch
                {

                }
            }

        }

        #region Mouse handling

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            m_mouseDownLocation = e.GetPosition(this);
            if (e.KeyModifiers == KeyModifiers.None)
            {
                long line = ((long)(m_mouseDownLocation.Y) / m_model.LineHeight);
                long index = m_model.TopEntryIndex + line;
                if (line >= m_entries.Count) index = -1L;

                if (!m_model.View.HeadMode || !m_model.ViewJustScrolled)
                {
                    m_model.View.HeadMode = false;
                    m_model.View.SetCurrentEntry(index, true);
                }
                else
                {
                    m_model.View.HeadMode = false;
                }

            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (e.KeyModifiers == KeyModifiers.Control)
            {
                var size = m_fontSize + e.Delta.Y * 0.8;
                if (size > 6.0 && size < 25.0)
                {
                    m_fontSize = size;
                    m_model.LineHeight = (int)(m_fontSize + 2.0);
                    m_dataInvalidated = true;
                    this.InvalidateVisual();
                }
            }
        }

        #endregion
    }
}
