using ActiproSoftware.Text;
using ActiproSoftware.Text.Parsing;
using ActiproSoftware.Text.Parsing.Implementation;

namespace StepBro.UI.SyntaxEditorSupport
{
    public class StepBroParser : ParserBase
    {
        public StepBroParser() : base("StepBro") { }

        public override IParseData Parse(IParseRequest request)
        {
            var lexer = request.Language.GetLexer();
            var data = new StepBroParseData();
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
