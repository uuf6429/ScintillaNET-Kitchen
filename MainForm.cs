using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ScintillaNET_Kitchen
{
    public partial class MainForm : Form
    {
        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            scintilla1.Text = "";
            comboBox1.Items.AddRange(Enum.GetNames(typeof(ScintillaNET.Lexer)));
            comboBox1.Text = "Null";

            scintilla2.StyleResetDefault();
            scintilla2.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintilla2.Styles[ScintillaNET.Style.Default].Size = 10;
            scintilla2.StyleClearAll();
            scintilla2.Styles[ScintillaNET.Style.Cpp.Default].ForeColor = Color.Silver;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0);
            scintilla2.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0);
            scintilla2.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128);
            scintilla2.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = Color.Olive;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = Color.Blue;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = Color.Blue;
            scintilla2.Styles[ScintillaNET.Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21);
            scintilla2.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21);
            scintilla2.Styles[ScintillaNET.Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21);
            scintilla2.Styles[ScintillaNET.Style.Cpp.StringEol].BackColor = Color.Pink;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = Color.Purple;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = Color.Maroon;

            this.UpdateResult();
        }

        #endregion

        #region Public Methods

        public void UpdateResult()
        {
            var text = new StringBuilder();
            var lexerName = Enum.GetName(typeof(ScintillaNET.Lexer), scintilla1.Lexer);
            var defaultStyle = scintilla1.Styles[ScintillaNET.Style.Default];
            var styleType = defaultStyle.GetType();

            // TODO somehow force scintilla to refresh (otherwise ui doesn't match user selection for some reason)

            text.AppendLine("scintilla1.Lexer = ScintillaNET.Lexer." + lexerName + ";");
            text.AppendLine("");

            foreach (var item in this.GetLexerStyles(scintilla1.Lexer))
            {
                foreach (var styleKey in styleKeys)
                {
                    var prop = styleType.GetProperty(styleKey);
                    var oldValue = prop.GetValue(defaultStyle, null);
                    var newValue = prop.GetValue(scintilla1.Styles[item.Key], null);
                    if (oldValue.ToString() != newValue.ToString() && !(styleKey == "Font" && newValue.ToString() == ""))
                    {
                        var serializedValue = this.SerializeValue(newValue);
                        if (serializedValue != null)
                        {
                            text.AppendLine("scintilla1.Styles[ScintillaNET.Style." + lexerName + "." + item.Value + "]." + styleKey + " = " + serializedValue + ";");
                        }
                    }
                }
            }

            var keywordSets = new Dictionary<int, string>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var keywords = (row.Cells[1].Value ?? "").ToString();
                scintilla1.SetKeywords(row.Index, keywords);
                if (keywords != "") keywordSets.Add(row.Index, keywords);
            }

            if (keywordSets.Any())
            {
                text.AppendLine();
                foreach (var item in keywordSets)
                    text.AppendLine("scintilla1.SetKeywords(" + item.Key + ", @\"" + item.Value + "\");");
            }

            scintilla2.ReadOnly = false;
            scintilla2.Text = text.ToString();
            scintilla2.ReadOnly = true;
        }

        #endregion

        #region Utility Methods

        private Dictionary<string, Func<object, string>> typeSerializers = new Dictionary<string, Func<object, string>>()
        {
            { "null", m => null },
            { typeof(Color).FullName, m => ((Color)m).IsNamedColor ? "Color." + ((Color)m).Name : "Color.FromName(@\"" + ((Color)m).Name + "\")" },
            { typeof(Boolean).FullName, m => (Boolean)m ? "true" : "false" },
            { typeof(String).FullName, m => "@\"" + m.ToString() + "\"" },
            { typeof(int).FullName, m => m.ToString() },
            { typeof(float).FullName, m => m.ToString() },
            { typeof(ScintillaNET.StyleCase).FullName, m => "ScintillaNET.StyleCase." + Enum.GetName(typeof(ScintillaNET.StyleCase), m) },
        };

        private string SerializeValue(object value)
        {
            var typeName = value == null ? "null" : value.GetType().FullName;

            if (!this.typeSerializers.ContainsKey(typeName))
            {
                throw new NotImplementedException("Cannot serialize value of type " + value.GetType().Name + ".");
            }

            return this.typeSerializers[typeName](value);
        }

        private Dictionary<int, string> GetLexerStyles(ScintillaNET.Lexer lexer)
        {
            var lexerName = Enum.GetName(typeof(ScintillaNET.Lexer), lexer);
            var lexerType = Type.GetType(typeof(ScintillaNET.Style).AssemblyQualifiedName.Replace(".Style", ".Style+" + lexerName));
            return lexerType == null
                ? new Dictionary<int, string>()
                : lexerType
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                    .ToDictionary(
                        fi => (int)fi.GetRawConstantValue(),
                        fi => fi.Name
                    );
        }
        
        // TODO maybe get this from styleType?
        private string[] styleKeys = new string[] {
                "BackColor", "Bold", "Case", "FillLine", "Font", "ForeColor", "Hotspot", "Italic", "Size", "SizeF", "Underline", "Visible", "Weight",
        };

        #endregion

        #region Event Handlers

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            scintilla1.Lexer = (ScintillaNET.Lexer)Enum.Parse(typeof(ScintillaNET.Lexer), comboBox1.Text);

            toolStripComboBox1.Items.Clear();

            var lexerType = Type.GetType(typeof(ScintillaNET.Style).AssemblyQualifiedName.Replace(".Style", ".Style+" + comboBox1.Text));

            if (lexerType != null)
            {
                toolStripComboBox1.Items
                    .AddRange(
                        this.GetLexerStyles(scintilla1.Lexer)
                            .Select(m => m.Key + ": " + m.Value)
                            .ToArray()
                    );
            }

            propertyGrid1.SelectedObject = null;

            dataGridView1.Rows.Clear();

            dataGridView1.Rows.AddRange(
                scintilla1
                    .DescribeKeywordSets()
                    .Split(new string[] { "\n" }, StringSplitOptions.None)
                    .Select((s, i) =>
                    {
                        var row = new DataGridViewRow();
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = s });
                        row.Cells.Add(new DataGridViewTextBoxCell() { Value = scintilla1.GetKeywords(i).Trim() });
                        return row;
                    })
                    .ToArray()
            );

            this.UpdateResult();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var pos = Int32.Parse(toolStripComboBox1.Text.Split(new char[] { ':' })[0]);
            propertyGrid1.SelectedObject = scintilla1.Styles[pos];
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.UpdateResult();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            this.UpdateResult();
        }

        private void menuStrip1_Resize(object sender, EventArgs e)
        {
            toolStripComboBox1.Width = menuStrip1.Width - toolStripMenuItem1.Width - 4;
        }

        private void resetStylesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var defaultStyle = scintilla1.Styles[ScintillaNET.Style.Default];
            var styleType = typeof(ScintillaNET.Style);

            foreach (var item in this.GetLexerStyles(scintilla1.Lexer))
            {
                foreach (var styleKey in styleKeys)
                {
                    var prop = styleType.GetProperty(styleKey);
                    prop.SetValue(scintilla1.Styles[item.Key], prop.GetValue(defaultStyle, null), null);
                }
            }

            this.UpdateResult();
        }

        private Random colorRng = new Random(2);

        private void prefillForeColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var styleType = typeof(ScintillaNET.Style);

            foreach (var item in this.GetLexerStyles(scintilla1.Lexer).Where(style => style.Value != "Default"))
            {
                scintilla1.Styles[item.Key].ForeColor = Color.FromArgb(
                    (byte)(colorRng.Next(128)),
                    (byte)(colorRng.Next(128)),
                    (byte)(colorRng.Next(128))
                );
            }

            this.UpdateResult();
        }

        #endregion
    }
}
