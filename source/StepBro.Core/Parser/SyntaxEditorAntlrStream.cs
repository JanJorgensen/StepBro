using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiproSoftware.Text;
using Antlr4.Runtime.Misc;
using ActiproLex = ActiproSoftware.Text.Lexing;
using Antlr = Antlr4.Runtime;

namespace StepBro.Core.Parser
{
    public class SyntaxEditorAntlrStream : Antlr.ICharStream
    {
        ITextSnapshotReader m_reader;

        public SyntaxEditorAntlrStream(ITextSnapshotReader reader)
        {
            m_reader = reader;
        }

        public int Index => throw new NotImplementedException();

        public int Size { get { throw new NotSupportedException(); } }

        public string SourceName
        {
            get { return "EditorSnapshot"; }
        }

        public void Consume()
        {
            throw new NotImplementedException();
        }

        [return: NotNull]
        public string GetText([NotNull] Interval interval)
        {
            throw new NotImplementedException();
        }

        public int La(int i)
        {
            throw new NotImplementedException();
        }

        public int Mark()
        {
            throw new NotImplementedException();
        }

        public void Release(int marker)
        {
            throw new NotImplementedException();
        }

        public void Seek(int index)
        {
            throw new NotImplementedException();
        }
    }
}
