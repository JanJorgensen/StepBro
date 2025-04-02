using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    public partial class ItemViewModel : ObservableObject
    {
        private string m_title = null;
        private bool m_isModified = false;
        //private ImageSource imageSource;
        public ItemViewModel(string serializationID)
        {
            m_serializationID = serializationID;
        }

        /// <summary>
        /// Gets or sets the name that uniquely identifies the view-model for layout serialization.
        /// </summary>
        /// <value>The name that uniquely identifies the view-model for layout serialization.</value>
        [ObservableProperty]
        private string m_serializationID;

        /// <summary>
        /// Gets or sets the description associated with the view-model.
        /// </summary>
        /// <value>The description associated with the view-model.</value>
        [ObservableProperty]
        private string m_description = null;

        [ObservableProperty]
        private object m_dataContext = null;

        [ObservableProperty]
        private bool m_isDocument = false;

        /// <summary>
        /// Gets or sets whether the view is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently active; otherwise, <c>false</c>.
        /// </value>
        [ObservableProperty]
        private bool m_isActive = false;

        /// <summary>
        /// Gets or sets whether the view is currently open.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently open; otherwise, <c>false</c>.
        /// </value>
        [ObservableProperty]
        private bool m_isOpen = false;

        /// <summary>
        /// Gets or sets whether the view is currently selected in its parent container.
        /// </summary>
        /// <value>
        /// <c>true</c> if the view is currently selected in its parent container; otherwise, <c>false</c>.
        /// </value>
        [ObservableProperty]
        private bool m_isSelected = false;



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
        /// Gets or sets the title associated with the view-model.
        /// </summary>
        /// <value>The title associated with the view-model.</value>
        public string Title
        {
            get
            {
                return this.m_isModified ? (m_title + "*") : m_title;
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
        [ObservableProperty]
        private string m_windowGroupName = null;
        /// <summary>
        /// The list of window group options for the view.
        /// </summary>
        [ObservableProperty]
        private string[] m_supportedWindowGroups = null;
    }


    public static class ItemViewModelSupport
    {
        public static bool IsShownInDocuments(this ItemViewModel view)
        {
            return view.WindowGroupName == HostAppModel.WG_DOCUMENT;
        }
        public static bool IsShownInPrimary(this ItemViewModel view)
        {
            return view.WindowGroupName == HostAppModel.WG_PRIMARY;
        }
        public static bool IsShownInSecondary(this ItemViewModel view)
        {
            return view.WindowGroupName == HostAppModel.WG_SECONDARY;
        }
        public static bool IsShownInBottom(this ItemViewModel view)
        {
            return view.WindowGroupName == HostAppModel.WG_BOTTOM;
        }
    }
}
