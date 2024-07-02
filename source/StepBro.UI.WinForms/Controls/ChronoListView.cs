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
        private IPresentationList<ChronoListViewEntry> m_source = null;
        private DateTime m_zeroTime;
        private bool m_headMode = true;
        private long m_topEntry = 0L;
        private long m_lastIndex = 0;
        private bool m_viewDirty = false;
        private bool m_updateVerticalScroll = false;
        private List<long> m_selectedEntries = new List<long>();
        //private long m_currentEntry = -1L;
        private long m_lastSingleSelectionEntry = -1L;
        private long m_rangeSelectionEnd = -1L;

        public ChronoListView()
        {
            InitializeComponent();
            panelHorizontal.Height = vScrollBar.Width;
            m_zeroTime = DateTime.UtcNow;
        }

        public DateTime ZeroTime { get { return m_zeroTime; } set { m_zeroTime = value; } }

        public IElementIndexer<ChronoListViewEntry> Source { get { return m_source; } }

        public void Setup(IPresentationList<ChronoListViewEntry> source)
        {
            m_source = source;
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
                    m_source.SetHeadMode(m_headMode);
                    if (m_headMode)
                    {
                        m_lastSingleSelectionEntry = -1L;
                        m_selectedEntries.Clear();
                        this.RequestViewPortUpdate();
                        //logWindow.SetCurrentEntry(null, false, true);
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
        }

        public event EventHandler HeadModeChanged;

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (m_source.InHeadMode)
            {
                m_source.Get(Int64.MaxValue);
                var state = m_source.GetState();
                if (m_viewDirty || state.LastIndex != m_lastIndex)
                {
                    m_lastIndex = state.LastIndex;
                    this.RequestViewPortUpdate();
                    m_updateVerticalScroll = true;
                    vScrollBar.Maximum = Math.Max(0, (int)(state.EffectiveCount - viewPort.MaxLinesVisible));
                    vScrollBar.Value = (int)m_topEntry;
                    m_updateVerticalScroll = false;
                }
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

        #region Selection

        public EntrySelectionState GetEntrySelectionState(long index)
        {
            if (m_selectedEntries.Count > 0 && m_selectedEntries.Contains(index))
            {
                return EntrySelectionState.Selected;
            }
            if (m_rangeSelectionEnd >= 0L)
            {
                if (m_lastSingleSelectionEntry < m_rangeSelectionEnd)
                {
                    if (index >= m_lastSingleSelectionEntry && index <= m_rangeSelectionEnd) return EntrySelectionState.Selected;
                }
                else
                {
                    if (index >= m_rangeSelectionEnd && index <= m_lastSingleSelectionEntry) return EntrySelectionState.Selected;
                }
            }
            return EntrySelectionState.Not;
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
                m_topEntry = m_source.GetState().FirstIndex;
                this.RequestViewPortUpdate();
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                viewPort.Invalidate();
            }
            //else if (e.Control && e.KeyCode == Keys.C)
            //{
            //}
        }

        private void chronoListViewPort_Click(object sender, EventArgs e)
        {
            this.HeadMode = false;
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
    }
}
