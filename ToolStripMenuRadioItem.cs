using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ScintillaNET_Kitchen
{
    public class ToolStripMenuRadioItem : ToolStripMenuItem
    {
        #region Constructors

        public ToolStripMenuRadioItem()
            : base()
        {
            this.Initialize();
        }

        public ToolStripMenuRadioItem(Image image)
            : base(image)
        {
            this.Initialize();
        }

        public ToolStripMenuRadioItem(string text)
            : base(text)
        {
            this.Initialize();
        }

        public ToolStripMenuRadioItem(string text, Image image)
            : base(text, image)
        {
            this.Initialize();
        }

        public ToolStripMenuRadioItem(string text, Image image, params ToolStripItem[] dropDownItems)
            : base(text, image, dropDownItems)
        {
            this.Initialize();
        }

        public ToolStripMenuRadioItem(string text, Image image, EventHandler onClick)
            : base(text, image, onClick)
        {
            this.Initialize();
        }

        public ToolStripMenuRadioItem(string text, Image image, EventHandler onClick, Keys shortcutKeys)
            : base(text, image, onClick, shortcutKeys)
        {
            this.Initialize();
        }

        public ToolStripMenuRadioItem(string text, Image image, EventHandler onClick, string name)
            : base(text, image, onClick, name)
        {
            this.Initialize();
        }

        #endregion

        private void Initialize()
        {
            this.Click += ToolStripMenuRadioItem_Click;
        }

        private void ToolStripMenuRadioItem_Click(object sender, EventArgs e)
        {
            foreach (var item in this.FindItemsInSameGroup())
                item.CheckState = CheckState.Unchecked;
            this.Checked = true;
        }

        protected ToolStripMenuRadioItem[] FindItemsInSameGroup()
        {
            ToolStripItem current, next;
            var parent = this.GetCurrentParent();
            var result = new List<ToolStripMenuRadioItem>();

            // find previous items
            current = this;
            while (
                    (next = current.GetPrevItem()) != null
                    && !(next.Text == "-" || next is ToolStripSeparator)
                )
            {
                current = next;
                if (current is ToolStripMenuRadioItem) result.Add(current as ToolStripMenuRadioItem);
            }

            // find next items
            current = this;
            while (
                    (next = current.GetNextItem()) != null
                    && !(next.Text == "-" || next is ToolStripSeparator)
                )
            {
                current = next;
                if (current is ToolStripMenuRadioItem) result.Add(current as ToolStripMenuRadioItem);
            }

            // return result
            return result.ToArray();
        }
    }

    public static class ToolStripItemExtensionMethods
    {
        public static int GetIndex(this ToolStripItem item)
        {
            var parent = item.GetCurrentParent();
            return parent == null ? -1 : parent.Items.IndexOf(item);
        }

        public static ToolStripItem GetPrevItem(this ToolStripItem item)
        {
            var i = item.GetIndex() - 1;
            return i < 0 ? null : item.GetCurrentParent().Items[i];
        }

        public static ToolStripItem GetNextItem(this ToolStripItem item)
        {
            var i = item.GetIndex() + 1;
            var c = item.GetCurrentParent().Items.Count;
            return i >= c ? null : item.GetCurrentParent().Items[i];
        }
    }
}
