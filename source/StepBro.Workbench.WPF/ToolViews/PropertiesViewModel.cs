using ActiproSoftware.Windows.Controls.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace StepBro.Workbench.ToolViews
{
    internal class PropertiesViewModel : ToolItemViewModel
    {
        public PropertiesViewModel()
        {
            this.SerializationId = "ToolProperties";
            this.Title = "Properties / Details";
        }

        private object m_contextObject = null;
        private Control m_content = null;

        public void SetContextObject(object @object)
        {
            if ((m_content == null) != (@object == null) || !Object.ReferenceEquals(m_content, @object))
            {
                m_content = null;
                this.NotifyPropertyChanged(nameof(Content));
            }
        }

        public Control Content
        {
            get
            {
                if (m_content == null && m_contextObject != null)
                {
                    var grid = new PropertyGrid();
                    m_content = grid;
                    grid.DataObject = m_contextObject;
                }
                return m_content;
            }
        }
    }
}
