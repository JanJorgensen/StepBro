using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class PropertyBlockDecoder
    {
        public abstract class Element<TParent>
        {
            public Element(string typeOrName, PropertyBlockEntryType type)
            {
                this.TypeOrName = typeOrName;
                this.EntryType = type;
            }

            public bool TryDecode(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (entry.BlockEntryType == this.EntryType && entry.TypeOrName == this.TypeOrName)
                {
                    this.TryCreateOrSet(entry, parent, errors);
                    return true;    // There might be errors, but name and type did match.
                }

                return false;   // Not me...
            }

            protected abstract void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors);

            public string TypeOrName { get; private set; }
            public PropertyBlockEntryType EntryType { get; private set; }
        }

        public class Block<TParent, TThis> : Element<TParent>
        {
            private Func<TParent, PropertyBlockEntry, TThis> m_creator;
            private Element<TThis>[] m_childs;

            public Block(string name, Func<TParent, PropertyBlockEntry, TThis> creator, params Element<TThis>[] childs) :
                base(name, PropertyBlockEntryType.Block)
            {
                m_creator = creator;
                m_childs = childs;
            }
            public Block(Func<TParent, PropertyBlockEntry, TThis> creator, params Element<TThis>[] childs) :
                this("<root>", creator, childs)
            { }

            public Block(string name, params Element<TThis>[] childs) :
                this(name, null, childs)
            { }
            public Block(params Element<TThis>[] childs) :
                this("<root>", null, childs)
            { }

            public void SetChilds(params Element<TThis>[] childs)
            {
                m_childs = childs;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_creator == null) return;
                var data = m_creator(parent, entry);
                if (data != null)
                {
                    this.DecodeData(entry, data, errors);
                }
                else
                {
                    errors.Add(new Tuple<int, string>(entry.Line, "Could not set or create \"" + entry.TypeOrName + "\"."));
                }
            }

            public void DecodeData(PropertyBlockEntry block, TThis home, List<Tuple<int, string>> errors)
            {
                foreach (var child in (PropertyBlock)block)
                {
                    bool found = false;
                    foreach (var check in m_childs)
                    {
                        if (check.TryDecode(child, home, errors))
                        {
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        errors.Add(new Tuple<int, string>(child.Line, "Unknown element \"" + child.TypeOrName + "\" or wrong usage."));
                    }
                }
            }
        }

        #region Values

        public abstract class Value<TParent> : Element<TParent>
        {
            public Value(string typeOrName) : base(typeOrName, PropertyBlockEntryType.Value)
            {
            }
        }

        public class ValueString<TParent> : Value<TParent>
        {
            private Func<TParent, PropertyBlockValue, string> m_setter;

            public ValueString(string typeOrName, Func<TParent, PropertyBlockValue, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var valueEntry = entry as PropertyBlockValue;
                if (valueEntry.IsStringOrIdentifier)
                {
                    try
                    {
                        var error = (m_setter(parent, valueEntry));
                        if (error != null)
                        {
                            errors.Add(new Tuple<int, string>(entry.Line, "Value could not be set for \"" + entry.TypeOrName + "\"; " + error));
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new Tuple<int, string>(entry.Line, "Error setting value for \"" + entry.TypeOrName + "\"; " + ex.Message));
                    }
                }
                else
                {
                    errors.Add(new Tuple<int, string>(entry.Line, "Value for \"" + entry.TypeOrName + "\"should be a string or an ID."));
                }
            }
        }

        public class ValueInt<TParent> : Value<TParent>
        {
            private Func<TParent, PropertyBlockValue, string> m_setter;

            public ValueInt(string typeOrName, Func<TParent, PropertyBlockValue, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var valueEntry = entry as PropertyBlockValue;
                if (valueEntry.Value is Int64)
                {
                    try
                    {
                        var error = (m_setter(parent, valueEntry));
                        if (error != null)
                        {
                            errors.Add(new Tuple<int, string>(entry.Line, "Value could not be set for \"" + entry.TypeOrName + "\"; " + error));
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new Tuple<int, string>(entry.Line, "Error setting value for \"" + entry.TypeOrName + "\"; " + ex.Message));
                    }
                }
                else
                {
                    errors.Add(new Tuple<int, string>(entry.Line, "Value for \"" + entry.TypeOrName + "\"should be a numeric integer."));
                }
            }
        }

        public class ValueBool<TParent> : Value<TParent>
        {
            private Func<TParent, PropertyBlockValue, string> m_setter;

            public ValueBool(string typeOrName, Func<TParent, PropertyBlockValue, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var valueEntry = entry as PropertyBlockValue;
                if (valueEntry.Value is bool)
                {
                    try
                    {
                        var error = (m_setter(parent, valueEntry));
                        if (error != null)
                        {
                            errors.Add(new Tuple<int, string>(entry.Line, "Value could not be set for \"" + entry.TypeOrName + "\"; " + error));
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new Tuple<int, string>(entry.Line, "Error setting value for \"" + entry.TypeOrName + "\"; " + ex.Message));
                    }
                }
                else
                {
                    errors.Add(new Tuple<int, string>(entry.Line, "Value for \"" + entry.TypeOrName + "\"should be a boolean (true or false)."));
                }
            }
        }

        #endregion

        public class Flag<TParent> : Element<TParent>
        {
            private Func<TParent, PropertyBlockFlag, string> m_setter;

            public Flag(string typeOrName, Func<TParent, PropertyBlockFlag, string> setter = null) : base(typeOrName, PropertyBlockEntryType.Flag)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var flagEntry = entry as PropertyBlockFlag;
                try
                {
                    var error = (m_setter(parent, flagEntry));
                    if (error != null)
                    {
                        errors.Add(new Tuple<int, string>(entry.Line, "Flag \"" + entry.TypeOrName + "\" could not be set; " + error));
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new Tuple<int, string>(entry.Line, "Error setting flag \"" + entry.TypeOrName + "\"; " + ex.Message));
                }
            }
        }
    }
}
