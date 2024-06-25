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
        private bool m_tailMode = true;
        private long m_topEntry = 0L;
        private long m_lastIndex = 0;
        private bool m_viewDirty = false;

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

        private void chronoListViewPort_Click(object sender, EventArgs e)
        {

        }

        public bool TailMode
        {
            get { return m_tailMode; }
            set
            {
                if (m_tailMode != value)
                {
                    m_tailMode = value;
                    m_source.SetTailMode(m_tailMode);
                    if (m_tailMode)
                    {
                        viewPort.RequestUpdate(m_topEntry, 0 - hScrollBar.Value);
                        //logWindow.SetCurrentEntry(null, false, true);
                    }
                    else
                    {

                    }
                    this.TailModeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.RequestViewPortUpdate();
        }

        public event EventHandler TailModeChanged;

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (m_source.InTailMode)
            {
                m_source.Get(Int64.MaxValue);
                var state = m_source.GetState();
                if (m_viewDirty || state.LastIndex != m_lastIndex)
                {
                    m_lastIndex = state.LastIndex;
                    this.RequestViewPortUpdate();
                    vScrollBar.Maximum = Math.Max(0, (int)(state.EffectiveCount - viewPort.MaxLinesVisible));
                    vScrollBar.Value = (int)m_topEntry;
                }
            }
        }

        private void RequestViewPortUpdate()
        {
            m_viewDirty = false;
            m_topEntry = Math.Max(0L, m_lastIndex - (viewPort.MaxLinesVisible - 1L));
            viewPort.RequestUpdate(m_topEntry, 0 - hScrollBar.Value);
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            m_viewDirty = true;
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            m_viewDirty = true;
        }
    }
}
