using StepBro.Core.General;

namespace StepBro.Workbench
{

    /// <summary>
    /// Represents a document item view-model.
    /// </summary>
    public class DocumentItemViewModel : DockingItemViewModelBase
    {
        private string m_fileName = null;
        private bool m_isReadOnly = true;
        private ILoadedFile m_loadedFile = null;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the file name associated with the view-model.
        /// </summary>
        /// <value>The file name associated with the view-model.</value>
        public string FileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                if (m_fileName != value)
                {
                    m_fileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        /// <summary>
        /// Gets or sets the read-only state of the associated with the view-model.
        /// </summary>
        /// <value>The read-only state of the associated with the view-model.</value>
        public bool IsReadOnly
        {
            get
            {
                return m_isReadOnly;
            }
            set
            {
                if (m_isReadOnly != value)
                {
                    m_isReadOnly = value;
                    NotifyPropertyChanged("IsReadOnly");
                }
            }
        }

        public ILoadedFile LoadedFile
        {
            get { return m_loadedFile; }
            set
            {
                if (!object.ReferenceEquals(value, m_loadedFile))
                {
                    m_loadedFile = value;
                    if (m_loadedFile != null)
                    {
                        this.FileName = m_loadedFile.FilePath;
                        this.SerializationId = m_loadedFile.FilePath;
                        this.Title = System.IO.Path.GetFileName(m_loadedFile.FilePath);
                    }
                    NotifyPropertyChanged("LoadedFile");
                }
            }
        }

        /// <summary>
        /// Gets whether the container generated for this view model should be a tool window.
        /// </summary>
        /// <value>
        /// <c>true</c> if the container generated for this view model should be a tool window; otherwise, <c>false</c>.
        /// </value>
        public override bool IsTool
        {
            get
            {
                return false;
            }
        }

    }

}
