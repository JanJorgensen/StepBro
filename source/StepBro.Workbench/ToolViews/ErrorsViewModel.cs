using ActiproSoftware.Windows;
using ActiproSoftware.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Workbench.ToolViews
{
    public class ErrorsViewModel : ToolItemViewModel
    {
        private readonly DeferrableObservableCollection<ErrorInfo> m_errors = new DeferrableObservableCollection<ErrorInfo>();
        //private DelegateCommand<object> m_commandActivateError;

        public ErrorsViewModel()
        {
            this.SerializationId = "ToolErrors";
            this.Title = "Errors";

            //m_errors.Add(new ErrorInfo(ErrorType.Environment, "Info", "Remember the breakfast", "todo.txt", 23));
        }

        public ErrorsViewModel(DeferrableObservableCollection<ErrorInfo> errors) : this()
        {
            m_errors = errors;
        }


        public DeferrableObservableCollection<ErrorInfo> ErrorList
        {
            get { return m_errors; }
        }

        //public DelegateCommand<string> ActivateErrorCommand
        //{
        //    get
        //    {
        //        if (m_commandActivateError == null)
        //        {
        //            m_commandActivateError = new DelegateCommand<string>(
        //                (param) =>
        //                {
        //                    //OpenFile(param);
        //                });
        //        }
        //        return m_commandActivateError;
        //    }
        //}
    }
}
