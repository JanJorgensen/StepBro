using System.Collections.Generic;

namespace FastColoredTextBoxNS
{
    public abstract class AutocompleteCreator
    {
        protected AutocompleteMenu m_menu;
        protected FastColoredTextBox m_fctb;
        public AutocompleteCreator(AutocompleteMenu menu, FastColoredTextBox fctb)
        {
            m_menu = menu;
            m_fctb = fctb;
        }

        public AutocompleteMenu Parent { get { return m_menu; } }

        public abstract IEnumerable<AutocompleteItem> CreateItems(int line, int column);
    }
}
