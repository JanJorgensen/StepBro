using FastColoredTextBoxNS;
using StepBro.Core.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StepBro.Core.Controls
{
    public class ScriptFileEditor : SourceCodeEditor
    {
        private readonly AutocompleteMenu popupMenu;
        private readonly string[] keywords = { "alt", "assert", "await", "base", "bool", "break", "const", "continue", "datatable", "datetime", "decimal", "do",
            "double", "dynamic", "else", "empty", "enum", "error", "execution", "expect", "fail", "false", "for", "foreach", "function", "if",
            "ignore", "in", "inconclusive", "int", "integer", "is", "log", "namespace", "not", "null", "object", "on", "or", "out", "pass",
            "private", "procedure", "protected", "public", "ref", "report", "return", "singleselection", "start", "static", "step", "string",
            "testlist", "this", "throw", "timeout", "timespan", "true", "unset", "using", "var", "verdict", "void", "warning", "while" };
        private readonly string[] methods = { "Equals()", "GetHashCode()", "GetType()", "ToString()" };
        private readonly string[] snippets = { "namespace ^;", "if (^)\n{\n;\n}", "if (^)\n{\n;\n}\nelse\n{\n;\n}", "while (^)\n{\n;\n}" };
        private readonly string[] declarationSnippets = { "public procedure void ^()\n{\n}" };

        public ScriptFileEditor()
        {
            this.FCTB.KeyPressed += this.FCTB_KeyPressed;
            this.FCTB.AutoIndentNeeded += this.FastColoredTextBox_AutoIndentNeeded;
            this.FCTB.AutoIndent = true;

            popupMenu = new AutocompleteMenu(this.FCTB);
            //popupMenu.Items.ImageList = imageList1;
            popupMenu.SearchPattern = @"[\w\.:=!<>]";
            popupMenu.AllowTabKey = true;
            //
            BuildAutocompleteMenu();
        }

        private void BuildAutocompleteMenu()
        {
            List<AutocompleteItem> items = new List<AutocompleteItem>();

            foreach (var item in snippets)
                items.Add(new SnippetAutocompleteItem(item) { ImageIndex = 1 });
            foreach (var item in declarationSnippets)
                items.Add(new DeclarationSnippet(item) { ImageIndex = 0 });
            foreach (var item in methods)
                items.Add(new MethodAutocompleteItem(item) { ImageIndex = 2 });
            foreach (var item in keywords)
                items.Add(new AutocompleteItem(item));

            //set as autocomplete source
            popupMenu.Items.SetAutocompleteItems(items);
        }

        /// <summary>
        /// This item appears when any part of snippet text is typed
        /// </summary>
        class DeclarationSnippet : SnippetAutocompleteItem
        {
            public DeclarationSnippet(string snippet)
                : base(snippet)
            {
            }

            public override CompareResult Compare(string fragmentText)
            {
                var pattern = Regex.Escape(fragmentText);
                if (Regex.IsMatch(Text, "\\b" + pattern, RegexOptions.IgnoreCase))
                    return CompareResult.Visible;
                return CompareResult.Hidden;
            }
        }

        /// <summary>
        /// Builds list of methods and properties for current class name was typed in the textbox
        /// </summary>
        internal class DynamicCollection : IEnumerable<AutocompleteItem>
        {
            private AutocompleteMenu menu;
            private FastColoredTextBox tb;

            public DynamicCollection(AutocompleteMenu menu, FastColoredTextBox tb)
            {
                this.menu = menu;
                this.tb = tb;
            }

            public IEnumerator<AutocompleteItem> GetEnumerator()
            {
                //get current fragment of the text
                var text = menu.Fragment.Text;
                System.Diagnostics.Trace.WriteLine($"AutoComplete from \"{text}\"");
                //extract class name (part before dot)
                var parts = text.Split('.');
                if (parts.Length < 2)
                    yield break;
                var className = parts[parts.Length - 2];

                //find type for given className
                var type = FindTypeByName(className);

                if (type == null)
                    yield break;

                //return static methods of the class
                foreach (var methodName in type.GetMethods().AsEnumerable().Select(mi => mi.Name).Distinct())
                    yield return new MethodAutocompleteItem(methodName)
                    {
                        ToolTipTitle = methodName,
                        ToolTipText = "Description of method " + methodName + " goes here.",
                        Tag = methodName + "___"
                    };

                //return static properties of the class
                foreach (var pi in type.GetProperties())
                    yield return new MethodAutocompleteItem(pi.Name)
                    {
                        ToolTipTitle = pi.Name,
                        ToolTipText = "Description of property " + pi.Name + " goes here.",
                    };
            }

            Type FindTypeByName(string name)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var a in assemblies)
                {
                    foreach (var t in a.GetTypes())
                    {
                        if (t.Name == name)
                        {
                            return t;
                        }
                    }
                }

                return null;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private void FCTB_KeyPressed(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            var line = this.FCTB.GetLineText(this.FCTB.Selection.FromLine).Trim();
            if (line.Length == 1)
            {
                if (e.KeyChar == '}' || e.KeyChar == ')')
                {
                    this.FCTB.DoAutoIndent(this.FCTB.Selection.FromLine);
                }
            }
        }

        private void FastColoredTextBox_AutoIndentNeeded(object sender, FastColoredTextBoxNS.AutoIndentEventArgs e)
        {
            var l = e.LineText.Trim();
            if (l.EndsWith("(") || l.EndsWith("{"))
            {
                e.ShiftNextLines = e.TabLength;
            }
            if (l.StartsWith(")") || l.StartsWith("}"))
            {
                e.Shift = -e.TabLength;
                e.ShiftNextLines = -e.TabLength;
            }
        }

        protected override IEditorSupport CreateSyntaxHighlighter()
        {
            return new StepBroEditorSupport(this);
        }
    }
}
