using Microsoft.CSharp;
using ScintillaNET;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ScintillaNET_Kitchen.Forms
{
    public partial class MainForm : Form
    {
        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            scintilla1.Text = "";

            lexerToolStripMenuItem.DropDownItems.AddRange(
                Enum.GetNames(typeof(Lexer))
                    .OrderBy(m => m)
                    .Select(m => new ToolStripMenuRadioItem(m)
                    {
                        CheckState = m == "Null" ? CheckState.Checked : CheckState.Unchecked
                    })
                    .ToArray()
            );

            scintilla2.StyleResetDefault();
            scintilla2.Styles[Style.Default].Font = "Consolas";
            scintilla2.Styles[Style.Default].Size = 10;
            scintilla2.StyleClearAll();
            scintilla2.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
            scintilla2.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0);
            scintilla2.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0);
            scintilla2.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128);
            scintilla2.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            scintilla2.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            scintilla2.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
            scintilla2.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21);
            scintilla2.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21);
            scintilla2.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21);
            scintilla2.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            scintilla2.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            scintilla2.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;

            this.UpdateResult();
        }

        #endregion

        #region Public Methods

        public void UpdateResult()
        {
            var text = new StringBuilder();
            var lexerName = Enum.GetName(typeof(Lexer), scintilla1.Lexer);
            var defaultStyle = scintilla1.Styles[Style.Default];
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
                    if (oldValue.ToString() != newValue.ToString() && !(styleKey == "Font" && String.IsNullOrEmpty(newValue.ToString())))
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
                if (String.IsNullOrEmpty(keywords) == false) keywordSets.Add(row.Index, keywords);
            }

            if (keywordSets.Any())
            {
                text.AppendLine();
                foreach (var item in keywordSets)
                    if (!String.IsNullOrEmpty(item.Value))
                        text.AppendLine("scintilla1.SetKeywords(" + item.Key + ", @\"" + item.Value + "\");");
            }

            scintilla2.ReadOnly = false;
            scintilla2.Text = text.ToString();
            scintilla2.ReadOnly = true;
        }

        #endregion

        #region Utility Methods

        private void LoadLexerStyles(Lexer lexer)
        {
            toolStripComboBox1.Items.Clear();

            var lexerType = Type.GetType(typeof(Style).AssemblyQualifiedName.Replace(".Style", ".Style+" + lexer));

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
        }

        private Dictionary<string, Func<object, string>> typeSerializers = new Dictionary<string, Func<object, string>>()
        {
            { typeof(Color).FullName, m => ((Color)m).IsNamedColor ? "Color." + ((Color)m).Name : "Color.FromArgb(" + ((Color)m).ToArgb() + ")" },
//            { typeof(StyleCase).FullName, m => "ScintillaNET.StyleCase." + Enum.GetName(typeof(StyleCase), m) },
        };

        protected string SerializeValue(object value)
        {
            if (value == null)
            {
                // code generation for null - nothing fancy here
                return "null";
            }

            var type = value.GetType();

            if (this.typeSerializers.ContainsKey(type.FullName))
            {
                // code generation by custom handler
                return this.typeSerializers[type.FullName](value);
            }
            else
            {
                try
                {
                    if (value is Enum)
                    {
                        // code generation for enums
                        return type.FullName + "." + Enum.GetName(type, value);
                    }
                    else
                    {
                        // code generation using codedom - works for most simple types
                        var w = new StringWriter();
                        var e = new CodePrimitiveExpression(value);
                        var o = new CodeGeneratorOptions();
                        var p = new CSharpCodeProvider();
                        p.GenerateCodeFromExpression(e, w, o);
                        return w.ToString();
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new NotImplementedException("Cannot serialize value of type " + type.Name + ".", ex);
                }
            }
        }

        protected Dictionary<int, string> GetLexerStyles(Lexer lexer)
        {
            var lexerName = Enum.GetName(typeof(Lexer), lexer);
            var lexerType = Type.GetType(typeof(Style).AssemblyQualifiedName.Replace(".Style", ".Style+" + lexerName));
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

        private void ExecuteCode(string code, Dictionary<string, object> args, Type[] requiredTypes)
        {
            var errors = new List<string>();

            try
            {
                var programCode = new List<string>();
                var className = "SNK_Loader_" + Guid.NewGuid().ToString("N");
                var ctorArgs = args.Select(m => ((m.Value == null) ? "object" : m.Value.GetType().Name) + " " + m.Key);

                // build program code
                programCode.AddRange(
                    requiredTypes
                        .Select(t => "using " + t.Namespace + ";")
                        .Distinct()
                        .OrderBy(s => s)
                );
                programCode.AddRange(new string[] {
                    "",
                    "namespace ScintillaNET_Kitchen",
                    "{",
                    "    public class " + className,
                    "    {",
                    "        public " + className + "( " + String.Join(", ", ctorArgs) + " )",
                    "        {",
                    "",
                    code,
                    "",
                    "        }",
                    "    }",
                    "}",
                });

                // load, compile and execute
                var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
                var results = csc.CompileAssemblyFromSource(
                    new CompilerParameters(
                        requiredTypes.Select(t => t.Assembly.Location).Distinct().ToArray()
                    )
                    {
                        GenerateExecutable = false,
                        GenerateInMemory = true,
                        TreatWarningsAsErrors = true,
                    },
                    String.Join(Environment.NewLine, programCode)
                );
                errors.AddRange(results.Errors.Cast<CompilerError>().Select(e => "Error " + e.ErrorNumber + ": " + e.ErrorText));

                var classType = results.CompiledAssembly.GetType("ScintillaNET_Kitchen." + className);
                Activator.CreateInstance(classType, args.Select(a => a.Value).ToArray());
            }
            catch (Exception ex)
            {
                errors.Add(ex.ToString());
            }

            if (errors.Any())
            {
                MessageBox.Show(
                    "Error loading C# file:\n\u2022 " + String.Join("\n\u2022 ", errors),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
            }
        }

        #endregion

        #region Event Handlers

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
            toolStripComboBox1.Width = menuStrip1.Width - toolStripMenuItem1.Width - 8;
        }

        #endregion

        #region Menu Event Handlers

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var defaultStyle = scintilla1.Styles[Style.Default];
            var styleType = typeof(Style);

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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // update ui
                newToolStripMenuItem_Click(sender, e);

                // update save dialog
                saveFileDialog1.FileName = openFileDialog1.FileName;
                saveFileDialog1.InitialDirectory = Path.GetFullPath(openFileDialog1.FileName);

                // load new code
                this.ExecuteCode(
                    File.ReadAllText(openFileDialog1.FileName),
                    new Dictionary<string, object>() { { "scintilla1", scintilla1 }, },
                    new Type[] {
                        typeof(Color),
                        typeof(Control),
                        typeof(Component),
                        typeof(Scintilla),
                        typeof(ScintillaEx),
                    }
                );

                // select lexer
                var lexerText = scintilla1.Lexer.ToString();
                foreach(ToolStripMenuRadioItem item in lexerToolStripMenuItem.DropDownItems)
                    if(item!=null)
                        item.Checked = item.Text == lexerText;
                this.LoadLexerStyles(scintilla1.Lexer);

                // reload ui
                this.UpdateResult();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // update open dialog
                openFileDialog1.FileName = saveFileDialog1.FileName;
                openFileDialog1.InitialDirectory = Path.GetFullPath(saveFileDialog1.FileName);

                // save code to file
                File.WriteAllText(saveFileDialog1.FileName, scintilla2.Text);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lexerToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            scintilla1.Lexer = (Lexer)Enum.Parse(typeof(Lexer), e.ClickedItem.Text);
            this.LoadLexerStyles(scintilla1.Lexer);
            this.UpdateResult();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog();
        }

        private void prefillForeColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] colourValues = new string[] {
                "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "800000",
                "008000", "000080", "808000", "800080", "008080", "808080", "C00000",
                "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0", "400000",
                "004000", "000040", "404000", "400040", "004040", "404040", "E0E0E0",
                "200000", "002000", "000020", "202000", "200020", "002020", "202020",
                "600000", "006000", "000060", "606000", "600060", "006060", "606060",
                "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0",
                "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "000000",
            };

            int i = 0;
            this.GetLexerStyles(scintilla1.Lexer)
                .Where(s => s.Value != "Default")
                .All(s => {
                    if (i < colourValues.Length)
                    {
                        scintilla1.Styles[s.Key].ForeColor = ColorTranslator.FromHtml("#" + colourValues[i]);
                        i++;
                    }
                    return true;
                });

            this.UpdateResult();
        }

        #endregion
    }
}
