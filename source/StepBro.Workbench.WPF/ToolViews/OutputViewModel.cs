using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.UI.Controls;

namespace StepBro.Workbench.ToolViews
{
    public class OutputViewModel : ToolItemViewModel
    {
        //LogViewerViewModel m_logViewModel;

        public OutputViewModel()
        {
            this.SerializationId = "ToolOutput";
            this.Title = "Output";
            //m_logViewModel = new LogViewerViewModel();
        }

        //public LogViewerViewModel LogViewData { get { return m_logViewModel; } }
    }
}
