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

namespace StepBro.UI.WinForms.Controls
{
    public partial class ChronoListView : UserControl, IChronoListViewer
    {
        private IPresentationList<ChronoListViewEntry> m_source = null;
        private DateTime m_zeroTime;

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
        }

        private void chronoListViewPort_Click(object sender, EventArgs e)
        {

        }
    }
}
