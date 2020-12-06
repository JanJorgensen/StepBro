using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.EditActions;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;
using System;

namespace StepBro.Workbench.Editor
{
    public class MacroPlaybackAction : EditActionBase, IMacroAction
    {
        private readonly IEditAction m_action;
        public MacroPlaybackAction(IEditAction action) : base("Repeat " + action.Key)
        {
            m_action = new NoMacroRecordAction(action);     // Ensure the action is never recorded.
        }

        public override void Execute(IEditorView view)
        {
        }

        public void Add(IEditAction action)
        {
            throw new NotImplementedException();
        }

        public override bool CanRecordInMacro { get { return true; } }

        public bool IsEmpty => throw new NotImplementedException();
    }
}
