using FastColoredTextBoxNS;
using StepBro.Core.Logging;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StepBro.Workbench
{
    public partial class OutputWindow : ToolWindow, ILogSink
    {
        private TextStyle infoStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        private TextStyle warningStyle = new TextStyle(Brushes.BurlyWood, null, FontStyle.Regular);
        private TextStyle errorStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        private static readonly string[] indentStrings;
        private DateTime m_startTime = DateTime.Now;
        private LogEntry m_firstSeen = null;
        private LogEntry m_lastHandled = null;
        private LogEntry m_oldestEntry = null;

        public OutputWindow()
        {
            this.InitializeComponent();

            var logSinkManager = StepBro.Core.Main.GetService<ILogSinkManager>();
            logSinkManager.Add(this);
        }

        static OutputWindow()
        {
            indentStrings = new string[32];
            for (int i = 0; i < 32; i++) indentStrings[i] = new string(' ', i * 4);
        }

        private void comboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {

        }

        void ILogSink.Start(LogEntry entry)
        {
            m_oldestEntry = entry;
            if (m_firstSeen == null)
            {
                m_firstSeen = entry;
            }
        }

        void ILogSink.Stop()
        {
        }

        void ILogSink.Add(LogEntry entry)
        {
            if (m_firstSeen == null)
            {
                m_firstSeen = entry;
            }
        }

        private void AddEntry(LogEntry entry)
        {
            string time = String.Format("{0,6}", ((long)(((TimeSpan)(DateTime.Now - m_startTime)).TotalMilliseconds)));
            if (String.IsNullOrEmpty(entry.Location))
            {
                if (!String.IsNullOrEmpty(entry.Text))
                {
                    fctb.AppendText(String.Concat(
                        time,
                        indentStrings[entry.IndentLevel],
                        entry.Text,
                        Environment.NewLine), infoStyle);
                }
            }
            else if (!String.IsNullOrEmpty(entry.Text))
            {
                if (String.IsNullOrEmpty(entry.Text))
                {
                    fctb.AppendText(String.Concat(
                        time,
                        indentStrings[entry.IndentLevel],
                        entry.Text,
                        Environment.NewLine), infoStyle);
                }
                else
                {
                    fctb.AppendText(String.Concat(
                        time,
                        indentStrings[entry.IndentLevel],
                        entry.Location,
                        " - ",
                        entry.Text,
                        Environment.NewLine), infoStyle);
                }
            }
        }

        private void OutputWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            var logSinkManager = StepBro.Core.Main.GetService<ILogSinkManager>();
            logSinkManager.Remove(this);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (m_lastHandled != null)
            {
                DateTime stopTime = DateTime.Now + TimeSpan.FromMilliseconds(updateTimer.Interval / 2);
                LogEntry entry = m_lastHandled.Next;
                if (entry != null)
                {
                    //some stuffs for best performance
                    fctb.BeginUpdate();
                    fctb.Selection.BeginUpdate();
                    //remember user selection
                    var userSelection = fctb.Selection.Clone();
                    //add text with predefined style
                    fctb.TextSource.CurrentTB = fctb;
                    bool gotoEnd = (userSelection.IsEmpty && userSelection.Start.iLine == (fctb.LinesCount - 1));

                    int i = 0;
                    while ((i < 1000 || DateTime.Now < stopTime) && entry != null)
                    {
                        this.AddEntry(entry);
                        m_lastHandled = entry;
                        i++;
                        entry = m_lastHandled.Next;
                    }

                    //restore user selection
                    if (gotoEnd)
                    {
                        fctb.GoEnd();//scroll to end of the text
                    }
                    else
                    {
                        fctb.Selection.Start = userSelection.Start;
                        fctb.Selection.End = userSelection.End;
                    }
                    //
                    fctb.Selection.EndUpdate();
                    fctb.EndUpdate();
                }
            }
            else
            {
                if (m_firstSeen != null)
                {
                    //some stuffs for best performance
                    fctb.BeginUpdate();
                    fctb.Selection.BeginUpdate();
                    //remember user selection
                    var userSelection = fctb.Selection.Clone();
                    //add text with predefined style
                    fctb.TextSource.CurrentTB = fctb;

                    this.AddEntry(m_firstSeen);
                    m_lastHandled = m_firstSeen;

                    //restore user selection
                    if (!userSelection.IsEmpty || userSelection.Start.iLine < fctb.LinesCount - 2)
                    {
                        fctb.Selection.Start = userSelection.Start;
                        fctb.Selection.End = userSelection.End;
                    }
                    else
                        fctb.GoEnd();//scroll to end of the text
                                     //
                    fctb.Selection.EndUpdate();
                    fctb.EndUpdate();
                }
            }
        }
    }
}