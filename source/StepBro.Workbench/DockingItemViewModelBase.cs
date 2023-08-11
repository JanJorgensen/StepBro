using ActiproSoftware.Windows;
using System.Windows.Media;

namespace StepBro.Workbench
{

    /// <summary>
    /// Represents a base class for all docking item view-models.
    /// </summary>
    public abstract class DockingItemViewModelBase : ObservableObjectBase
    {
        private string description;
        private ImageSource imageSource;
        private bool isActive;
        private bool isFloating;
        private bool isOpen;
        private bool isSelected;
        private string serializationId;
        private string title;
        private bool isModified = false;

        private string windowGroupName;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the description associated with the view-model.
        /// </summary>
        /// <value>The description associated with the view-model.</value>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        /// <summary>
        /// Gets or sets the image associated with the view-model.
        /// </summary>
        /// <value>The image associated with the view-model.</value>
        public ImageSource ImageSource
        {
            get
            {
                return imageSource;
            }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    NotifyPropertyChanged("ImageSource");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the view is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    NotifyPropertyChanged("IsActive");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the view is floating.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is floating; otherwise, <c>false</c>.
        /// </value>
        public bool IsFloating
        {
            get
            {
                return isFloating;
            }
            set
            {
                if (isFloating != value)
                {
                    isFloating = value;
                    NotifyPropertyChanged("IsFloating");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the view is currently open.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen
        {
            get
            {
                return isOpen;
            }
            set
            {
                if (isOpen != value)
                {
                    isOpen = value;
                    NotifyPropertyChanged("IsOpen");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the view is currently selected in its parent container.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently selected in its parent container; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        /// <summary>
        /// Gets whether the container generated for this view model should be a tool window.
        /// </summary>
        /// <value>
        /// <c>true</c> if the container generated for this view model should be a tool window; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsTool { get; }

        /// <summary>
        /// Gets or sets the name that uniquely identifies the view-model for layout serialization.
        /// </summary>
        /// <value>The name that uniquely identifies the view-model for layout serialization.</value>
        public string SerializationId
        {
            get
            {
                return serializationId;
            }
            set
            {
                if (serializationId != value)
                {
                    serializationId = value;
                    NotifyPropertyChanged("SerializationId");
                }
            }
        }

        /// <summary>
        /// Gets or sets the title associated with the view-model.
        /// </summary>
        /// <value>The title associated with the view-model.</value>
        public string Title
        {
            get
            {
                return isModified ? (title + "*") : title;
            }
            set
            {
                if (title != value)
                {
                    title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        public bool IsModified
        {
            get
            {
                return isModified;
            }
            set
            {
                if (isModified != value)
                {
                    isModified = value;
                    NotifyPropertyChanged(nameof(IsModified));
                    NotifyPropertyChanged(nameof(Title));
                }
            }
        }

        /// <summary>
        /// Gets or sets the window group name associated with the view-model.
        /// </summary>
        /// <value>The window group name associated with the view-model.</value>
        public string WindowGroupName
        {
            get
            {
                return windowGroupName;
            }
            set
            {
                if (windowGroupName != value)
                {
                    windowGroupName = value;
                    NotifyPropertyChanged("WindowGroupName");
                }
            }
        }

    }

}
