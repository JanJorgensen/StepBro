using StepBro.Core;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.PanelCreator.UI
{
    public class IPanelControlContext
    {
        public ServiceManager ServiceManager { get; }
        ILoggerScope Logger { get; }
    }

    public interface IPanelControl
    {
        void SetupPanelControl(IPanelControlContext context);
        IPanelControlInteraction Interaction { get; }
    }

    public interface IPanelControlInteraction : StepBro.PanelCreator.IPanelElement
    {
        /// <summary>
        /// Get the names of the virtual properties for child controls. This is only used for panel controls that hosts child controls.
        /// </summary>
        /// <returns>List of child control properties used by the <seealso cref="SetChildProperty"/> method.</returns>
        IEnumerable<string> GetChildProperties();
        void SetChildProperty(string name, IPanelControl childControl);
        /// <summary>
        /// Gets the names and data types of the settable properties on the control.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Tuple<string, Type>> GetValueProperties();
    }
}
