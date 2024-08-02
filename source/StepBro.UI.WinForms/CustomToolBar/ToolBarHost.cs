using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Parser.Grammar;
using StepBro.UI.WinForms.CustomToolBar;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public partial class ToolBarHost : UserControl
    {
        private bool m_visible = true;
        private ICoreAccess m_coreAccess = null;
        private bool m_settingVisibility = false;
        private List<Tuple<string, ToolBar>> m_addedToolbars = new List<Tuple<string, ToolBar>>();
        private List<Tuple<string, ToolBar>> m_shownToolbars = new List<Tuple<string, ToolBar>>();
        private List<string> m_hiddenToolbars = new List<string>();

        public ToolBarHost()
        {
            InitializeComponent();
            m_visible = this.Visible;
            this.AdjustHeight();    // Initially hidden; no toolbars yet.
        }

        public void Setup(ICoreAccess coreAccess)
        {
            m_coreAccess = coreAccess;
        }

        public void AddOrSet(string name, StepBro.ToolBarCreator.ToolBar toolbar)
        {
            System.Diagnostics.Debug.WriteLine("Add toolbar: " + toolbar.Name);
            ToolBar toolBar = null;
            var existing = m_addedToolbars.Where(t => t.Item1 == name).FirstOrDefault();
            if (existing != null)
            {
                toolBar = existing.Item2;
            }
            else
            {
                toolBar = new ToolBar(m_coreAccess);
                m_addedToolbars.Add(new Tuple<string, ToolBar>(name, toolBar));
            }

            toolBar.Setup(StepBro.Core.Main.RootLogger, name, toolbar.Definition);

            this.Controls.Clear();

            m_shownToolbars = m_addedToolbars.ToList();     // New list, to be sorted by display order.

            m_shownToolbars.Sort((l, r) => (1000000 - r.Item2.Index).CompareTo(1000000 - l.Item2.Index));    // Toolbars with same index in reading order from script, otherwise use after index order.
            m_shownToolbars.Reverse();

            int tabIndex = m_shownToolbars.Count;
            foreach (var tbData in m_shownToolbars)
            {
                tbData.Item2.DefaultBackColor = (tabIndex % 2 == 0) ? SystemColors.Control : Color.Beige;
                tbData.Item2.TabIndex = tabIndex--;
                this.Controls.Add(tbData.Item2);
            }

            if (m_shownToolbars.Count > 0)
            {
                m_shownToolbars[0].Item2.AdjustColumns();
            }

            this.AdjustHeight();
        }

        public IEnumerable<Tuple<string, ToolBar>> ListToolbars()
        {
            foreach (var toolbar in m_shownToolbars) yield return toolbar;
        }

        public void AdjustHeight()
        {
            if (this.Controls.Count > 0)
            {
                if (!this.Visible && m_visible)
                {
                    m_settingVisibility = true;
                    this.Visible = true;
                    m_settingVisibility = false;
                }
                this.Height = this.Controls[0].Bounds.Bottom + 3;
            }
            else
            {
                if (this.Visible)
                {
                    m_settingVisibility = true;
                    this.Visible = false;
                    m_settingVisibility = false;
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!m_settingVisibility)
            {
                m_visible = this.Visible;
            }

        }

        public void SetToolbarVisibility(string name, bool visible)
        {
            int i = -1;
            for (int j = 0; j < m_hiddenToolbars.Count; j++)
            {
                if (m_hiddenToolbars[j] == name)
                {
                    i = j;
                    break;
                }
            }
            if (visible)
            {
                if (i >= 0)
                {
                    m_hiddenToolbars.RemoveAt(i);
                }
            }
            else
            {
                if (i < 0)
                {
                    m_hiddenToolbars.Add(name);
                }
            }
            this.UpdateToolbarVisibility();
        }

        public IEnumerable<string> ListHiddenToolbars()
        {
            foreach (string toolbar in m_hiddenToolbars) yield return toolbar;
        }

        public bool IsToolbarHidden(string name)
        {
            foreach (string toolbar in m_hiddenToolbars)
            {
                if (String.Equals(name, toolbar, StringComparison.InvariantCulture)) return true;
            }
            return false;
        }

        private void UpdateToolbarVisibility()
        {
            int missingHeight = 0 - this.ClientRectangle.Height;

            // We can not use Controls[0] here, as that could give an invisible toolbar
            // so we search for the bottom-most visible toolbar
            StepBro.UI.WinForms.CustomToolBar.ToolBar bottomVisible = null; // Actually the first visible in the list, as toolbars are added in reverse order.
            if (m_shownToolbars.Count > 0)
            {
                foreach (var tb in m_shownToolbars)
                {
                    bool visible = !m_hiddenToolbars.Contains(tb.Item1);
                    tb.Item2.Visible = visible;
                    if (bottomVisible == null && visible)
                    {
                        bottomVisible = tb.Item2;
                    }
                }
                if (bottomVisible != null)
                {
                    missingHeight = bottomVisible.Bounds.Bottom - this.ClientRectangle.Height;
                }
            }

            this.Height += missingHeight;
        }

    }
}
