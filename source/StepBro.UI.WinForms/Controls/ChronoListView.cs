using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.HostSupport;
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

namespace StepBro.UI.WinForms.Controls;

public partial class ChronoListView : UserControl, IChronoListViewer
{
    private IPresentationList<ChronoListViewEntry> m_presentationSource = null;
    private DateTime m_zeroTime;
    private bool m_headMode = true;
    private long m_topEntry = 0L;
    private long m_lastIndex = 0;
    private bool m_viewDirty = false;
    private bool m_updateVerticalScroll = false;
    private long m_currentEntryIndex = -1L;
    private ChronoListViewEntry m_currentEntry = null;
    private List<long> m_selectedEntries = new List<long>();
    private long m_lastSingleSelectionEntry = -1L;
    private long m_rangeSelectionEnd = -1L;
    private Func<long, ChronoListViewEntry, long, ChronoListViewEntry, EntryMarkState> m_searchMatchChecker = null;

    public ChronoListView()
    {
        InitializeComponent();
        panelHorizontal.Height = vScrollBar.Width;
        m_zeroTime = DateTime.UtcNow;
        viewPort.MouseWheel += ViewPort_MouseWheel;
    }

    public DateTime ZeroTime { get { return m_zeroTime; } set { m_zeroTime = value; } }

    public long TopEntry { get { return m_topEntry; } }

    public void Setup(IPresentationList<ChronoListViewEntry> source)
    {
        m_presentationSource = source;
        m_topEntry = 0;
        viewPort.SetDataSource(this, m_presentationSource);
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
        m_topEntry = Math.Min(m_topEntry, vScrollBar.Maximum);
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
        if (this.CurrentEntry >= 0L)
        {
            var selectedLine = m_presentationSource.Get(this.CurrentEntry);
            if (selectedLine != null)
            {

                string newline = System.Environment.NewLine;
                System.Windows.Forms.Clipboard.Clear();
                StringBuilder text = new StringBuilder(0x10000);

                var logEntry = selectedLine.DataObject as LogEntry;

                text.Append(logEntry.ToClearText(this.ZeroTime, true));

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
        }
        // Else:
        return String.Empty;
    }

    public void SetupSearchMatchChecker(Func<long, ChronoListViewEntry, long, ChronoListViewEntry, EntryMarkState> matchChecker)
    {
        m_searchMatchChecker = matchChecker;
        this.RequestViewPortUpdate();
    }

    public EntryMarkState GetEntryMarkState(long index, ChronoListViewEntry entry)
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
            if (index >= 0 && (index < m_topEntry || index > (m_topEntry + (viewPort.MaxLinesVisible - 2))))
            {
                if (index < m_topEntry)
                {
                    m_topEntry = Math.Max(0L, index - 4L);      // Set selection in top.
                }
                else
                {
                    m_topEntry = Math.Max(0L, index - (viewPort.MaxLinesVisible - 5));  // Set selection in bottom.
                }
                m_updateVerticalScroll = true;
                vScrollBar.Value = (int)m_topEntry;
                m_updateVerticalScroll = false;
            }

            this.RequestViewPortUpdate();
        }
    }

    #endregion

    #region Key Press

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
            if (!this.HeadMode && this.CurrentEntry >= 0L)
            {
                this.Copy();
            }
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (viewPort.Focused)
        {
            if (keyData == Keys.Up)
            {
                if (m_currentEntryIndex < 0)
                {
                    return true;    // Nothing shown; just skip. 
                }
                if (m_currentEntryIndex > 0)
                {
                    this.SetCurrentEntryInternal(--m_currentEntryIndex, true, true, false);
                }
                if (m_currentEntryIndex < m_topEntry)
                {
                    m_topEntry = m_currentEntryIndex;
                }
                m_updateVerticalScroll = true;
                vScrollBar.Value = (int)m_topEntry;
                m_updateVerticalScroll = false;
                this.RequestViewPortUpdate();
                return true;
            }
            if (keyData == Keys.Down)
            {
                if (m_currentEntryIndex < 0)
                {
                    return true;    // Nothing shown; just skip. 
                }
                if (m_currentEntryIndex < m_presentationSource.GetState().LastIndex)
                {
                    this.SetCurrentEntryInternal(++m_currentEntryIndex, true, true, false);
                }
                if (m_currentEntryIndex >= m_topEntry + viewPort.MaxLinesVisible)
                {
                    m_topEntry = m_currentEntryIndex - (viewPort.MaxLinesVisible - 1);
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

    #endregion

    #region Mouse Events

    private void viewPort_MouseDownOnLine(object sender, MouseOnLineEventArgs e)
    {
        if (Control.ModifierKeys == Keys.None)
        {
            if (!this.HeadMode || !viewPort.ViewJustScrolled)
            {
                this.HeadMode = false;
                this.SetCurrentEntry(e.Index, true);
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
                try
                {
                    m_topEntry = Math.Max(0L, m_topEntry - 3L);
                    m_updateVerticalScroll = true;
                    vScrollBar.Value = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                    this.RequestViewPortUpdate();
                }
                finally { }
            }
        }
        else
        {
            if (m_topEntry < vScrollBar.Maximum)
            {
                try
                {
                    m_topEntry = Math.Min(vScrollBar.Maximum, m_topEntry + 3L);
                    m_updateVerticalScroll = true;
                    vScrollBar.Value = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                    this.RequestViewPortUpdate();
                }
                finally { }
            }
        }
    }

    #endregion
}
