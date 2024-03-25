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
            public string TypeOrName { get; private set; }
            public string AlternativeTypeOrName { get; private set; } = null;
            public PropertyBlockEntryType EntryType { get; private set; }

            public Element(string typeOrName, PropertyBlockEntryType type)
            {
                this.TypeOrName = typeOrName;
                this.EntryType = type;
            }

            public Element(string typeOrName, string altTypeOrName, PropertyBlockEntryType type)
            {
                this.TypeOrName = typeOrName;
                this.AlternativeTypeOrName = altTypeOrName;
                this.EntryType = type;
            }

            public bool TryDecode(PropertyBlockEntry entry, object parent, List<Tuple<int, string>> errors)
            {
                if (entry.BlockEntryType == this.EntryType && (entry.TypeOrName == this.TypeOrName || (this.AlternativeTypeOrName != null && entry.TypeOrName == this.AlternativeTypeOrName)))
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
            public Element(string typeOrName, PropertyBlockEntryType type) : base(typeOrName, type)
            {
            }

            public Element(string typeOrName, string altTypeOrName, PropertyBlockEntryType type) : base(typeOrName, altTypeOrName, type)
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

            public Block(string name, Func<TParent, string, TThis> creator, params Element[] childs) :
                base(name, PropertyBlockEntryType.Block)
            {
                m_creator = creator;
                m_childs = childs;
            }
            public Block(string name, string altName, Func<TParent, string, TThis> creator, params Element[] childs) :
                base(name, altName, PropertyBlockEntryType.Block)
            {
                m_creator = creator;
                m_childs = childs;
            }
            public Block(Func<TParent, string, TThis> creator, params Element[] childs) :
                this("<root>", creator, childs)
            { }

            public Block(string name, params Element[] childs) :
                this(name, null, childs)
            { }
            public Block(params Element[] childs) :
                this("<root>", null, childs)
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
            public ArrayBase(string typeOrName) : base(typeOrName, PropertyBlockEntryType.Array)
            {
            }
            public ArrayBase(string typeOrName, string altTypeOrName) : base(typeOrName, altTypeOrName, PropertyBlockEntryType.Array)
            {
            }
        }

        public class ArrayString<TParent> : ArrayBase<TParent> where TParent : class
        {
            private Func<TParent, List<string>, string> m_setter;

            public ArrayString(string typeOrName, Func<TParent, List<string>, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }
            public ArrayString(string typeOrName, string altTypeOrName, Func<TParent, List<string>, string> setter = null) : base(typeOrName, altTypeOrName)
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

            public Array(string typeOrName, Func<TParent, List<object>, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }
            public Array(string typeOrName, string altTypeOrName, Func<TParent, List<object>, string> setter = null) : base(typeOrName, altTypeOrName)
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

        public abstract class ValueBase<TParent> : Element<TParent> where TParent : class
        {
            public ValueBase(string typeOrName) : base(typeOrName, PropertyBlockEntryType.Value)
            {
            }
            public ValueBase(string typeOrName, string altTypeOrName) : base(typeOrName, altTypeOrName, PropertyBlockEntryType.Value)
            {
            }
        }

        public class ValueString<TParent> : ValueBase<TParent> where TParent : class
        {
            private Func<TParent, PropertyBlockValue, string> m_setter;

            public ValueString(string typeOrName, Func<TParent, PropertyBlockValue, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }
            public ValueString(string typeOrName, string altTypeOrName, Func<TParent, PropertyBlockValue, string> setter = null) : base(typeOrName, altTypeOrName)
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

        public class ValueBool<TParent> : ValueBase<TParent> where TParent : class
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

        public class Value<TParent> : ValueBase<TParent> where TParent : class
        {
            private Func<TParent, PropertyBlockValue, string> m_setter;

            public Value(string typeOrName, Func<TParent, PropertyBlockValue, string> setter = null) : base(typeOrName)
            {
                m_setter = setter;
            }
            public Value(string typeOrName, string altTypeOrName, Func<TParent, PropertyBlockValue, string> setter = null) : base(typeOrName, altTypeOrName)
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
