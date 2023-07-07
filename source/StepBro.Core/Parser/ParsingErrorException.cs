using System;

namespace StepBro.Core.Parser
{
    public class ParsingErrorException : Exception
    {
        private readonly int m_line;
        private readonly string m_elementName;
        private readonly string m_fileName;
        public ParsingErrorException(int line, string name, string fileName, string message) : base(message)
        {
            m_line = line;
            m_elementName = name;
            m_fileName = fileName;
        }
        public int Line { get { return m_line; } }
        public string Name { get { return m_elementName; } }
        public string FileName { get { return m_fileName; } }
    }
}
