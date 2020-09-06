using System;
using StepBro.Core.Controls;
using StepBro.Core.General;

namespace StepBro.Workbench
{
    public class TextEditorDockContent : DocumentViewDockContent
    {
        public TextEditorDockContent() : this(new object()) { }

        public TextEditorDockContent(object scriptOwner) : base(scriptOwner)
        {
        }

        protected virtual TextEditor Editor { get { throw new NotImplementedException(); } }

        protected override void DoOpenFile(ILoadedFile file)
        {
            this.Editor.OpenFile(file.FilePath);
        }

        public void SaveToFile()
        {
            System.Diagnostics.Debug.WriteLine("SaveToFile");
            try
            {
                this.Editor.SaveToFile(this.File.FilePath);
                //this.Text = this.Text.Replace("*", "");
            }
            catch (Exception)
            {

            }
        }

        protected override ILoadedFile DoOpenFile(string filepath)
        {
            throw new NotImplementedException();
        }

        protected void OnEditorIsChangedChanged()
        {
            if (this.Editor.IsChanged) this.Text += "*";
            else this.Text = this.Text.Replace("*", "");
        }

        public int SelectionStartLine { get { return this.Editor.SelectionStartLine; } }

        public override void ShowLine(int line, int selectionStart, int selectionEnd)
        {
            this.Editor.ShowLine(line, selectionStart, selectionEnd);
        }
    }
}
