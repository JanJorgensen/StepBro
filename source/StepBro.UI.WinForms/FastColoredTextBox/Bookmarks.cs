using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Base class for bookmark collection
    /// </summary>
    public abstract class BaseBookmarks : ICollection<Bookmark>, IDisposable
    {
        #region ICollection
        public abstract void Add(Bookmark item);
        public abstract void Clear();
        public abstract bool Contains(Bookmark item);
        public abstract void CopyTo(Bookmark[] array, int arrayIndex);
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool Remove(Bookmark item);
        public abstract IEnumerator<Bookmark> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable
        public abstract void Dispose();
        #endregion

        #region Additional properties

        public abstract void Add(int lineIndex);
        public abstract void Add(int lineIndex, object group);
        public abstract bool Contains(int lineIndex);
        public abstract bool Remove(int lineIndex);
        public abstract Bookmark GetBookmark(int i);

        #endregion
    }

    /// <summary>
    /// Collection of bookmarks
    /// </summary>
    public class Bookmarks : BaseBookmarks
    {
        protected FastColoredTextBox tb;
        protected List<Bookmark> items = new List<Bookmark>();
        protected int counter;

        public Bookmarks(FastColoredTextBox tb)
        {
            this.tb = tb;
            tb.LineInserted += tb_LineInserted;
            tb.LineRemoved += tb_LineRemoved;
        }

        protected virtual void tb_LineRemoved(object sender, LineRemovedEventArgs e)
        {
            for(int i=0; i<Count; i++)
            if (items[i].LineIndex >= e.Index)
            {
                if (items[i].LineIndex >= e.Index + e.Count)
                {
                    items[i].LineIndex = items[i].LineIndex - e.Count;
                    continue;
                }

                var was = e.Index <= 0;
                foreach (var b in items)
                    if (b.LineIndex == e.Index - 1)
                        was = true;

                if(was)
                {
                    items.RemoveAt(i);
                    i--;
                }else
                    items[i].LineIndex = e.Index - 1;

                //if (items[i].LineIndex == e.Index + e.Count - 1)
                //{
                //    items[i].LineIndex = items[i].LineIndex - e.Count;
                //    continue;
                //}
                //
                //items.RemoveAt(i);
                //i--;
            }
        }

        protected virtual void tb_LineInserted(object sender, LineInsertedEventArgs e)
        {
            for (int i = 0; i < Count; i++)
                if (items[i].LineIndex >= e.Index)
                {
                    items[i].LineIndex = items[i].LineIndex + e.Count;
                }else
                if (items[i].LineIndex == e.Index - 1 && e.Count == 1)
                {
                    if(tb[e.Index - 1].StartSpacesCount == tb[e.Index - 1].Count)
                        items[i].LineIndex = items[i].LineIndex + e.Count;
                }
        }
    
        public override void Dispose()
        {
            tb.LineInserted -= tb_LineInserted;
            tb.LineRemoved -= tb_LineRemoved;
        }

        public override IEnumerator<Bookmark> GetEnumerator()
        {
            foreach (var item in items)
                yield return item;
        }

        public override void Add(int lineIndex)
        {
            Add(new Bookmark(tb, lineIndex, null));
        }

        public override void Add(int lineIndex, object group)
        {
            Add(new Bookmark(tb, lineIndex, group));
        }

        public override void Clear()
        {
            items.Clear();
            counter = 0;
        }

        public override void Add(Bookmark bookmark)
        {
            foreach (var bm in items)
                if (bm.LineIndex == bookmark.LineIndex)
                    return;

            items.Add(bookmark);
            counter++;
            tb.Invalidate();
        }

        public override bool Contains(Bookmark item)
        {
            return items.Contains(item);
        }

        public override bool Contains(int lineIndex)
        {
            foreach (var item in items)
                if (item.LineIndex == lineIndex)
                    return true;
            return false;
        }

        public override void CopyTo(Bookmark[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool Remove(Bookmark item)
        {
            tb.Invalidate();
            return items.Remove(item);
        }

        /// <summary>
        /// Removes bookmark by line index
        /// </summary>
        public override bool Remove(int lineIndex)
        {
            bool was = false;
            for (int i = 0; i < Count; i++)
            if (items[i].LineIndex == lineIndex)
            {
                items.RemoveAt(i);
                i--;
                was = true;
            }
            tb.Invalidate();

            return was;
        }

        /// <summary>
        /// Returns Bookmark by index.
        /// </summary>
        public override Bookmark GetBookmark(int i)
        {
            return items[i];
        }
    }

    /// <summary>
    /// Bookmark of FastColoredTextbox
    /// </summary>
    public class Bookmark
    {
        public FastColoredTextBox TB { get; private set; }
        /// <summary>
        /// Name of bookmark
        /// </summary>
        public string Name { get; set; } = null;
        /// <summary>
        /// Line index
        /// </summary>
        public int LineIndex { get; set; }
        /// <summary>
        /// Color of bookmark sign
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// Object reference that identifies the bookmark type or group.
        /// </summary>
        public object Group { get; set; } = null;
        /// <summary>
        /// Indicates whether the bookmark should be shown in the editor.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Scroll textbox to the bookmark
        /// </summary>
        public virtual void DoVisible()
        {
            TB.Selection.Start = new Place(0, LineIndex);
            TB.DoRangeVisible(TB.Selection, true);
            TB.Invalidate();
        }

        public Bookmark(FastColoredTextBox tb, int lineIndex, object group)
        {
            this.TB = tb;
            this.LineIndex = lineIndex;
            this.Group = group;
            Color = tb.BookmarkColor;
        }

        public virtual void Paint(Graphics gr, Rectangle lineRect)
        {
            var fieldSize = TB.CharHeight - 3;
            var size = (fieldSize * 2) / ((this.Group == null) ? 3 : 3);
            var offset = (fieldSize - size) / 2;
            using (var brush = new LinearGradientBrush(new Rectangle(0, lineRect.Top, size, size), Color.White, Color, 45))
                gr.FillEllipse(brush, offset, lineRect.Top + offset, size, size);
            using (var pen = new Pen(Color))
                gr.DrawEllipse(pen, offset, lineRect.Top + offset, size, size);
        }
    }
}
