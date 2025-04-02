namespace StepBro.UI.WinForms.Dialogs
{
    partial class DialogOpenProjectFile
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonOK = new Button();
            buttonCancel = new Button();
            label1 = new Label();
            listBoxRecent = new ListBox();
            listBoxFavorites = new ListBox();
            label2 = new Label();
            labelFavorites = new Label();
            panelBack = new Panel();
            checkBoxSameFolder = new CheckBox();
            labelFilePath = new Label();
            label3 = new Label();
            buttonSelectFileOnDisk = new Button();
            buttonAddToFavorites = new Button();
            panelBack.SuspendLayout();
            SuspendLayout();
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Enabled = false;
            buttonOK.Location = new Point(707, 466);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 0;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(788, 466);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(201, 23);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel / Do not open file";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(11, 8);
            label1.Name = "label1";
            label1.Size = new Size(161, 50);
            label1.TabIndex = 2;
            label1.Text = "StepBro";
            // 
            // listBoxRecent
            // 
            listBoxRecent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBoxRecent.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            listBoxRecent.FormattingEnabled = true;
            listBoxRecent.ItemHeight = 17;
            listBoxRecent.Location = new Point(11, 122);
            listBoxRecent.Name = "listBoxRecent";
            listBoxRecent.Size = new Size(480, 293);
            listBoxRecent.TabIndex = 3;
            listBoxRecent.SelectedIndexChanged += listBoxRecent_SelectedIndexChanged;
            listBoxRecent.DoubleClick += listBoxRecent_DoubleClick;
            listBoxRecent.KeyDown += listBoxRecent_KeyDown;
            // 
            // listBoxFavorites
            // 
            listBoxFavorites.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxFavorites.Enabled = false;
            listBoxFavorites.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            listBoxFavorites.FormattingEnabled = true;
            listBoxFavorites.ItemHeight = 17;
            listBoxFavorites.Location = new Point(509, 122);
            listBoxFavorites.Name = "listBoxFavorites";
            listBoxFavorites.Size = new Size(480, 293);
            listBoxFavorites.TabIndex = 4;
            listBoxFavorites.SelectedIndexChanged += listBoxFavorites_SelectedIndexChanged;
            listBoxFavorites.DoubleClick += listBoxFavorites_DoubleClick;
            listBoxFavorites.KeyDown += listBoxFavorites_KeyDown;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(11, 89);
            label2.Name = "label2";
            label2.Size = new Size(128, 30);
            label2.TabIndex = 5;
            label2.Text = "Recent Files";
            // 
            // labelFavorites
            // 
            labelFavorites.AutoSize = true;
            labelFavorites.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelFavorites.Location = new Point(509, 89);
            labelFavorites.Name = "labelFavorites";
            labelFavorites.Size = new Size(100, 30);
            labelFavorites.TabIndex = 5;
            labelFavorites.Text = "Favorites";
            // 
            // panelBack
            // 
            panelBack.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelBack.BackColor = SystemColors.Control;
            panelBack.Controls.Add(checkBoxSameFolder);
            panelBack.Controls.Add(label1);
            panelBack.Controls.Add(labelFilePath);
            panelBack.Controls.Add(label3);
            panelBack.Controls.Add(buttonSelectFileOnDisk);
            panelBack.Controls.Add(buttonAddToFavorites);
            panelBack.Controls.Add(listBoxFavorites);
            panelBack.Controls.Add(labelFavorites);
            panelBack.Controls.Add(listBoxRecent);
            panelBack.Controls.Add(label2);
            panelBack.Controls.Add(buttonOK);
            panelBack.Controls.Add(buttonCancel);
            panelBack.Location = new Point(4, 4);
            panelBack.Name = "panelBack";
            panelBack.Padding = new Padding(8);
            panelBack.Size = new Size(1000, 500);
            panelBack.TabIndex = 6;
            // 
            // checkBoxSameFolder
            // 
            checkBoxSameFolder.AutoSize = true;
            checkBoxSameFolder.Location = new Point(284, 470);
            checkBoxSameFolder.Name = "checkBoxSameFolder";
            checkBoxSameFolder.Size = new Size(211, 19);
            checkBoxSameFolder.TabIndex = 10;
            checkBoxSameFolder.Text = "... from same folder as selected file.";
            checkBoxSameFolder.UseVisualStyleBackColor = true;
            checkBoxSameFolder.Visible = false;
            // 
            // labelFilePath
            // 
            labelFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelFilePath.ForeColor = Color.SteelBlue;
            labelFilePath.Location = new Point(11, 418);
            labelFilePath.Name = "labelFilePath";
            labelFilePath.Size = new Size(978, 26);
            labelFilePath.TabIndex = 9;
            labelFilePath.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label3.Font = new Font("Segoe UI", 21.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(11, 16);
            label3.Name = "label3";
            label3.Size = new Size(978, 58);
            label3.TabIndex = 8;
            label3.Text = "Open File / Project";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // buttonSelectFileOnDisk
            // 
            buttonSelectFileOnDisk.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonSelectFileOnDisk.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonSelectFileOnDisk.Location = new Point(11, 447);
            buttonSelectFileOnDisk.Name = "buttonSelectFileOnDisk";
            buttonSelectFileOnDisk.Size = new Size(267, 42);
            buttonSelectFileOnDisk.TabIndex = 7;
            buttonSelectFileOnDisk.Text = "Select File On Disk";
            buttonSelectFileOnDisk.UseVisualStyleBackColor = true;
            buttonSelectFileOnDisk.Click += buttonSelectFileOnDisk_Click;
            // 
            // buttonAddToFavorites
            // 
            buttonAddToFavorites.Location = new Point(366, 93);
            buttonAddToFavorites.Name = "buttonAddToFavorites";
            buttonAddToFavorites.Size = new Size(125, 23);
            buttonAddToFavorites.TabIndex = 6;
            buttonAddToFavorites.Text = "Add to favorites";
            buttonAddToFavorites.UseVisualStyleBackColor = true;
            buttonAddToFavorites.Visible = false;
            buttonAddToFavorites.Click += buttonAddToFavorites_Click;
            // 
            // DialogOpenProjectFile
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = SystemColors.WindowFrame;
            CancelButton = buttonCancel;
            ClientSize = new Size(1008, 508);
            ControlBox = false;
            Controls.Add(panelBack);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MdiChildrenMinimizedAnchorBottom = false;
            MinimizeBox = false;
            Name = "DialogOpenProjectFile";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            panelBack.ResumeLayout(false);
            panelBack.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button buttonOK;
        private Button buttonCancel;
        private Label label1;
        private ListBox listBoxRecent;
        private ListBox listBoxFavorites;
        private Label label2;
        private Label labelFavorites;
        private Panel panelBack;
        private Button buttonAddToFavorites;
        private Button buttonSelectFileOnDisk;
        private Label label3;
        private Label labelFilePath;
        private CheckBox checkBoxSameFolder;
    }
}