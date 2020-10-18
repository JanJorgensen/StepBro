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
        private readonly string[] snippets = { "namespace ^;", "if (^)\n{\n;\n}", "if (^)\n{\n;\n}\nelse\n{\n;\n}", "while (^)\n{\n;\n}" };
        private readonly string[] declarationSnippets = { "public procedure void ^()\n{\n}" };
        //private DynamicCollection m_autoCompleteItems = null;


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
            this.BuildAutocompleteMenu();
        }

        private void BuildAutocompleteMenu()
        {
            List<AutocompleteItem> staticItems = new List<AutocompleteItem>();

            foreach (var item in snippets)
                staticItems.Add(new SnippetAutocompleteItem(item) { ImageIndex = 1 });
            foreach (var item in declarationSnippets)
                staticItems.Add(new DeclarationSnippet(item) { ImageIndex = 0 });
            foreach (var item in keywords)
                staticItems.Add(new AutocompleteItem(item));

            //var declSnippets = new DeclarationSnippetCreator(popupMenu, this.FCTB);
            //popupMenu.Items.SetCreator(declSnippets);


            //m_autoCompleteItems = new DynamicCollection(popupMenu, this.FCTB, staticItems);
            //set as autocomplete source
            popupMenu.Items.SetAutocompleteItems(staticItems);
        }

        //private class DeclarationSnippetCreator : AutocompleteCreator
        //{
        //    //private 
        //    public DeclarationSnippetCreator(AutocompleteMenu menu, FastColoredTextBox fctb) : base(menu, fctb)
        //    {

        //    }
        //    public override IEnumerable<AutocompleteItem> CreateItems(int line, int column)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        /// <summary>
        /// This item appears when any part of snippet text is typed
        /// </summary>
        private class DeclarationSnippet : SnippetAutocompleteItem
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
                return CompareResult.Visible;
            }
        }

        /// <summary>
        /// Builds list of methods and properties for current class name was typed in the textbox
        /// </summary>
        internal class DynamicCollection : IEnumerable<AutocompleteItem>
        {
            private class Item : AutocompleteItem
            {

            }

            private AutocompleteMenu menu;
            private readonly FastColoredTextBox tb;
            private readonly List<AutocompleteItem> staticItems;

            public DynamicCollection(AutocompleteMenu menu, FastColoredTextBox tb, List<AutocompleteItem> staticItems)
            {
                this.menu = menu;
                this.tb = tb;
                this.staticItems = staticItems;
            }

            public IEnumerator<AutocompleteItem> GetEnumerator()
            {
                foreach (var si in staticItems) yield return si;

                //get current fragment of the text
                var text = menu.Fragment.Text;
                System.Diagnostics.Trace.WriteLine($"AutoComplete from \"{text}\"");
                //extract class name (part before dot)
                var parts = text.Split('.');
                if (parts.Length < 2)
                    yield break;
                var className = parts[parts.Length - 2];

                //find type for given className
                var type = this.FindTypeByName(className);

                if (type == null)
                    yield break;

                //return static methods of the class
                foreach (var methodName in type.GetMethods(BindingFlags.Public | BindingFlags.Static).AsEnumerable().Select(mi => mi.Name).Distinct())
                {
                    yield return new MethodAutocompleteItem(methodName)
                    {
                        ToolTipTitle = methodName,
                        ToolTipText = "Description of method " + methodName + " goes here.",
                        Tag = methodName + "___"
                    };
                    System.Diagnostics.Debug.WriteLine(type.FullName + "." + methodName);
                }

                //return static properties of the class
                foreach (var pi in type.GetProperties())
                {
                    yield return new MethodAutocompleteItem(pi.Name)
                    {
                        ToolTipTitle = pi.Name,
                        ToolTipText = "Description of property " + pi.Name + " goes here.",
                    };
                    System.Diagnostics.Debug.WriteLine(type.FullName + "." + pi.Name);
                }
            }

            private Type FindTypeByName(string name)
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
                return this.GetEnumerator();
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
