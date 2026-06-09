using CommunityToolkit.Mvvm.ComponentModel;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models;

public class LogViewerModel<TViewEntryType> : ItemViewModel where TViewEntryType : class, ITimestampedViewEntry
{
    private class ViewPresentationList : PresentationListForListData<ITimestampedData, TViewEntryType>
    {
        private IChronoListViewEntryFactory<TViewEntryType> m_viewEntryCreator;

        public ViewPresentationList(IDataListSource<ITimestampedData> source, IChronoListViewEntryFactory<TViewEntryType> viewEntryCreator) :
            base(source, 1000000, 50)
        {
            m_viewEntryCreator = viewEntryCreator;
        }

        public override void CreatePresentationEntry(ITimestampedData entry, long sourceIndex, Action<TViewEntryType> adder)
        {
            m_viewEntryCreator.CreatePresentationEntry(entry, sourceIndex, adder);
            //if ((entry.EntryType & LogEntry.Type.Special) != LogEntry.Type.Special)
            //{
            //    adder(new LogViewEntry(entry, sourceIndex));
            //}
            //else
            //{
            //    adder(new LogViewEntrySpecial(entry, sourceIndex));     // TODO: Get reference to the special handler for this data type.
            //    // TODO: Maybe the special handler could throw in a decoded/translated entry.
            //}
        }

        public override LogEntry PresentationToSource(TViewEntryType entry)
        {
            return entry.DataObject as LogEntry;
        }
    }

    private class PresentationListSearchingForFirstSource : PresentationListForListData<ITimestampedData, TViewEntryType>
    {
        private class EmptySource : IDataListSource<ITimestampedData>
        {
            public ITimestampedData Get(long index)
            {
                return null;
            }

            public Tuple<long, ITimestampedData> GetFirst()
            {
                return new Tuple<long, ITimestampedData>(-1L, null);
            }

            public ITimestampedData GetLast()
            {
                return null;
            }

            public IndexerStateSnapshot GetState()
            {
                return new IndexerStateSnapshot(-1L, -1L, 0L);
            }

            public IDataWalker<ITimestampedData> WalkFrom(long start = -1)
            {
                return null;
            }
        }

        private LogViewerModel<TViewEntryType> m_parent;
        private IDataListSource<ITimestampedData> m_source;
        private long m_lastBefore;

        public PresentationListSearchingForFirstSource(LogViewerModel<TViewEntryType> parent, IDataListSource<ITimestampedData> source, long lastBefore) :
            base(new EmptySource(), 100, 10)
        {
            m_parent = parent;
            m_source = source;
            m_lastBefore = lastBefore;
        }

        public override void CreatePresentationEntry(ITimestampedData entry, long sourceIndex, Action<TViewEntryType> adder)
        {
        }

        public override LogEntry PresentationToSource(TViewEntryType entry)
        {
            return null;
        }

        public override void UpdateHead()
        {
            if (m_source.GetState().LastIndex > m_lastBefore)
            {
                m_parent.SetupZeroStart();
            }
        }
    }

    private class NewLogStart : IDataListSource<ITimestampedData>
    {
        private IDataListSource<ITimestampedData> m_source;
        private ITimestampedData m_firstEntry;
        private long m_firstIndex;

        public NewLogStart(IDataListSource<ITimestampedData> source, ITimestampedData first, long firstIndex)
        {
            m_source = source;
            m_firstEntry = first;
            m_firstIndex = firstIndex;
        }

        public Tuple<long, ITimestampedData> GetFirst()
        {
            return new Tuple<long, ITimestampedData>(m_firstIndex, m_firstEntry);
        }

        public ITimestampedData GetLast()
        {
            return m_source.GetLast();
        }

        public IndexerStateSnapshot GetState()
        {
            var sourceState = m_source.GetState();
            return new IndexerStateSnapshot(m_firstIndex, sourceState.LastIndex, sourceState.LastIndex - m_firstIndex + 1L);
        }

        public IDataWalker<ITimestampedData> WalkFrom(long start = -1)
        {
            if (start < 0L) start = m_firstIndex;
            return m_source.WalkFrom(start);
        }

        public ITimestampedData Get(long index)
        {
            return m_source.Get(index);
        }
    }

    private delegate bool SkipChecker(LogEntry entry);

    private IDataListSource<ITimestampedData> m_source = null;
    private PresentationListForListData<ITimestampedData, TViewEntryType> m_presentationList = null;
    private IChronoListViewEntryFactory<TViewEntryType> m_viewEntryCreator = null;
    private long m_lastEntryIndexBeforeClear = -1L;
    private ChronoListViewModel<TViewEntryType> m_listView = null;
    private static ITimestampedData s_lastEntryBeforeClear = null;
    private NewLogStart m_zeroStartSource = null;
    private int m_visibleLevels = 1000;
    private Predicate<ITimestampedData>[] m_filter = null;
    private bool m_enoughCharsInSearchText = false;
    private bool m_markSearchMatches = true;
    private string m_searchText = "";
    private bool m_quickSearchActivated = false;

    public LogViewerModel(IChronoListViewEntryFactory<TViewEntryType> viewEntryCreator) : base(ViewType.ExecutionLog, "LogViewer")
    {
        m_viewEntryCreator = viewEntryCreator;
        m_listView = new ChronoListViewModel<TViewEntryType>();
        this.Title = "LOG";
        this.SupportedWindowGroups = [HostAppModel.WG_DOCUMENT, HostAppModel.WG_SECONDARY, HostAppModel.WG_BOTTOM];
        this.DataContext = this;

    }

    public void Setup()
    {
        m_source = StepBro.Core.Main.Logger;
        m_presentationList = new ViewPresentationList(m_source, m_viewEntryCreator);
        this.CreateFilter();
        m_listView.ZeroTime = StepBro.Core.Main.Logger.GetFirst().Item2.Timestamp;
        m_listView.Setup(m_presentationList);
        m_listView.SetupSearchMatchChecker(this.SearchMarkMatchChecker);
        //this.SetupFromHeadMode();
    }

    public ChronoListViewModel<TViewEntryType> ListViewModel { get { return m_listView; } }

    public string SearchText
    {
        get => m_searchText;
        set => SetProperty(ref m_searchText, value);
    }

    public bool QuickSearchActivated
    {
        get => m_quickSearchActivated;
        set => SetProperty(ref m_quickSearchActivated, value);
    }

    public event EventHandler PresentationListChanged;

    #region Filters

    private bool LevelFilter(ITimestampedData entry)
    {
        return ((entry as LogEntry).IndentLevel < m_visibleLevels);
    }

    private bool CombinedFilter(ITimestampedData entry)
    {
        foreach (var f in m_filter)
        {
            if (f(entry as LogEntry) == false) return false;
        }
        return true;
    }

    private void CreateFilter()
    {
        var filter = new List<Predicate<ITimestampedData>>();
        if (m_visibleLevels < 1000)
        {
            filter.Add(this.LevelFilter);
        }
        filter.Add(LogFilters.Normal);
        if (m_quickSearchActivated && m_searchText.Length > 1)
        {
            filter.Add(this.SearchMatching);
        }
        m_filter = filter.ToArray();

        m_presentationList.Reset(this.CombinedFilter, Int64.MaxValue);
        m_presentationList.Get(Int64.MaxValue);
    }

    public int VisibleLevels
    {
        get => m_visibleLevels;
        set
        {
            if (this.SetProperty(ref m_visibleLevels, value))
            {
                this.CreateFilter();
                //logView.Setup(m_presentationList);
                //logView.RequestViewUpdate(true);
            }
        }
    }

    #endregion

    //private void toolStripButtonFollowHead_CheckedChanged(object sender, EventArgs e)
    //{
    //    logView.HeadMode = toolStripButtonFollowHead.Checked;
    //}

    //private void SetupFromHeadMode()
    //{
    //    //    toolStripButtonFollowHead.Checked = logView.HeadMode;

    //    //    toolStripButtonSkipPrevious.Enabled = !logView.HeadMode;
    //    //    toolStripButtonSkipNext.Enabled = !logView.HeadMode;
    //}

    public void JumpTo(LogEntry logEntry, bool clearSelection)
    {
        try
        {
            var state = m_presentationList.GetState();
            var entryIndex = state.FirstIndex;

            while (true)
            {
                var entry = m_presentationList.Get(entryIndex);
                if (entry != null)
                {
                    if (Object.ReferenceEquals(entry.DataObject, logEntry))
                    {
                        //logView.HeadMode = false;
                        //logView.SetCurrentEntry(entryIndex, clearSelection);
                    }
                }
                else break;

                entryIndex++;
            }
        }
        finally { }
    }

    private void toolStripButtonClear_Click(object sender, EventArgs e)
    {
        m_lastEntryIndexBeforeClear = m_source.GetState().LastIndex;
        s_lastEntryBeforeClear = m_source.Get(m_lastEntryIndexBeforeClear);
        m_presentationList = new PresentationListSearchingForFirstSource(this, m_source, m_lastEntryIndexBeforeClear);
        //logView.Setup(m_presentationList);
    }

    private void SetupZeroStart()
    {
        bool headMode = m_presentationList.InHeadMode;
        var zeroTime = m_source.Get(m_lastEntryIndexBeforeClear + 1L).Timestamp;
        m_zeroStartSource = new NewLogStart(m_source, m_source.Get(m_lastEntryIndexBeforeClear + 1L), m_lastEntryIndexBeforeClear + 1L);
        m_presentationList = new ViewPresentationList(m_zeroStartSource, m_viewEntryCreator);
        m_presentationList.SetHeadMode(headMode);
        this.CreateFilter();
        //logView.ZeroTime = zeroTime;
        //logView.Setup(m_presentationList);
    }

    private void toolStripDropDownButtonLoggers_DropDownOpening(object sender, EventArgs e)
    {
        //toolStripDropDownButtonLoggers.DropDownItems.Clear();
        //var specialLogging = Core.Main.GetService<IComponentLoggerService>();

        //foreach (var logger in specialLogging.ListLoggers())
        //{
        //    var item = new ToolStripMenuItem();
        //    item.CheckOnClick = true;
        //    item.Checked = logger.Enabled;
        //    item.Name = "toolStripMenuItemSpecialLogger" + logger.Name;
        //    item.Size = new Size(180, 22);
        //    item.Text = logger.Name;
        //    item.Tag = logger;
        //    item.CheckedChanged += Item_CheckedChanged;
        //}

        //if (toolStripDropDownButtonLoggers.DropDownItems.Count == 0)
        //{
        //    toolStripDropDownButtonLoggers.DropDownItems.Add(toolStripMenuItemSpecialLoggersNoneAvailable);
        //}
    }

    //private void Item_CheckedChanged(object sender, EventArgs e)
    //{
    //    var item = (ToolStripMenuItem)sender;
    //    var logger = (IComponentLogging)item.Tag;
    //    logger.Enabled = item.Checked;
    //}

    #region Searching

    /// <summary>
    /// Indicates whether the specified log entry should be marked in the view as a search match.
    /// </summary>
    /// <param name="index">The view entry index.</param>
    /// <param name="entry"></param>
    /// <param name="curentIndex"></param>
    /// <param name="currentEntry"></param>
    /// <returns>Whether to mark as a match.</returns>
    private EntryMarkState SearchMarkMatchChecker(
        long index, ITimestampedViewEntry entry,
        long curentIndex, ITimestampedViewEntry currentEntry)
    {
        var markings = EntryMarkState.None;
        //if (m_markSearchMatches && toolStripTextBoxQuickSearch.Text.Length > 1)
        //{
        //    if (this.SearchMatching(m_presentationList.Get(index).DataObject as LogEntry))
        //    {
        //        markings |= EntryMarkState.SearchMatch;
        //    }
        //}
        if (currentEntry != null && !Object.ReferenceEquals(entry, currentEntry) && entry.DataObject is LogEntry logEntry && currentEntry.DataObject is LogEntry currentLogEntry)
        {
            if (Object.ReferenceEquals(logEntry.Parent, currentLogEntry.Parent))
            {
                markings |= EntryMarkState.Sibling;
            }
            if (logEntry.IsParentOf(currentLogEntry, true))
            {
                markings |= EntryMarkState.Parent;
            }
        }
        return markings;
    }

    //private void toolStripTextBoxQuickSearch_TextChanged(object sender, EventArgs e)
    //{
    //    var text = toolStripTextBoxQuickSearch.Text;
    //    if (!String.IsNullOrEmpty(text))
    //    {
    //        SetupSkipChoises(toolStripMenuItemSkipSearchMatches);
    //    }
    //    toolStripButtonClearSearch.Visible = !String.IsNullOrEmpty(text);
    //    bool enoughChars = false;
    //    if (toolStripTextBoxQuickSearch.Text.Length < 2)
    //    {
    //        toolStripDropDownButtonQuickSearchOptions.Text = "Search";
    //    }
    //    else
    //    {
    //        enoughChars = true;
    //        toolStripDropDownButtonQuickSearchOptions.Text = "Search: " + this.CountSearchMatches();
    //    }
    //    bool entryCountMightHaveChanged = false;
    //    if (toolStripMenuItemQuickSearchFilter.Checked && (enoughChars || (enoughChars != m_enoughCharsInSearchText)))
    //    {
    //        this.CreateFilter();
    //        logView.Setup(m_presentationList);
    //        entryCountMightHaveChanged = true;
    //    }
    //    m_enoughCharsInSearchText = enoughChars;
    //    logView.RequestViewUpdate(entryCountMightHaveChanged);
    //}

    //private void toolStripButtonClearSearch_Click(object sender, EventArgs e)
    //{
    //    toolStripTextBoxQuickSearch.Text = String.Empty;
    //}

    private int CountSearchMatches()
    {
        var state = m_presentationList.GetState();
        var entryIndex = state.FirstIndex;
        int count = 0;

        while (true)
        {
            var entry = m_presentationList.Get(entryIndex);
            if (entry == null) break;
            if (this.SearchMatching(entry.DataObject as LogEntry)) count++;
            entryIndex++;
        }
        return count;
    }

    //private void toolStripMenuItemQuickSearchMarkMatching_CheckedChanged(object sender, EventArgs e)
    //{
    //    m_markSearchMatches = toolStripMenuItemQuickSearchMarkMatching.Checked;
    //    if (m_markSearchMatches)
    //    {
    //        toolStripMenuItemQuickSearchFilter.Checked = false;
    //        this.CreateFilter();
    //        logView.Setup(m_presentationList);
    //        logView.RequestViewUpdate(true);
    //    }
    //}

    //private void toolStripMenuItemQuickSearchFilter_CheckedChanged(object sender, EventArgs e)
    //{
    //    if (toolStripMenuItemQuickSearchFilter.Checked)
    //    {
    //        toolStripMenuItemQuickSearchMarkMatching.Checked = false;
    //        this.CreateFilter();
    //        logView.Setup(m_presentationList);
    //        logView.RequestViewUpdate(true);
    //    }
    //}

    #endregion

    #region Skipping

    //private void SetupSkipChoises(ToolStripMenuItem selected)
    //{
    //    if (!Object.ReferenceEquals(selected, m_selectedSkipOption))
    //    {
    //        m_selectedSkipOption = selected;
    //        toolStripDropDownButtonSkipSelection.Text = selected.Text;

    //        var choises = new ToolStripMenuItem[] {
    //            toolStripMenuItemSkipSearchMatches,
    //            toolStripMenuItemSkipError ,
    //            toolStripMenuItemSkipScriptExecutionStart ,
    //            toolStripMenuItemSkipMeasurement };

    //        foreach (ToolStripMenuItem item in choises)
    //        {
    //            item.Checked = Object.Equals(selected, item);
    //        }
    //    }
    //}

    private void toolStripButtonSkipPrevious_Click(object sender, EventArgs e)
    {
        this.DoSkipSearch(false);
    }

    private void toolStripButtonSkipNext_Click(object sender, EventArgs e)
    {
        this.DoSkipSearch(true);
    }

    private void DoSkipSearch(bool forward)
    {
        //    try
        //    {
        //        var checker = (SkipChecker)m_selectedSkipOption.Tag;

        //        var state = m_presentationList.GetState();
        //        var entryIndex = state.FirstIndex;

        //        var focusEntry = logView.CurrentEntry;
        //        if (focusEntry < 0L)
        //        {
        //            focusEntry = logView.TopEntry;
        //        }

        //        long firstFound = -1L;
        //        long lastFound = -1L;
        //        long found = -1L;

        //        while (true)
        //        {
        //            var entry = m_presentationList.Get(entryIndex);
        //            if (entry != null)
        //            {
        //                bool match = checker(entry.DataObject as LogEntry);
        //                if (match)  // FOUND A MATCH
        //                {
        //                    System.Diagnostics.Debug.WriteLine("Found " + entry.DataObject.ToString());
        //                    if (firstFound < 0L) firstFound = entryIndex;

        //                    if (forward && entryIndex > focusEntry)
        //                    {
        //                        found = entryIndex;
        //                        break;
        //                    }
        //                }

        //                if (!forward)
        //                {
        //                    if (lastFound >= 0L && lastFound < focusEntry && entryIndex >= focusEntry)
        //                    {
        //                        // The last entry found was the one we were looking for.
        //                        found = lastFound;
        //                        break;
        //                    }
        //                }

        //                if (match) lastFound = entryIndex;  // Set now, to allow checking for previous first, when focus is on a matching entry.
        //            }
        //            else break;

        //            entryIndex++;
        //        }

        //        if (found < 0L)
        //        {
        //            //if (toolStripMenuItemSkipWrapAround.Checked)
        //            //{
        //            //    if (forward)
        //            //    {
        //            //        if (firstFound >= 0L && firstFound < focusEntry)
        //            //        {
        //            //            found = firstFound;
        //            //        }
        //            //    }
        //            //    else
        //            //    {
        //            //        if (lastFound >= 0L && lastFound > focusEntry)
        //            //        {
        //            //            found = lastFound;
        //            //        }
        //            //    }
        //            //}
        //        }

        //        if (found >= 0L)
        //        {
        //            logView.SetCurrentEntry(found, true);
        //        }
        //    }
        //    finally { }
    }

    //private void toolStripMenuItemSkipSearchMatches_Click(object sender, EventArgs e)
    //{
    //    SetupSkipChoises(toolStripMenuItemSkipSearchMatches);
    //}

    //private void toolStripMenuItemSkipError_Click(object sender, EventArgs e)
    //{
    //    SetupSkipChoises(toolStripMenuItemSkipError);
    //}

    //private void toolStripMenuItemSkipScriptExecutionStart_Click(object sender, EventArgs e)
    //{
    //    SetupSkipChoises(toolStripMenuItemSkipScriptExecutionStart);
    //}

    //private void toolStripMenuItemSkipMeasurement_Click(object sender, EventArgs e)
    //{
    //    SetupSkipChoises(toolStripMenuItemSkipMeasurement);
    //}

    //private void toolStripMenuItemSkipWrapAround_CheckedChanged(object sender, EventArgs e)
    //{
    //    // Maybe do nothing; The menu item can be read by the skip operation.
    //}

    private bool SearchMatching(ITimestampedData entry)
    {
        var e = entry as LogEntry;
        if (String.IsNullOrEmpty(m_searchText)) return false;
        if (entry == null) return false;
        if (e.Location != null && e.Location.Contains(m_searchText, StringComparison.InvariantCultureIgnoreCase)) return true;
        if (e.Text != null && e.Text.Contains(m_searchText, StringComparison.InvariantCultureIgnoreCase)) return true;
        return false;
    }

    private bool SkipErrorMatching(LogEntry entry)
    {
        return (entry != null) && (entry.EntryType & LogEntry.Type.FlagFilter) == LogEntry.Type.Error;
    }

    private bool SkipExecutionStartMatching(LogEntry entry)
    {
        return (entry != null) &&
            (entry.EntryType & LogEntry.Type.FlagFilter) == LogEntry.Type.TaskEntry &&
            entry.Location.StartsWith(Constants.STEPBRO_SCRIPT_EXECUTION_LOG_LOCATION, StringComparison.InvariantCultureIgnoreCase);
    }

    private bool SkipMeasurementMatching(LogEntry entry)
    {
        return false;
    }

    #endregion

}
