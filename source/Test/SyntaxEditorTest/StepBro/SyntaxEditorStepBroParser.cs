using ActiproSoftware.Text;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Parsing.Implementation;

namespace StepBro.Core.Parser
{
    public class SyntaxEditorStepBroParser : ParserBase
    {
        public SyntaxEditorStepBroParser() : base("StepBro") { }

        public override IParseData Parse(IParseRequest request)
        {
            var lexer = request.Language.GetLexer();
            var data = new SyntaxEditorStepBroParseData();
            var snapshot = request.Snapshot;
            if (snapshot != null)
            {
                //ITextSnapshotReader reader = request.Snapshot.GetReader(0);

                //while (!reader.IsAtSnapshotEnd)
                //{
                //    var read = reader.ReadText(1000);
                //    System.Diagnostics.Debug.WriteLine($"Parse read {read.Length} chars");
                //}
            }

            return data;
        }
    }
}
