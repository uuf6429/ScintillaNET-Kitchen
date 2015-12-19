using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ScintillaNET_Kitchen
{
    class ToolStripMenuRadioItem : ToolStripMenuItem
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
            this.CheckOnClick = true;
            this.Click += ToolStripMenuRadioItem_Click;
        }

        private void ToolStripMenuRadioItem_Click(object sender, EventArgs e)
        {
            foreach (var item in this.FindItemsInSameGroup()) item.CheckState = CheckState.Unchecked;
        }

        protected ToolStripMenuRadioItem[] FindItemsInSameGroup()
        {
            ToolStripItem current, next;
            var parent = this.GetCurrentParent();
            var result = new List<ToolStripMenuRadioItem>();

            // find previous items
            current = this;
            while (
                    (next = parent.GetNextItem(current, ArrowDirection.Up)) != null
                    && next.GetIndex() < current.GetIndex()
                    && next.Text != "-"
                )
            {
                current = next;
                if (current is ToolStripMenuRadioItem) result.Add(current as ToolStripMenuRadioItem);
            }

            // find next items
            current = this;
            while (
                    (next = parent.GetNextItem(current, ArrowDirection.Down)) != null
                    && next.GetIndex() > current.GetIndex()
                    && next.Text != "-"
                )
            {
                current = next;
                if (current is ToolStripMenuRadioItem) result.Add(current as ToolStripMenuRadioItem);
            }

            // return result
            return result.ToArray();
        }
    }

    static class ToolStripItemExtensionMethods
    {
        public static int GetIndex(this ToolStripItem item)
        {
            return item.GetCurrentParent().Items.IndexOf(item);
        }
    }
}
