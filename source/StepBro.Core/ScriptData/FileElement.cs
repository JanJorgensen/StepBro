using StepBro.Core.Data;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core.ScriptData
{
    public abstract class FileElement : IFileElement
    {
        private IScriptFile m_parentFile;
        private readonly AccessModifier m_accessModifier;
        private readonly int m_line;
        private string m_baseElementName = null;
        private IFileElement m_baseElement;
        private IFileElement m_parentElement;
        private string m_elementName;
        private string m_elementFullName = null;
        private FileElementType m_elementType;
        private string m_purpose;
        private static int g_nextID = 1000;
        private readonly int m_uid = g_nextID++;
        protected PropertyBlock m_propertyBlock = null;
        private readonly List<IPartner> m_partners = new List<IPartner>();

        public FileElement(IScriptFile file, int line, IFileElement parentElement, string @namespace, string name, AccessModifier access, FileElementType type)
        {
            m_parentFile = file;
            m_line = line;
            m_baseElement = null;
            m_parentElement = parentElement;
            m_elementName = name;
            m_accessModifier = access;
            m_elementType = type;
            if (String.IsNullOrEmpty(@namespace))
            {
                m_elementFullName = m_elementName;
            }
            else
            {
                m_elementFullName = @namespace + "." + m_elementName;
            }
            //if (m_parentElement != null)
            //{
            //    m_elementFullName = m_parentElement.FullName + "." + m_elementName;
            //}
            //else
            //{
            //}
        }

        public override string ToString()
        {
            return $"{m_uid} {m_elementType} {m_elementName}";
        }

        public string Name { get { return m_elementName; } }

        internal void SetName(string @namespace, string name)
        {
            m_elementName = name;
            m_elementFullName = @namespace + "." + name;
        }

        public FileElementType ElementType
        {
            get
            {
                return m_elementType;
            }
            internal set { m_elementType = value; }
        }

        public string FullName { get { return m_elementFullName; } }

        public AccessModifier AccessLevel { get { return m_accessModifier; } }

        public IFileElement BaseElement
        {
            get
            {
                return m_baseElement;
            }
            internal set { m_baseElement = value; }
        }

        public IFileElement ParentElement
        {
            get
            {
                return m_parentElement;
            }
            internal set { m_parentElement = value; }
        }

        public IScriptFile ParentFile
        {
            get
            {
                return m_parentFile;
            }
            internal set { m_parentFile = value; }
        }

        public string Purpose
        {
            get
            {
                return m_purpose;
            }
            internal set { m_purpose = value; }
        }

        public IdentifierType Type
        {
            get
            {
                return IdentifierType.FileElement;
            }
        }

        public TypeReference DataType
        {
            get
            {
                return this.GetDataType();
            }
        }

        protected abstract TypeReference GetDataType();

        public object Reference
        {
            get
            {
                return this;
            }
        }

        public int UniqueID
        {
            get
            {
                return m_uid;
            }
        }

        public int Line
        {
            get { return m_line; }
        }

        internal virtual int ParseSignature(StepBroListener listener, bool reportErrors)
        {
            return 0;
        }

        public IEnumerable<IPartner> ListPartners()
        {
            foreach (var p in m_partners) yield return p;
            if (this.BaseElement != null)
            {
                foreach (var p in this.BaseElement.ListPartners())
                {
                    yield return p;
                }
            }
        }

        internal void SetPropertyBlockData(PropertyBlock data)
        {
            if (data != null && m_propertyBlock != null)
            {
                throw new Exception("The propertyblock has already been set.");
            }
            m_propertyBlock = data;
        }

        internal bool ParseBaseElement()
        {
            if (!String.IsNullOrEmpty(m_baseElementName) && m_parentFile != null)
            {
                var parentFile = m_parentFile as ScriptFile;
                var files = new List<ScriptFile>();
                files.Add(parentFile);
                files.AddRange(parentFile.ListResolvedFileUsings());
                files.AddRange(parentFile.ListResolvedNamespaceUsings().Where(u => u.Type == IdentifierType.FileNamespace).SelectMany(u => ((IEnumerable<ScriptFile>)u.Reference)));
                foreach (var f in files)
                {
                    foreach (var e in f.ListElements())
                    {
                        if (e.ElementType == this.ElementType && e.Name.Equals(m_baseElementName, StringComparison.InvariantCulture))
                        {
                            this.BaseElement = e;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected virtual bool ParsePropertyBlockFlag(string name)
        {
            return false;
        }

        internal virtual void ParsePropertyBlock(StepBroListener listener)
        {
            if (m_propertyBlock == null) return;
            int i = -1;
            foreach (var entry in m_propertyBlock)
            {
                i++;    // Increment here; first value will be 0.
                if (entry.BlockEntryType == PropertyBlockEntryType.Flag)
                {
                    if (this.ParsePropertyBlockFlag(entry.Name))
                    {
                        continue;   // Flag was a known one
                    }
                    if (i == 0)
                    {
                        m_baseElementName = entry.Name;
                        if (!this.ParseBaseElement())
                        {
                            throw new ParsingErrorException(entry.Line, m_baseElementName, "Base element unknown.");
                        }
                    }
                    continue;
                }
                if (entry.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueProp = entry as PropertyBlockValue;
                    var typename = entry.SpecifiedTypeName;
                    object value = valueProp.Value;

                    if (this.ParsePartnerProperty(listener, entry.Line, typename, entry.Name, value)) continue;
                }

            }
        }

        private bool ParsePartnerProperty(StepBroListener listener, int line, string type, string name, object value)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrWhiteSpace(name));
            if (type != null &&
                (type.Equals("partner", StringComparison.InvariantCulture) ||
                type.Equals("partner new", StringComparison.InvariantCulture) ||
                type.Equals("partner override", StringComparison.InvariantCulture)))
            {
                string reference = null;
                if (value is string)
                {
                    reference = value as string;
                }
                else if (value is Identifier)
                {
                    reference = ((Identifier)value).Name;
                }
                else
                {
                    throw new ParsingErrorException(line, name, "Value is not a string or an identifier.");
                }

                var element = listener.TryGetFileElementInScope(reference);
                if (element != null)
                {
                    if (element is IFileProcedure)
                    {
                        m_partners.Add(new FileElementPartner(this, name, reference, element as IFileProcedure));
                    }
                    else
                    {
                        throw new ParsingErrorException(line, name, $"Element '{reference}' is not a procedure.");
                    }
                }
                else
                {
                    throw new ParsingErrorException(line, name, $"Element '{reference}' was not found.");
                }

                return true;
            }
            return false;
        }
    }
}
