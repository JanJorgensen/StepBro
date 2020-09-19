using FastColoredTextBoxNS;
using StepBro.Core.Data;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StepBro.Core.Controls
{
    public partial class SimpleLogView : UserControl
    {
        private TextStyle StyleInfo = new TextStyle(Brushes.DarkGoldenrod, null, FontStyle.Regular);
        private TextStyle StyleSent = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        private TextStyle StyleReceived = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        private TextStyle StyleReceivedPartial = new TextStyle(Brushes.CornflowerBlue, null, FontStyle.Regular);
        private TextStyle StyleReceivedError = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        private TextStyle StyleReceivedAsync = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        private LogLineData m_firstSeen = null;
        private LogLineData m_lastHandled = null;
        //private LogLineData m_oldestEntry = null;
        private ILogLineSource m_source = null;
        private Predicate<LogLineData> m_filter = (LogLineData l) => { return true; };

        public SimpleLogView()
        {
            this.InitializeComponent();
        }

        public void SetSource(ILogLineSource source)
        {
            if (m_source != null)
            {
                throw new NotImplementedException();
            }
            m_source = source;
            m_source.LinesAdded += this.source_LinesAdded;
        }

        private void source_LinesAdded(object sender, LogLineEventArgs args)
        {
            if (m_firstSeen == null)
            {
                m_firstSeen = args.Line;
            }
        }

        public void SetViewFilter(Predicate<LogLineData> filter)
        {
            m_filter = filter;
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (m_lastHandled != null)
            {
                DateTime stopTime = DateTime.Now + TimeSpan.FromMilliseconds(updateTimer.Interval / 2);
                LogLineData entry = m_lastHandled.Next;
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

        private void AddEntry(LogLineData entry)
        {
            if (!m_filter(entry)) return;
            TextStyle style = StyleInfo;
            switch (entry.Type)
            {
                case LogLineData.LogType.Neutral:
                    style = StyleInfo;
                    break;
                case LogLineData.LogType.Sent:
                    style = StyleSent;
                    break;
                case LogLineData.LogType.ReceivedEnd:
                    style = StyleReceived;
                    break;
                case LogLineData.LogType.ReceivedPartial:
                    style = StyleReceivedPartial;
                    break;
                case LogLineData.LogType.ReceivedError:
                    style = StyleReceivedError;
                    break;
                case LogLineData.LogType.ReceivedAsync:
                    style = StyleReceivedAsync;
                    break;
                case LogLineData.LogType.ReceivedTrace:
                    style = StyleReceivedAsync;
                    break;
                default:
                    break;
            }
            fctb.AppendText(String.Concat(entry.LineText, Environment.NewLine), style);
        }

    }
}
