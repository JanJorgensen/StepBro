namespace StepBro.SimpleWorkbench
{
    partial class UserInteractionPanel
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
            flowLayoutPanelSections = new FlowLayoutPanel();
            userInteractionSingleSelectionSectionPanel1 = new UserInteractionSingleSelectionSectionPanel();
            flowLayoutPanelBottom = new FlowLayoutPanel();
            buttonCancel = new Button();
            buttonNo = new Button();
            buttonYes = new Button();
            buttonOK = new Button();
            panelTop = new Panel();
            labelHeader = new Label();
            userInteractionTextSectionPanel1 = new UserInteractionTextSectionPanel();
            flowLayoutPanelSections.SuspendLayout();
            flowLayoutPanelBottom.SuspendLayout();
            panelTop.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanelSections
            // 
            flowLayoutPanelSections.Controls.Add(userInteractionTextSectionPanel1);
            flowLayoutPanelSections.Controls.Add(userInteractionSingleSelectionSectionPanel1);
            flowLayoutPanelSections.Dock = DockStyle.Fill;
            flowLayoutPanelSections.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelSections.Location = new Point(0, 36);
            flowLayoutPanelSections.Margin = new Padding(3, 3, 3, 20);
            flowLayoutPanelSections.Name = "flowLayoutPanelSections";
            flowLayoutPanelSections.Padding = new Padding(0, 0, 0, 50);
            flowLayoutPanelSections.Size = new Size(369, 192);
            flowLayoutPanelSections.TabIndex = 0;
            flowLayoutPanelSections.WrapContents = false;
            // 
            // userInteractionSingleSelectionSectionPanel1
            // 
            userInteractionSingleSelectionSectionPanel1.Location = new Point(3, 88);
            userInteractionSingleSelectionSectionPanel1.Name = "userInteractionSingleSelectionSectionPanel1";
            userInteractionSingleSelectionSectionPanel1.Size = new Size(363, 78);
            userInteractionSingleSelectionSectionPanel1.TabIndex = 4;
            // 
            // flowLayoutPanelBottom
            // 
            flowLayoutPanelBottom.BackColor = SystemColors.ControlLight;
            flowLayoutPanelBottom.Controls.Add(buttonCancel);
            flowLayoutPanelBottom.Controls.Add(buttonNo);
            flowLayoutPanelBottom.Controls.Add(buttonYes);
            flowLayoutPanelBottom.Controls.Add(buttonOK);
            flowLayoutPanelBottom.Dock = DockStyle.Bottom;
            flowLayoutPanelBottom.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanelBottom.Location = new Point(0, 228);
            flowLayoutPanelBottom.Name = "flowLayoutPanelBottom";
            flowLayoutPanelBottom.Size = new Size(369, 40);
            flowLayoutPanelBottom.TabIndex = 1;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(287, 10);
            buttonCancel.Margin = new Padding(3, 10, 7, 3);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonNo
            // 
            buttonNo.Location = new Point(202, 10);
            buttonNo.Margin = new Padding(3, 10, 7, 3);
            buttonNo.Name = "buttonNo";
            buttonNo.Size = new Size(75, 23);
            buttonNo.TabIndex = 2;
            buttonNo.Text = "No";
            buttonNo.UseVisualStyleBackColor = true;
            buttonNo.Click += buttonNo_Click;
            // 
            // buttonYes
            // 
            buttonYes.Location = new Point(117, 10);
            buttonYes.Margin = new Padding(3, 10, 7, 3);
            buttonYes.Name = "buttonYes";
            buttonYes.Size = new Size(75, 23);
            buttonYes.TabIndex = 1;
            buttonYes.Text = "Yes";
            buttonYes.UseVisualStyleBackColor = true;
            buttonYes.Click += buttonYes_Click;
            // 
            // buttonOK
            // 
            buttonOK.Location = new Point(32, 10);
            buttonOK.Margin = new Padding(3, 10, 7, 3);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 0;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // panelTop
            // 
            panelTop.BackColor = SystemColors.ControlLight;
            panelTop.Controls.Add(labelHeader);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(369, 36);
            panelTop.TabIndex = 2;
            // 
            // labelHeader
            // 
            labelHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelHeader.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelHeader.Location = new Point(3, 4);
            labelHeader.Name = "labelHeader";
            labelHeader.Size = new Size(363, 23);
            labelHeader.TabIndex = 0;
            labelHeader.Text = "User Input";
            // 
            // userInteractionTextSectionPanel1
            // 
            userInteractionTextSectionPanel1.Location = new Point(3, 3);
            userInteractionTextSectionPanel1.Name = "userInteractionTextSectionPanel1";
            userInteractionTextSectionPanel1.Size = new Size(363, 79);
            userInteractionTextSectionPanel1.TabIndex = 5;
            // 
            // UserInteractionPanel
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(flowLayoutPanelSections);
            Controls.Add(flowLayoutPanelBottom);
            Controls.Add(panelTop);
            Name = "UserInteractionPanel";
            Size = new Size(369, 268);
            flowLayoutPanelSections.ResumeLayout(false);
            flowLayoutPanelBottom.ResumeLayout(false);
            panelTop.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanelSections;
        private FlowLayoutPanel flowLayoutPanelBottom;
        private Button buttonCancel;
        private Button buttonNo;
        private Button buttonYes;
        private Button buttonOK;
        private UserInteractionSingleSelectionSectionPanel userInteractionSingleSelectionSectionPanel1;
        private Panel panelTop;
        private Label labelHeader;
        private UserInteractionTextSectionPanel userInteractionTextSectionPanel1;
    }
}
