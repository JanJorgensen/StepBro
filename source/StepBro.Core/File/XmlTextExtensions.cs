using System;
using System.Xml;
using StepBro.Core.Data;

namespace StepBro.Core.File
{
    /// <summary>
    /// Extention to XmlTextWriter with many convenience methods.
    /// </summary>
    public static class XmlTextExtensions
    {
        #region XmlTextWriter Extensions

        /// <summary>
        /// Write a datetime attribute to the current element in a format which can be read back by XmlDocReader.
        /// </summary>
        public static void WriteAttribute(this XmlTextWriter writer, string name, DateTime dateTime)
        {
            //writer.WriteAttributeString(name, DateTimeUtils.ToString(dateTime));
        }

        /// <summary>
        /// Write a boolean attribute which can be read back by XmlDocReader.
        /// </summary>
        public static void WriteAttribute(this XmlTextWriter writer, string name, bool value)
        {
            writer.WriteAttributeString(name, value ? "true" : "false");
        }

        /// <summary>
        /// Write an element which contains a single boolean value.
        /// </summary>
        public static void WriteElementBoolean(this XmlTextWriter writer, string name, bool value)
        {
            writer.WriteElementString(name, value ? "true" : "false");
        }

        /// <summary>
        /// Write an element which contains a single boolean value.
        /// </summary>
        public static void WriteElementBoolean(this XmlTextWriter writer, string name, string ns, bool value)
        {
            writer.WriteElementString(name, ns, value ? "true" : "false");
        }

        //public static void WritePrimitiveKeyValueElement(this XmlTextWriter writer, string name, string key, object value)
        //{
        //    writer.WriteStartElement(name);
        //    writer.WriteAttributeString("name", key);
        //    writer.WriteAttributeString("type", value.GetType().Name);
        //    writer.WriteString(TypeUtils.ToSerializedString(value));
        //    writer.WriteEndElement();
        //}

        #endregion

        #region XmlTextReader Extensions

        //bool m_lastReadWasOuterXml = false;
        //bool m_enableReadOuterXmlFix = true;

        ///// <see cref="System.Xml.XmlTextReader.Read()"/>
        //public override bool Read()
        //{
        //    if (m_lastReadWasOuterXml)
        //    {
        //        m_lastReadWasOuterXml = false;
        //        return this.NodeType != XmlNodeType.None;
        //        //return true;
        //    }
        //    else
        //    {
        //        return base.Read();
        //    }
        //}

        ///// <see cref="System.Xml.XmlReader.ReadOuterXml()" />
        //public override string ReadOuterXml()
        //{
        //    if (m_enableReadOuterXmlFix)
        //    {
        //        m_lastReadWasOuterXml = false;
        //        string ret = base.ReadOuterXml();
        //        m_lastReadWasOuterXml = true;
        //        return ret;
        //    }
        //    else
        //    {
        //        return base.ReadOuterXml();
        //    }
        //}

        /// <summary>
        /// Read the innertext of the current element if the name is correct.
        /// </summary>
        /// <param name="name">Expected (required) name of the current element.</param>
        /// <param name="value">Found innertext of the element.</param>
        /// <returns>true if the element was found and contained text, false otherwise.</returns>
        public static bool ReadEntry(this XmlTextReader reader, string name, ref string value)
        {
            if (reader.LocalName.ToLower() == name.ToLower())
            {
                if (reader.IsEmptyElement)
                {
                    value = "";
                    return true;
                }
                reader.Read();
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    value = "";
                    return true;
                }
                else if (reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.Whitespace)
                {
                    System.Diagnostics.Debug.Fail("File-loading error!");
                    throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Expected text data in element. ");
                }

                value = reader.ReadString();
                return true;
            }
            else
                return false;
        }

        public static bool ReadEntry(this XmlTextReader reader, ref string value)
        {
            return reader.ReadEntry(reader.LocalName, ref value);   // User the name we're actually at.
        }

        public static bool ReadEntry(this XmlTextReader reader, string name, System.Action<string> setter)
        {
            string s = "";
            if (reader.ReadEntry(name, ref s))
            {
                setter(s);
                return true;
            }
            return false;
        }

        //public static bool ReadEntry(this XmlTextReader reader, string name, UndoRedoProperty<string> property)
        //{
        //    string value = "";
        //    if (reader.ReadEntry(name, ref value))
        //    {
        //        property.SetValueDirect(value, false);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public static bool ReadEntry(this XmlTextReader reader, string name, ref bool value)
        {
            string valuestring = null;
            if (reader.ReadEntry(name, ref valuestring))
            {
                if (!TypeUtils.TryParse(valuestring, out value))
                {
                    System.Diagnostics.Debug.Fail("File-loading error!");
                    throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Value does not evaluate to a boolean ( " + valuestring + " ). ");
                }
                return true;
            }
            return false;
        }

        public static bool ReadEntry(this XmlTextReader reader, string name, System.Action<bool> setter)
        {
            bool b = false;
            if (reader.ReadEntry(name, ref b))
            {
                setter(b);
                return true;
            }
            return false;
        }

        //public static bool ReadEntry(this XmlTextReader reader, string name, UndoRedoProperty<bool> property)
        //{
        //    bool value = false;
        //    if (reader.ReadEntry(name, ref value))
        //    {
        //        property.SetValueDirect(value, false);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public static bool ReadEntry(this XmlTextReader reader, string name, ref Int32 value)
        {
            string valuestring = null;
            if (reader.ReadEntry(name, ref valuestring))
            {
                if (!Int32.TryParse(valuestring, out value))
                {
                    System.Diagnostics.Debug.Fail("File-loading error!");
                    throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Value does not evaluate to an integer value ( " + valuestring + " ). ");
                }
                return true;
            }
            return false;
        }

        //public static bool ReadEntry(this XmlTextReader reader, string name, UndoRedoProperty<Int32> property)
        //{
        //    Int32 value = 0;
        //    if (reader.ReadEntry(name, ref value))
        //    {
        //        property.SetValueDirect(value, false);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public static bool ReadEntry(this XmlTextReader reader, string name, ref Int64 value)
        {
            string valuestring = null;
            if (reader.ReadEntry(name, ref valuestring))
            {
                if (!Int64.TryParse(valuestring, out value))
                {
                    System.Diagnostics.Debug.Fail("File-loading error!");
                    throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Value does not evaluate to an integer value ( " + valuestring + " ). ");
                }
                return true;
            }
            return false;
        }

        //public static bool ReadEntry(this XmlTextReader reader, string name, UndoRedoProperty<Int64> property)
        //{
        //    Int64 value = 0L;
        //    if (this.ReadEntry(name, ref value))
        //    {
        //        property.SetValueDirect(value, false);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// Read the inner text of the current element named _name as an enum value.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum that the text is a value of</typeparam>
        /// <param name="name">The name of the current element</param>
        /// <param name="value">Found enum value.</param>
        /// <returns>true if the value was correctly retrieved, false otherwise.</returns>
        public static bool ReadEntry<TEnum>(this XmlTextReader reader, string name, ref TEnum value) where TEnum : struct
        {
            if (reader.LocalName == name)
            {
                reader.Read();
                if (reader.NodeType != XmlNodeType.Text)
                {
                    System.Diagnostics.Debug.Fail("File-loading error!");
                    throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Expected text data in element. ");
                }
                string str = reader.ReadString();
                try
                {
                    value = (TEnum)Enum.Parse(typeof(TEnum), str);
                    return true;
                }
                catch (ArgumentNullException)
                {
                    if (!typeof(TEnum).IsEnum)
                    {
                        System.Diagnostics.Debug.Fail("File-loading error!");
                        throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". The data type being parsed by XmlDocReader.ReadEntry(string,System.Type) in not an enum-type. ");
                    }
                    else if (str.Trim() == "")
                    {
                        System.Diagnostics.Debug.Fail("File-loading error!");
                        throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Expected text data in element but element was empty. ");
                    }
                    else
                    {
                        System.Diagnostics.Debug.Fail("File-loading error!");
                        throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Unexpected element value. ");
                    }
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Debug.Fail("File-loading error!");
                    throw new XmlDataReadingException("Error reading file element \"" + name + "\" in line " + reader.LineNumber + ". Unexpected element value. ");
                }
            }
            else
            {
                return false;
            }
        }

        //public static bool ReadEntry<TEnum>(this XmlTextReader reader, string name, UndoRedoProperty<TEnum> property) where TEnum : struct
        //{
        //    TEnum value = default(TEnum);
        //    if (reader.ReadEntry<TEnum>(name, ref value))
        //    {
        //        property.SetValueDirect(value, false);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public static bool ReadEntry<T>(this XmlTextReader reader, string name, ref KeyValuePair<string, T> nameAndValue) where T : struct
        //{
        //    if (reader.LocalName.ToLower() == name.ToLower() && reader.HasAttributes)
        //    {
        //        if (reader.IsEmptyElement)
        //        {
        //            nameAndValue = default(KeyValuePair<string, T>);
        //            return true;
        //        }
        //        string n = reader.GetAttribute("name");
        //        string type = reader.GetAttribute("type");
        //        reader.Read();
        //        if (reader.NodeType == XmlNodeType.EndElement)
        //        {
        //            nameAndValue = new KeyValuePair<string, T>(n, default(T));
        //            return true;
        //        }
        //        else if (reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.Whitespace)
        //        {
        //            System.Diagnostics.Debug.Fail("File-loading error!");
        //            throw new XmlDataReadingException("Error reading file element \"" + n + "\" in line " + reader.LineNumber + ". Expected text data in element. ");
        //        }

        //        string value = reader.ReadString();
        //        object created = TypeUtils.GetValueFromString(type, value);
        //        nameAndValue = new KeyValuePair<string, T>(n, (T)created);
        //        return true;
        //    }
        //    else
        //        return false;
        //}

        public static void EatSection(this XmlTextReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        reader.EatSection();
                        break;
                    case XmlNodeType.Text:
                        reader.ReadString();
                        return;
                    case XmlNodeType.EndElement:
                        return;
                    default:
                        throw new XmlDataReadingException("Error reading file in line " + reader.LineNumber.ToString() + ".");
                }
            }
            throw new XmlDataReadingException("Error reading file. Unexpected end of file.");
        }

        /// <summary>
        /// Behaves as ReadUntilElement() but will stop immediately if the cursor is already on an element.
        /// </summary>
        /// <see cref="ReadUntilElement()"/>
        public static bool ElementOrReadUntil(this XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                return true;
            }
            return reader.ReadUntilElement();
        }

        /// <summary>
        /// Behaves as ReadUntilElement(string) but will stop immediately if the cursor is already on an element with the wanted name.
        /// </summary>
        /// <see cref="ReadUntilElement(string)"/>
        public static bool ElementOrReadUntil(this XmlTextReader reader, string _name)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == _name)
            {
                return true;
            }
            return reader.ReadUntilElement(_name);
        }

        /// <summary>
        /// Keep calling Read() until an element is found.
        /// </summary>
        /// <returns>true if an element was found, false otherwise.</returns>
        public static bool ReadUntilElement(this XmlTextReader reader)
        {
            while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }

            return reader.NodeType == XmlNodeType.Element;
        }

        /// <summary>
        /// Keep calling Read() until an element with the given name is found.
        /// </summary>
        /// <returns>true if an element with the given _name was found, false otherwise.</returns>
        public static bool ReadUntilElement(this XmlTextReader reader, string _name)
        {
            while (reader.Read() && (reader.NodeType != XmlNodeType.Element || reader.Name != _name)) { }

            return reader.NodeType == XmlNodeType.Element && reader.Name == _name;
        }

        /// <summary>
        /// Get a reader like in ReadDocSubtree() but positioned the reader "inside" the current element, this means that you should not start by doing a subReader.Read(), but rather use the do/while idiom to read, otherwise you will read over the first element in the file.
        ///
        /// <code>
        /// XmlDocReader subReader = reader.ReadDocSubtreeInner();
        /// do {
        /// //
        /// } while ( subReader.Read() );
        /// </code>
        /// </summary>
        public static XmlTextReader ReadDocSubtreeInner(this XmlTextReader reader)
        {
            string element = reader.Name;
            bool empty = reader.IsEmptyElement;
            XmlTextReader newReader = reader.ReadDocSubtree();
            if (!empty)
            {
                newReader.ReadUntilElement(element);
            }

            newReader.Read();

            // Since we are reading the inner sub-tree, we skip the current element and any white space that might come before the first element.
            //while ( reader.Read() && reader.NodeType == XmlNodeType.Whitespace )
            //   ;

            return newReader;
        }

        /// <summary> 
        /// Get a sub-XmlDocReader to protect the rest of the xml
        /// reading process from a badly designed reader - and also to
        /// make writing sub-readers as simple as writing a
        /// primary-reader, that is: just use the standard while
        /// (reader.Read() ) { /* handle */ } idiom.
        ///
        /// The returned reader is positioned just before the element
        /// this reader is positioned on.
        /// </summary>
        public static XmlTextReader ReadDocSubtree(this XmlTextReader reader)
        {
            string data = reader.ReadOuterXml();
            XmlTextReader subReader = new XmlTextReader(data);
            subReader.WhitespaceHandling = reader.WhitespaceHandling;
            return subReader;
        }

        /// <summary>
        /// Retrieve the value of an attribute as a int value.
        /// </summary>
        public static int GetAttributeAsInt(this XmlTextReader reader, string _name, int defaultValue)
        {
            string value = reader.GetAttribute(_name);
            if (String.IsNullOrEmpty(value))
                return defaultValue;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Retrieve the value of an attribute as an uint value.
        /// </summary>
        public static uint GetAttributeAsUInt(this XmlTextReader reader, string _name, uint defaultValue)
        {
            string value = reader.GetAttribute(_name);
            if (String.IsNullOrEmpty(value))
                return defaultValue;
            return Convert.ToUInt32(value);
        }

        /// <summary>
        /// Retrieve the value of an attribute as an uint value.
        /// </summary>
        public static uint GetAttributeAsUint(this XmlTextReader reader, string _name)
        {
            string value = reader.GetAttribute(_name);
            return Convert.ToUInt32(value);
        }

        /// <summary>
        /// Retrieve an attribute as a DateTime value (uses the format defined in XmlDocWriter.DATE_TIME_FORMAT).
        /// </summary>
        public static DateTime GetAttributeAsDateTime(this XmlTextReader reader, string name)
        {
            return reader.GetAttribute(name).ParseDateTime();
        }

        /// <summary>
        /// Retrive the attribute value as an enum value.
        /// </summary>
        /// <see cref="TryGetAttributeAsEnum"/>
        public static TEnum GetAttributeAsEnum<TEnum>(this XmlTextReader reader, string _name)
        {
            string value = reader.GetAttribute(_name);
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }

        /// <summary>
        /// Try to retrieve an attribute which contains a string name of an enum value.
        /// </summary>
        /// <typeparam name="TEnum">The type of enum to search for.</typeparam>
        /// <param name="_name">The name of the attribute holding the enum value.</param>
        /// <param name="_value">The found value, if any.</param>
        /// <returns>true if the enum value was successfully retrieved</returns>
        public static bool TryGetAttributeAsEnum<TEnum>(this XmlTextReader reader, string _name, ref TEnum _value)
        {
            string value = reader.GetAttribute(_name);
            if (value != null)
            {
                _value = (TEnum)Enum.Parse(typeof(TEnum), value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieve the boolean value of the attribute _name.
        /// </summary>
        public static bool GetAttributeAsBool(this XmlTextReader reader, string _name)
        {
            string value = reader.GetAttribute(_name).ToLowerInvariant();
            return (value == "true" || value == "yes" || value == "ok" || value == "1");
        }

        /// <summary>
        /// Retrieve the boolean value of the attribute named "_name", if the attribute can not be found this method returns the "_defaultValue".
        /// </summary>
        public static bool GetAttributeAsBool(this XmlTextReader reader, string _name, bool _defaultValue)
        {
            string value = reader.GetAttribute(_name);
            if (value == null)
            {
                return _defaultValue;
            }
            else
            {
                value = value.ToLowerInvariant();
                return (value == "true" || value == "yes" || value == "ok" || value == "1");
            }
        }
        #endregion
    }
}
