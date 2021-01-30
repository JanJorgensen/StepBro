using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Parser
{
    public class SyntaxEditorStepBroSyntaxLanguage : SyntaxLanguage
    {
        public SyntaxEditorStepBroSyntaxLanguage() : base("StepBro")
        {
            this.RegisterParser(new SyntaxEditorStepBroParser());
            this.RegisterLexer(new SyntaxEditorStepBroLexer());
            this.RegisterService(new SyntaxEditorStepBroTokenTaggerProvider(new SyntaxEditorStepBroClassificationTypeProvider()));


            //this.RegisterService(new TokenTaggerProvider<MyCustomTokenTagger>());
            //this.RegisterLineCommenter(new LineBasedLineCommenter() { StartDelimiter = "//" });
        }
    }
}
