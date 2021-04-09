using ActiproSoftware.Text.Lexing;
using StepBro.Core.Parser.Grammar;

namespace StepBro.UI.SyntaxEditorSupport
{
    public class StepBroTokenIdProvider : ITokenIdProvider
    {
        private readonly int m_maxId;
        public StepBroTokenIdProvider()
        {
            m_maxId = ((Antlr4.Runtime.Vocabulary)Core.Parser.Grammar.StepBroLexer.DefaultVocabulary).getMaxTokenType();
        }

        public int MaxId { get { return m_maxId; } }

        public int MinId { get { return 1; } }

        public bool ContainsId(int id)
        {
            return id >= 1 && id <= m_maxId;
        }

        public string GetDescription(int id)
        {
            return Core.Parser.Grammar.StepBroLexer.DefaultVocabulary.GetLiteralName(id);
        }

        public string GetKey(int id)
        {
            return Core.Parser.Grammar.StepBroLexer.DefaultVocabulary.GetSymbolicName(id);
        }
    }
}
