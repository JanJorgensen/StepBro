namespace StepBro.UI.WinForms.Controls
{
    partial class ChronoListView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelHorizontal = new Panel();
            hScrollBar = new HScrollBar();
            panelFillRight = new Panel();
            vScrollBar = new VScrollBar();
            viewPort = new ChronoListViewPort();
            panelHorizontal.SuspendLayout();
            SuspendLayout();
            // 
            // panelHorizontal
            // 
            panelHorizontal.Controls.Add(hScrollBar);
            panelHorizontal.Controls.Add(panelFillRight);
            panelHorizontal.Dock = DockStyle.Bottom;
            panelHorizontal.Location = new Point(0, 125);
            panelHorizontal.Name = "panelHorizontal";
            panelHorizontal.Size = new Size(510, 25);
            panelHorizontal.TabIndex = 0;
            // 
            // hScrollBar
            // 
            hScrollBar.Dock = DockStyle.Bottom;
            hScrollBar.Location = new Point(0, 8);
            hScrollBar.Name = "hScrollBar";
            hScrollBar.Size = new Size(485, 17);
            hScrollBar.TabIndex = 1;
            // 
            // panelFillRight
            // 
            panelFillRight.Dock = DockStyle.Right;
            panelFillRight.Location = new Point(485, 0);
            panelFillRight.Name = "panelFillRight";
            panelFillRight.Size = new Size(25, 25);
            panelFillRight.TabIndex = 0;
            // 
            // vScrollBar
            // 
            vScrollBar.Dock = DockStyle.Right;
            vScrollBar.Location = new Point(493, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new Size(17, 125);
            vScrollBar.TabIndex = 1;
            // 
            // viewPort
            // 
            viewPort.BackColor = Color.Black;
            viewPort.Dock = DockStyle.Fill;
            viewPort.ForeColor = Color.White;
            viewPort.HorizontalScrollPosition = 0;
            viewPort.Location = new Point(0, 0);
            viewPort.Name = "viewPort";
            viewPort.Size = new Size(493, 125);
            viewPort.TabIndex = 2;
            viewPort.Text = "ViewPort";
            viewPort.Click += chronoListViewPort_Click;
            // 
            // ChronoListView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(viewPort);
            Controls.Add(vScrollBar);
            Controls.Add(panelHorizontal);
            Name = "ChronoListView";
            Size = new Size(510, 150);
            panelHorizontal.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHorizontal;
        private Panel panelFillRight;
        private HScrollBar hScrollBar;
        private VScrollBar vScrollBar;
        private ChronoListViewPort viewPort;
    }
}
