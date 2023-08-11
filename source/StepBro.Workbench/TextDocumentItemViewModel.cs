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
        private int m_caretLine = -1;
        private int m_caretCharacter = -1;
        private int m_caretDisplayCharColumn = -1;
        private Tuple<int, int> m_surrentSelectionStart;

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
            m_surrentSelectionStart = new Tuple<int, int>(-1, -1);
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
                    this.NotifyPropertyChanged(nameof(Text));
                }
            }
        }

        public Tuple<int, int> CaretPosition
        {
            get
            {
                return m_surrentSelectionStart;
            }
            set
            {
                if (m_surrentSelectionStart != value)
                {
                    m_surrentSelectionStart = value;
                    this.NotifyPropertyChanged(nameof(CaretPosition));
                }
            }
        }

        public int CaretLine
        {
            get
            {
                return m_caretLine;
            }
            set
            {
                if (m_caretLine != value)
                {
                    m_caretLine = value;
                    this.NotifyPropertyChanged(nameof(CaretLine));
                }
            }
        }


        public int CaretCharacter
        {
            get
            {
                return m_caretCharacter;
            }
            set
            {
                if (m_caretCharacter != value)
                {
                    m_caretCharacter = value;
                    this.NotifyPropertyChanged(nameof(CaretCharacter));
                }
            }
        }


        public int CaretDisplayCharColumn
        {
            get
            {
                return m_caretDisplayCharColumn;
            }
            set
            {
                if (m_caretDisplayCharColumn != value)
                {
                    m_caretDisplayCharColumn = value;
                    this.NotifyPropertyChanged(nameof(CaretDisplayCharColumn));
                }
            }
        }
    }
}
