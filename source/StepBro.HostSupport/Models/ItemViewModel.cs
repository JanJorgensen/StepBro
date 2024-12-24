using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    public class ItemViewModel : ObservableObject
    {
        private string m_description;
        //private ImageSource imageSource;
        private bool m_isActive;
        private bool m_isFloating;
        private bool m_isOpen;
        private bool m_isSelected;
        private string m_serializationID;
        private string m_title;
        private bool m_isModified = false;
        private string m_windowGroupName = null;
        private string[] m_supportedWindowGroups = null;

        public ItemViewModel(string serializationID)
        {
            m_serializationID = serializationID;
        }

        /// <summary>
        /// Gets or sets the description associated with the view-model.
        /// </summary>
        /// <value>The description associated with the view-model.</value>
        public string Description
        {
            get { return m_description; }
            set => SetProperty(ref m_description, value);
        }

        ///// <summary>
        ///// Gets or sets the image associated with the view-model.
        ///// </summary>
        ///// <value>The image associated with the view-model.</value>
        //public ImageSource ImageSource
        //{
        //    get
        //    {
        //        return imageSource;
        //    }
        //    set
        //    {
        //        if (imageSource != value)
        //        {
        //            imageSource = value;
        //            NotifyPropertyChanged("ImageSource");
        //        }
        //    }
        //}

        /// <summary>
        /// Gets or sets whether the view is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get { return m_isActive; }
            set => SetProperty(ref m_isActive, value);
        }

        ///// <summary>
        ///// Gets or sets whether the view is floating.
        ///// </summary>
        ///// <value>
        ///// <c>true</c> if the view is floating; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsFloating
        //{
        //    get { return m_isFloating; }
        //    set => SetProperty(ref m_isFloating, value);
        //}

        /// <summary>
        /// Gets or sets whether the view is currently open.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen
        {
            get { return m_isOpen; }
            set => SetProperty(ref m_isOpen, value);
        }

        /// <summary>
        /// Gets or sets whether the view is currently selected in its parent container.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently selected in its parent container; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get { return m_isSelected; }
            set => SetProperty(ref m_isSelected, value);
        }

        ///// <summary>
        ///// Gets whether the container generated for this view model should be a tool window.
        ///// </summary>
        ///// <value>
        ///// <c>true</c> if the container generated for this view model should be a tool window; otherwise, <c>false</c>.
        ///// </value>
        //public abstract bool IsTool { get; }

        /// <summary>
        /// Gets or sets the name that uniquely identifies the view-model for layout serialization.
        /// </summary>
        /// <value>The name that uniquely identifies the view-model for layout serialization.</value>
        public string SerializationId
        {
            get { return m_serializationID; }
            set => SetProperty(ref m_serializationID, value);
        }

        /// <summary>
        /// Gets or sets the title associated with the view-model.
        /// </summary>
        /// <value>The title associated with the view-model.</value>
        public string Title
        {
            get
            {
                return m_isModified ? (m_title + "*") : m_title;
            }
            set => SetProperty(ref m_title, value);
        }

        /// <summary>
        /// Whether the information for the view has changed from its origin value (e.g. data in file).
        /// </summary>
        public bool IsModified
        {
            get
            {
                return m_isModified;
            }
            set
            {
                if (SetProperty(ref m_isModified, value))
                {
                    this.OnPropertyChanged(nameof(Title));
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
                return m_windowGroupName;
            }
            set => SetProperty(ref m_windowGroupName, value);
        }

        /// <summary>
        /// The list of window group options for the view.
        /// </summary>
        public string[] SupportedWindowGroups
        {
            get
            {
                return m_supportedWindowGroups;
            }
            set => SetProperty(ref m_supportedWindowGroups, value);
        }
    }
}
