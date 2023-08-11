using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;
using StepBro.Core.Host;

namespace StepBro.Workbench.Editor
{
    public class CalculateAction : EditActionBase
    {
        private readonly string m_expression;

        public CalculateAction(string expression) : base("Calculate " + expression)
        {
            m_expression = expression;
        }

        public override void Execute(IEditorView view)
        {
            StepBro.Core.Main.GetService<UICalculator>().Evaluate(m_expression);
        }
    }
}
