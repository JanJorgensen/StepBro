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

        public LogViewer()
        {
            InitializeComponent();
        }

        public void Setup()
        {
            m_presentationList = new PresentationList(StepBro.Core.Main.Logger);
            m_presentationList.Reset(LogFilters.Normal, Int64.MaxValue);
            m_presentationList.Get(Int64.MaxValue);
            logView.ZeroTime = StepBro.Core.Main.Logger.GetFirst().Item2.Timestamp;
            logView.Setup(m_presentationList);
        }
    }
}
