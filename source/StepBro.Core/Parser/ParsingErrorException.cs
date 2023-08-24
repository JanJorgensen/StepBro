using System;
using System.Xml.Linq;

namespace StepBro.Core.Parser
{
    public class ParsingErrorException : Exception
    {
        private readonly string m_fileName;
        private readonly int m_line;
        private readonly string m_elementName;
        public ParsingErrorException(string fileName, int line, string name, string message) : base(message)
        {
            m_fileName = fileName;
            m_line = line;
            m_elementName = name;
        }

        public ParsingErrorException(int line, string name, string message) : base(message)
        {
            m_fileName = "";
            m_line = line;
            m_elementName = name;
        }

        public ParsingErrorException(string message = "") : base(message)
        {
            m_fileName = "";
            m_line = -1;
            m_elementName = "";
        }

        public string FileName { get { return m_fileName; } }
        public int Line { get { return m_line; } }
        public string Name { get { return m_elementName; } }
    }
}
