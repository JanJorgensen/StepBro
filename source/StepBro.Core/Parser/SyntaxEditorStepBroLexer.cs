using ActiproSoftware.Text;
using ActiproLex = ActiproSoftware.Text.Lexing;
using Antlr = Antlr4.Runtime;
using System;

namespace StepBro.Core.Parser
{
    public class SyntaxEditorStepBroLexer : ActiproLex.ILexer
    {
        private readonly ActiproLex.ITokenIdProvider m_tokenIdProvider = new SyntaxEditorStepBroTokenIdProvider();

        public ActiproLex.ITokenIdProvider TokenIdProvider { get { return m_tokenIdProvider; } }

        public string Key { get { return "StepBroLexer"; } }

        public TextRange Parse(TextSnapshotRange snapshotRange, ActiproLex.ILexerTarget parseTarget)
        {
            throw new NotImplementedException();
            //if (parseTarget.HasInitialContext)
            //{
            //}
            //snapshotRange.Snapshot.
            //return new Antlr.AntlrInputStream(snapshotRange);
        }

        internal Antlr.AntlrInputStream GetParserStream(int startLine, int startColumn, int endLine, int endColumn)
        {
            var text = "";// m_editor.GetText(startLine, startColumn, endLine, endColumn);
            System.Diagnostics.Debug.WriteLine($"Parser stream ([{startLine}, {startColumn}] - [{endLine}, {endColumn}]:");
            System.Diagnostics.Debug.WriteLine(text);
            return new Antlr.AntlrInputStream(text);
        }

    }
}
