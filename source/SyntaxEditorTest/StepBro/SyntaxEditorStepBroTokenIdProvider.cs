using ActiproSoftware.Text.Lexing;
using StepBro.Core.Parser.Grammar;

namespace StepBro.Core.Parser
{
    public class SyntaxEditorStepBroTokenIdProvider : ITokenIdProvider
    {
        private readonly int m_maxId;
        public SyntaxEditorStepBroTokenIdProvider()
        {
            m_maxId = StepBroLexer.DefaultVocabulary.MaxTokenType;
        }

        public int MaxId { get { return m_maxId; } }

        public int MinId { get { return 1; } }

        public bool ContainsId(int id)
        {
            return id >= 1 && id <= m_maxId;
        }

        public string GetDescription(int id)
        {
            return StepBroLexer.DefaultVocabulary.GetLiteralName(id);
        }

        public string GetKey(int id)
        {
            return StepBroLexer.DefaultVocabulary.GetSymbolicName(id);
        }
    }
}
