using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StepBro.HostSupport.Models
{
    public class ChronoListViewModel<TViewEntryType> : ObservableObject where TViewEntryType : class, ITimestampedViewEntry
    {
        private IPresentationList<TViewEntryType> m_presentationSource = null;
        private DateTime m_zeroTime;
        private bool m_headMode = true;
        private long m_topEntry = 0L;
        private long m_lastIndex = 0;
        private bool m_viewDirty = false;
        private int m_verticalScrollMaximum = 0;
        private int m_verticalScrollValue = 0;
        private bool m_updateVerticalScroll = false;
        private int m_viewPortMaxLinesVisible = 10;
        private long m_currentEntryIndex = -1L;
        private TViewEntryType m_currentEntry = null;
        private List<long> m_selectedEntries = new List<long>();
        private long m_lastSingleSelectionEntry = -1L;
        private long m_rangeSelectionEnd = -1L;
        private Func<long, TViewEntryType, long, TViewEntryType, EntryMarkState> m_searchMatchChecker = null;

        public ChronoListViewModel()
        {
            m_zeroTime = DateTime.UtcNow;
        }

        public DateTime ZeroTime { get { return m_zeroTime; } set { m_zeroTime = value; } }

        public IElementIndexer<TViewEntryType> Source { get { return m_presentationSource; } }

        public long TopEntry { get { return m_topEntry; } }

        public int VerticalScrollMaximum
        {
            get { return m_verticalScrollMaximum; }
            set { this.SetProperty(ref m_verticalScrollMaximum, value); }
        }
        public int VerticalScrollValue
        {
            get { return m_verticalScrollValue; }
            set { this.SetProperty(ref m_verticalScrollValue, value); }
        }

        public int ViewPortMaxLinesVisible
        {
            get { return m_viewPortMaxLinesVisible; }
            set { this.SetProperty(ref m_viewPortMaxLinesVisible, value); }
        }

        public void Setup(IPresentationList<TViewEntryType> source)
        {
            m_presentationSource = source;
            m_topEntry = 0;
            //viewPort.SetDataSource(this);
        }

        public bool HeadMode
        {
            get { return m_headMode; }
            set
            {
                if (m_headMode != value)
                {
                    m_headMode = value;
                    m_presentationSource.SetHeadMode(m_headMode);
                    if (m_headMode)
                    {
                        m_lastSingleSelectionEntry = -1L;
                        this.SetCurrentEntryInternal(-1L, false, true, false);
                        m_presentationSource.UpdateHead();
                        this.RequestViewPortUpdate();
                    }
                    else
                    {

                    }
                    this.HeadModeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void UpdateVerticalScrollbar(long entryCount)
        {
            m_updateVerticalScroll = true;
            //this.VerticalScrollMaximum = Math.Max(0, (int)(entryCount - (viewPort.MaxLinesVisible - 10)));
            //if (m_presentationSource.InHeadMode || !viewPort.IsViewFilled())
            //{
            //    this.RequestViewPortUpdate();
            //}
            //m_topEntry = Math.Min(m_topEntry, vScrollBar.Maximum);
            //if (m_presentationSource.InHeadMode)
            //{
            //    vScrollBar.Value = (int)m_topEntry;
            //}
            m_updateVerticalScroll = false;
        }

        public event EventHandler HeadModeChanged;

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            m_presentationSource.UpdateHead();
            var state = m_presentationSource.GetState();
            if (m_viewDirty || state.LastIndex != m_lastIndex)
            {
                m_lastIndex = state.LastIndex;
                //if (m_presentationSource.InHeadMode || !viewPort.IsViewFilled())
                //{
                //    this.RequestViewPortUpdate();
                //}
                this.UpdateVerticalScrollbar(state.EffectiveCount);
            }
        }

        public void RequestViewUpdate(bool entryCountMightHaveChanged)
        {
            this.RequestViewPortUpdate();
            if (entryCountMightHaveChanged)
            {
                var state = m_presentationSource.GetState();
                this.UpdateVerticalScrollbar(state.EffectiveCount);
            }
        }

        private void RequestViewPortUpdate()
        {
            m_viewDirty = false;
            if (m_headMode)
            {
                m_topEntry = Math.Max(0L, m_lastIndex - (this.ViewPortMaxLinesVisible - 1L));
            }
            //viewPort.RequestUpdate(m_topEntry, 0 - hScrollBar.Value);
        }

        public void SetupSearchMatchChecker(Func<long, TViewEntryType, long, TViewEntryType, EntryMarkState> matchChecker)
        {
            m_searchMatchChecker = matchChecker;
            this.RequestViewPortUpdate();
        }

        public EntryMarkState GetEntryMarkState(long index, TViewEntryType entry)
        {
            var selectionState = EntryMarkState.None;
            if (m_selectedEntries.Count > 0)
            {
                if (m_selectedEntries.Contains(index))
                {
                    selectionState |= EntryMarkState.Selected;
                }
            }
            else
            {
                if (m_rangeSelectionEnd >= 0L)
                {
                    if (m_lastSingleSelectionEntry < m_rangeSelectionEnd)
                    {
                        if (index >= m_lastSingleSelectionEntry && index <= m_rangeSelectionEnd) selectionState |= EntryMarkState.Selected; ;
                    }
                    else
                    {
                        if (index >= m_rangeSelectionEnd && index <= m_lastSingleSelectionEntry) selectionState |= EntryMarkState.Selected; ;
                    }
                }
            }
            if (index == m_currentEntryIndex)
            {
                selectionState |= EntryMarkState.Current;
            }
            if (m_searchMatchChecker != null) selectionState |= m_searchMatchChecker(index, entry, m_currentEntryIndex, m_currentEntry);

            return selectionState;
        }

        #region Selection

        public long CurrentEntry { get { return m_currentEntryIndex; } }

        public void SetCurrentEntry(long index, bool setSelection, bool clearCurrentSelection = true)
        {
            this.SetCurrentEntryInternal(index, setSelection, clearCurrentSelection, true);
        }

        private void SetCurrentEntryInternal(long index, bool setSelection, bool clearCurrentSelection, bool updateView)
        {
            if (clearCurrentSelection)
            {
                m_selectedEntries.Clear();
            }

            m_currentEntryIndex = index;
            if (m_currentEntryIndex >= 0)
            {
                m_currentEntry = m_presentationSource.Get(m_currentEntryIndex);
            }
            else
            {
                m_currentEntry = null;
            }
            if (setSelection)
            {
                m_selectedEntries.Clear();
                if (index >= 0L)
                {
                    m_selectedEntries.Add(index);
                }
            }

            if (updateView)
            {
                if (index >= 0 && (index < m_topEntry || index > (m_topEntry + (this.ViewPortMaxLinesVisible - 2))))
                {
                    if (index < m_topEntry)
                    {
                        m_topEntry = Math.Max(0L, index - 4L);      // Set selection in top.
                    }
                    else
                    {
                        m_topEntry = Math.Max(0L, index - (this.ViewPortMaxLinesVisible - 5));  // Set selection in bottom.
                    }
                    m_updateVerticalScroll = true;
                    this.VerticalScrollValue = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                }

                this.RequestViewPortUpdate();
            }
        }

        #endregion

    }
}
