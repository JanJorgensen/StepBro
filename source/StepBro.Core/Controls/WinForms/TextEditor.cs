using FastColoredTextBoxNS;
using StepBro.Core.Language;
using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StepBro.Core.Controls
{
    public partial class TextEditor : UserControl, IEditorTextControl
    {
        private static readonly Style BlueBoldStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        private static readonly Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        private static readonly Style DarkBlueStyle = new TextStyle(Brushes.DarkSlateBlue, null, FontStyle.Regular);
        private static readonly Style BoldStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        private static readonly Style BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);
        private static readonly Style GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        private static readonly Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        private static readonly Style OliveStyle = new TextStyle(Brushes.Olive, null, FontStyle.Regular);
        private static readonly Style MagentaStyle = new TextStyle(Brushes.DarkMagenta, null, FontStyle.Regular);
        //public static readonly Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        //public static readonly Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        //public static readonly Style BlackStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);
        //public static readonly Style DarkGreenStyle = new TextStyle(Brushes.DarkGreen, null, FontStyle.Regular);
        private static readonly Style OrangeStyle = new TextStyle(Brushes.Orange, null, FontStyle.Regular);
        private static readonly Style ErrorStyle = new SyntaxErrorCustomStyle();
        private static readonly Pen ErrorPen = new Pen(Color.Red, 4.0F);
        private static readonly Font BetterFont;

        private class SyntaxErrorCustomStyle : Style
        {
            public override void Draw(Graphics gr, Point position, Range range)
            {
                //get size of rectangle
                Size size = GetSizeOfRange(range);
                //create rectangle
                Rectangle rect = new Rectangle(position, size);
                //inflate it
                rect.Inflate(2, 2);
                //get rounded rectangle
                var path = GetRoundedRectangle(rect, 7);
                //draw rounded rectangle
                gr.DrawLine(ErrorPen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
            }
        }

        private Style KeywordsStyle = BlueStyle;
        private Style TypeStyle = OliveStyle;
        //private Style MemberStyle = BoldStyle;
        private Style FileObjectStyle = OrangeStyle;
        private Style TimespanStyle = DarkBlueStyle;
        private Style StringStyle = BrownStyle;
        private Style CommentStyle = GreenStyle;
        private Style NumberStyle = DarkBlueStyle;
        private Style AttributeStyle = GreenStyle;
        private Style DataTableRowStyle = BoldStyle;
        private Style DataTableCellStyle = GrayStyle;
        private bool m_isChanged = false;

        static TextEditor()
        {
            ErrorPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            if (FontFamily.Families.FirstOrDefault(f => f.Name == "Consolas") != null)
            {
                BetterFont = new Font(new FontFamily("Consolas"), 10.0f, FontStyle.Regular);
            }
            else
            {
                BetterFont = null;
            }
        }


        public TextEditor()
        {
            this.InitializeComponent();
            fastColoredTextBox.AddStyle(BlueBoldStyle);
            fastColoredTextBox.AddStyle(BlueStyle);
            fastColoredTextBox.AddStyle(DarkBlueStyle);
            fastColoredTextBox.AddStyle(BoldStyle);
            fastColoredTextBox.AddStyle(BrownStyle);
            fastColoredTextBox.AddStyle(GrayStyle);
            fastColoredTextBox.AddStyle(GreenStyle);
            fastColoredTextBox.AddStyle(OliveStyle);
            fastColoredTextBox.AddStyle(MagentaStyle);
            fastColoredTextBox.AddStyle(DarkBlueStyle);
            fastColoredTextBox.AddStyle(OrangeStyle);
            fastColoredTextBox.AddStyle(ErrorStyle);

            if (BetterFont != null)
            {
                fastColoredTextBox.Font = BetterFont;
            }
        }

        internal FastColoredTextBox FCTB { get { return fastColoredTextBox; } }

        public void OpenFile(string filepath)
        {
            fastColoredTextBox.OpenFile(filepath);
            m_isChanged = false;
            this.IsChangedChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SaveToFile(string filepath)
        {
            try
            {
                try
                {
                    fastColoredTextBox.SaveToFile(filepath, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    throw new System.IO.IOException("Error saving file: \"" + filepath + "\"", ex);
                }
                if (m_isChanged)
                {
                    m_isChanged = false;
                    this.IsChangedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
                throw;
            }
        }

        public int SelectionStartLine { get { return fastColoredTextBox.Selection.Bounds.iStartLine; } }

        private void fastColoredTextBox_TextChangedDelayed(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            this.OnTextChanged(e);
            //syntaxController.TextChanged(fastColoredTextBox.TextVersion, e.ChangedRange.Start.iLine, e.ChangedRange.Start.iChar, e.ChangedRange.End.iLine, e.ChangedRange.End.iChar);
        }

        protected virtual void OnTextChanged(TextChangedEventArgs e)
        {

        }

        private void fastColoredTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("fastColoredTextBox_TextChanged");
            this.OnTextChanged(e);
            if (fastColoredTextBox.IsChanged && !m_isChanged)
            {
                m_isChanged = true;
                this.IsChangedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler IsChangedChanged;

        public bool IsChanged { get { return fastColoredTextBox.IsChanged; } }

        public void ShowLine(int line, int selectionStart, int selectionEnd)
        {
            fastColoredTextBox.Selection = new Range(fastColoredTextBox, selectionStart, line, selectionEnd, line);
            fastColoredTextBox.DoSelectionVisible();
        }

        #region IEditorTextControl interface

        int IEditorTextControl.GetStyleIndex(string styleName)
        {
            throw new NotImplementedException();
        }

        string IEditorTextControl.GetText(int startLine, int startColumn, int endLine, int endColumn)
        {
            var range = new Range(fastColoredTextBox, startColumn, startLine, endColumn, endLine);
            return range.Text;
        }

        void IEditorTextControl.InsertText(int startLine, int startColumn, string text)
        {
            throw new NotImplementedException();
        }

        void IEditorTextControl.ClearStyles(int startLine, int startColumn, int endLine, int endColumn)
        {
            var range = new Range(fastColoredTextBox, startColumn, startLine, endColumn, endLine);
            range.ClearStyle(StyleIndex.All);
        }

        void IEditorTextControl.SetStyle(EditorTextStyles style, int startLine, int startColumn, int endLine, int endColumn)
        {
            Style editorStyle = null;
            switch (style)
            {
                case EditorTextStyles.Normal:
                    break;
                case EditorTextStyles.Comment:
                    editorStyle = CommentStyle;
                    break;
                case EditorTextStyles.DocComment:
                    editorStyle = CommentStyle;
                    break;
                case EditorTextStyles.Keyword:
                    editorStyle = KeywordsStyle;
                    break;
                case EditorTextStyles.FileObjectName:
                    editorStyle = FileObjectStyle;
                    break;
                case EditorTextStyles.Numeric:
                    editorStyle = NumberStyle;
                    break;
                case EditorTextStyles.String:
                    editorStyle = StringStyle;
                    break;
                case EditorTextStyles.Boolean:
                    editorStyle = KeywordsStyle;
                    break;
                case EditorTextStyles.Timespan:
                    editorStyle = TimespanStyle;
                    break;
                case EditorTextStyles.DateTime:
                    editorStyle = TimespanStyle;
                    break;
                case EditorTextStyles.DataTableRow:
                    editorStyle = DataTableRowStyle;
                    break;
                case EditorTextStyles.DataTableCell:
                    editorStyle = DataTableCellStyle;
                    break;
                case EditorTextStyles.Error:
                    editorStyle = ErrorStyle;
                    break;
                case EditorTextStyles.Warning:
                    editorStyle = ErrorStyle;
                    break;
                default:
                    //private Style KeywordsStyle = BlueStyle;
                    //private Style FunctionNameStyle = BoldStyle;
                    //private Style TimespanStyle = MagentaStyle;
                    //private Style StringStyle = BrownStyle;
                    //private Style CommentStyle = GreenStyle;
                    //private Style NumberStyle = MagentaStyle;
                    //private Style AttributeStyle = GreenStyle;
                    break;
            }
            if (editorStyle != null)
            {
                var range = new Range(fastColoredTextBox, startColumn, startLine, endColumn, endLine);
                range.SetStyle(editorStyle);
            }
        }

        int IEditorTextControl.GetLastLineInfo(out int length)
        {
            var iLast = fastColoredTextBox.LinesCount - 1;
            length = fastColoredTextBox[iLast].Count;
            return iLast;
        }

        #endregion
    }
}
