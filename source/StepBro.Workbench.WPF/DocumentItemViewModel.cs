﻿namespace StepBro.Workbench
{

    /// <summary>
    /// Represents a document item view-model.
    /// </summary>
    public class DocumentItemViewModel : DockingItemViewModelBase
    {

        private string fileName;
        private bool isReadOnly;

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
                return fileName;
            }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    this.NotifyPropertyChanged("FileName");
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
                return isReadOnly;
            }
            set
            {
                if (isReadOnly != value)
                {
                    isReadOnly = value;
                    this.NotifyPropertyChanged("IsReadOnly");
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
