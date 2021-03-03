using StepBro.Core.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace StepBro.UI.Panels
{
    /// <summary>
    /// Base class for an object that identifies creatable custom panel types within an assembly.
    /// </summary>
    public abstract class CustomPanelCreator
    {
        private List<CustomPanelType> m_list = null;

        public IEnumerable<CustomPanelType> ListPanels()
        {
            return m_list;
        }

        public CustomPanelCreator() { }

        public void UpdatePanelsList()
        {
            if (m_list != null) throw new InvalidOperationException();
            m_list = this.CreatePanelList().ToList();
        }

        /// <summary>
        /// Method to be implemented in inherited class to identify/enumerate all its custom panel types.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<CustomPanelType> CreatePanelList();
    }
}
