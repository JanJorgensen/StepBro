namespace StepBro.UI.WinForms.Test
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControlMain = new TabControl();
            tabPageCustomPanel = new TabPage();
            customPanelContainer = new CustomPanelContainer();
            panelTop = new Panel();
            buttonLoadFile = new Button();
            splitContainerMain = new SplitContainer();
            listViewLog = new ListView();
            columnHeaderTime = new ColumnHeader();
            columnHeaderType = new ColumnHeader();
            columnHeaderLocation = new ColumnHeader();
            columnHeaderText = new ColumnHeader();
            timerLogUpdate = new System.Windows.Forms.Timer(components);
            tabControlMain.SuspendLayout();
            tabPageCustomPanel.SuspendLayout();
            panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            SuspendLayout();
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabPageCustomPanel);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Location = new Point(0, 0);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(443, 550);
            tabControlMain.TabIndex = 0;
            // 
            // tabPageCustomPanel
            // 
            tabPageCustomPanel.Controls.Add(customPanelContainer);
            tabPageCustomPanel.Location = new Point(4, 24);
            tabPageCustomPanel.Name = "tabPageCustomPanel";
            tabPageCustomPanel.Padding = new Padding(3);
            tabPageCustomPanel.Size = new Size(435, 522);
            tabPageCustomPanel.TabIndex = 0;
            tabPageCustomPanel.Text = "Custom Panel";
            tabPageCustomPanel.UseVisualStyleBackColor = true;
            // 
            // customPanelContainer
            // 
            customPanelContainer.Dock = DockStyle.Fill;
            customPanelContainer.Location = new Point(3, 3);
            customPanelContainer.Name = "customPanelContainer";
            customPanelContainer.Size = new Size(429, 516);
            customPanelContainer.TabIndex = 0;
            // 
            // panelTop
            // 
            panelTop.Controls.Add(buttonLoadFile);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1037, 30);
            panelTop.TabIndex = 1;
            // 
            // buttonLoadFile
            // 
            buttonLoadFile.Location = new Point(12, 3);
            buttonLoadFile.Name = "buttonLoadFile";
            buttonLoadFile.Size = new Size(75, 23);
            buttonLoadFile.TabIndex = 0;
            buttonLoadFile.Text = "Load File";
            buttonLoadFile.UseVisualStyleBackColor = true;
            buttonLoadFile.Click += buttonLoadFile_Click;
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = DockStyle.Fill;
            splitContainerMain.Location = new Point(0, 30);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(tabControlMain);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(listViewLog);
            splitContainerMain.Size = new Size(1037, 550);
            splitContainerMain.SplitterDistance = 443;
            splitContainerMain.TabIndex = 2;
            // 
            // listViewLog
            // 
            listViewLog.Columns.AddRange(new ColumnHeader[] { columnHeaderTime, columnHeaderType, columnHeaderLocation, columnHeaderText });
            listViewLog.Dock = DockStyle.Fill;
            listViewLog.Location = new Point(0, 0);
            listViewLog.Name = "listViewLog";
            listViewLog.Size = new Size(590, 550);
            listViewLog.TabIndex = 0;
            listViewLog.UseCompatibleStateImageBehavior = false;
            listViewLog.View = View.Details;
            // 
            // columnHeaderTime
            // 
            columnHeaderTime.Text = "Time";
            // 
            // columnHeaderType
            // 
            columnHeaderType.Text = "Type";
            // 
            // columnHeaderLocation
            // 
            columnHeaderLocation.Text = "Location";
            columnHeaderLocation.Width = 180;
            // 
            // columnHeaderText
            // 
            columnHeaderText.Text = "Text";
            columnHeaderText.Width = 500;
            // 
            // timerLogUpdate
            // 
            timerLogUpdate.Enabled = true;
            timerLogUpdate.Interval = 250;
            timerLogUpdate.Tick += timerLogUpdate_Tick;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1037, 580);
            Controls.Add(splitContainerMain);
            Controls.Add(panelTop);
            Name = "FormMain";
            Text = "StepBro WinForms Test";
            tabControlMain.ResumeLayout(false);
            tabPageCustomPanel.ResumeLayout(false);
            panelTop.ResumeLayout(false);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControlMain;
        private TabPage tabPageCustomPanel;
        private CustomPanelContainer customPanelContainer;
        private Panel panelTop;
        private Button buttonLoadFile;
        private SplitContainer splitContainerMain;
        private ListView listViewLog;
        private System.Windows.Forms.Timer timerLogUpdate;
        private ColumnHeader columnHeaderTime;
        private ColumnHeader columnHeaderType;
        private ColumnHeader columnHeaderLocation;
        private ColumnHeader columnHeaderText;
    }
}