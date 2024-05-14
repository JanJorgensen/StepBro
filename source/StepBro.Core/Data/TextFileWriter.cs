using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class TextFileWriter : ITextWriter
    {
        private StreamWriter m_stream;

        public TextFileWriter(StreamWriter stream)
        {
            m_stream = stream;
        }

        public void Dispose()
        {
            m_stream.Dispose();
            m_stream = null;
        }

        public void Write(string text)
        {
            m_stream.Write(text);
        }

        public void WriteLine(string text)
        {
            m_stream.WriteLine(text);
        }
    }
}
