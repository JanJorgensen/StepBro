using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.SimpleWorkbench
{
    public partial class UserInteractionTextSectionPanel : UserControl
    {
        public UserInteractionTextSectionPanel()
        {
            InitializeComponent();

            labelHeader.Visible = false;
            labelText.Text = "First,\r\nSecond,\r\nThird and actually all the rest.";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public void SetHeader(string header)
        {
            labelHeader.Text = header;
            labelHeader.Visible = !String.IsNullOrEmpty(header);
        }

        public void SetText(string text)
        {
            labelText.Text = text;
            this.Height = labelText.Bottom + 5;
        }
    }
}
