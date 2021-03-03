using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.UI.Panels
{
    public interface ICustomPanelManager
    {
        /// <summary>
        /// List all the registered panel types.
        /// </summary>
        /// <returns></returns>
        IEnumerable<CustomPanelType> ListPanelTypes();

        /// <summary>
        /// Searches through the list of known panels, to find the panel with the specified name.
        /// </summary>
        /// <param name="name">The name of the panel.</param>
        /// <returns>Information object for the found panel type, or <code>null</code> if not found.</returns>
        CustomPanelType FindPanelType(string name);

        PanelCreationOption GetPanelCreationOption(CustomPanelType type, object @object = null);

        CustomPanelInstanceData CreateStaticPanel(string type);
        CustomPanelInstanceData CreateStaticPanel(CustomPanelType type);
        CustomPanelInstanceData CreateObjectPanel(string type, string objectReference);
        CustomPanelInstanceData CreateObjectPanel(CustomPanelType type, string objectReference);
        CustomPanelInstanceData CreateObjectPanel(CustomPanelType type, IObjectContainer container);

        IEnumerable<CustomPanelInstanceData> ListCreatedPanels();
    }
}
