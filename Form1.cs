using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scintilla_Theme_Designer
{
    public partial class Form1 : Form
    {
        public Form1()
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
            scintilla2.Styles[ScintillaNET.Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla2.Styles[ScintillaNET.Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla2.Styles[ScintillaNET.Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
            scintilla2.Styles[ScintillaNET.Style.Cpp.Number].ForeColor = Color.Olive;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Word].ForeColor = Color.Blue;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Word2].ForeColor = Color.Blue;
            scintilla2.Styles[ScintillaNET.Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla2.Styles[ScintillaNET.Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla2.Styles[ScintillaNET.Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla2.Styles[ScintillaNET.Style.Cpp.StringEol].BackColor = Color.Pink;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Operator].ForeColor = Color.Purple;
            scintilla2.Styles[ScintillaNET.Style.Cpp.Preprocessor].ForeColor = Color.Maroon;

            this.UpdateResult();
        }

        protected void UpdateResult()
        {
            var text = new StringBuilder();
            var lexerName = Enum.GetName(typeof(ScintillaNET.Lexer), scintilla1.Lexer);
            var defaultStyle = scintilla1.Styles[ScintillaNET.Style.Default];
            var styleType = defaultStyle.GetType();

            // TODO maybe get this from styleType?
            var styleKeys = new string[] {
                "BackColor", "Bold", "Case", "FillLine", "Font", "ForeColor", "Hotspot", "Italic", "Size", "SizeF", "Underline", "Visible", "Weight",
            };

            text.AppendLine("scintilla1.Lexer = ScintillaNET.Lexer." + lexerName + ";");
            text.AppendLine("");

            foreach (var item in this.GetLexerStyles(scintilla1.Lexer))
            {
                foreach (var styleKey in styleKeys)
                {
                    var oldValue = styleType.GetProperty(styleKey).GetValue(defaultStyle);
                    var newValue = styleType.GetProperty(styleKey).GetValue(scintilla1.Styles[item.Key]);
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

            scintilla2.ReadOnly = false;
            scintilla2.Text = text.ToString();
            scintilla2.ReadOnly = true;
        }

        private Dictionary<string, Func<object, string>> typeSerializers = new Dictionary<string, Func<object, string>>()
        {
            { "null", m => null },
            { typeof(Color).FullName, m => ((Color)m).IsNamedColor ? "Color." + ((Color)m).Name : "Color.FromName(\"" + ((Color)m).Name + "\")" },
            { typeof(Boolean).FullName, m => (Boolean)m ? "true" : "false" },
            { typeof(String).FullName, m => "\"" + m.ToString() + "\"" },
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            scintilla1.Lexer = (ScintillaNET.Lexer)Enum.Parse(typeof(ScintillaNET.Lexer), comboBox1.Text);

            dataGridView1.Rows.Clear();

            var lexerType = Type.GetType(typeof(ScintillaNET.Style).AssemblyQualifiedName.Replace(".Style", ".Style+" + comboBox1.Text));

            if (lexerType != null)
            {
                dataGridView1.Rows
                    .AddRange(
                        this
                            .GetLexerStyles(scintilla1.Lexer)
                            .Select(m =>
                            {
                                var row = new DataGridViewRow();
                                row.HeaderCell.Value = m.Key.ToString();
                                row.Cells.Add(new DataGridViewTextBoxCell() { Value = m.Value });
                                return row;
                            })
                            .ToArray()
                    );
            }

            this.UpdateResult();
        }
    }
}
