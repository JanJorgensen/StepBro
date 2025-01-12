using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Drawing;

namespace StepBro.HostSupport.Models;

public partial class ChronoListViewModel<TViewEntryType> : ObservableObject where TViewEntryType : class, ITimestampedViewEntry
{
    private LogViewerModel<TViewEntryType> m_logViewModel = null;
    private IPresentationList<TViewEntryType> m_presentationSource = null;
    private ViewPortModel m_viewPort = null;

    private DateTime m_zeroTime;
    private bool m_headMode = true;
    private long m_topEntry = 0L;
    private long m_lastIndex = 0;
    private bool m_viewDirty = false;
    private int m_verticalScrollValue = 0;
    private bool m_updateVerticalScroll = false;
    private long m_currentEntryIndex = -1L;
    private TViewEntryType m_currentEntry = null;
    private List<long> m_selectedEntries = new List<long>();
    private long m_lastSingleSelectionEntry = -1L;
    private long m_rangeSelectionEnd = -1L;
    private Func<long, TViewEntryType, long, TViewEntryType, EntryMarkState> m_searchMatchChecker = null;

    private RelayCommand m_commandGotoHome;
    private RelayCommand m_commandGotoEnd;
    private RelayCommand m_commandMoveUp;
    private RelayCommand m_commandMoveDown;
    private RelayCommand m_commandMovePageUp;
    private RelayCommand m_commandMovePageDown;
    private RelayCommand m_commandSelectUp;
    private RelayCommand m_commandSelectDown;
    private RelayCommand m_commandSelectPageUp;
    private RelayCommand m_commandSelectPageDown;

    public ChronoListViewModel()
    {
        m_zeroTime = DateTime.UtcNow;
    }

    public DateTime ZeroTime { get { return m_zeroTime; } set { m_zeroTime = value; } }

    public IElementIndexer<TViewEntryType> Source { get { return m_presentationSource; } }

    public ViewPortModel ViewPort {  get { return m_viewPort; } }

    public long TopEntry { get { return m_topEntry; } }

    [ObservableProperty]
    private int m_verticalScrollMaximum = 0;

    public int VerticalScrollValue
    {
        get { return m_verticalScrollValue; }
        set
        {
            if (!m_updateVerticalScroll)
            {
                m_updateVerticalScroll = true;
                this.SetProperty(ref m_verticalScrollValue, value);
                m_updateVerticalScroll = false;
            }
        }
    }

    //[ObservableProperty]
    //private int m_viewPortMaxLinesVisible = 10;

    [ObservableProperty]
    private int m_horizontalScrollValue = 0;

    public void Setup(LogViewerModel<TViewEntryType> model)
    {
        m_logViewModel = model;
        m_presentationSource = m_logViewModel.PresentationList;
        m_topEntry = 0;
        m_viewPort = new ViewPortModel(this);
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


    public class ViewChangedEventArgs : EventArgs
    {
        public long TopEntry { get; private set; }
        public int HorizontalScroll { get; private set; }

        public ViewChangedEventArgs(long top, int horizontalScroll)
        {
            this.TopEntry = top;
            this.HorizontalScroll = horizontalScroll;
        }
    }

    public event EventHandler<ViewChangedEventArgs> ViewChanged;

    public void RequestUpdate()
    {
        m_presentationSource.UpdateHead();
        var state = m_presentationSource.GetState();
        if (m_viewDirty || state.LastIndex != m_lastIndex)
        {
            m_lastIndex = state.LastIndex;
            if (m_presentationSource.InHeadMode || !m_viewPort.IsViewFilled())
            {
                this.RequestViewPortUpdate();
            }
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
            m_topEntry = Math.Max(0L, m_lastIndex - (m_viewPort.MaxLinesVisible - 1L));
        }
        this.ViewChanged?.Invoke(this, new ViewChangedEventArgs(m_topEntry, 0 - this.HorizontalScrollValue));
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
            if (index >= 0 && (index < m_topEntry || index > (m_topEntry + (m_viewPort.MaxLinesVisible - 2))))
            {
                if (index < m_topEntry)
                {
                    m_topEntry = Math.Max(0L, index - 4L);      // Set selection in top.
                }
                else
                {
                    m_topEntry = Math.Max(0L, index - (m_viewPort.MaxLinesVisible - 5));  // Set selection in bottom.
                }
                m_updateVerticalScroll = true;
                this.VerticalScrollValue = (int)m_topEntry;
                m_updateVerticalScroll = false;
            }

            this.RequestViewPortUpdate();
        }
    }

    #endregion

    #region Commands

    public ICommand GotoHomeCommand
    {
        get
        {
            if (m_commandGotoHome == null)
            {
                m_commandGotoHome = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandGotoHome;
        }
    }

    public ICommand GotoEndCommand
    {
        get
        {
            if (m_commandGotoEnd == null)
            {
                m_commandGotoEnd = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandGotoEnd;
        }
    }

    public ICommand MoveUpCommand
    {
        get
        {
            if (m_commandMoveUp == null)
            {
                m_commandMoveUp = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandMoveUp;
        }
    }

    public ICommand MoveDownCommand
    {
        get
        {
            if (m_commandMoveDown == null)
            {
                m_commandMoveDown = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandMoveDown;
        }
    }

    public ICommand MovePageUpCommand
    {
        get
        {
            if (m_commandMovePageUp == null)
            {
                m_commandMovePageUp = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandMovePageUp;
        }
    }

    public ICommand MovePageDownCommand
    {
        get
        {
            if (m_commandMovePageDown == null)
            {
                m_commandMovePageDown = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandMovePageDown;
        }
    }

    public ICommand SelectUpCommand
    {
        get
        {
            if (m_commandSelectUp == null)
            {
                m_commandSelectUp = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandSelectUp;
        }
    }

    public ICommand SelectDownCommand
    {
        get
        {
            if (m_commandSelectDown == null)
            {
                m_commandSelectDown = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandSelectDown;
        }
    }

    public ICommand SelectPageUpCommand
    {
        get
        {
            if (m_commandSelectPageUp == null)
            {
                m_commandSelectPageUp = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandSelectPageUp;
        }
    }

    public ICommand SelectPageDownCommand
    {
        get
        {
            if (m_commandSelectPageDown == null)
            {
                m_commandSelectPageDown = new RelayCommand(
                    () =>
                    {
                    }
                );
            }
            return m_commandSelectPageDown;
        }
    }


    #endregion
}
