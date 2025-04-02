using StepBro.Core.Data.Report;
using StepBro.Core;
using StepBro.Core.General;
using static StepBro.Core.Controls.ParsingErrorListView;

namespace StepBro.UI.WinForms.Controls
{
    public partial class TestReportOverview : UserControl
    {
        private IReportManager m_manager = null;
        private DataReport m_currentReport = null;

        public TestReportOverview()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            m_manager = StepBro.Core.Main.GetService<IReportManager>();
            m_manager.ReportAdded += Manager_ReportAdded;
        }

        private void Manager_ReportAdded(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(this.OnNewReport));
        }

        private void OnNewReport()
        {
            var index = m_manager.Count - 1;
            this.SetReport(m_manager.GetReport(index));
        }

        public void SetReport(DataReport report)
        {
            if (m_currentReport != null)
            {
                report.GroupAdded -= Report_GroupAdded;
                report.SummaryUpdated -= Report_SummaryUpdated;
            }
            m_currentReport = report;
            listBox.Items.Clear();
            textBoxType.Text = report.Type;
            textBoxTitle.Text = report.Title;
            report.GroupAdded += Report_GroupAdded;
            report.SummaryUpdated += Report_SummaryUpdated;
            this.UpdateListBox();
        }

        private void Report_GroupAdded(object sender, EventArgs e)
        {
            if (toolStripMenuItemViewGroups.Checked)
            {
                this.BeginInvoke(this.UpdateListBox);
            }
        }

        private void Report_SummaryUpdated(object sender, EventArgs e)
        {
            if (toolStripMenuItemViewSummary.Checked)
            {
                this.BeginInvoke(this.UpdateListBox);
            }
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void UpdateListBox()
        {
            if (m_currentReport == null) return;
            if (toolStripMenuItemViewGroups.Checked)
            {
                foreach (var group in m_currentReport.ListGroups().Skip(listBox.Items.Count))
                {
                    listBox.Items.Add(group);
                }
            }
            else if (toolStripMenuItemViewSummary.Checked)
            {
                if (m_currentReport.Summary != null)
                {
                    foreach (var result in m_currentReport.Summary.ListResults().Skip(listBox.Items.Count))
                    {
                        listBox.Items.Add(result);
                    }
                }
            }
            listBox.Invalidate();
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Brush myBrush = Brushes.Black;
            if ((e.State & DrawItemState.Selected) != DrawItemState.None)
            {
                myBrush = Brushes.White;
            }
            if (e.Index >= 0)
            {
                var obj = listBox.Items[e.Index];
                if (obj is ReportGroup group)
                {
                    e.Graphics.DrawString(group.Name,
                        e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                }
                else if (obj is ReportTestSummary.SummaryData summaryData)
                {
                    string s = summaryData.Title;
                    if (summaryData.Result != null)
                    {
                        s += " - " + summaryData.Result.Verdict.ToString();
                    }
                    e.Graphics.DrawString(s,
                        e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
                }
            }
            e.DrawFocusRectangle();
        }

        private void toolStripMenuItemViewSummary_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItemViewSummary.Checked)
            {
                toolStripMenuItemViewSummary.Checked = true;
                toolStripMenuItemViewGroups.Checked = false;
                listBox.Items.Clear();
                this.UpdateListBox();
            }
        }

        private void toolStripMenuItemViewGroups_Click(object sender, EventArgs e)
        {
            if (!toolStripMenuItemViewGroups.Checked)
            {
                toolStripMenuItemViewGroups.Checked = true;
                toolStripMenuItemViewSummary.Checked = false;
                listBox.Items.Clear();
                this.UpdateListBox();
            }
        }

        public class DoubleClickResultEventArgs : EventArgs
        {
            public DoubleClickResultEventArgs(ReportTestSummary.SummaryData data) { this.Data = data; }
            public ReportTestSummary.SummaryData Data { get; private set; }
        }
        public delegate void DoubleClickResultEventHandler(object sender, DoubleClickResultEventArgs args);

        public class DoubleClickGroupEventArgs : EventArgs
        {
            public DoubleClickGroupEventArgs(ReportGroup group) { this.Group = group; }
            public ReportGroup Group { get; private set; }
        }
        public delegate void DoubleClickGroupEventHandler(object sender, DoubleClickGroupEventArgs args);


        public event DoubleClickResultEventHandler DoubleClickedResult;

        public event DoubleClickGroupEventHandler DoubleClickedGroup;

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex >= 0)
            {
                var obj = listBox.SelectedItem;
                if (obj != null && obj is ReportTestSummary.SummaryData data)
                {
                    this.DoubleClickedResult?.Invoke(this, new DoubleClickResultEventArgs(data));
                }
                else if (obj != null && obj is ReportGroup group)
                {
                    this.DoubleClickedGroup?.Invoke(this, new DoubleClickGroupEventArgs(group));
                }
            }
        }
    }
}
