namespace StepBro.UI.WinForms.Controls
{
    partial class TestReportOverview
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestReportOverview));
            toolStrip = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            toolStripMenuItemAutoShowNew = new ToolStripMenuItem();
            toolStripComboBoxReports = new ToolStripComboBox();
            toolStripDropDownButtonView = new ToolStripDropDownButton();
            toolStripMenuItemViewSummary = new ToolStripMenuItem();
            toolStripMenuItemViewGroups = new ToolStripMenuItem();
            textBoxType = new TextBox();
            panelInfo = new Panel();
            textBoxTitle = new TextBox();
            labelTitle = new Label();
            labelType = new Label();
            listBox = new ListBox();
            toolStrip.SuspendLayout();
            panelInfo.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1, toolStripComboBoxReports, toolStripDropDownButtonView });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(350, 25);
            toolStrip.TabIndex = 2;
            toolStrip.Text = "toolStrip";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemAutoShowNew });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(31, 22);
            toolStripDropDownButton1.Text = "M";
            // 
            // toolStripMenuItemAutoShowNew
            // 
            toolStripMenuItemAutoShowNew.Checked = true;
            toolStripMenuItemAutoShowNew.CheckOnClick = true;
            toolStripMenuItemAutoShowNew.CheckState = CheckState.Checked;
            toolStripMenuItemAutoShowNew.Name = "toolStripMenuItemAutoShowNew";
            toolStripMenuItemAutoShowNew.Size = new Size(207, 22);
            toolStripMenuItemAutoShowNew.Text = "Automatically Show New";
            toolStripMenuItemAutoShowNew.ToolTipText = "Whether to automatically show new report when one is created.";
            // 
            // toolStripComboBoxReports
            // 
            toolStripComboBoxReports.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripComboBoxReports.Name = "toolStripComboBoxReports";
            toolStripComboBoxReports.Size = new Size(121, 25);
            // 
            // toolStripDropDownButtonView
            // 
            toolStripDropDownButtonView.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonView.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemViewSummary, toolStripMenuItemViewGroups });
            toolStripDropDownButtonView.Image = (Image)resources.GetObject("toolStripDropDownButtonView.Image");
            toolStripDropDownButtonView.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonView.Name = "toolStripDropDownButtonView";
            toolStripDropDownButtonView.Size = new Size(45, 22);
            toolStripDropDownButtonView.Text = "View";
            // 
            // toolStripMenuItemViewSummary
            // 
            toolStripMenuItemViewSummary.Checked = true;
            toolStripMenuItemViewSummary.CheckState = CheckState.Checked;
            toolStripMenuItemViewSummary.Name = "toolStripMenuItemViewSummary";
            toolStripMenuItemViewSummary.Size = new Size(125, 22);
            toolStripMenuItemViewSummary.Text = "Summary";
            toolStripMenuItemViewSummary.Click += toolStripMenuItemViewSummary_Click;
            // 
            // toolStripMenuItemViewGroups
            // 
            toolStripMenuItemViewGroups.Name = "toolStripMenuItemViewGroups";
            toolStripMenuItemViewGroups.Size = new Size(125, 22);
            toolStripMenuItemViewGroups.Text = "Groups";
            toolStripMenuItemViewGroups.Click += toolStripMenuItemViewGroups_Click;
            // 
            // textBoxType
            // 
            textBoxType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxType.Location = new Point(43, 3);
            textBoxType.Name = "textBoxType";
            textBoxType.ReadOnly = true;
            textBoxType.Size = new Size(304, 23);
            textBoxType.TabIndex = 3;
            // 
            // panelInfo
            // 
            panelInfo.Controls.Add(textBoxTitle);
            panelInfo.Controls.Add(labelTitle);
            panelInfo.Controls.Add(textBoxType);
            panelInfo.Controls.Add(labelType);
            panelInfo.Dock = DockStyle.Top;
            panelInfo.Location = new Point(0, 25);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new Size(350, 59);
            panelInfo.TabIndex = 4;
            // 
            // textBoxTitle
            // 
            textBoxTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxTitle.Location = new Point(43, 32);
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.ReadOnly = true;
            textBoxTitle.Size = new Size(304, 23);
            textBoxTitle.TabIndex = 6;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.Location = new Point(3, 35);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(32, 15);
            labelTitle.TabIndex = 5;
            labelTitle.Text = "Title:";
            // 
            // labelType
            // 
            labelType.AutoSize = true;
            labelType.Location = new Point(3, 6);
            labelType.Name = "labelType";
            labelType.Size = new Size(34, 15);
            labelType.TabIndex = 4;
            labelType.Text = "Type:";
            // 
            // listBox
            // 
            listBox.BorderStyle = BorderStyle.FixedSingle;
            listBox.Dock = DockStyle.Fill;
            listBox.DrawMode = DrawMode.OwnerDrawFixed;
            listBox.FormattingEnabled = true;
            listBox.Location = new Point(0, 84);
            listBox.Name = "listBox";
            listBox.Size = new Size(350, 292);
            listBox.TabIndex = 5;
            listBox.DrawItem += listBox_DrawItem;
            listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
            listBox.DoubleClick += listBox_DoubleClick;
            // 
            // TestReportOverview
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(listBox);
            Controls.Add(panelInfo);
            Controls.Add(toolStrip);
            Name = "TestReportOverview";
            Size = new Size(350, 376);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            panelInfo.ResumeLayout(false);
            panelInfo.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ToolStrip toolStrip;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem toolStripMenuItemAutoShowNew;
        private ToolStripComboBox toolStripComboBoxReports;
        private TextBox textBoxType;
        private Panel panelInfo;
        private Label labelType;
        private Label labelTitle;
        private TextBox textBoxTitle;
        private ListBox listBox;
        private ToolStripDropDownButton toolStripDropDownButtonView;
        private ToolStripMenuItem toolStripMenuItemViewSummary;
        private ToolStripMenuItem toolStripMenuItemViewGroups;
    }
}
