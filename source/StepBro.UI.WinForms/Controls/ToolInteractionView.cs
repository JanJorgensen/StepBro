using StepBro.HostSupport.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.Controls
{
    public partial class ToolInteractionView : UserControl
    {
        private bool m_handlingCheck = false;

        public ToolInteractionView()
        {
            InitializeComponent();
        }

        private ToolsInteractionModel Model { get { return this.DataContext as ToolsInteractionModel; } }

        public event EventHandler TextCommandToolSelected;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.Model == null) return;

            this.Model.Synchronize();       // Make sure everything is updated.
            int i = 0;
            m_handlingCheck = true;
            int selectedIndex = 0;
            var sortedList = new List<ToolsInteractionModel.SelectableTool>(this.Model.SelectableTools);
            sortedList.Sort((t1, t2) => (t1.HasTextCommandInput != t2.HasTextCommandInput) ? (t1.HasTextCommandInput ? -1 : 1) : String.Compare(t1.PresentationName, t2.PresentationName));
            foreach (var tool in sortedList)
            {
                ListViewItem item = new ListViewItem("");   // First column is checkbox; so no text.
                item.ForeColor = tool.HasTextCommandInput ? Color.OrangeRed : Color.Black;
                item.SubItems.Add(tool.PresentationName);   // Second column is the variable name.
                if (tool.Equals(this.Model.CurrentTextCommandTool))
                {
                    item.Checked = true;
                    selectedIndex = i;
                }
                item.Tag = tool;
                listViewToolSelection.Items.Add(item);
                i++;
            }
            m_handlingCheck = false;
            listViewToolSelection.SelectedIndices.Clear();
            listViewToolSelection.SelectedIndices.Add(selectedIndex);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.ResizePanelControls();
        }

        private void ResizePanelControls()
        {
            if (this.Size.Width != 0)
            {
                foreach (Control c in flowLayoutPanel.Controls)
                {
                    c.Width = flowLayoutPanel.ClientSize.Width - (c.Margin.Left + c.Margin.Right);
                }
            }
        }

        private void listViewToolSelection_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (m_handlingCheck) return;
            m_handlingCheck = true;
            try
            {
                if (e.Item.Checked)
                {
                    var tool = e.Item.Tag as ToolsInteractionModel.SelectableTool;
                    if (tool.HasTextCommandInput)
                    {
                        this.Model.CurrentTextCommandTool = tool;

                        int i = 0;
                        int selectedIndex = 0;
                        foreach (var item in listViewToolSelection.Items)
                        {
                            if (item.Equals(e.Item))
                            {
                                selectedIndex = i;
                            }
                            else
                            {
                                ((ListViewItem)item).Checked = false;
                            }
                            i++;
                        }
                        listViewToolSelection.SelectedItems.Clear();
                        listViewToolSelection.SelectedIndices.Add(selectedIndex);
                        this.TextCommandToolSelected?.Invoke(this, new EventArgs());
                    }
                    else
                    {
                        e.Item.Checked = false;  // Set it back, because the it is not a text command tool.
                    }
                }
                else
                {
                    e.Item.Checked = true;  // Set it back, because deactivation is not wanted. Uncheck only when selecting another tool.
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR {ex.Message}");
            }

            m_handlingCheck = false;
        }

        private void listViewToolSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("listViewToolSelection_SelectedIndexChanged " + listViewToolSelection.SelectedIndices.Count.ToString());
            try
            {
                if (listViewToolSelection.SelectedIndices.Count == 1)
                {
                    this.Model.SelectedTool = listViewToolSelection.SelectedItems[0].Tag as ToolsInteractionModel.SelectableTool;
                    this.UpdateFromSelectedTool();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR {ex.Message}");
            }
        }

        private void UpdateFromSelectedTool()
        {
            try
            {
                flowLayoutPanel.Controls.Clear();

                foreach (var proc in this.Model.ListActivatableToolProcedures(this.Model.SelectedTool))
                {
                    Button button = new Button();
                    button.Margin = new Padding(1);
                    button.Name = "buttonFor" + proc.FullName.Replace('.', '_');
                    button.Size = new Size(200, 23);
                    button.Text = proc.Name;
                    button.UseVisualStyleBackColor = true;
                    flowLayoutPanel.Controls.Add(button);
                }
                flowLayoutPanel.Controls.Add(propertyGrid);
                //propertyGrid.Visible = true;
                //propertyGrid.SelectedObject = this.Model.SelectedTool.ToolContainer.Object;

                this.ResizePanelControls();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR {ex.Message}");
            }
        }
    }
}
