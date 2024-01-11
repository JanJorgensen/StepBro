using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace StepBro.UI.WinForms.PanelElements
{
    public partial class FlowTopDown : PanelElementBase
    {
        public FlowTopDown()
        {
            InitializeComponent();
            flowLayoutPanel.FlowDirection = this.Direction;
        }

        protected virtual FlowDirection Direction { get { return FlowDirection.TopDown; } }

        public bool StretchChilds { get; set; } = false;
        public bool SizeToChilds { get; set; } = false;

        protected override bool Setup(PanelElementBase parent, PropertyBlock definition)
        {
            SuspendLayout();
            flowLayoutPanel.SuspendLayout();
            base.Setup(parent, definition);
            foreach (var element in definition)
            {
                if (element.BlockEntryType == PropertyBlockEntryType.Value)
                {
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Flag)
                {
                    var flagField = element as PropertyBlockFlag;
                    if (flagField.Name == nameof(StretchChilds))
                    {
                        StretchChilds = true;
                        SizeToChilds = false;
                    }
                    else if (flagField.Name == nameof(SizeToChilds))
                    {
                        SizeToChilds = true;
                        StretchChilds = false;
                    }
                }
                else if (element.BlockEntryType == PropertyBlockEntryType.Block)
                {
                    if (element.Name == "Controls")
                    {
                        var controlDefinitions = element as PropertyBlock;
                        foreach (var controlDef in controlDefinitions)
                        {
                            if (controlDef.BlockEntryType == PropertyBlockEntryType.Block)
                            {
                                var control = PanelElementBase.Create(this, controlDef as PropertyBlock, this.CoreAccess);
                                if (control != null)
                                {
                                    flowLayoutPanel.Controls.Add(control);
                                    if (this.StretchChilds)
                                    {
                                        ResizeControl(control);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            flowLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
            return true;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            System.Diagnostics.Debug.WriteLine("FlowTopDownWrap.OnLayout " + this.Size.ToString());
            if (this.StretchChilds)
            {
                foreach (Control c in flowLayoutPanel.Controls)
                {
                    ResizeControl(c);
                }
            }
            else if (this.SizeToChilds)
            {
                switch (this.Direction)
                {
                    case FlowDirection.LeftToRight:
                        {
                            var height = 0;
                            foreach (Control c in flowLayoutPanel.Controls)
                            {
                                height = Math.Max(height, c.Height);
                            }
                            this.Height = height + (flowLayoutPanel.Margin.Top + flowLayoutPanel.Margin.Bottom);
                        }
                        break;
                    case FlowDirection.TopDown:
                        break;
                    case FlowDirection.RightToLeft:
                        break;
                    case FlowDirection.BottomUp:
                        break;
                    default:
                        break;
                }
            }
        }

        private void ResizeControl(Control control)
        {
            switch (this.Direction)
            {
                case FlowDirection.LeftToRight:
                    control.Size = new Size(control.Width, flowLayoutPanel.ClientSize.Height - (flowLayoutPanel.Margin.Top + flowLayoutPanel.Margin.Bottom));
                    break;
                case FlowDirection.TopDown:
                    control.Size = new Size(flowLayoutPanel.ClientSize.Width - (flowLayoutPanel.Margin.Left + flowLayoutPanel.Margin.Right), control.Height);
                    break;
                case FlowDirection.RightToLeft:
                case FlowDirection.BottomUp:
                default:
                    throw new NotImplementedException();
            }
        }

        //protected override void OnSizeChanged(EventArgs e)
        //{
        //    base.OnSizeChanged(e);
        //    System.Diagnostics.Debug.WriteLine("FlowTopDownWrap.OnSizeChanged " + this.Size.ToString());
        //    if (this.StretchChilds)
        //    {
        //        foreach (Control c in flowLayoutPanel.Controls)
        //        {
        //            c.Size = new Size(flowLayoutPanel.ClientSize.Width - (flowLayoutPanel.Margin.Left + flowLayoutPanel.Margin.Right), c.Height);
        //        }
        //    }
        //}
    }

    public class FlowLeftRight : FlowTopDown
    {
        public FlowLeftRight() : base()
        {
            this.SizeToChilds = true;
            this.StretchChilds = false;
        }
        protected override FlowDirection Direction { get { return FlowDirection.LeftToRight; } }
    }
}
