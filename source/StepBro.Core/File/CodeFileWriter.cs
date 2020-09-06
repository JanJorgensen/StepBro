using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.File
{
    public class CodeFileWriter : IDisposable
    {
        private bool m_creator;
        System.IO.StreamWriter m_writer;
        Stack<string> m_scopeIndents = new Stack<string>();
        private string m_indent;

        public CodeFileWriter(string path)
        {
            m_creator = true;
            m_writer = new System.IO.StreamWriter(path);
            m_indent = "";
        }

        private CodeFileWriter(System.IO.StreamWriter writer, string indent)
        {
            m_creator = false;
            m_writer = writer;
            m_indent = indent;
        }

        public string CurrentIndent { get { return m_indent; } }

        public void WriteLine()
        {
            m_writer.WriteLine();
        }

        public void WriteLine(string line)
        {
            m_writer.WriteLine(m_indent + line);
        }

        public void WriteLine(string format, params object[] arguments)
        {
            m_writer.WriteLine(m_indent + String.Format(format, arguments));
        }

        public void WriteLineNoIndent(string line)
        {
            m_writer.WriteLine(line);
        }

        public void WriteLineNoIndent(string format, params object[] arguments)
        {
            m_writer.WriteLine(String.Format(format, arguments));
        }

        public void WriteDirect(string text)
        {
            m_writer.Write(text);
        }

        public void WriteDirect(string format, params object[] arguments)
        {
            m_writer.Write(String.Format(format, arguments));
        }

        public CodeFileWriter CreateScope(string indent)
        {
            return new CodeFileWriter(m_writer, m_indent + indent);
        }

        public void EnterScope(string indent)
        {
            m_scopeIndents.Push(m_indent);
            m_indent += indent;
        }

        public void ExitScope()
        {
            if (m_scopeIndents.Count == 0)
            {
                throw new Exception("ExitScope has no matching BeginScope.");
            }
            m_indent = m_scopeIndents.Pop();
        }

        public void Dispose()
        {
            if (m_creator)
            {
                m_writer.Dispose();
            }
        }
    }
}
