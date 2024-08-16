using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StepBro.UI.WinForms
{
    public static class Utils
    {
        /// <summary>
        /// Invoke the specified action on the GUI thread for the object. 
        /// </summary>
        /// <param name="host">The context GUI object/control.</param>
        /// <param name="action">The action to invoke on the GUI thread.</param>
        /// <exception cref="NotImplementedException">Thrown if the host object is not a supported GUI object.</exception>
        public static void InvokeAction(this object host, Action action)
        {
            if (host is Control control)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action);
                }
                else
                {
                    action();
                }
            }
            else if (host is ToolStripItem item)
            {
                if (item.GetCurrentParent().InvokeRequired)
                {
                    item.GetCurrentParent().Invoke(action);
                }
                else
                {
                    action();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
