using StepBro.Core.Logging;
using StepBro.Core.Data;
using StepBro.Core.Api;
using System;

namespace StepBro.UI.WinForms.Controls
{
    public partial class LogViewer : UserControl
    {
        private class PresentationList : PresentationListForListData<LogEntry, ChronoListViewEntry>
        {
            public PresentationList(IDataListSource<LogEntry> source) :
                base(source, 1000000, 50)
            {
            }

            public override void CreatePresentationEntry(LogEntry entry, long sourceIndex, Action<ChronoListViewEntry> adder)
            {
                if ((entry.EntryType & LogEntry.Type.Special) != LogEntry.Type.Special)
                {
                    adder(new LogViewEntry(entry, sourceIndex));
                }
                else
                {
                    adder(new LogViewEntrySpecial(entry, sourceIndex));     // TODO: Get reference to the special handler for this data type.
                    // TODO: Maybe the special handler could throw in a decoded/translated entry.
                }
            }

            public override LogEntry PresentationToSource(ChronoListViewEntry entry)
            {
                return entry.DataObject as LogEntry;
            }
        }

        private class PresentationListSearchingForFirstSource : PresentationListForListData<LogEntry, ChronoListViewEntry>
        {
            private class EmptySource : IDataListSource<LogEntry>
            {
                public LogEntry Get(long index)
                {
                    return null;
                }

                public Tuple<long, LogEntry> GetFirst()
                {
                    return new Tuple<long, LogEntry>(-1L, null);
                }

                public LogEntry GetLast()
                {
                    return null;
                }

                public IndexerStateSnapshot GetState()
                {
                    return new IndexerStateSnapshot(-1L, -1L, 0L);
                }

                public IDataWalker<LogEntry> WalkFrom(long start = -1)
                {
                    return null;
                }
            }

            private LogViewer m_parent;
            private IDataListSource<LogEntry> m_source;
            private long m_lastBefore;

            public PresentationListSearchingForFirstSource(LogViewer parent, IDataListSource<LogEntry> source, long lastBefore) :
                base(new EmptySource(), 100, 10)
            {
                m_parent = parent;
                m_source = source;
                m_lastBefore = lastBefore;
            }

            public override void CreatePresentationEntry(LogEntry entry, long sourceIndex, Action<ChronoListViewEntry> adder)
            {
            }

            public override LogEntry PresentationToSource(ChronoListViewEntry entry)
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

        private class NewLogStart : IDataListSource<LogEntry>
        {
            private IDataListSource<LogEntry> m_source;
            private LogEntry m_firstEntry;
            private long m_firstIndex;

            public NewLogStart(IDataListSource<LogEntry> source, LogEntry first, long firstIndex)
            {
                m_source = source;
                m_firstEntry = first;
                m_firstIndex = firstIndex;
            }

            public Tuple<long, LogEntry> GetFirst()
            {
                return new Tuple<long, LogEntry>(m_firstIndex, m_firstEntry);
            }

            public LogEntry GetLast()
            {
                return m_source.GetLast();
            }

            public IndexerStateSnapshot GetState()
            {
                var sourceState = m_source.GetState();
                return new IndexerStateSnapshot(m_firstIndex, sourceState.LastIndex, sourceState.LastIndex - m_firstIndex + 1L);
            }

            public IDataWalker<LogEntry> WalkFrom(long start = -1)
            {
                if (start < 0L) start = m_firstIndex;
                return m_source.WalkFrom(start);
            }

            public LogEntry Get(long index)
            {
                return m_source.Get(index);
            }
        }

        private delegate bool SkipChecker(LogEntry entry);

        private IDataListSource<LogEntry> m_source = null;
        private PresentationListForListData<LogEntry, ChronoListViewEntry> m_presentationList = null;
        private long m_lastEntryBeforeClear = -1L;
        private NewLogStart m_zeroStartSource = null;
        private int m_visibleLevels = 1000;
        private Predicate<LogEntry>[] m_filter = null;
        private ToolStripMenuItem m_selectedSkipOption = null;
        private bool m_enoughCharsInSearchText = false;
        private bool m_markSearchMatches = true;

        public LogViewer()
        {
            InitializeComponent();
            this.UpdatePresentationLevels();
            toolStripButtonFollowHead.Text = "\u23EC";
            toolStripButtonSkipPrevious.Text = "\u23EB";
            toolStripButtonSkipNext.Text = "\u23EC";
            toolStripButtonClearSearch.Text = "\u2716";
            this.SetupSkipChoises(toolStripMenuItemSkipError);
            toolStripMenuItemSkipSearchMatches.Tag = (object)(SkipChecker)this.SearchMatching;
            toolStripMenuItemSkipError.Tag = (object)(SkipChecker)this.SkipErrorMatching;
            toolStripMenuItemSkipScriptExecutionStart.Tag = (object)(SkipChecker)this.SkipExecutionStartMatching;
            toolStripMenuItemSkipMeasurement.Tag = (object)(SkipChecker)this.SkipMeasurementMatching;
        }

        public void Setup()
        {
            m_source = StepBro.Core.Main.Logger;
            m_presentationList = new PresentationList(m_source);
            this.CreateFilter();
            logView.ZeroTime = StepBro.Core.Main.Logger.GetFirst().Item2.Timestamp;
            logView.Setup(m_presentationList);
            logView.SetupSearchMatchChecker(this.SearchMarkMatchChecker);
            this.SetupFromHeadMode();
        }

        private bool LevelFilter(LogEntry entry)
        {
            return (entry.IndentLevel < m_visibleLevels);
        }

        private bool CombinedFilter(LogEntry entry)
        {
            foreach (var f in m_filter)
            {
                if (f(entry) == false) return false;
            }
            return true;
        }

        private void CreateFilter()
        {
            var filter = new List<Predicate<LogEntry>>();
            if (m_visibleLevels < 1000)
            {
                filter.Add(this.LevelFilter);
            }
            filter.Add(LogFilters.Normal);
            if (toolStripMenuItemQuickSearchFilter.Checked && toolStripTextBoxQuickSearch.Text.Length > 1)
            {
                filter.Add(this.SearchMatching);
            }
            m_filter = filter.ToArray();

            m_presentationList.Reset(this.CombinedFilter, Int64.MaxValue);
            m_presentationList.Get(Int64.MaxValue);
        }

        private void UpdatePresentationLevels()
        {
            toolStripDropDownButtonDisplayLevels.Text = "∞";
        }

        private void toolStripMenuItemDisplayLevel_CheckedChanged(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Checked)
            {
                int choise = -1;
                if (sender == toolStripMenuItemLevels2) { choise = 2; toolStripDropDownButtonDisplayLevels.Text = "2"; } else toolStripMenuItemLevels2.Checked = false;
                if (sender == toolStripMenuItemLevels3) { choise = 3; toolStripDropDownButtonDisplayLevels.Text = "3"; } else toolStripMenuItemLevels3.Checked = false;
                if (sender == toolStripMenuItemLevels4) { choise = 4; toolStripDropDownButtonDisplayLevels.Text = "4"; } else toolStripMenuItemLevels4.Checked = false;
                if (sender == toolStripMenuItemLevels5) { choise = 5; toolStripDropDownButtonDisplayLevels.Text = "5"; } else toolStripMenuItemLevels5.Checked = false;
                if (sender == toolStripMenuItemLevels6) { choise = 6; toolStripDropDownButtonDisplayLevels.Text = "6"; } else toolStripMenuItemLevels6.Checked = false;
                if (sender == toolStripMenuItemLevelsAll) { choise = 1000; toolStripDropDownButtonDisplayLevels.Text = "∞"; } else toolStripMenuItemLevelsAll.Checked = false;
                if (choise != m_visibleLevels)
                {
                    m_visibleLevels = choise;
                    this.CreateFilter();
                    logView.Setup(m_presentationList);
                    logView.RequestViewUpdate(true);
                }
            }
        }

        private void toolStripButtonFollowHead_CheckedChanged(object sender, EventArgs e)
        {
            logView.HeadMode = toolStripButtonFollowHead.Checked;
        }

        private void logView_HeadModeChanged(object sender, EventArgs e)
        {
            this.SetupFromHeadMode();
        }

        private void SetupFromHeadMode()
        {
            toolStripButtonFollowHead.Checked = logView.HeadMode;

            toolStripButtonSkipPrevious.Enabled = !logView.HeadMode;
            toolStripButtonSkipNext.Enabled = !logView.HeadMode;
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            m_lastEntryBeforeClear = m_source.GetState().LastIndex;
            m_presentationList = new PresentationListSearchingForFirstSource(this, m_source, m_lastEntryBeforeClear);
            logView.Setup(m_presentationList);
        }

        private void SetupZeroStart()
        {
            bool headMode = m_presentationList.InHeadMode;
            var zeroTime = m_source.Get(m_lastEntryBeforeClear + 1L).Timestamp;
            m_zeroStartSource = new NewLogStart(m_source, m_source.Get(m_lastEntryBeforeClear + 1L), m_lastEntryBeforeClear + 1L);
            m_presentationList = new PresentationList(m_zeroStartSource);
            m_presentationList.SetHeadMode(headMode);
            this.CreateFilter();
            logView.ZeroTime = zeroTime;
            logView.Setup(m_presentationList);
        }

        private void toolStripDropDownButtonLoggers_DropDownOpening(object sender, EventArgs e)
        {
            toolStripDropDownButtonLoggers.DropDownItems.Clear();
            var specialLogging = Core.Main.GetService<IComponentLoggerService>();

            foreach (var logger in specialLogging.ListLoggers())
            {
                var item = new ToolStripMenuItem();
                item.CheckOnClick = true;
                item.Checked = logger.Enabled;
                item.Name = "toolStripMenuItemSpecialLogger" + logger.Name;
                item.Size = new Size(180, 22);
                item.Text = logger.Name;
                item.Tag = logger;
                item.CheckedChanged += Item_CheckedChanged;
            }

            if (toolStripDropDownButtonLoggers.DropDownItems.Count == 0)
            {
                toolStripDropDownButtonLoggers.DropDownItems.Add(toolStripMenuItemSpecialLoggersNoneAvailable);
            }
        }

        private void Item_CheckedChanged(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var logger = (IComponentLogging)item.Tag;
            logger.Enabled = item.Checked;
        }

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
            long index, ChronoListViewEntry entry, 
            long curentIndex, ChronoListViewEntry currentEntry)
        {
            var markings = EntryMarkState.None;
            if (m_markSearchMatches && toolStripTextBoxQuickSearch.Text.Length > 1)
            {
                if (this.SearchMatching(m_presentationList.Get(index).DataObject as LogEntry))
                {
                    markings |= EntryMarkState.SearchMatch;
                }
            }
            if (currentEntry != null && entry.DataObject is LogEntry logEntry && currentEntry.DataObject is LogEntry currentLogEntry)
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

        private void toolStripTextBoxQuickSearch_TextChanged(object sender, EventArgs e)
        {
            var text = toolStripTextBoxQuickSearch.Text;
            if (!String.IsNullOrEmpty(text))
            {
                SetupSkipChoises(toolStripMenuItemSkipSearchMatches);
            }
            toolStripButtonClearSearch.Visible = !String.IsNullOrEmpty(text);
            bool enoughChars = false;
            if (toolStripTextBoxQuickSearch.Text.Length < 2)
            {
                toolStripDropDownButtonQuickSearchOptions.Text = "Search";
            }
            else
            {
                enoughChars = true;
                toolStripDropDownButtonQuickSearchOptions.Text = "Search: " + this.CountSearchMatches();
            }
            bool entryCountMightHaveChanged = false;
            if (toolStripMenuItemQuickSearchFilter.Checked && (enoughChars || (enoughChars != m_enoughCharsInSearchText)))
            {
                this.CreateFilter();
                logView.Setup(m_presentationList);
                entryCountMightHaveChanged = true;
            }
            m_enoughCharsInSearchText = enoughChars;
            logView.RequestViewUpdate(entryCountMightHaveChanged);
        }

        private void toolStripButtonClearSearch_Click(object sender, EventArgs e)
        {
            toolStripTextBoxQuickSearch.Text = String.Empty;
        }

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

        private void toolStripMenuItemQuickSearchMarkMatching_CheckedChanged(object sender, EventArgs e)
        {
            m_markSearchMatches = toolStripMenuItemQuickSearchMarkMatching.Checked;
            if (m_markSearchMatches)
            {
                toolStripMenuItemQuickSearchFilter.Checked = false;
                this.CreateFilter();
                logView.Setup(m_presentationList);
                logView.RequestViewUpdate(true);
            }
        }

        private void toolStripMenuItemQuickSearchFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (toolStripMenuItemQuickSearchFilter.Checked)
            {
                toolStripMenuItemQuickSearchMarkMatching.Checked = false;
                this.CreateFilter();
                logView.Setup(m_presentationList);
                logView.RequestViewUpdate(true);
            }
        }

        #endregion

        #region Skipping

        private void SetupSkipChoises(ToolStripMenuItem selected)
        {
            if (!Object.ReferenceEquals(selected, m_selectedSkipOption))
            {
                m_selectedSkipOption = selected;
                toolStripDropDownButtonSkipSelection.Text = selected.Text;

                var choises = new ToolStripMenuItem[] {
                    toolStripMenuItemSkipSearchMatches,
                    toolStripMenuItemSkipError ,
                    toolStripMenuItemSkipScriptExecutionStart ,
                    toolStripMenuItemSkipMeasurement };

                foreach (ToolStripMenuItem item in choises)
                {
                    item.Checked = Object.Equals(selected, item);
                }
            }
        }

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
            var checker = (SkipChecker)m_selectedSkipOption.Tag;

            var state = m_presentationList.GetState();
            var entryIndex = state.FirstIndex;

            var focusEntry = logView.CurrentEntry;
            if (focusEntry < 0L)
            {
                focusEntry = logView.TopEntry;
            }

            long firstFound = -1L;
            long lastFound = -1L;
            long found = -1L;

            while (true)
            {
                var entry = m_presentationList.Get(entryIndex);
                if (entry != null)
                {
                    bool match = checker(entry.DataObject as LogEntry);
                    if (match)  // FOUND A MATCH
                    {
                        System.Diagnostics.Debug.WriteLine("Found " + entry.DataObject.ToString());
                        if (firstFound < 0L) firstFound = entryIndex;

                        if (forward && entryIndex > focusEntry)
                        {
                            found = entryIndex;
                            break;
                        }
                    }

                    if (!forward)
                    {
                        if (lastFound >= 0L && lastFound < focusEntry && entryIndex >= focusEntry)
                        {
                            // The last entry found was the one we were looking for.
                            found = lastFound;
                            break;
                        }
                    }

                    if (match) lastFound = entryIndex;  // Set now, to allow checking for previous first, when focus is on a matching entry.
                }
                else break;

                entryIndex++;
            }

            if (found < 0L)
            {
                if (toolStripMenuItemSkipWrapAround.Checked)
                {
                    if (forward)
                    {
                        if (firstFound >= 0L && firstFound < focusEntry)
                        {
                            found = firstFound;
                        }
                    }
                    else
                    {
                        if (lastFound >= 0L && lastFound > focusEntry)
                        {
                            found = lastFound;
                        }
                    }
                }
            }

            if (found >= 0L)
            {
                logView.SetCurrentEntry(found, true);
            }
        }

        private void toolStripMenuItemSkipSearchMatches_Click(object sender, EventArgs e)
        {
            SetupSkipChoises(toolStripMenuItemSkipSearchMatches);
        }

        private void toolStripMenuItemSkipError_Click(object sender, EventArgs e)
        {
            SetupSkipChoises(toolStripMenuItemSkipError);
        }

        private void toolStripMenuItemSkipScriptExecutionStart_Click(object sender, EventArgs e)
        {
            SetupSkipChoises(toolStripMenuItemSkipScriptExecutionStart);
        }

        private void toolStripMenuItemSkipMeasurement_Click(object sender, EventArgs e)
        {
            SetupSkipChoises(toolStripMenuItemSkipMeasurement);
        }

        private void toolStripMenuItemSkipWrapAround_CheckedChanged(object sender, EventArgs e)
        {
            // Maybe do nothing; The menu item can be read by the skip operation.
        }

        private bool SearchMatching(LogEntry entry)
        {
            var text = toolStripTextBoxQuickSearch.Text;
            if (String.IsNullOrEmpty(text)) return false;
            if (entry == null) return false;
            if (entry.Location != null && entry.Location.Contains(text, StringComparison.InvariantCultureIgnoreCase)) return true;
            if (entry.Text != null && entry.Text.Contains(text, StringComparison.InvariantCultureIgnoreCase)) return true;
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
}
