using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace StepBro.Core.Data
{
    public class TreeDataElement : ITreeDataElement, IEnumerable<ITreeDataElement>
    {
        private int m_fileLine = -1;
        private ITreeDataElement m_parent = null;
        private string m_name;
        private string m_value = null;
        private List<ITreeDataElement> m_subElements = null;
        private NamedString[] m_attributes;

        public TreeDataElement(ITreeDataElement parent, string name, string value = null, params NamedString[] attributes)
        {
            m_parent = parent;
            m_name = name;
            m_value = value;
            if (attributes.Length > 0)
            {
                m_attributes = (NamedString[])attributes.Clone();
            }
            else m_attributes = null;
        }

        #region ITreeDataElement

        public ITreeDataElement Parent
        {
            get
            {
                return m_parent;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string Value
        {
            get
            {
                return m_value;
            }
        }

        public IEnumerable<ITreeDataElement> SubElements
        {
            get
            {
                if (m_subElements != null) return m_subElements;
                else return new List<ITreeDataElement>();
            }
        }

        public bool HasValue
        {
            get
            {
                return (m_value != null);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this.HasValue == false && (m_subElements == null || m_subElements.Count == 0);
            }
        }

        public int ElementCount
        {
            get
            {
                if (m_subElements != null) return m_subElements.Count;
                else return 0;
            }
        }

        public string DataOrigin
        {
            get
            {
                return "Line " + m_fileLine.ToString();
            }
        }

        public IEnumerable<NamedString> Attributes
        {
            get
            {
                if (m_attributes == null) return new NamedString[] { };
                else return m_attributes;
            }
        }

        #endregion

        public TreeDataElement AddElement(TreeDataElement element)
        {
            element.m_parent = this;
            if (m_subElements == null) m_subElements = new List<ITreeDataElement>();
            m_subElements.Add(element);
            return element;
        }

        #region XML File Reading

        public void Read(XmlTextReader reader)
        {
            m_fileLine = reader.LineNumber;

            if (reader.HasAttributes)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    if (m_subElements == null) m_subElements = new List<ITreeDataElement>();
                    m_subElements.Add(new TreeDataElement(this, reader.Name, reader.Value));
                }
                reader.MoveToElement();
            }
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        m_value = reader.ReadString();
                        return;

                    case XmlNodeType.Element:
                        {
                            TreeDataElement sub = new TreeDataElement(this, reader.LocalName);
                            if (m_subElements == null) m_subElements = new List<ITreeDataElement>();
                            m_subElements.Add(sub);

                            if (reader.IsEmptyElement)
                            {
                                continue;
                            }
                            else
                            {
                                sub.Read(reader);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                    case XmlNodeType.Whitespace:
                        return;
                    case XmlNodeType.Comment:
                        break;
                    default:
                        throw new Exception("Error reading file in line " + reader.LineNumber.ToString() + ".");
                }
            }
        }

        public static TreeDataElement ReadFile(string path)
        {
            using (var reader = new XmlTextReader(path))
            {
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.Read();
                if (reader.NodeType != XmlNodeType.XmlDeclaration)
                {
                    throw new Exception("Error reading file in line " + reader.LineNumber.ToString() + ".");
                }
                reader.Read();
                while (reader.NodeType == XmlNodeType.Comment)
                {
                    reader.Read();
                }
                TreeDataElement root = new TreeDataElement(null, reader.LocalName);
                root.Read(reader);
                return root;
            }
        }

        #endregion

        public override string ToString()
        {
            if (this.HasValue)
            {
                return String.Format("Element '{0}': {1}'", this.Name, this.Value);
            }
            else
            {
                return String.Format("Element '{0}'", this.Name);
            }
        }

        public IEnumerator<ITreeDataElement> GetEnumerator()
        {
            return ((IEnumerable<ITreeDataElement>)m_subElements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ITreeDataElement>)m_subElements).GetEnumerator();
        }
    }
}