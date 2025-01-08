using CommunityToolkit.Mvvm.ComponentModel;
using StepBro.Core.Data;
using System.Drawing;
using System;
using static StepBro.Core.Data.PropertyBlockDecoder;
using System.Collections.Generic;

namespace StepBro.HostSupport.Models;

public partial class ChronoListViewModel<TViewEntryType>
{
    public partial class ViewPortModel : ObservableObject
    {
        private ChronoListViewModel<TViewEntryType> m_view;
        private IElementIndexer<TViewEntryType> m_source = null;
        private List<TViewEntryType> m_viewEntries = new List<TViewEntryType>(200);
        private int m_viewEntryCount = 0;
        private DynamicViewSettings m_viewSettings = new DynamicViewSettings();
        private Point m_mouseDownLocation = new Point();
        private DateTime m_lastViewScroll = DateTime.MinValue;

        private long m_topIndex = 0L;
        private long m_lastShown = -1L;
        private int m_horizontalScrollPosition = 0;

        private bool m_invalidated = false;
        private long m_newTopIndex = 0;
        private int m_newHorizontalScrollPosition = 0;


        internal ViewPortModel(ChronoListViewModel<TViewEntryType> view)
        {
            m_view = view;
            m_source = view.Source;
            view.ViewChanged += View_ViewChanged;
        }

        public int HorizontalScrollPosition
        {
            get { return m_horizontalScrollPosition; }
            set { this.SetProperty(ref m_horizontalScrollPosition, value); }
        }

        [ObservableProperty]
        private int m_height = 100;

        [ObservableProperty]
        private int m_lineHeight = 20;

        public int MaxLinesVisible { get { return this.Height / this.LineHeight; } }
        public int MaxLinesPartlyVisible { get { return (this.Height + (this.LineHeight - 1)) / this.LineHeight; } }

        public long TopEntryIndex { get { return m_topIndex; } }
        public long LastShownEntryIndex { get { return m_lastShown; } }

        public DateTime LastViewScrollTime { get { return m_lastViewScroll; } }

        public bool ViewJustScrolled { get { return (DateTime.UtcNow - m_lastViewScroll) < TimeSpan.FromMilliseconds(500); } }

        public bool IsViewFilled()
        {
            return (m_viewEntryCount >= this.MaxLinesPartlyVisible);
        }

        public event EventHandler Invalidated;

        public bool IsInvalidated { get { return m_invalidated; } }

        private void View_ViewChanged(object sender, ViewChangedEventArgs e)
        {
            m_invalidated = true;
            m_newHorizontalScrollPosition = e.HorizontalScroll;
            if (e.TopEntry != m_newTopIndex)
            {
                m_lastViewScroll = DateTime.UtcNow;
            }
            m_newTopIndex = e.TopEntry;
            this.Invalidated?.Invoke(this, EventArgs.Empty);
        }

        public IList<TViewEntryType> Refresh()
        {
            m_viewEntries.Clear();
            m_invalidated = false;
            m_topIndex = m_newTopIndex;
            m_horizontalScrollPosition = m_newHorizontalScrollPosition;

            m_viewEntryCount = 0;

            if (m_source != null)
            {
                m_viewSettings.ZeroTime = m_view.ZeroTime;
                var sourceState = m_source.GetState();
                long lastIndex = sourceState.LastIndex;
                if (lastIndex >= 0L)
                {
                    long i = m_topIndex;
                    int n = 0;
                    long lastShown = 0;
                    while (i <= lastIndex)
                    {
                        var entry = m_source.Get(i);
                        if (entry == null) break;
                        lastShown = i;
                        m_viewEntries.Add(entry);
                        n++;
                        i++;
                    }
                    m_lastShown = lastShown;
                    m_viewEntryCount = n;
                }
            }
            return m_viewEntries;
        }
    }
}
