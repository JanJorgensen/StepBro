using StepBro.Core.Data;

namespace StepBro.Core.ScriptData
{
    public class UsingData
    {
        private readonly int m_line;
        private readonly IIdentifierInfo m_identifier;
        public UsingData(int line, string name, IdentifierType type)
        {
            m_line = line;
            m_identifier = new IdentifierInfo(name, name, type, null, null);
        }
        public UsingData(int line, string name, IdentifierType type, object reference)
        {
            m_line = line;
            m_identifier = new IdentifierInfo(name, name, type, null, reference);
        }
        public UsingData(int line, IIdentifierInfo identifier)
        {
            m_line = line;
            m_identifier = identifier;
        }
        public int Line { get { return m_line; } }
        public IIdentifierInfo Identifier { get { return m_identifier; } }
    }
}
