using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;
using StepBro.Workbench.Editor;
using System.Windows.Input;

namespace StepBro.Workbench
{

    /// <summary>
    /// Provides the text document item view.
    /// </summary>
    public partial class TextDocumentItemView
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes an instance of the <c>TextDocumentItemView</c> class.
        /// </summary>
        public TextDocumentItemView()
        {
            InitializeComponent();
            this.AddEditorCommandBinding(CustomEditCommands.RepeatActivationCommand);
            //this.AddCommandBinding(new InsertCalculatorResultAction());
        }

        private void AddEditorCommandBinding(ICommand action)
        {
            editor.CommandBindings.Add(new CommandBinding(action));
        }

        private void OnEditorDocumentIsModifiedChanged(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void OnEditorDocumentParseDataChanged(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void OnEditorViewSearch(object sender, ActiproSoftware.Windows.Controls.SyntaxEditor.EditorViewSearchEventArgs e)
        {

        }

        private void OnEditorUserInterfaceUpdate(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }

}
