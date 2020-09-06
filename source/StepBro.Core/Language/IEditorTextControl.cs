namespace StepBro.Core.Language
{
    public interface IEditorTextControl
    {
        int GetLastLineInfo(out int length);
        int GetStyleIndex(string styleName);
        string GetText(int startLine, int startColumn, int endLine, int endColumn);
        void InsertText(int startLine, int startColumn, string text);
        void ClearStyles(int startLine, int startColumn, int endLine, int endColumn);
        void SetStyle(EditorTextStyles style, int startLine, int startColumn, int endLine, int endColumn);
    }
}
