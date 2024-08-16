using FastColoredTextBoxNS;
using StepBro.Core.Language;

namespace StepBro.Core.Controls
{
    public class SourceCodeEditor : TextEditor
    {
        private readonly IEditorSupport syntaxController;

        public SourceCodeEditor() : base()
        {
            syntaxController = this.CreateSyntaxHighlighter();
            this.FCTB.CurrentLineColor = System.Drawing.Color.Khaki;
        }

        protected virtual IEditorSupport CreateSyntaxHighlighter() { return null; }

        internal void ForceSyntaxRefresh()
        {
            syntaxController?.ResetSyntax();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            syntaxController?.TextChanged(this.FCTB.TextVersion, e.ChangedRange.Start.iLine, e.ChangedRange.Start.iChar, e.ChangedRange.End.iLine, e.ChangedRange.End.iChar);
        }
    }
}
