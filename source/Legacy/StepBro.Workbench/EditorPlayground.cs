using FastColoredTextBoxNS;
using System;

namespace StepBro.Workbench
{
    public partial class EditorPlayground : ToolWindow
    {
        public EditorPlayground()
        {
            this.InitializeComponent();
        }

        private void menuItemClear_Click(object sender, EventArgs e)
        {
            fctb.Clear();
        }

        private void menuItemAdd50Lines_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 50; i++)
            {
                fctb.AppendText($"This is line #{i}. Here are some letters: ABCDEFGHIJKLMNO" + Environment.NewLine);
            }
        }

        private void menuItemAction1_Click(object sender, EventArgs e)
        {
            fctb.InsertTextAndRestoreSelection(new Range(fctb, 10, 5, 13, 5), "XYZ", fctb.DefaultStyle);
        }

        private void menuItemAction2_Click(object sender, EventArgs e)
        {

        }

        private void fctb_Click(object sender, EventArgs e)
        {

        }

        private void fctb_LineInserted(object sender, FastColoredTextBoxNS.LineInsertedEventArgs e)
        {

        }

        private void fctb_LineRemoved(object sender, FastColoredTextBoxNS.LineRemovedEventArgs e)
        {

        }
    }
}
