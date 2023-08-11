using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;
using StepBro.Core.Host;
using System;

namespace StepBro.Workbench.Editor
{
    public class RepeatAction : EditActionBase
    {
        private readonly IEditAction m_action;
        public RepeatAction(IEditAction action) : base("Repeat " + action.Key)
        {
            m_action = new NoMacroRecordAction(action);     // Ensure this action is never recorded.
        }

        public override void Execute(IEditorView view)
        {
            object calcResult = StepBro.Core.Main.GetService<UICalculator>().LastResult;
            int c = Convert.ToInt32(calcResult);
            for (int i = 0; i < c; i++)
            {
                view.ExecuteEditAction(m_action);
            }
        }

        public override bool CanRecordInMacro { get { return true; } }
    }
}
