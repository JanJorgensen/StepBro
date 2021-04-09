using ActiproSoftware.Text;
using ActiproSoftware.Text.Lexing.Implementation;
using StepBro.Core.Parser.Grammar;
using Antlr = Antlr4.Runtime;

namespace StepBro.UI.SyntaxEditorSupport
{
    internal class SyntaxEditorAntlrToken : TokenBase
    {
        private readonly Antlr.IToken m_token;
        private string m_key = null;

        public SyntaxEditorAntlrToken(Antlr.IToken token, int offset = 0, int lineOffset = 0) :
            base(
                token.StartIndex + offset,
                (token.StopIndex - token.StartIndex) + 1,
                new TextPosition(token.Line + lineOffset, token.Column + 1),
                new TextPosition(token.Line + lineOffset, token.Column + (token.StopIndex - token.StartIndex) + 1))
        {
            m_token = token;
        }

        public override int Id { get { return m_token.Type; } }

        public override string Key
        {
            get
            {
                if (m_key == null) m_key = Core.Parser.Grammar.StepBroLexer.DefaultVocabulary.GetSymbolicName(m_token.Type);
                return m_key;
            }
        }
    }
}
