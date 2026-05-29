using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace StepBro.SimpleWorkbench
{
    public partial class UserInteractionTextSectionPanel : UserControl
    {
        public UserInteractionTextSectionPanel()
        {
            InitializeComponent();

            labelHeader.Visible = false;
            //labelText.Text = "First,\r\nSecond,\r\nThird and actually all the rest.";
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
            labelHeader.Visible = !String.IsNullOrEmpty(labelHeader.Text);
            labelText.Text = text;
            labelText.Visible = !String.IsNullOrEmpty(text);
            this.Height = (labelText.Visible ? labelText.Bottom : labelHeader.Bottom) + 5;
        }
    }
}
