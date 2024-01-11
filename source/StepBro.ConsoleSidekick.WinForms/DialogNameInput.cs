using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.ConsoleSidekick.WinForms
{
    public partial class DialogNameInput : Form
    {
        public DialogNameInput()
        {
            InitializeComponent();
        }

        public DialogNameInput(String caption, string description, string defaultvalue)
        {
            InitializeComponent();
            this.Text = caption;
            labelDescription.Text = description;
            textBoxString.Text = defaultvalue;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            textBoxString.Focus();
            textBoxString.SelectAll();
        }

        public string Value { get { return textBoxString.Text; } set { textBoxString.Text = Value; } }
    }
}
