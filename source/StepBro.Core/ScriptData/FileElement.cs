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
        private AccessModifier m_accessModifier;
        private int m_line;
        private string m_baseElementName = null;
        private IFileElement m_baseElement;
        private IFileElement m_parentElement;
        private string m_elementName;
        private string m_elementFullName = null;
        private FileElementType m_elementType;
        private string m_summary;
        private string m_docReference;
        private static int g_nextID = 1000;
        protected readonly int m_uid = g_nextID++;
        protected PropertyBlock m_propertyBlock = null;
        private readonly List<IPartner> m_partners = new List<IPartner>();

        public FileElement(IScriptFile file, int line, IFileElement parentElement, string @namespace, string name, AccessModifier access, FileElementType type)
        {
            //System.Diagnostics.Debug.WriteLine(
            //    "~~~~~~~~ FILE ELEMENT " +
            //    type.ToString().ToUpper() + ": " +
            //    m_uid.ToString() + " " +
            //    ((file != null) ? file.FileName : "<no file>") + " - " +
            //    (String.IsNullOrEmpty(@namespace) ? "" : @namespace) + " " +
            //    name);

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

        public AccessModifier AccessLevel
        {
            get
            {
                return m_accessModifier;
            }
            internal set
            {
                m_accessModifier = value;
            }
        }

        internal string BaseElementName
        {
            get { return m_baseElementName; }
            set { m_baseElementName = value; }
        }

        public IFileElement BaseElement
        {
            get
            {
                return m_baseElement;
            }
            internal set
            {
                // Value can be null if the value the element is overriding has not been defined.
                // This is for example the case when we have a Device with a specific name, but that name is not defined in the station properties file
                // and we override that with an element that is defined in the station properties file. (Check issue #172)
                // Note: This only makes the error message not be an internal error, it does not make it possible to create an
                //       element with an element that is not defined.
                if (value != null)
                {
                    // Assume not the same element.
                    System.Diagnostics.Debug.Assert(!object.ReferenceEquals(value, this));
                    // Assume not both override elements or elements are from different files.
                    System.Diagnostics.Debug.Assert(this.ElementType != FileElementType.Override || value.ElementType != FileElementType.Override || !Object.ReferenceEquals(m_parentFile, value.ParentFile));
                    m_baseElement = value;
                }
            }
        }

        public FileElement GetRootBaseElement()
        {
            if (m_baseElement != null && m_elementType == FileElementType.Override)
            {
                return ((FileElement)m_baseElement).GetRootBaseElement();
            }
            return (FileElement)((m_baseElement != null) ? m_baseElement : this);
        }

        public bool IsA(IFileElement elementType)
        {
            if (Object.ReferenceEquals(this, elementType)) return true;
            if (m_baseElement == null) return false;
            return ((FileElement)m_baseElement).IsA(elementType);
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

        public string Summary
        {
            get
            {
                return m_summary;
            }
            internal set { m_summary = value; }
        }

        public string DocReference
        {
            get
            {
                return m_docReference;
            }
            internal set { m_docReference = value; }
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

        object IIdentifierInfo.Reference
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
            internal set { m_line = value; }
        }

        IInheritable IInheritable.Base { get { return this.BaseElement as IInheritable; } }

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
            var baseName = (this.ElementType == FileElementType.Override) ? m_elementName : m_baseElementName;
            if (!String.IsNullOrEmpty(baseName) && m_parentFile != null)
            {
                var file = m_parentFile as ScriptFile;

                bool allowOverrideElements = false;
                switch (this.ElementType)
                {
                    case FileElementType.Override:
                        allowOverrideElements = true;
                        break;
                    case FileElementType.Using:
                    case FileElementType.Namespace:
                    case FileElementType.EnumDefinition:
                    case FileElementType.ProcedureDeclaration:
                    case FileElementType.FileVariable:
                    case FileElementType.TestList:
                    case FileElementType.Datatable:
                    case FileElementType.TypeDef:
                        break;
                    default:
                        break;
                }

                var found = file.LookupIdentifier(
                    baseName,
                    predicate: (IIdentifierInfo id) => (
                        id.Type == IdentifierType.FileElement &&
                        !Object.ReferenceEquals(id, this) &&
                        (allowOverrideElements || ((FileElement)id).ElementType != FileElementType.Override)));
                var element = (found != null) ? (found[0] as IFileElement) : null;
                if (element != null)
                {
                    this.BaseElement = element;
                    return true;
                }

                //var parentFile = m_parentFile as ScriptFile;
                //var files = new List<ScriptFile>();
                //files.Add(parentFile);
                //files.AddRange(parentFile.ListResolvedFileUsings());
                //files.AddRange(parentFile.ListResolvedNamespaceUsings().Where(u => u.Type == IdentifierType.FileNamespace).SelectMany(u => ((IEnumerable<ScriptFile>)u.Reference)));
                //foreach (var f in files)
                //{
                //    foreach (var e in f.ListElements())
                //    {
                //        if (e.ElementType == this.ElementType && e.Name.Equals(m_baseElementName, StringComparison.InvariantCulture))
                //        {
                //            this.BaseElement = e;
                //            return true;
                //        }
                //    }
                //}
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
                        // Flag was a known one.
                        entry.IsUsedOrApproved = true;
                        continue;
                    }
                    if (i == 0)
                    {
                        m_baseElementName = entry.Name;
                        if (this.ParseBaseElement())
                        {
                            entry.IsUsedOrApproved = true;
                        }
                        else
                        {
                            (this.ParentFile as ScriptFile).ErrorsInternal.SymanticError(entry.Line, 1, false, "Unknown flag or file element: '" + entry.Name + "'.");
                        }
                    }
                    continue;
                }
                if (entry.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueProp = entry as PropertyBlockValue;
                    var typename = entry.SpecifiedTypeName;
                    object value = valueProp.Value;

                    if (this.ParsePartnerProperty(listener, entry.Line, typename, entry.Name, value))
                    {
                        entry.IsUsedOrApproved = true;
                        continue;
                    }
                }
            }
        }

        private bool ParsePartnerProperty(StepBroListener listener, int line, string type, string name, object value)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrWhiteSpace(name));
            if (type != null &&
                (type.Equals("partner", StringComparison.InvariantCulture) ||
                type.Equals("partner model", StringComparison.InvariantCulture) ||
                type.Equals("model", StringComparison.InvariantCulture) ||
                type.Equals("partner override", StringComparison.InvariantCulture) ||
                type.Equals("model override", StringComparison.InvariantCulture)))
                //type.Equals("partner new", StringComparison.InvariantCulture)))
            {
                string referenceName = null;
                if (value is string)
                {
                    referenceName = value as string;
                }
                else if (value is Identifier)
                {
                    referenceName = ((Identifier)value).Name;
                }
                else
                {
                    throw new ParsingErrorException(line, name, "The partner value must be a procedure reference.");
                }

                var referenceElement = listener.TryGetFileElementInScope(referenceName);
                if (referenceElement != null)
                {
                    if (referenceElement is IFileProcedure)
                    {
                        if (this.ElementType == FileElementType.Override)   // If an "override element" set/change the partner reference in the base file element.
                        {
                            var baseElement = this.GetRootBaseElement();
                            if (baseElement != null)
                            {
                                var existing = baseElement.m_partners.Where(p => p.Name.Equals(name)).FirstOrDefault() as FileElementPartner;
                                if (existing != null)
                                {
                                    existing.ProcedureName = referenceName;
                                    existing.ProcedureReference = referenceElement as FileProcedure;
                                }
                                else
                                {
                                    baseElement.m_partners.Add(new FileElementPartner(this, name, referenceName, referenceElement as IFileProcedure));
                                }
                            }
                        }
                        else
                        {
                            var partner = new FileElementPartner(this, name, referenceName, referenceElement as IFileProcedure);
                            if (type.Contains("model", StringComparison.InvariantCulture))
                            {
                                partner.IsModelDirect = true;
                            }
                            m_partners.Add(partner);
                        }
                    }
                    else
                    {
                        throw new ParsingErrorException(line, name, $"Element '{referenceName}' is not a procedure.");
                    }
                }
                else
                {
                    throw new ParsingErrorException(line, name, $"Element '{referenceName}' was not found.");
                }

                return true;
            }
            return false;
        }
    }
}
