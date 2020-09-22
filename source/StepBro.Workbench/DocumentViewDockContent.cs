using StepBro.Core.File;
using StepBro.Core.General;
using System;
using WeifenLuo.WinFormsUI.Docking;

namespace StepBro.Workbench
{
    public abstract class DocumentViewDockContent : DockContent
    {
        private readonly object m_scriptFileOwner;
        private ILoadedFile m_file = null;

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

        public bool FileChanged()
        {
            return this.IsFileChanged();
        }

        protected abstract bool IsFileChanged();

        #region Open File

        public bool OpenFileView(ILoadedFile file)
        {
            m_file = file;
            try
            {
                this.DoOpenFileView(file);
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

        protected abstract void DoOpenFileView(ILoadedFile file);

        public bool OpenFile(string filepath)
        {
            if (System.IO.File.Exists(filepath))
            {
                try
                {
                    var file = this.DoOpenFile(filepath);
                    return this.OpenFileView(file);
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

        protected abstract ILoadedFile DoOpenFile(string filepath);

        #endregion

        #region Save File

        /// <summary>
        /// Saves the content of the editor/view to a file.
        /// </summary>
        /// <param name="option">The type of save operation to do.</param>
        /// <param name="filepath">The file path and name to save to.</param>
        /// <returns>Whether the save operation succeeded.</returns>
        public bool SaveFile(SaveOption option, string filepath = null)
        {
            return this.DoSaveFile(option, filepath);
        }

        protected abstract bool DoSaveFile(SaveOption option, string filepath);

        #endregion

        #region Discard Changes

        public bool DiscardChanges()
        {
            return this.DoDiscardChanges();
        }

        protected abstract bool DoDiscardChanges();

        #endregion
    }
}
