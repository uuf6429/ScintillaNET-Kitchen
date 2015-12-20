using ScintillaNET;
using System;
using System.Collections.Generic;

namespace ScintillaNET_Kitchen
{
    public class ScintillaEx : Scintilla
    {
        #region Constructors & Private Fields

        public ScintillaEx()
        {
            this.TextChanged += (object sender, EventArgs e) => this.UpdateLineMargin();
            this.UpdateLineMargin();
        }

        private Dictionary<int, string> keywords = new Dictionary<int, string>();

        private bool showLineMargin = true;

        private int maxLineNumberCharLength;

        #endregion

        #region Properties

        public bool ShowLineMargin
        {
            get { return this.showLineMargin; }
            set
            {
                this.showLineMargin = value;
                this.UpdateLineMargin();
            }
        }

        #endregion

        #region Public Methods

        public new void SetKeywords(int sid, string keywords)
        {
            this.keywords[sid] = keywords;
            base.SetKeywords(sid, String.IsNullOrEmpty(keywords) ? " " : keywords);
        }

        public string GetKeywords(int sid)
        {
            return this.keywords.ContainsKey(sid) ? this.keywords[sid] : "";
        }

        #endregion

        #region Utility Methods

        protected virtual void UpdateLineMargin()
        {
            var lineMarginChars = this.ShowLineMargin ? this.Lines.Count.ToString().Length : 0;
            if (lineMarginChars != this.maxLineNumberCharLength)
            {
                this.DoUpdateLineMargin(lineMarginChars);
            }
        }

        protected virtual void DoUpdateLineMargin(int lineMarginChars)
        {
            const int padding = 2;
            this.Margins[0].Width = !this.ShowLineMargin ? 0
                : this.TextWidth(Style.LineNumber, new string('9', lineMarginChars + 1)) + padding;
            this.maxLineNumberCharLength = lineMarginChars;
        }

        #endregion
    }
}
