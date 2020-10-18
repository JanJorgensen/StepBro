using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Popup menu for autocomplete
    /// </summary>
    [Browsable(false)]
    public class AutocompleteMenu : ToolStripDropDown, IDisposable
    {
        private AutocompleteListView listView;
        public ToolStripControlHost host;
        //public Range Fragment { get; internal set; }
        //public int FragmentStart { get; internal set; }
        //public string EditLine { get; internal set; }
        //public int CursorColumn { get; internal set; }

        /// <summary>
        /// Regex pattern for serach fragment around caret
        /// </summary>
        public string SearchPattern { get; set; }
        /// <summary>
        /// Minimum fragment length for popup
        /// </summary>
        public int MinFragmentLength { get; set; }
        /// <summary>
        /// User selects item
        /// </summary>
        public event EventHandler<SelectingEventArgs> Selecting;
        /// <summary>
        /// It fires after item inserting
        /// </summary>
        public event EventHandler<SelectedEventArgs> Selected;
        /// <summary>
        /// Occurs when popup menu is opening
        /// </summary>
        public new event EventHandler<CancelEventArgs> Opening;
        /// <summary>
        /// Allow TAB for select menu item
        /// </summary>
        public bool AllowTabKey { get { return listView.AllowTabKey; } set { listView.AllowTabKey = value; } }
        /// <summary>
        /// Interval of menu appear (ms)
        /// </summary>
        public int AppearInterval { get { return listView.AppearInterval; } set { listView.AppearInterval = value; } }
        /// <summary>
        /// Sets the max tooltip window size
        /// </summary>
        public Size MaxTooltipSize { get { return listView.MaxToolTipSize; } set { listView.MaxToolTipSize = value; } }
        /// <summary>
        /// Tooltip will perm show and duration will be ignored
        /// </summary>
        public bool AlwaysShowTooltip { get { return listView.AlwaysShowTooltip; } set { listView.AlwaysShowTooltip = value; } }

        /// <summary>
        /// Back color of selected item
        /// </summary>
        [DefaultValue(typeof(Color), "Orange")]
        public Color SelectedColor
        {
            get { return listView.SelectedColor; }
            set { listView.SelectedColor = value; }
        }

        /// <summary>
        /// Border color of hovered item
        /// </summary>
        [DefaultValue(typeof(Color), "Red")]
        public Color HoveredColor
        {
            get { return listView.HoveredColor; }
            set { listView.HoveredColor = value; }
        }

        public AutocompleteMenu(FastColoredTextBox tb)
        {
            // create a new popup and add the list view to it 
            this.AutoClose = false;
            this.AutoSize = false;
            this.Margin = Padding.Empty;
            this.Padding = Padding.Empty;
            this.BackColor = Color.White;
            listView = new AutocompleteListView(tb);
            host = new ToolStripControlHost(listView);
            host.Margin = new Padding(2, 2, 2, 2);
            host.Padding = Padding.Empty;
            host.AutoSize = false;
            host.AutoToolTip = false;
            this.CalcSize();
            base.Items.Add(host);
            listView.Parent = this;
            this.SearchPattern = @"[\w\.]";
            this.MinFragmentLength = 2;

        }

        public new Font Font
        {
            get { return listView.Font; }
            set { listView.Font = value; }
        }

        new internal void OnOpening(CancelEventArgs args)
        {
            if (Opening != null)
                Opening(this, args);
        }

        public new void Close()
        {
            listView.toolTip.Hide(listView);
            base.Close();
        }

        internal void CalcSize()
        {
            host.Size = listView.Size;
            this.Size = new System.Drawing.Size(listView.Size.Width + 4, listView.Size.Height + 4);
        }

        public virtual void OnSelecting()
        {
            listView.OnSelecting();
        }

        public void SelectNext(int shift)
        {
            listView.SelectNext(shift);
        }

        internal void OnSelecting(SelectingEventArgs args)
        {
            if (Selecting != null)
                Selecting(this, args);
        }

        public void OnSelected(SelectedEventArgs args)
        {
            if (Selected != null)
                Selected(this, args);
        }

        public new AutocompleteListView Items
        {
            get { return listView; }
        }

        /// <summary>
        /// Shows popup menu immediately
        /// </summary>
        /// <param name="forced">If True - MinFragmentLength will be ignored</param>
        public void Show(bool forced)
        {
            this.Items.DoAutocomplete(forced);
        }

        /// <summary>
        /// Minimal size of menu
        /// </summary>
        public new Size MinimumSize
        {
            get { return this.Items.MinimumSize; }
            set { this.Items.MinimumSize = value; }
        }

        /// <summary>
        /// Image list of menu
        /// </summary>
        public new ImageList ImageList
        {
            get { return this.Items.ImageList; }
            set { this.Items.ImageList = value; }
        }

        /// <summary>
        /// Tooltip duration (ms)
        /// </summary>
        public int ToolTipDuration
        {
            get { return this.Items.ToolTipDuration; }
            set { this.Items.ToolTipDuration = value; }
        }

        /// <summary>
        /// Tooltip
        /// </summary>
        public ToolTip ToolTip
        {
            get { return this.Items.toolTip; }
            set { this.Items.toolTip = value; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (listView != null && !listView.IsDisposed)
                listView.Dispose();
        }
    }

    [ToolboxItem(false)]
    public class AutocompleteListView : UserControl, IDisposable
    {
        public event EventHandler FocussedItemIndexChanged;

        internal List<AutocompleteItem> visibleItems;
        private readonly List<AutocompleteCreator> autocompleteCreators = new List<AutocompleteCreator>();
        //private IEnumerable<AutocompleteItem> sourceItems = new List<AutocompleteItem>();
        private int focussedItemIndex = 0;
        private readonly int hoveredItemIndex = -1;

        private int ItemHeight
        {
            get { return this.Font.Height + 2; }
        }

        private AutocompleteMenu Menu { get { return this.Parent as AutocompleteMenu; } }

        private int oldItemCount = 0;
        private FastColoredTextBox tb;
        internal ToolTip toolTip = new ToolTip();
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        internal bool AllowTabKey { get; set; }
        public ImageList ImageList { get; set; }
        internal int AppearInterval { get { return timer.Interval; } set { timer.Interval = value; } }
        internal int ToolTipDuration { get; set; }
        internal Size MaxToolTipSize { get; set; }
        internal bool AlwaysShowTooltip
        {
            get { return toolTip.ShowAlways; }
            set { toolTip.ShowAlways = value; }
        }

        public Color SelectedColor { get; set; }
        public Color HoveredColor { get; set; }
        public int FocussedItemIndex
        {
            get { return focussedItemIndex; }
            set
            {
                if (focussedItemIndex != value)
                {
                    focussedItemIndex = value;
                    if (FocussedItemIndexChanged != null)
                        FocussedItemIndexChanged(this, EventArgs.Empty);
                }
            }
        }

        public AutocompleteItem FocussedItem
        {
            get
            {
                if (this.FocussedItemIndex >= 0 && focussedItemIndex < visibleItems.Count)
                    return visibleItems[focussedItemIndex];
                return null;
            }
            set
            {
                this.FocussedItemIndex = visibleItems.IndexOf(value);
            }
        }

        internal AutocompleteListView(FastColoredTextBox tb)
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            base.Font = new Font(FontFamily.GenericSansSerif, 9);
            visibleItems = new List<AutocompleteItem>();
            this.VerticalScroll.SmallChange = this.ItemHeight;
            this.MaximumSize = new Size(this.Size.Width, 180);
            toolTip.ShowAlways = false;
            this.AppearInterval = 500;
            timer.Tick += new EventHandler(this.timer_Tick);
            this.SelectedColor = Color.Orange;
            this.HoveredColor = Color.Red;
            this.ToolTipDuration = 3000;
            toolTip.Popup += this.ToolTip_Popup;

            this.tb = tb;

            tb.KeyDown += new KeyEventHandler(this.tb_KeyDown);
            tb.SelectionChanged += new EventHandler(this.tb_SelectionChanged);
            tb.KeyPressed += new KeyPressEventHandler(this.tb_KeyPressed);

            Form form = tb.FindForm();
            if (form != null)
            {
                form.LocationChanged += delegate { this.SafetyClose(); };
                form.ResizeBegin += delegate { this.SafetyClose(); };
                form.FormClosing += delegate { this.SafetyClose(); };
                form.LostFocus += delegate { this.SafetyClose(); };
            }

            tb.LostFocus += (o, e) =>
            {
                if (this.Menu != null && !this.Menu.IsDisposed)
                    if (!this.Menu.Focused)
                        this.SafetyClose();
            };

            tb.Scroll += delegate { this.SafetyClose(); };

            this.VisibleChanged += (o, e) =>
            {
                if (this.Visible)
                    this.DoSelectedVisible();
            };
        }

        public void SetCreator(AutocompleteCreator creator)
        {
            autocompleteCreators.Add(creator);
        }

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            if (this.MaxToolTipSize.Height > 0 && this.MaxToolTipSize.Width > 0)
                e.ToolTipSize = this.MaxToolTipSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (toolTip != null)
            {
                toolTip.Popup -= this.ToolTip_Popup;
                toolTip.Dispose();
            }
            if (tb != null)
            {
                tb.KeyDown -= this.tb_KeyDown;
                tb.KeyPressed -= this.tb_KeyPressed;
                tb.SelectionChanged -= this.tb_SelectionChanged;
            }

            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= this.timer_Tick;
                timer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SafetyClose()
        {
            if (this.Menu != null && !this.Menu.IsDisposed)
                this.Menu.Close();
        }

        private void tb_KeyPressed(object sender, KeyPressEventArgs e)
        {
            bool backspaceORdel = e.KeyChar == '\b' || e.KeyChar == 0xff;

            /*
            if (backspaceORdel)
                prevSelection = tb.Selection.Start;*/

            if (this.Menu.Visible && !backspaceORdel)
                this.DoAutocomplete(false);
            else
                this.ResetTimer(timer);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            this.DoAutocomplete(false);
        }

        private void ResetTimer(System.Windows.Forms.Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        internal void DoAutocomplete()
        {
            this.DoAutocomplete(false);
        }

        internal void DoAutocomplete(bool forced)
        {
            if (!this.Menu.Enabled)
            {
                this.Menu.Close();
                return;
            }

            visibleItems.Clear();
            this.FocussedItemIndex = 0;
            this.VerticalScroll.Value = 0;
            //some magic for update scrolls
            this.AutoScrollMinSize -= new Size(1, 0);
            this.AutoScrollMinSize += new Size(1, 0);
            //get fragment around caret
            Range fragment = tb.Selection.GetFragment(this.Menu.SearchPattern);
            var line = tb.GetLine(tb.Selection.FromLine).Text;
            var column = tb.Selection.Start.iChar;
            var text = fragment.Text;
            var fragmentStart = fragment.Start.iChar;
            System.Diagnostics.Debug.WriteLine($"Autocomplete: \"{text}\", {fragmentStart}, \"{line}\", {column}");
            //calc screen point for popup menu
            Point point = tb.PlaceToPoint(fragment.End);
            point.Offset(2, tb.CharHeight);
            //
            if (forced || (text.Length >= this.Menu.MinFragmentLength
                && tb.Selection.IsEmpty /*pops up only if selected range is empty*/
                && (tb.Selection.Start > fragment.Start || text.Length == 0/*pops up only if caret is after first letter*/)))
            {
                //this.Menu.Fragment = fragment;
                //this.Menu.FragmentStart = fragmentStart;
                //this.Menu.EditLine = line;
                //this.Menu.CursorColumn = column;
                bool foundSelected = false;
                //build popup menu
                var items = new List<AutocompleteItem>();
                foreach (var creator in autocompleteCreators)
                {
                    foreach (var item in creator.CreateItems(tb.Selection.FromLine, tb.Selection.Start.iChar))
                    {
                        items.Add(item);
                    }
                }

                // Sorting: MatchWeight first. If MatchWeight is the same, compare the text.
                items.Sort((f, s) => (f.MatchWeight == s.MatchWeight) ? String.Compare(f.Text, s.Text) : ((f.MatchWeight < s.MatchWeight) ? -1 : 1));
                var maxItem = items.OrderByDescending(i => i.MatchWeight).FirstOrDefault();
                if (maxItem != null)
                {
                    foundSelected = true;
                    this.FocussedItemIndex = items.IndexOf(maxItem);
                }
                visibleItems.AddRange(items);

                if (foundSelected)
                {
                    this.AdjustScroll();
                    this.DoSelectedVisible();
                }
            }

            //show popup menu
            if (this.Count > 0)
            {
                if (!this.Menu.Visible)
                {
                    CancelEventArgs args = new CancelEventArgs();
                    this.Menu.OnOpening(args);
                    if (!args.Cancel)
                        this.Menu.Show(tb, point);
                }

                this.DoSelectedVisible();
                this.Invalidate();
            }
            else
                this.Menu.Close();
        }

        private void tb_SelectionChanged(object sender, EventArgs e)
        {
            /*
            FastColoredTextBox tb = sender as FastColoredTextBox;
            
            if (Math.Abs(prevSelection.iChar - tb.Selection.Start.iChar) > 1 ||
                        prevSelection.iLine != tb.Selection.Start.iLine)
                Menu.Close();
            prevSelection = tb.Selection.Start;*/
            if (this.Menu.Visible)
            {
                bool needClose = false;

                if (!tb.Selection.IsEmpty)
                {
                    needClose = true;
                }
                else
                {
                    if (!this.Menu.Fragment.Contains(tb.Selection.Start))
                    {
                        if (tb.Selection.Start.iLine == this.Menu.Fragment.End.iLine && tb.Selection.Start.iChar == this.Menu.Fragment.End.iChar + 1)
                        {
                            //user press key at end of fragment
                            char c = tb.Selection.CharBeforeStart;
                            if (!Regex.IsMatch(c.ToString(), this.Menu.SearchPattern))//check char
                                needClose = true;
                        }
                        else
                        {
                            needClose = true;
                        }
                    }
                }

                if (needClose)
                    this.Menu.Close();
            }

        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as FastColoredTextBox;

            if (this.Menu.Visible)
                if (this.ProcessKey(e.KeyCode, e.Modifiers))
                    e.Handled = true;

            if (!this.Menu.Visible)
            {
                if (tb.HotkeysMapping.ContainsKey(e.KeyData) && tb.HotkeysMapping[e.KeyData] == FCTBAction.AutocompleteMenu)
                {
                    this.DoAutocomplete();
                    e.Handled = true;
                }
                else
                {
                    if (e.KeyCode == Keys.Escape && timer.Enabled)
                        timer.Stop();
                }
            }
        }

        private void AdjustScroll()
        {
            if (oldItemCount == visibleItems.Count)
                return;

            int needHeight = this.ItemHeight * visibleItems.Count + 1;
            this.Height = Math.Min(needHeight, this.MaximumSize.Height);
            this.Menu.CalcSize();

            this.AutoScrollMinSize = new Size(0, needHeight);
            oldItemCount = visibleItems.Count;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.AdjustScroll();

            var itemHeight = this.ItemHeight;
            int startI = this.VerticalScroll.Value / itemHeight - 1;
            int finishI = (this.VerticalScroll.Value + this.ClientSize.Height) / itemHeight + 1;
            startI = Math.Max(startI, 0);
            finishI = Math.Min(finishI, visibleItems.Count);
            int y = 0;
            int leftPadding = 18;
            for (int i = startI; i < finishI; i++)
            {
                y = i * itemHeight - this.VerticalScroll.Value;

                var item = visibleItems[i];

                if (item.BackColor != Color.Transparent)
                    using (var brush = new SolidBrush(item.BackColor))
                        e.Graphics.FillRectangle(brush, 1, y, this.ClientSize.Width - 1 - 1, itemHeight - 1);

                if (this.ImageList != null && visibleItems[i].ImageIndex >= 0)
                    e.Graphics.DrawImage(this.ImageList.Images[item.ImageIndex], 1, y);

                if (i == this.FocussedItemIndex)
                    using (var selectedBrush = new LinearGradientBrush(new Point(0, y - 3), new Point(0, y + itemHeight), Color.Transparent, this.SelectedColor))
                    using (var pen = new Pen(this.SelectedColor))
                    {
                        e.Graphics.FillRectangle(selectedBrush, leftPadding, y, this.ClientSize.Width - 1 - leftPadding, itemHeight - 1);
                        e.Graphics.DrawRectangle(pen, leftPadding, y, this.ClientSize.Width - 1 - leftPadding, itemHeight - 1);
                    }

                if (i == hoveredItemIndex)
                    using (var pen = new Pen(this.HoveredColor))
                        e.Graphics.DrawRectangle(pen, leftPadding, y, this.ClientSize.Width - 1 - leftPadding, itemHeight - 1);

                using (var brush = new SolidBrush(item.ForeColor != Color.Transparent ? item.ForeColor : this.ForeColor))
                    e.Graphics.DrawString(item.ToString(), this.Font, brush, leftPadding, y);
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            this.Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.FocussedItemIndex = this.PointToItemIndex(e.Location);
                this.DoSelectedVisible();
                this.Invalidate();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            this.FocussedItemIndex = this.PointToItemIndex(e.Location);
            this.Invalidate();
            this.OnSelecting();
        }

        internal virtual void OnSelecting()
        {
            if (this.FocussedItemIndex < 0 || this.FocussedItemIndex >= visibleItems.Count)
                return;
            tb.TextSource.Manager.BeginAutoUndoCommands();
            try
            {
                AutocompleteItem item = this.FocussedItem;
                SelectingEventArgs args = new SelectingEventArgs()
                {
                    Item = item,
                    SelectedIndex = FocussedItemIndex
                };

                this.Menu.OnSelecting(args);

                if (args.Cancel)
                {
                    this.FocussedItemIndex = args.SelectedIndex;
                    this.Invalidate();
                    return;
                }

                if (!args.Handled)
                {
                    var fragment = this.Menu.Fragment;
                    this.DoAutocomplete(item, fragment);
                }

                this.Menu.Close();
                //
                SelectedEventArgs args2 = new SelectedEventArgs()
                {
                    Item = item,
                    Tb = this.Menu.Fragment.tb
                };
                item.OnSelected(this.Menu, args2);
                this.Menu.OnSelected(args2);
            }
            finally
            {
                tb.TextSource.Manager.EndAutoUndoCommands();
            }
        }

        private void DoAutocomplete(AutocompleteItem item, Range fragment)
        {
            string newText = item.GetTextForReplace();

            //replace text of fragment
            var tb = fragment.tb;

            tb.BeginAutoUndo();
            tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
            if (tb.Selection.ColumnSelectionMode)
            {
                var start = tb.Selection.Start;
                var end = tb.Selection.End;
                start.iChar = fragment.Start.iChar;
                end.iChar = fragment.End.iChar;
                tb.Selection.Start = start;
                tb.Selection.End = end;
            }
            else
            {
                item.SetSelectionForTextToReplace(fragment, item.Parent.FragmentStart, item.Parent.EditLine, item.Parent.CursorColumn);
                //tb.Selection.Start = fragment.Start;
                //tb.Selection.End = fragment.End;
            }
            tb.InsertText(newText);
            tb.TextSource.Manager.ExecuteCommand(new SelectCommand(tb.TextSource));
            tb.EndAutoUndo();
            tb.Focus();
        }

        private int PointToItemIndex(Point p)
        {
            return (p.Y + this.VerticalScroll.Value) / this.ItemHeight;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            this.ProcessKey(keyData, Keys.None);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool ProcessKey(Keys keyData, Keys keyModifiers)
        {
            if (keyModifiers == Keys.None)
                switch (keyData)
                {
                    case Keys.Down:
                        this.SelectNext(+1);
                        return true;
                    case Keys.PageDown:
                        this.SelectNext(+10);
                        return true;
                    case Keys.Up:
                        this.SelectNext(-1);
                        return true;
                    case Keys.PageUp:
                        this.SelectNext(-10);
                        return true;
                    case Keys.Enter:
                        this.OnSelecting();
                        return true;
                    case Keys.Tab:
                        if (!this.AllowTabKey)
                            break;
                        this.OnSelecting();
                        return true;
                    case Keys.Escape:
                        this.Menu.Close();
                        return true;
                }

            return false;
        }

        public void SelectNext(int shift)
        {
            this.FocussedItemIndex = Math.Max(0, Math.Min(this.FocussedItemIndex + shift, visibleItems.Count - 1));
            this.DoSelectedVisible();
            //
            this.Invalidate();
        }

        private void DoSelectedVisible()
        {
            if (this.FocussedItem != null)
                this.SetToolTip(this.FocussedItem);

            var y = this.FocussedItemIndex * this.ItemHeight - this.VerticalScroll.Value;
            if (y < 0)
                this.VerticalScroll.Value = this.FocussedItemIndex * this.ItemHeight;
            if (y > this.ClientSize.Height - this.ItemHeight)
                this.VerticalScroll.Value = Math.Min(this.VerticalScroll.Maximum, this.FocussedItemIndex * this.ItemHeight - this.ClientSize.Height + this.ItemHeight);
            //some magic for update scrolls
            this.AutoScrollMinSize -= new Size(1, 0);
            this.AutoScrollMinSize += new Size(1, 0);
        }

        private void SetToolTip(AutocompleteItem autocompleteItem)
        {
            var title = autocompleteItem.ToolTipTitle;
            var text = autocompleteItem.ToolTipText;

            if (string.IsNullOrEmpty(title))
            {
                toolTip.ToolTipTitle = null;
                toolTip.SetToolTip(this, null);
                return;
            }

            if (this.Parent != null)
            {
                IWin32Window window = this.Parent ?? this;
                Point location;

                if ((this.PointToScreen(this.Location).X + this.MaxToolTipSize.Width + 105) < Screen.FromControl(this.Parent).WorkingArea.Right)
                    location = new Point(this.Right + 5, 0);
                else
                    location = new Point(this.Left - 105 - this.MaximumSize.Width, 0);

                if (string.IsNullOrEmpty(text))
                {
                    toolTip.ToolTipTitle = null;
                    toolTip.Show(title, window, location.X, location.Y, this.ToolTipDuration);
                }
                else
                {
                    toolTip.ToolTipTitle = title;
                    toolTip.Show(text, window, location.X, location.Y, this.ToolTipDuration);
                }
            }
        }

        public int Count
        {
            get { return visibleItems.Count; }
        }
    }

    public class SelectingEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public bool Cancel { get; set; }
        public int SelectedIndex { get; set; }
        public bool Handled { get; set; }
    }

    public class SelectedEventArgs : EventArgs
    {
        public AutocompleteItem Item { get; internal set; }
        public FastColoredTextBox Tb { get; set; }
    }
}
