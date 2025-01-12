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
            flowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // listViewToolSelection
            // 
            listViewToolSelection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listViewToolSelection.CheckBoxes = true;
            listViewToolSelection.Columns.AddRange(new ColumnHeader[] { column, columnTool });
            listViewToolSelection.FullRowSelect = true;
            listViewToolSelection.Location = new Point(0, 0);
            listViewToolSelection.MultiSelect = false;
            listViewToolSelection.Name = "listViewToolSelection";
            listViewToolSelection.ShowGroups = false;
            listViewToolSelection.Size = new Size(173, 131);
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
            columnTool.Width = 140;
            // 
            // propertyGrid
            // 
            propertyGrid.HelpVisible = false;
            propertyGrid.Location = new Point(1, 51);
            propertyGrid.Margin = new Padding(1);
            propertyGrid.Name = "propertyGrid";
            propertyGrid.Size = new Size(208, 196);
            propertyGrid.TabIndex = 0;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.Visible = false;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel.Controls.Add(button1);
            flowLayoutPanel.Controls.Add(button2);
            flowLayoutPanel.Controls.Add(propertyGrid);
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.Location = new Point(0, 133);
            flowLayoutPanel.Margin = new Padding(2);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(173, 285);
            flowLayoutPanel.TabIndex = 2;
            flowLayoutPanel.WrapContents = false;
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
            // ToolInteractionView
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(flowLayoutPanel);
            Controls.Add(listViewToolSelection);
            Name = "ToolInteractionView";
            Size = new Size(173, 422);
            flowLayoutPanel.ResumeLayout(false);
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
    }
}
