using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;
using StepBro.Core.Data;
using StepBro.Core.Host;

namespace StepBro.Workbench.Editor
{
    internal class TextChangeInsertCalculatorResult : ITextChangeType
    {
        private static readonly TextChangeInsertCalculatorResult g_instance = new TextChangeInsertCalculatorResult();
        public static TextChangeInsertCalculatorResult Instance { get { return g_instance; } }
        public string Description { get { return "Inserted the last result of the calculator."; } }

        public string Key { get { return "InsertCalculatorResult"; } }
    }

    /// <summary>
    /// Action to insert the last calculator result into the editor document as text.
    /// </summary>
    public class InsertCalculatorResultAction : EditActionBase
    {
        public InsertCalculatorResultAction() : base("InsertCalculatorResult") { }

        public override void Execute(IEditorView view)
        {
            view.ReplaceSelectedText(
                TextChangeInsertCalculatorResult.Instance,
                StringUtils.ObjectToString(StepBro.Core.Main.GetService<UICalculator>().LastResult, true));
        }
    }
}
