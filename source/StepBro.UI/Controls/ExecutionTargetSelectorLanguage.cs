using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Tagging.Implementation;

namespace StepBro.UI.Controls
{
    public class ExecutionTargetSelectorLanguage : SyntaxLanguage
    {
        public ExecutionTargetSelectorLanguage() : base("ExecutionTargetSelectorLanguage")
        {
            ExecutionTargetSelectorClassificationTypeProvider classificationTypeProvider = new ExecutionTargetSelectorClassificationTypeProvider();
            classificationTypeProvider.RegisterAll();

            this.RegisterLexer(new ExecutionTargetSelectorLexer());
            //this.RegisterService(new ExecutionTargetSelectorTokenTagger());
        }
    }
}
