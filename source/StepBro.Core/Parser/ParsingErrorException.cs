using System;

namespace StepBro.Core.Parser
{
    internal class ParsingErrorException : Exception
    {
        private readonly int m_line;
        private readonly string m_elementName;
        public ParsingErrorException(int line, string name, string message) : base(message)
        {
            m_line = line;
            m_elementName = name;
        }
        public int Line { get { return m_line; } }
        public string Name { get { return m_elementName; } }
    }
}
