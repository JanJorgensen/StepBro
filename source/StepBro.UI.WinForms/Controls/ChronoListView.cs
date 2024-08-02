using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static StepBro.UI.WinForms.Controls.ChronoListViewPort;

namespace StepBro.UI.WinForms.Controls
{
    public partial class ChronoListView : UserControl, IChronoListViewer
    {
        private IPresentationList<ChronoListViewEntry> m_presentationSource = null;
        private DateTime m_zeroTime;
        private bool m_headMode = true;
        private long m_topEntry = 0L;
        private long m_lastIndex = 0;
        private bool m_viewDirty = false;
        private bool m_updateVerticalScroll = false;
        private long m_currentEntry = -1L;
        private List<long> m_selectedEntries = new List<long>();
        private long m_lastSingleSelectionEntry = -1L;
        private long m_rangeSelectionEnd = -1L;

        public ChronoListView()
        {
            InitializeComponent();
            panelHorizontal.Height = vScrollBar.Width;
            m_zeroTime = DateTime.UtcNow;
            viewPort.MouseWheel += ViewPort_MouseWheel;
        }

        public DateTime ZeroTime { get { return m_zeroTime; } set { m_zeroTime = value; } }

        public IElementIndexer<ChronoListViewEntry> Source { get { return m_presentationSource; } }

        public void Setup(IPresentationList<ChronoListViewEntry> source)
        {
            m_presentationSource = source;
            m_topEntry = 0;
            viewPort.SetDataSource(this);
            timerUpdate.Start();
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
                        m_currentEntry = -1L;
                        m_selectedEntries.Clear();
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

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.RequestViewPortUpdate();
            if (m_presentationSource != null)
            {
                this.UpdateVerticalScrollbar(m_presentationSource.GetState().EffectiveCount);
            }
        }

        private void UpdateVerticalScrollbar(long entryCount)
        {
            m_updateVerticalScroll = true;
            vScrollBar.Maximum = Math.Max(0, (int)(entryCount - (viewPort.MaxLinesVisible - 10)));
            if (m_presentationSource.InHeadMode || !viewPort.IsViewFilled())
            {
                this.RequestViewPortUpdate();
            }
            if (m_presentationSource.InHeadMode)
            {
                vScrollBar.Value = (int)m_topEntry;
            }
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
                if (m_presentationSource.InHeadMode || !viewPort.IsViewFilled())
                {
                    this.RequestViewPortUpdate();
                }
                this.UpdateVerticalScrollbar(state.EffectiveCount);
            }
        }

        private void RequestViewPortUpdate()
        {
            m_viewDirty = false;
            if (m_headMode)
            {
                m_topEntry = Math.Max(0L, m_lastIndex - (viewPort.MaxLinesVisible - 1L));
            }
            viewPort.RequestUpdate(m_topEntry, 0 - hScrollBar.Value);
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            m_viewDirty = true;
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (!m_updateVerticalScroll)
            {
                this.HeadMode = false;
                m_topEntry = vScrollBar.Value;
                this.RequestViewPortUpdate();
            }
        }

        public string Copy(/*SelectedItemsOperationChoise itemsChoise*/)
        {
            string newline = System.Environment.NewLine;
            System.Windows.Forms.Clipboard.Clear();
            StringBuilder text = new StringBuilder(0x10000);
            //switch (itemsChoise)
            //{
            //    case SelectedItemsOperationChoise.OnlySelected:
            //        {
            //            bool first = true;
            //            DateTime lastTime = DateTime.MinValue;
            //            foreach (FormattedLogEntry e in this.GetSelectedEntries())
            //            {
            //                if (first)
            //                {
            //                    lastTime = e.TimeStamp;
            //                    first = false;
            //                }
            //                text.Append(e.GetPrintableString(lastTime, 8, m_timestampType, m_zeroTime, m_firstSelectedTime));
            //                text.Append(newline);
            //                lastTime = e.TimeStamp;
            //            }
            //        }
            //        break;
            //    case SelectedItemsOperationChoise.AllInSelectedRange:
            //        {
            //            int first, last;
            //            this.GetSelectionBounds(out first, out last);
            //            FormattedLogEntry entry = m_topEntry;
            //            bool isFirst = true;
            //            DateTime lastTime = DateTime.MinValue;
            //            for (int i = first; i <= last; i++)
            //            {
            //                entry = FormattedLogEntry.GetEntry(entry, i);
            //                if (isFirst)
            //                {
            //                    lastTime = entry.TimeStamp;
            //                    isFirst = false;
            //                }
            //                text.Append(entry.GetPrintableString(lastTime, 8, m_timestampType, m_zeroTime, m_firstSelectedTime));
            //                text.Append(newline);
            //                lastTime = entry.TimeStamp;
            //            }
            //        }
            //        break;
            //    case SelectedItemsOperationChoise.OnlyCurrentItem:
            //        if (m_currentEntry != null)
            //        {
            //            text.Append(m_currentEntry.GetPrintableString(m_currentEntry.TimeStamp, 8, m_timestampType, m_zeroTime, m_firstSelectedTime));
            //        }
            //        break;
            //    case SelectedItemsOperationChoise.AllInView:
            //        {
            //            bool first = true;
            //            DateTime lastTime = DateTime.MinValue;
            //            FormattedLogEntry e = m_viewer.FirstKnownEntry;
            //            while (e != null)
            //            {
            //                if (first)
            //                {
            //                    lastTime = e.TimeStamp;
            //                    first = false;
            //                }
            //                text.Append(e.GetPrintableString(lastTime, 8, m_timestampType, m_zeroTime, m_firstSelectedTime));
            //                text.Append(newline);
            //                lastTime = e.TimeStamp;
            //                e = e.Next;
            //            }
            //        }
            //        break;
            //    default:
            //        break;
            //}
            string s = text.ToString();
            System.Windows.Forms.Clipboard.SetText(s);
            return s;
        }

        #region Selection

        public EntrySelectionState GetEntrySelectionState(long index)
        {
            var selectionState = EntrySelectionState.Not;
            if (m_selectedEntries.Count > 0 && m_selectedEntries.Contains(index))
            {
                selectionState = EntrySelectionState.Selected;
            }
            else
            {
                if (m_rangeSelectionEnd >= 0L)
                {
                    if (m_lastSingleSelectionEntry < m_rangeSelectionEnd)
                    {
                        if (index >= m_lastSingleSelectionEntry && index <= m_rangeSelectionEnd) selectionState = EntrySelectionState.Selected; ;
                    }
                    else
                    {
                        if (index >= m_rangeSelectionEnd && index <= m_lastSingleSelectionEntry) selectionState = EntrySelectionState.Selected; ;
                    }
                }
            }
            if (index == m_currentEntry)
            {
                selectionState = (selectionState == EntrySelectionState.Selected) ? EntrySelectionState.SelectedCurrent : EntrySelectionState.Current;
            }
            return selectionState;
        }

        #endregion

        private void viewPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.End)
            {
                this.HeadMode = true;
            }
            else if (e.Control && e.KeyCode == Keys.Home)
            {
                this.HeadMode = false;
                m_topEntry = m_presentationSource.GetState().FirstIndex;
                this.RequestViewPortUpdate();
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                viewPort.Invalidate();
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (viewPort.Focused)
            {
                if (keyData == Keys.Up)
                {
                    if (m_currentEntry > 0)
                    {
                        m_currentEntry--;
                    }
                    m_selectedEntries.Clear();
                    m_selectedEntries.Add(m_currentEntry);
                    if (m_currentEntry < m_topEntry)
                    {
                        m_topEntry = m_currentEntry;
                    }
                    m_updateVerticalScroll = true;
                    vScrollBar.Value = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                    this.RequestViewPortUpdate();
                    return true;
                }
                if (keyData == Keys.Down)
                {
                    if (m_currentEntry < m_presentationSource.GetState().LastIndex)
                    {
                        m_currentEntry++;
                    }
                    m_selectedEntries.Clear();
                    m_selectedEntries.Add(m_currentEntry);
                    if (m_currentEntry >= m_topEntry + viewPort.MaxLinesVisible)
                    {
                        m_topEntry = m_currentEntry - (viewPort.MaxLinesVisible - 1);
                    }
                    m_updateVerticalScroll = true;
                    vScrollBar.Value = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                    this.RequestViewPortUpdate();
                    return true;
                }
                //else if (keyData == Keys.Down)
                //{
                //    FormattedLogEntry e = null;
                //    if (logWindow.CurrentEntry != null)
                //    {
                //        e = logWindow.CurrentEntry;
                //    }
                //    else if (logWindow.TopEntry != null)
                //    {
                //        e = logWindow.TopEntry;
                //    }
                //    else
                //    {
                //        return true;
                //    }
                //    this.SetCurrentEntry(
                //       e.TryGetEntryAt(1, true, m_logOwner.Last.Index, logWindow.DisplayEntryFilterPredicate),
                //       true);
                //    return true;
                //}
                //else if (keyData == Keys.Left)
                //{
                //    return true;
                //}
                //else if (keyData == Keys.Right)
                //{
                //    return true;
                //}
                //else if (keyData == Keys.PageUp)
                //{
                //    FormattedLogEntry e = null;
                //    if (logWindow.CurrentEntry != null)
                //    {
                //        e = logWindow.CurrentEntry;
                //    }
                //    else if (logWindow.TopEntry != null)
                //    {
                //        e = logWindow.TopEntry;
                //    }
                //    else
                //    {
                //        return true;
                //    }
                //    e = e.TryGetEntryAt(0 - logWindow.MaxLinesInView, true, m_first.Index, logWindow.DisplayEntryFilterPredicate);
                //    this.SetCurrentEntry(e, true);
                //    return true;
                //}
                //else if (keyData == Keys.PageDown)
                //{
                //    FormattedLogEntry e = null;
                //    if (logWindow.CurrentEntry != null)
                //    {
                //        e = logWindow.CurrentEntry;
                //    }
                //    else if (logWindow.TopEntry != null)
                //    {
                //        e = logWindow.TopEntry;
                //    }
                //    else
                //    {
                //        return true;
                //    }
                //    e = e.TryGetEntryAt(logWindow.MaxLinesInView, true, m_logOwner.Last.Index, logWindow.DisplayEntryFilterPredicate);
                //    this.SetCurrentEntry(e, true);
                //    return true;
                //}
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void chronoListViewPort_Click(object sender, EventArgs e)
        {
            //if (this.HeadMode)
            //{
            //    this.HeadMode = false;
            //}
        }

        private void viewPort_MouseDown(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("viewPort_MouseDown " + e.X + " " + e.Y);
        }

        private void viewPort_MouseUp(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("viewPort_MouseUp " + e.X + " " + e.Y);
        }

        private void viewPort_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void viewPort_MouseDownOnLine(object sender, MouseOnLineEventArgs e)
        {
            if (Control.ModifierKeys == Keys.None)
            {
                if (!this.HeadMode || !viewPort.ViewJustScrolled)
                {
                    this.HeadMode = false;
                    m_selectedEntries.Clear();
                    if (e.Index >= 0L)
                    {
                        m_selectedEntries.Add(e.Index);
                        m_currentEntry = e.Index;
                    }
                    this.RequestViewPortUpdate();
                }
                else
                {
                    this.HeadMode = false;
                }
            }
        }

        private void viewPort_MouseUpOnLine(object sender, MouseOnLineEventArgs e)
        {

        }

        private void ViewPort_MouseWheel(object sender, MouseEventArgs e)
        {
            if (this.HeadMode)
            {
                this.HeadMode = false;
            }
            if (e.Delta > 0)    // Up?
            {
                if (m_topEntry > 0)
                {
                    m_topEntry = Math.Max(0L, m_topEntry - 3L);
                    m_updateVerticalScroll = true;
                    vScrollBar.Value = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                    this.RequestViewPortUpdate();
                }
            }
            else
            {
                if (m_topEntry < vScrollBar.Maximum)
                {
                    m_topEntry = Math.Min(vScrollBar.Maximum, m_topEntry + 3L);
                    m_updateVerticalScroll = true;
                    vScrollBar.Value = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                    this.RequestViewPortUpdate();
                }
            }
        }
    }
}
