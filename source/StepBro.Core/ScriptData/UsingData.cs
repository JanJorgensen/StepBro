using StepBro.Core.Data;

namespace StepBro.Core.ScriptData
{
    public class UsingData
    {
        private readonly int m_line;
        private readonly string m_alias;
        private readonly IIdentifierInfo m_identifier;
        private bool m_isPublic = false;
        
        public UsingData(int line, bool isPublic, string name, IdentifierType type)
        {
            m_line = line;
            m_isPublic = isPublic;
            m_alias = null;
            m_identifier = new IdentifierInfo(name, name, type, null, null);
        }
        
        public UsingData(int line, bool isPublic, string name, IdentifierType type, object reference)
        {
            m_line = line;
            m_isPublic = isPublic;
            m_alias = null;
            m_identifier = new IdentifierInfo(name, name, type, null, reference);
        }
        
        public UsingData(int line, bool isPublic, string alias, IIdentifierInfo identifier)
        {
            m_line = line;
            m_isPublic = isPublic;
            m_alias = alias;
            m_identifier = identifier;
        }
        
        public UsingData(int line, bool isPublic, IIdentifierInfo identifier)
        {
            m_line = line;
            m_isPublic = isPublic;
            m_alias = null;
            m_identifier = identifier;
        }
        
        public int Line { get { return m_line; } }
        public bool IsPublic { get { return m_isPublic; } }
        public bool IsAlias { get { return m_alias != null; } }
        public string Alias { get { return m_alias; } }
        public IIdentifierInfo Identifier { get { return m_identifier; } }

        public override string ToString()
        {
            string alias = string.IsNullOrEmpty(m_alias) ? "" : ($"\"{m_alias}\" ");
            string privacy = IsPublic ? "public" : "private";
            return $"From line {m_line}: {privacy} {alias}{m_identifier.Type} - {m_identifier.Name} {((m_identifier.Reference != null) ? "found!" : "NOT FOUND")}";
        }
    }
}
