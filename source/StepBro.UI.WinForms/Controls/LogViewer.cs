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

        private PresentationList m_presentationList = null;
        private int m_visibleLevels = 1000;

        public LogViewer()
        {
            InitializeComponent();
            this.UpdatePresentationLevels();
            toolStripButtonFollowHead.Text = "\u23EC";
        }

        public void Setup()
        {
            m_presentationList = new PresentationList(StepBro.Core.Main.Logger);
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

        }
    }
}
