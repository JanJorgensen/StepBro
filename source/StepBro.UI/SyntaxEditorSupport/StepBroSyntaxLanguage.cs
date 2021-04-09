using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.SyntaxEditorSupport
{
    public class StepBroSyntaxLanguage : SyntaxLanguage
    {
        public StepBroSyntaxLanguage() : base("StepBro")
        {
            this.RegisterParser(new StepBroParser());
            this.RegisterLexer(new StepBroLexer());
            this.RegisterService(new StepBroTokenTaggerProvider(new StepBroClassificationTypeProvider()));


            //this.RegisterService(new TokenTaggerProvider<MyCustomTokenTagger>());
            //this.RegisterLineCommenter(new LineBasedLineCommenter() { StartDelimiter = "//" });
        }
    }
}
