using StepBro.Core.Controls;
using StepBro.Core.File;
using StepBro.Core.General;
using System;

namespace StepBro.Workbench
{
    public class TextEditorDockContent : DocumentViewDockContent
    {
        public TextEditorDockContent() : this(new object()) { }

        public TextEditorDockContent(object scriptOwner) : base(scriptOwner)
        {
        }

        protected virtual TextEditor Editor { get { throw new NotImplementedException(); } }

        protected override void DoOpenFileView(ILoadedFile file)
        {
            this.Editor.OpenFile(file.FilePath);
        }

        protected override bool DoDiscardChanges()
        {
            throw new NotImplementedException();
        }

        protected override bool IsFileChanged()
        {
            return this.Editor.IsChanged;
        }

        protected override bool DoSaveFile(SaveOption option, string filepath)
        {
            System.Diagnostics.Debug.WriteLine("SaveToFile");
            try
            {
                switch (option)
                {
                    case SaveOption.SaveToExisting:
                        this.Editor.SaveToFile(this.File.FilePath);
                        return true;

                    case SaveOption.SaveAsCopy:
                        try
                        {
                            this.Editor.SaveToFile(filepath);
                            return this.DiscardChanges();
                        }
                        catch (Exception)
                        {
                            return false;
                        }

                    case SaveOption.Rename:
                        throw new NotImplementedException();

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception)
            {
                return false;
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
