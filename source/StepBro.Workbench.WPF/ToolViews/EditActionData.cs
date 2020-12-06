using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace StepBro.Workbench
{
    public class EditActionData
    {
        public IEditAction Action { get; set; }
        public string Category { get; set; }
        public string Key { get; set; }
        public string Name
        {
            get
            {
                return this.Action.Key;
            }
        }
    }
}