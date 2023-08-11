using ActiproSoftware.Windows;

namespace StepBro.UI.Controls
{

    /// <summary>
    /// Represents a base class for all docking item view-models.
    /// </summary>
    public abstract class LoadedFileViewModel : ObservableObjectBase
    {
        private string filename;
        private string filepath;
        private string filetype;

        private string groupName;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        public string FileName
        {
            get
            {
                return filename;
            }
            set
            {
                if (filename != value)
                {
                    filename = value;
                    NotifyPropertyChanged(nameof(FileName));
                }
            }
        }

        public string FilePath
        {
            get
            {
                return filepath;
            }
            set
            {
                if (filepath != value)
                {
                    filepath = value;
                    NotifyPropertyChanged(nameof(FilePath));
                }
            }
        }

        public string FileType
        {
            get
            {
                return filetype;
            }
            set
            {
                if (filetype != value)
                {
                    filetype = value;
                    NotifyPropertyChanged(nameof(FileType));
                }
            }
        }

        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                if (groupName != value)
                {
                    groupName = value;
                    NotifyPropertyChanged(nameof(GroupName));
                }
            }
        }

    }

}
