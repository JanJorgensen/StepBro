using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StepBro.Core.Logging;
using StepBro.Core.Data;
using StepBro.Core;

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
                adder(new LogViewEntry(entry, sourceIndex));
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


        private IDataListSource<LogEntry> m_source = null;
        private PresentationList m_presentationList = null;
        private long m_lastEntryBeforeClear = -1L;
        private NewLogStart m_zeroStartSource = null;
        private int m_visibleLevels = 1000;

        public LogViewer()
        {
            InitializeComponent();
            this.UpdatePresentationLevels();
            toolStripButtonFollowHead.Text = "\u23EC";
        }

        public void Setup()
        {
            m_source = StepBro.Core.Main.Logger;
            m_presentationList = new PresentationList(m_source);
            m_presentationList.Reset(LogFilters.Normal, Int64.MaxValue);
            m_presentationList.Get(Int64.MaxValue);
            logView.ZeroTime = StepBro.Core.Main.Logger.GetFirst().Item2.Timestamp;
            logView.Setup(m_presentationList);
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
                m_visibleLevels = choise;
            }
        }

        private void toolStripButtonFollowHead_CheckedChanged(object sender, EventArgs e)
        {
            logView.HeadMode = toolStripButtonFollowHead.Checked;
        }

        private void logView_HeadModeChanged(object sender, EventArgs e)
        {
            toolStripButtonFollowHead.Checked = logView.HeadMode;
        }

        private void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            m_lastEntryBeforeClear = m_source.GetState().LastIndex;
            var startSearcher = new PresentationListSearchingForFirstSource(this, m_source, m_lastEntryBeforeClear);
            logView.Setup(startSearcher);
        }

        private void SetupZeroStart()
        {
            var zeroTime = m_source.Get(m_lastEntryBeforeClear + 1L).Timestamp;
            m_zeroStartSource = new NewLogStart(m_source, m_source.Get(m_lastEntryBeforeClear + 1L), m_lastEntryBeforeClear + 1L);
            m_presentationList = new PresentationList(m_zeroStartSource);
            m_presentationList.Reset(LogFilters.Normal, Int64.MaxValue);    // TODO: Set current filter.
            m_presentationList.Get(Int64.MaxValue);
            logView.ZeroTime = zeroTime;
            logView.Setup(m_presentationList);
        }
    }
}
