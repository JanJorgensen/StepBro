using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StepBro.Core.General;

namespace StepBro.Workbench
{
    public partial class ErrorsWindow : ToolWindow
    {
        public ErrorsWindow()
        {
            InitializeComponent();
        }

        private void parsingErrorListView_DoubleClickedLine(object sender, StepBro.Core.Controls.ParsingErrorListView.DoubleClickLineEventArgs args)
        {
            MainForm.Instance.ShowFileEditor(args.File, args.Line, args.Column, args.Column);
        }
    }
}
