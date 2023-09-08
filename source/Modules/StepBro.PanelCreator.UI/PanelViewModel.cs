using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.PanelCreator.UI
{
    public class PanelViewModel
    {
        private Panel m_definition = null;

        public Panel PanelDefinition
        {
            get
            {
                return m_definition;
            }
            set
            {
                if ((value == null) != (m_definition == null) || !Object.ReferenceEquals(m_definition, value))
                {
                    //if (m_definition != null)
                    //{
                    //    //m_definition.
                    //}
                    m_definition = value;
                    //if (m_definition != null)
                    //{
                    //}
                    this.PanelDefinitionChanged?.Invoke(this, this);
                }
            }
        }

        public event EventHandler<PanelViewModel> PanelDefinitionChanged;
    }
}
