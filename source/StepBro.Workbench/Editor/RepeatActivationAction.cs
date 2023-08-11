using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;
using StepBro.Core.Host;
using System.Windows.Threading;

namespace StepBro.Workbench.Editor
{
    public class RepeatActivationAction : EditActionBase
    {
        private UICalculator m_calc;
        private IEditorView m_view = null;

        public RepeatActivationAction() : base("RepeatActivation")
        {
        }

        private UICalculator Calc
        {
            get
            {
                if (m_calc == null) m_calc = StepBro.Core.Main.GetService<UICalculator>();
                return m_calc;
            }
        }

        public override bool CanExecute(IEditorView view)
        {
            object calcResult = this.Calc.LastResult;
            if (calcResult is int || calcResult is long)
            {
                return true;
            }
            else return false;
        }

        public override void Execute(IEditorView view)
        {
            if (m_view == null)
            {
                view.SyntaxEditor.ViewActionExecuting += this.SyntaxEditor_ViewActionExecuting;
                m_view = view;
            }
            else
            {
                m_view.SyntaxEditor.ViewActionExecuting -= this.SyntaxEditor_ViewActionExecuting;
                m_view = null;
            }
        }

        private void SyntaxEditor_ViewActionExecuting(object sender, EditActionEventArgs e)
        {
            if (e.Action.CanRecordInMacro)
            {
                e.Cancel = true;
                var repeatAction = new RepeatAction(e.Action);
                m_view.SyntaxEditor.Dispatcher.BeginInvoke(DispatcherPriority.Send, (DispatcherOperationCallback)delegate (object arg)
                {
                    m_view.SyntaxEditor.Focus();
                    m_view.ExecuteEditAction(new RepeatAction(e.Action));
                    m_view = null;
                    return null;
                }, null);
            }
            else
            {
                m_view = null;
            }
            m_view.SyntaxEditor.ViewActionExecuting -= this.SyntaxEditor_ViewActionExecuting;   // Stop waiting for action to repeat.
        }

        public override bool CanRecordInMacro { get { return false; } }
    }
}
