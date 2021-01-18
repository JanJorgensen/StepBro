using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface IObjectPanelManager
    {
        /// <summary>
        /// List all the registered panel types.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ObjectPanelInfo> ListPanelTypes();

        /// <summary>
        /// Searches through the list of known panels, to find the panel with the specified name.
        /// </summary>
        /// <param name="name">The name of the panel.</param>
        /// <returns>Information object for the found panel type, or <code>null</code> if not found.</returns>
        ObjectPanelInfo FindPanel(string name);

        PanelCreationOption GetPanelCreationOption(ObjectPanelInfo type, object @object = null);

        Controls.ObjectPanel CreateStaticPanel(ObjectPanelInfo type);
        Controls.ObjectPanel CreateObjectPanel(ObjectPanelInfo type, IObjectContainer container);
        Controls.ObjectPanel CreateObjectPanel(ObjectPanelInfo type, string objectReference);
        Controls.WinForms.ObjectPanel CreateStaticWinFormsPanel(ObjectPanelInfo type);
        Controls.WinForms.ObjectPanel CreateObjectWinFormsPanel(ObjectPanelInfo type, IObjectContainer container);
        Controls.WinForms.ObjectPanel CreateObjectWinFormsPanel(ObjectPanelInfo type, string objectReference);

        IEnumerable<Controls.ObjectPanel> ListCreatedPanels();
        IEnumerable<Controls.WinForms.ObjectPanel> ListCreatedPanelsWinForms();
    }
}
