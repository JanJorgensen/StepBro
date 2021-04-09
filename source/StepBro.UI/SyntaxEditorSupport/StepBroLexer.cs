using ActiproSoftware.Text;
using System.Text;
using ActiproLex = ActiproSoftware.Text.Lexing;
using Antlr = Antlr4.Runtime;
using SBLexer = StepBro.Core.Parser.Grammar.StepBroLexer;

namespace StepBro.UI.SyntaxEditorSupport
{
    public class StepBroLexer : ActiproLex.ILexer
    {
        public StepBroLexer()
        {
        }

        private readonly ActiproLex.ITokenIdProvider m_tokenIdProvider = new StepBroTokenIdProvider();

        public ActiproLex.ITokenIdProvider TokenIdProvider { get { return m_tokenIdProvider; } }

        public string Key { get { return "StepBro"; } }

        public TextRange Parse(TextSnapshotRange snapshotRange, ActiproLex.ILexerTarget parseTarget)
        {
            //System.Diagnostics.Debug.WriteLine("LexParse " + snapshotRange.ToString());
            int index = snapshotRange.StartOffset;
            int ix = index;
            parseTarget.OnPreParse(ref ix);

            if (parseTarget.HasInitialContext)
            {
            }
            else
            {
                int l = snapshotRange.EndOffset - index;
                ITextSnapshotReader reader = snapshotRange.Snapshot.GetReader(index);
                if (reader.Offset != index) throw new System.Exception("What??!!?");
                StringBuilder text = new StringBuilder();
                var read = reader.ReadText(l);
                text.Append(read);
                //System.Diagnostics.Debug.WriteLine($"Parse read {read.Length} chars: {text.ToString()}");

                var lexer = new SBLexer(new Antlr.AntlrInputStream(text.ToString()));
                var tokens = new Antlr.CommonTokenStream(lexer);
                tokens.Fill();
                foreach (var token in tokens.GetTokens())
                {
                    if (token.Type >= 0)
                    {
                        parseTarget.OnTokenParsed(new SyntaxEditorAntlrToken(token, index, snapshotRange.StartLine.Index), null);
                    }
                }
            }

            parseTarget.OnPostParse(snapshotRange.EndOffset);

            return snapshotRange.TextRange;
        }
    }
}
