using Antlr4.Runtime;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StepBro.Core.ScriptData
{
    internal class ScriptFile : LoadedFileBase, IScriptFile, IParsingContext
    {
        private StreamReader m_fileStream = null;
        private AntlrInputStream m_parserFileStream;
        private string m_namespace = "";
        private bool m_wasLoadedByNamespace;
        private readonly ErrorCollector m_errors;
        private List<IIdentifierInfo> m_namespaceUsings = new List<IIdentifierInfo>();
        private List<IIdentifierInfo> m_fileUsings = new List<IIdentifierInfo>();
        private PropertyBlock m_fileProperties = null;
        private List<FileVariableContainer> m_fileScopeVariables = new List<FileVariableContainer>();
        private List<FileElement> m_elements = new List<FileElement>();
        private bool m_typeScanIncluded = false;
        private DateTime m_lastFileChange = DateTime.MinValue;
        private readonly DateTime m_lastTypeScan = DateTime.MinValue;
        private readonly DateTime m_lastParsing = DateTime.MinValue;

        public event EventHandler ObjectContainerListChanged;

        //private readonly Dictionary<string, List<IIdentifierInfo>> m_fileScopeIdentifiers = null;
        //private readonly Dictionary<string, List<IIdentifierInfo>> m_fileAndProcedureScopeIdentifiers = null;

        internal ScriptFile() : base(null, LoadedFileType.StepBroScript)
        {
            m_wasLoadedByNamespace = false;
            m_errors = new ErrorCollector(this, false);
            m_lastFileChange = DateTime.Now;   // TODO: take this from file timestamp.
            m_parserFileStream = null;
        }

        internal ScriptFile(string filepath) :
                base(filepath, LoadedFileType.StepBroScript)
        {
            m_wasLoadedByNamespace = false;
            m_errors = new ErrorCollector(this, false);
            m_lastFileChange = DateTime.Now;   // TODO: take this from file timestamp.
            m_parserFileStream = null;
        }

        internal ScriptFile(
            string filepath,
            AntlrInputStream filestream) :
        base(filepath, LoadedFileType.StepBroScript)
        {
            m_wasLoadedByNamespace = false;
            m_errors = new ErrorCollector(this, false);
            m_lastFileChange = DateTime.Now;   // TODO: take this from file timestamp.
            m_parserFileStream = filestream;
        }

        protected override void DoDispose()
        {
            foreach (var fileVariable in m_fileScopeVariables)
            {
                fileVariable.VariableAccess.Dispose();
            }
            m_fileScopeVariables = null;
        }

        public override string ToString()
        {
            return $"File: {this.FileName}";
        }

        public string Namespace { get { return m_namespace; } }

        internal void SetNamespace(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException();
            if (!String.Equals(name, m_namespace, StringComparison.InvariantCulture))
            {
                m_namespace = name;
                this.NotifyPropertyChanged(nameof(this.Namespace));
            }
        }

        public bool WasLoadedByNamespace
        {
            get { return m_wasLoadedByNamespace; }
            internal set
            {
                if (value != m_wasLoadedByNamespace)
                {
                    m_wasLoadedByNamespace = value;
                    if (value && String.IsNullOrEmpty(m_namespace))
                    {
                        m_namespace = System.IO.Path.GetFileNameWithoutExtension(this.FileName);
                    }
                }
            }
        }

        public IErrorCollector Errors { get { return m_errors; } }

        public bool HasFileChanged()
        {
            var path = this.GetFullPath();
            var exist = System.IO.File.Exists(path);
            if (!exist) return false;
            //if (m_parserFileStream == null) return true;
            var lastWrite = System.IO.File.GetLastWriteTime(path);
            return (lastWrite != m_lastFileChange);
        }

        internal AntlrInputStream GetParserFileStream()
        {
            if (m_parserFileStream != null && m_fileStream == null)
            {
                return m_parserFileStream;
            }
            var path = this.GetFullPath();
            var exist = System.IO.File.Exists(path);
            if (m_parserFileStream != null)
            {
                if (exist)
                {
                    var lastWrite = System.IO.File.GetLastWriteTime(path);
                    if (lastWrite == m_lastFileChange)
                    {
                        return m_parserFileStream;
                    }
                }
                else
                {
                    throw new System.IO.FileNotFoundException($"Script file '{path}' has been deleted.");
                }
            }
            if (exist)
            {
                m_lastFileChange = System.IO.File.GetLastWriteTime(path);
                m_fileStream = System.IO.File.OpenText(path);
                m_parserFileStream = new Antlr4.Runtime.AntlrInputStream(m_fileStream);
                return m_parserFileStream;
            }
            else
            {
                throw new System.IO.FileNotFoundException($"Script file '{path}' not found.");
            }
        }

        internal void DisposeFileStream()
        {
            if (m_fileStream != null)
            {
                m_parserFileStream = null;
                m_fileStream.Dispose();
                m_fileStream = null;
            }
        }

        internal StepBroTypeScanListener.FileContent PreScanFileContent { get; set; }

        public string Author
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FileRevision
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DateTime LastFileChange { get; internal set; }
        public DateTime LastTypeScan { get; internal set; }
        public DateTime LastParsing { get; internal set; }

        public void MarkForTypeScanning()
        {
            m_typeScanIncluded = true;
        }

        /// <summary>
        /// ResetBeforeParsing
        /// </summary>
        /// <param name="preserveUpdateableElements">Whether to save element objects and just update the changes.</param>
        public void ResetBeforeParsing(bool preserveUpdateableElements)
        {
            m_namespaceUsings = new List<IIdentifierInfo>();
            m_fileProperties = null;
            if (preserveUpdateableElements)
            {
                throw new NotImplementedException();
            }
            else
            {
                foreach (var fsv in ((IEnumerable<FileVariableContainer>)m_fileScopeVariables).Reverse())
                {
                    object value = fsv.VariableAccess.Container.GetValue(null);
                    if (value != null && value is IDisposable)
                    {
                        ((IDisposable)value).Dispose();
                    }
                }
                foreach (var fu in m_fileUsings.Where(u => u.Type == IdentifierType.FileNamespace))
                {
                    if (fu.Reference != null)
                    {
                        foreach (var f in (IEnumerable<ScriptFile>)fu.Reference)
                        {
                            f.UnregisterDependant(this);
                        }
                    }
                }
                m_fileUsings = new List<IIdentifierInfo>();
                m_fileScopeVariables = new List<FileVariableContainer>();
                m_elements = new List<FileElement>();
            }
            m_typeScanIncluded = false;
        }

        /// <summary>
        /// Whether a type scan will take care of the prototype parsing before procdure content is parsed.
        /// </summary>
        public bool TypeScanIncluded { get { return m_typeScanIncluded; } }

        internal void AddFileProperties(PropertyBlock props)
        {
            m_fileProperties = props;
        }

        internal bool AddNamespaceUsing(string namespaceOrType)
        {
            if (m_namespaceUsings.Select(u => u.Name).FirstOrDefault(u => String.Equals(u, namespaceOrType, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                return false;
            }
            m_namespaceUsings.Add(new IdentifierInfo(namespaceOrType, namespaceOrType, IdentifierType.UnresolvedType, null, null));
            return true;
        }

        internal bool AddNamespaceUsing(IIdentifierInfo identifier)
        {
            if (m_namespaceUsings.Select(u => u.FullName).FirstOrDefault(u => String.Equals(u, identifier.FullName, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                return false;
            }
            m_namespaceUsings.Add(identifier);
            return true;
        }

        internal bool AddFileUsing(string file)
        {
            if (m_fileUsings.Select(u => u.Name).FirstOrDefault(u => String.Equals(u, file, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                return false;
            }
            m_fileUsings.Add(new IdentifierInfo(file, file, IdentifierType.FileByName, null, null));
            return true;
        }

        private IValueContainerOwnerAccess TryGetVariable(int id)
        {
            foreach (var v in m_fileScopeVariables)
            {
                if (v.VariableAccess.Container.UniqueID == id)
                {
                    return v.VariableAccess;
                }
            }
            return null;    // Not found
        }

        internal int CreateOrGetFileVariable(
            string @namespace,
            string name,
            TypeReference datatype,
            bool @readonly,
            int line,
            int column,
            VariableContainerAction resetter = null,
            VariableContainerAction creator = null,
            VariableContainerAction initializer = null)
        {
            foreach (var v in m_fileScopeVariables)
            {
                if (v.VariableAccess.Container.Name == name && v.VariableAccess.Container.DataType.Type == datatype.Type && ((int)v.VariableAccess.Tag) == line * 1000 + column)
                {
                    if (resetter != null)
                    {
                        v.VariableAccess.DataResetter = resetter;
                    }
                    if (creator != null)
                    {
                        v.VariableAccess.DataCreator = creator;
                    }
                    if (initializer != null)
                    {
                        v.VariableAccess.DataInitializer = initializer;
                    }
                    return v.ID;
                }
            }

            var vc = FileVariableContainer.Create(@namespace, name, datatype, @readonly, line, column, resetter, creator, initializer);
            m_fileScopeVariables.Add(vc);
            this.ObjectContainerListChanged?.Invoke(this, EventArgs.Empty);
            return vc.ID;
        }

        internal void SetFileVariableModifier(int id, AccessModifier access)
        {
            var variable = this.TryGetVariable(id);
            if (variable == null) throw new ArgumentException("Unknown variable ID.");
            variable.SetAccessModifier(access);
        }

        internal IProcedureReference GetProcedure(int id)
        {
            foreach (var e in m_elements)
            {
                if (e is FileProcedure && e.UniqueID == id)
                {
                    return (e as FileProcedure).ProcedureReference;
                }
            }
            return null;
        }

        internal IFileElement GetFileElement(int id)
        {
            foreach (var e in m_elements)
            {
                if (e.UniqueID == id)
                {
                    return e;
                }
            }
            return null;
        }

        public T GetFileElement<T>(string name) where T : class, IFileElement
        {
            return m_elements.FirstOrDefault(e => String.Equals(name, e.Name, StringComparison.InvariantCulture)) as T;
        }

        public IValueContainer<T> GetVariableContainer<T>(int id)
        {
            foreach (var v in m_fileScopeVariables)
            {
                if (v.ID == id)
                {
                    if (v.VariableAccess.Container is IValueContainer<T>)
                    {
                        return v.VariableAccess.Container as IValueContainer<T>;
                    }
                    else
                    {
                        throw new ArgumentException("The data type for the variable is not the same as the specified type.");
                    }
                }
            }
            throw new ArgumentException("The specified variable id was not found.");
        }

        public IEnumerable<IValueContainer> ListFileVariables()
        {
            foreach (var v in m_fileScopeVariables)
            {
                yield return v.VariableAccess.Container;
            }
        }

        public void InitializeFileVariables(ILogger logger)
        {
            foreach (var v in m_fileScopeVariables)
            {
                if (!v.VariableAccess.DataCreated)
                {
                    v.VariableAccess.DataCreator?.Invoke(v.VariableAccess, logger);
                }
                v.VariableAccess.DataResetter?.Invoke(v.VariableAccess, logger);
                v.VariableAccess.DataInitializer?.Invoke(v.VariableAccess, logger);
            }
        }

        internal void AddProcedure(FileProcedure function)
        {
            m_elements.Add(function);
        }

        internal void AddTestList(FileTestList list)
        {
            m_elements.Add(list);
        }

        internal void AddDatatable(FileDatatable table)
        {
            m_elements.Add(table);
        }

        public IEnumerable<IFileElement> ListElements()
        {
            foreach (var e in m_elements)
            {
                yield return e;
            }
        }

        public IFileElement this[string name]
        {
            get
            {
                return this.ListElements().FirstOrDefault(e => e.Name == name);
            }
        }

        public IEnumerable<IIdentifierInfo> Usings
        {
            get
            {
                foreach (var u in m_namespaceUsings)
                {
                    yield return u;
                }
            }
        }

        internal void ResolveNamespaceUsings(Converter<string, IIdentifierInfo> resolver)
        {
            var c = m_namespaceUsings.Count;
            for (int i = 0; i < c; i++)
            {
                if (m_namespaceUsings[i].Type == IdentifierType.UnresolvedType)
                {
                    var resolved = resolver(m_namespaceUsings[i].Name);
                    if (resolved != null)
                    {
                        m_namespaceUsings[i] = resolved;
                    }
                }
            }
        }

        internal void ResolveFileUsings(Converter<string, ScriptFile> resolver)
        {
            var c = m_fileUsings.Count;
            for (int i = 0; i < c; i++)
            {
                if (m_fileUsings[i].Reference == null)
                {
                    var resolved = resolver(m_fileUsings[i].Name);
                    if (resolved != null)
                    {
                        m_fileUsings[i] = new IdentifierInfo(m_fileUsings[i].Name, m_fileUsings[i].FullName, IdentifierType.FileByName, null, resolved);
                        resolved.RegisterDependant(this);
                    }
                }
            }
        }

        internal IEnumerable<IIdentifierInfo> ListResolvedNamespaceUsings()
        {
            return m_namespaceUsings.Where(e => e.Type != IdentifierType.UnresolvedType);
        }

        internal IEnumerable<ScriptFile> ListResolvedFileUsings()
        {
            foreach (var u in m_fileUsings)
            {
                if (u.Reference != null && u.Type == IdentifierType.FileByName) yield return u.Reference as ScriptFile;
            }
        }

        internal IEnumerable<ScriptFile> ListReferencedScriptFiles()
        {
            foreach (var u in this.ListResolvedFileUsings())
            {
                yield return u;
            }
            foreach (var xd in this.ListResolvedNamespaceUsings())
            {
                if (xd.Type == IdentifierType.FileNamespace && xd.Reference != null)
                {
                    foreach (var u in ((IEnumerable<ScriptFile>)xd.Reference))
                    {
                        yield return u;
                    }
                }
            }
        }

        public IScriptFile FileInfo
        {
            get
            {
                return this;
            }
        }

        IEnumerable<IIdentifierInfo> IParsingContext.KnownIdentifiers()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IObjectContainer> ListObjectContainers()
        {
            foreach (var fv in m_fileScopeVariables)
            {
                yield return fv.VariableAccess.Container;
            }
        }
    }
}
