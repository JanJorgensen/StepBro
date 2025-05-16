namespace StepBro.UI.WinForms.Controls
{
    partial class ToolInteractionView
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
            listViewToolSelection = new ListView();
            column = new ColumnHeader();
            columnTool = new ColumnHeader();
            propertyGrid = new PropertyGrid();
            flowLayoutPanel = new FlowLayoutPanel();
            button1 = new Button();
            button2 = new Button();
            splitContainer = new SplitContainer();
            flowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // listViewToolSelection
            // 
            listViewToolSelection.CheckBoxes = true;
            listViewToolSelection.Columns.AddRange(new ColumnHeader[] { column, columnTool });
            listViewToolSelection.Dock = DockStyle.Top;
            listViewToolSelection.FullRowSelect = true;
            listViewToolSelection.Location = new Point(0, 0);
            listViewToolSelection.MultiSelect = false;
            listViewToolSelection.Name = "listViewToolSelection";
            listViewToolSelection.ShowGroups = false;
            listViewToolSelection.Size = new Size(347, 177);
            listViewToolSelection.TabIndex = 0;
            listViewToolSelection.UseCompatibleStateImageBehavior = false;
            listViewToolSelection.View = View.Details;
            listViewToolSelection.ItemChecked += listViewToolSelection_ItemChecked;
            listViewToolSelection.SelectedIndexChanged += listViewToolSelection_SelectedIndexChanged;
            // 
            // column
            // 
            column.Text = "";
            column.Width = 20;
            // 
            // columnTool
            // 
            columnTool.Text = "Tool";
            columnTool.Width = 200;
            // 
            // propertyGrid
            // 
            propertyGrid.BackColor = SystemColors.Control;
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.HelpVisible = false;
            propertyGrid.Location = new Point(0, 177);
            propertyGrid.Margin = new Padding(1);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.PropertySort = PropertySort.Alphabetical;
            propertyGrid.Size = new Size(347, 323);
            propertyGrid.TabIndex = 0;
            propertyGrid.ToolbarVisible = false;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.AutoScroll = true;
            flowLayoutPanel.Controls.Add(button1);
            flowLayoutPanel.Controls.Add(button2);
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.Location = new Point(0, 0);
            flowLayoutPanel.Margin = new Padding(2);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(252, 500);
            flowLayoutPanel.TabIndex = 2;
            flowLayoutPanel.WrapContents = false;
            flowLayoutPanel.Resize += flowLayoutPanel_Resize;
            // 
            // button1
            // 
            button1.Location = new Point(1, 1);
            button1.Margin = new Padding(1);
            button1.Name = "button1";
            button1.Size = new Size(208, 23);
            button1.TabIndex = 1;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(1, 26);
            button2.Margin = new Padding(1);
            button2.Name = "button2";
            button2.Size = new Size(208, 23);
            button2.TabIndex = 2;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(propertyGrid);
            splitContainer.Panel1.Controls.Add(listViewToolSelection);
            splitContainer.Panel1MinSize = 250;
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(flowLayoutPanel);
            splitContainer.Panel2MinSize = 200;
            splitContainer.Size = new Size(603, 500);
            splitContainer.SplitterDistance = 347;
            splitContainer.TabIndex = 3;
            // 
            // ToolInteractionView
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(splitContainer);
            Name = "ToolInteractionView";
            Size = new Size(603, 500);
            flowLayoutPanel.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListView listViewToolSelection;
        private ColumnHeader column;
        private ColumnHeader columnTool;
        private PropertyGrid propertyGrid;
        private FlowLayoutPanel flowLayoutPanel;
        private Button button1;
        private Button button2;
        private SplitContainer splitContainer;
    }
}
