using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StepBro.UI.Controls
{
    /// <summary>
    /// Interaction logic for ExecutionTargetSelector.xaml
    /// </summary>
    public partial class ExecutionTargetSelector : UserControl
    {
        public ExecutionTargetSelector()
        {
            InitializeComponent();

            editor.Document.Language = new ExecutionTargetSelectorLanguage();
            // Register a custom completion provider on the language used by the editor
            editor.Document.Language.RegisterService<ICompletionProvider>(new ExecutionTargetCompletionProvider());
        }

        public SyntaxEditor EditorInside { get { return editor; } }
    }
}
