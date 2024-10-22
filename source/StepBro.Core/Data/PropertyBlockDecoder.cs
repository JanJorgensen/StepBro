using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.Core.Data
{
    public class PropertyBlockDecoder
    {
        public abstract class Element
        {
            public string TypeOrName { get; private set; } = null;
            public string AlternativeTypeOrName { get; private set; } = null;
            public string Documentation { get; private set; } = null;
            public PropertyBlockEntryType EntryType { get; private set; }

            public Element(DocString doc, PropertyBlockEntryType type)
            {
                this.Documentation = doc.Text;
                this.EntryType = type;
            }

            public Element(string typeOrName, DocString doc, PropertyBlockEntryType type)
            {
                this.TypeOrName = typeOrName;
                this.Documentation = doc.Text;
                this.EntryType = type;
            }

            public Element(string typeOrName, string altTypeOrName, DocString doc, PropertyBlockEntryType type)
            {
                this.TypeOrName = typeOrName;
                this.AlternativeTypeOrName = altTypeOrName;
                this.Documentation = doc.Text;
                this.EntryType = type;
            }

            public bool TryDecode(PropertyBlockEntry entry, object parent, List<Tuple<int, string>> errors)
            {
                if (    entry.BlockEntryType == this.EntryType && 
                        (
                            String.IsNullOrEmpty(this.TypeOrName) ||
                            entry.TypeOrName == this.TypeOrName || 
                            (this.AlternativeTypeOrName != null && entry.TypeOrName == this.AlternativeTypeOrName)
                        )
                   )
                {
                    this.TryCreateOrSet(parent, entry, errors);
                    return true;    // There might be errors, but name and type did match.
                }

                return false;   // Not me...
            }

            protected abstract void TryCreateOrSet(object parent, PropertyBlockEntry entry, List<Tuple<int, string>> errors);
        }

        public abstract class Element<TParent> : Element where TParent : class
        {
            public Element(DocString doc, PropertyBlockEntryType type) : base(doc, type)
            {
            }

            public Element(string typeOrName, DocString doc, PropertyBlockEntryType type) : base(typeOrName, doc, type)
            {
            }

            public Element(string typeOrName, string altTypeOrName, DocString doc, PropertyBlockEntryType type) : base(typeOrName, altTypeOrName, doc, type)
            {
            }

            protected override void TryCreateOrSet(object parent, PropertyBlockEntry entry, List<Tuple<int, string>> errors)
            {
                var typedParent = parent as TParent;
                if (parent == null || typedParent != null)
                {
                    this.TryCreateOrSet(entry, typedParent, errors);
                }
                else
                {
                    errors.Add(new Tuple<int, string>(entry.Line, $"The '{this.GetType().Name}' cannot be added to a parent of type '{parent.GetType().Name}'."));
                }
            }

            protected abstract void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors);
        }

        public class Block<TParent, TThis> : Element<TParent> where TParent : class
        {
            private Func<TParent, string, TThis> m_creator;
            private Element[] m_childs;

            public Block(string name, DocString doc, Func<TParent, string, TThis> creator, params Element[] childs) :
                base(name, doc, PropertyBlockEntryType.Block)
            {
                m_creator = creator;
                m_childs = childs;
            }
            public Block(string name, string altName, DocString doc, Func<TParent, string, TThis> creator, params Element[] childs) :
                base(name, altName, doc, PropertyBlockEntryType.Block)
            {
                m_creator = creator;
                m_childs = childs;
            }
            public Block(DocString doc, Func<TParent, string, TThis> creator, params Element[] childs) :
                this((string)null, doc, creator, childs)
            { }

            public Block(string name, DocString doc, params Element[] childs) :
                this(name, (string)null, doc, null, childs)
            { }
            public Block(string name, string altName, DocString doc, params Element[] childs) :
                this(name, altName, doc, null, childs)
            { }
            public Block(DocString doc, params Element[] childs) :
                this((string)null, (string)null, doc, null, childs)
            { }

            public void SetChilds(params Element[] childs)
            {
                m_childs = childs;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_creator == null) return;
                string name = entry.HasTypeSpecified ? entry.Name : null;
                var data = m_creator(parent, name);
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

        #region Arrays

        public abstract class ArrayBase<TParent> : Element<TParent> where TParent : class
        {
            public ArrayBase(string typeOrName, DocString doc) : base(typeOrName, doc, PropertyBlockEntryType.Array)
            {
            }
            public ArrayBase(string typeOrName, string altTypeOrName, DocString doc) : base(typeOrName, altTypeOrName, doc, PropertyBlockEntryType.Array)
            {
            }
        }

        public class ArrayString<TParent> : ArrayBase<TParent> where TParent : class
        {
            private Func<TParent, List<string>, string> m_setter;

            public ArrayString(string typeOrName, DocString doc, Func<TParent, List<string>, string> setter = null) : base(typeOrName, doc)
            {
                m_setter = setter;
            }
            public ArrayString(string typeOrName, string altTypeOrName, DocString doc, Func<TParent, List<string>, string> setter = null) : base(typeOrName, altTypeOrName, doc)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var arrayEntry = entry as PropertyBlockArray;
                bool allAreStrings = arrayEntry.All(e => e.BlockEntryType == PropertyBlockEntryType.Value && (e as PropertyBlockValue).IsStringOrIdentifier);
                if (allAreStrings)
                {
                    var stringList = arrayEntry.Select(e => (e as PropertyBlockValue).ValueAsString()).ToList();
                    try
                    {
                        var error = m_setter(parent, stringList);
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
                    errors.Add(new Tuple<int, string>(entry.Line, "Value for \"" + entry.TypeOrName + "\"should be an array of strings."));
                }
            }
        }

        public class Array<TParent> : ArrayBase<TParent> where TParent : class
        {
            private Func<TParent, List<object>, string> m_setter;

            public Array(string typeOrName, DocString doc, Func<TParent, List<object>, string> setter = null) : base(typeOrName, doc)
            {
                m_setter = setter;
            }
            public Array(string typeOrName, string altTypeOrName, DocString doc, Func<TParent, List<object>, string> setter = null) : base(typeOrName, altTypeOrName, doc)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var arrayEntry = entry as PropertyBlockArray;
                var arguments = arrayEntry.Select(e => (e as PropertyBlockValue).Value).ToList();
                try
                {
                    var error = m_setter(parent, arguments);
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
        }

        #endregion

        #region Values

        public delegate string ValueSetter<in TParent>(TParent parent, PropertyBlockValue value);

        public abstract class ValueBase<TParent> : Element<TParent> where TParent : class
        {
            public ValueBase(DocString doc) : base(doc, PropertyBlockEntryType.Value)
            {
            }
            public ValueBase(string typeOrName, DocString doc) : base(typeOrName, doc, PropertyBlockEntryType.Value)
            {
            }
            public ValueBase(string typeOrName, string altTypeOrName, DocString doc) : base(typeOrName, altTypeOrName, doc, PropertyBlockEntryType.Value)
            {
            }
        }

        public class ValueString<TParent> : ValueBase<TParent> where TParent : class
        {
            private ValueSetter<TParent> m_setter;

            public ValueString(string typeOrName, DocString doc, ValueSetter<TParent> setter = null) : base(typeOrName, doc)
            {
                m_setter = setter;
            }
            public ValueString(string typeOrName, string altTypeOrName, DocString doc, ValueSetter<TParent> setter = null) : base(typeOrName, altTypeOrName, doc)
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
                        var error = m_setter(parent, valueEntry);
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

        public class ValueInt<TParent> : ValueBase<TParent> where TParent : class
        {
            private ValueSetter<TParent> m_setter;

            public ValueInt(string typeOrName, DocString doc, ValueSetter<TParent> setter = null) : base(typeOrName, doc)
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

        public class ValueBool<TParent> : ValueBase<TParent> where TParent : class
        {
            private ValueSetter<TParent> m_setter;

            public ValueBool(string typeOrName, DocString doc, ValueSetter<TParent> setter = null) : base(typeOrName, doc)
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

        public class Value<TParent> : ValueBase<TParent> where TParent : class
        {
            private ValueSetter<TParent> m_setter;

            public Value(DocString doc, ValueSetter<TParent> setter = null) : base(doc)
            {
                m_setter = setter;
            }
            public Value(string typeOrName, DocString doc, ValueSetter<TParent> setter = null) : base(typeOrName, doc)
            {
                m_setter = setter;
            }
            public Value(string typeOrName, string altTypeOrName, DocString doc, ValueSetter<TParent> setter = null) : base(typeOrName, altTypeOrName, doc)
            {
                m_setter = setter;
            }

            protected override void TryCreateOrSet(PropertyBlockEntry entry, TParent parent, List<Tuple<int, string>> errors)
            {
                if (m_setter == null) return;
                var valueEntry = entry as PropertyBlockValue;
                try
                {
                    var error = m_setter(parent, valueEntry);
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
        }

        #endregion

        public class Flag<TParent> : Element<TParent> where TParent : class
        {
            private Func<TParent, PropertyBlockFlag, string> m_setter;

            public Flag(string typeOrName, DocString doc, Func<TParent, PropertyBlockFlag, string> setter = null) : base(typeOrName, doc, PropertyBlockEntryType.Flag)
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

        public class DocString
        {
            private string m_text;
            public DocString(string text) { m_text = text; }
            public string Text { get { return m_text; } }
        }

        public static DocString Doc(string text) { return new DocString(text); }
    }
}
