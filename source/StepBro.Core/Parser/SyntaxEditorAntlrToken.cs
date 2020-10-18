using ActiproSoftware.Text;
using StepBro.Core.Parser.Grammar;
using ActiproLex = ActiproSoftware.Text.Lexing;
using Antlr = Antlr4.Runtime;

namespace StepBro.Core.Parser
{
    internal class SyntaxEditorAntlrToken : ActiproLex.IToken
    {
        private readonly Antlr.IToken m_token;

        public SyntaxEditorAntlrToken(Antlr.IToken token)
        {
            m_token = token;
            this.StartOffset = 0;
            this.Length = token.StartIndex - token.StartIndex + 1;
        }

        public int EndOffset { get { return m_token.StopIndex; } }

        public TextPosition EndPosition
        {
            get { return new TextPosition(m_token.Line, m_token.Column); }
        }

        public int Id { get { return m_token.Type; } }

        public string Key { get { return StepBroLexer.DefaultVocabulary.GetSymbolicName(m_token.Type); } }

        public int Length { get; set; }         // Seems to be set by SyntaxEditor.

        public int LexicalStateId
        {
            get { return m_token.Channel; }     // TODO: Is this correct??
        }

        public TextPositionRange PositionRange
        {
            get { return new TextPositionRange(this.StartPosition, this.EndPosition); }
        }

        public int StartOffset { get; set; }

        public TextPosition StartPosition
        {
            get { return new TextPosition(m_token.Line, m_token.Column); }
        }

        public TextRange TextRange
        {
            get { return new TextRange(m_token.StartIndex, m_token.StopIndex); }
        }

        public bool Contains(int offset)
        {
            return (offset >= m_token.StartIndex && offset <= m_token.StopIndex);
        }

        public bool Contains(TextPosition position)
        {
            return position.Line == m_token.Line && position.Character >= m_token.Column && position.Character < (m_token.Column + this.Length);
        }
    }
}
