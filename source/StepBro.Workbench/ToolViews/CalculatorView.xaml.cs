using ActiproSoftware.Windows.Controls.SyntaxEditor;
using StepBro.Core.Host;
using System.Windows.Input;

namespace StepBro.Workbench.ToolViews
{
    public partial class CalculatorView : IEditorViewKeyInputEventSink
    {
        public CalculatorView()
        {
            this.InitializeComponent();

            expressionEditor.Document.Language = new StepBro.UI.SyntaxEditorSupport.StepBroSyntaxLanguage();
            // Register a key input event sink to be able to act on Enter.
            expressionEditor.Document.Language.RegisterService<IEditorViewKeyInputEventSink>(this);
        }

        public void NotifyKeyDown(IEditorView view, KeyEventArgs e)
        {
        }

        public void NotifyKeyUp(IEditorView view, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                var context = this.DataContext as CalculatorViewModel;
                if (context != null && context.ActivatingDocument != null)
                {
                    var doc = context.ActivatingDocument;   // Save doc ref ...
                    doc.IsActive = true;                    // ... because this line will result in context.ActivatingDocument being set to null.
                    doc.ExecuteEditorCalculation(expressionEditor.Text);
                }
                else
                {
                    StepBro.Core.Main.GetService<UICalculator>().Evaluate(expressionEditor.Text);
                }
                expressionEditor.Text = "";
            }
        }
    }
}
