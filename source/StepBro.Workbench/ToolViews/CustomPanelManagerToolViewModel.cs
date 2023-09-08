using ActiproSoftware.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Workbench.ToolViews
{
    internal class CustomPanelManagerToolViewModel : ToolItemViewModel
    {
        private readonly DeferrableObservableCollection<ErrorInfo> m_errors = new DeferrableObservableCollection<ErrorInfo>();
        //private DelegateCommand<object> m_commandActivateError;

        public CustomPanelManagerToolViewModel()
        {
            this.SerializationId = "ToolErrors";
            this.Title = "Errors";

            //m_errors.Add(new ErrorInfo(ErrorType.Environment, "Info", "Remember the breakfast", "todo.txt", 23));
        }

        public CustomPanelManagerToolViewModel(DeferrableObservableCollection<ErrorInfo> errors) : this()
        {
            m_errors = errors;
        }


        public DeferrableObservableCollection<ErrorInfo> ErrorList
        {
            get { return m_errors; }
        }
    }
}
