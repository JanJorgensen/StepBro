using StepBro.Core.General;
using System;
using WeifenLuo.WinFormsUI.Docking;

namespace StepBro.Workbench
{
    public abstract class DocumentViewDockContent : DockContent
    {
        private readonly object m_scriptFileOwner;
        private ILoadedFile m_file;

        public DocumentViewDockContent(object scriptOwner) : base()
        {
            m_scriptFileOwner = scriptOwner;
            this.FormClosing += this.DocumentViewDockContent_FormClosing;
        }

        private void DocumentViewDockContent_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (this.File != null)
            {
                m_file.UnregisterDependant(m_scriptFileOwner);
                StepBro.Core.Main.GetLoadedFilesManager().UnloadAllFilesWithoutDependants();
            }
        }

        public ILoadedFile File { get { return m_file; } }
        protected object FileOwner { get { return m_scriptFileOwner; } }

        protected override string GetPersistString()
        {
            // Add extra information into the persist string for this document
            // so that it is available when deserialized.
            string title = this.Text.Replace("*", "");
            return this.GetType().ToString() + "," + this.File.FilePath + "," + title;
        }

        public virtual void ShowLine(int line, int selectionStart, int selectionEnd)
        {
            throw new NotImplementedException();
        }

        public bool OpenFile(ILoadedFile file)
        {
            m_file = file;
            try
            {
                this.DoOpenFile(file);
                m_file.RegisterDependant(this.FileOwner);
                StepBro.Core.Main.GetLoadedFilesManager().RegisterLoadedFile(m_file);
                this.ToolTipText = file.FilePath;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool OpenFile(string filepath)
        {
            if (System.IO.File.Exists(filepath))
            {
                try
                {
                    var file = this.DoOpenFile(filepath);
                    return this.OpenFile(file);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected abstract void DoOpenFile(ILoadedFile file);

        protected abstract ILoadedFile DoOpenFile(string filepath);
    }
}
