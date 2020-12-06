using System;
using System.Windows.Media.Imaging;

namespace StepBro.Workbench
{
    /// <summary>
    /// Represents the text document view-model.
    /// </summary>
    public class TextDocumentItemViewModel : DocumentItemViewModel
    {

        private string text;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="TextDocumentItemViewModel"/> class.
        /// </summary>
        public TextDocumentItemViewModel()
        {
            this.Description = "Text document";
            this.ImageSource = new BitmapImage(new Uri("Resources/Images/TextDocument16.png", UriKind.Relative));
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the text associated with the view-model.
        /// </summary>
        /// <value>The text associated with the view-model.</value>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text != value)
                {
                    text = value;
                    this.NotifyPropertyChanged("Text");
                }
            }
        }

    }

}
