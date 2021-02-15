using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Input;

namespace StepBro.Workbench
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            editor.Document.Language = new StepBro.UI.Controls.ExecutionTargetSelectorLanguage();
            editor.Document.Language.RegisterService<ICompletionProvider>(new StepBro.UI.Controls.ExecutionTargetCompletionProvider());

        }

        #region Open File Command

        private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            System.Diagnostics.Trace.WriteLine("OpenCanExecute");
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "StepBro files (*.sbs)|*.sbs|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                var viewModel = (MainViewModel)DataContext;
                if (viewModel.OpenFileCommand.CanExecute(null))
                {
                    viewModel.OpenFileCommand.Execute(dialog.FileName);
                }
            }
        }

        #endregion
    }
}
