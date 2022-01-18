﻿using Antlr4.Runtime;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StepBro.Core.ScriptData
{
    internal class ScriptFile : LoadedFileBase, IScriptFile, IParsingContext
    {
        public static readonly string VARIABLE_CUSTOM_PROPS_TAG = "CustomPropsTag";
        public static readonly string VARIABLE_ALL_DATA_PROPS = "AllDataProps";

        private StreamReader m_fileStream = null;
        private AntlrInputStream m_parserFileStream;
        private int m_parsingFloor = 0;     // The lower the floor(number), the earlier in the parsing sequence.
        private string m_namespace = "";
        private bool m_wasLoadedByNamespace;
        private readonly ErrorCollector m_errors;
        private List<UsingData> m_namespaceUsings = new List<UsingData>();
        private List<UsingData> m_fileUsings = new List<UsingData>();
        private PropertyBlock m_fileProperties = null;
        private List<FileVariable> m_fileScopeVariables = new List<FileVariable>();
        private List<FileVariable> m_fileScopeVariablesBefore = new List<FileVariable>();
        private List<FileElement> m_elements = new List<FileElement>();
        private bool m_typeScanIncluded = false;
        private DateTime m_lastFileChange = DateTime.MinValue;
        private readonly DateTime m_lastTypeScan = DateTime.MinValue;
        private readonly DateTime m_lastParsing = DateTime.MinValue;

        /// <summary>
        /// Reachable script elements and namespaces with this files current usings.
        /// </summary>
        private Dictionary<string, List<IIdentifierInfo>> m_rootIdentifiers = null;

        public event EventHandler ObjectContainerListChanged;

        //private readonly Dictionary<string, List<IIdentifierInfo>> m_fileScopeIdentifiers = null;
        //private readonly Dictionary<string, List<IIdentifierInfo>> m_fileAndProcedureScopeIdentifiers = null;

        internal ScriptFile(string filepath = null, AntlrInputStream filestream = null) : base(filepath, LoadedFileType.StepBroScript)
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
                fileVariable.VariableOwnerAccess.Dispose();
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
        public ErrorCollector ErrorsInternal { get { return m_errors; } }

        public bool HasFileChanged()
        {
            var path = this.GetFullPath();
            var exist = System.IO.File.Exists(path);
            if (!exist) return false;
            //if (m_parserFileStream == null) return true;
            var lastWrite = System.IO.File.GetLastWriteTime(path);
            return (lastWrite != m_lastFileChange);
        }

        internal void SetParserFileStream(string content)
        {
            m_parserFileStream = new AntlrInputStream(content);
        }

        internal int ParsingFloor
        {
            get { return m_parsingFloor; }
            set { m_parsingFloor = value; }
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
            m_namespaceUsings = new List<UsingData>();
            m_fileProperties = null;

            if (!preserveUpdateableElements)
            {
                foreach (var fsv in ((IEnumerable<FileVariable>)m_fileScopeVariables).Reverse())
                {
                    object value = fsv.VariableOwnerAccess.Container.GetValue(null);
                    if (value != null && value is IDisposable)
                    {
                        ((IDisposable)value).Dispose();
                    }
                    // TODO: Set container.CreateNeeded
                }
                m_fileScopeVariables.Clear();
            }
            foreach (var fu in m_fileUsings.Where(u => u.Identifier.Type == IdentifierType.FileNamespace))
            {
                if (fu.Identifier.Reference != null)
                {
                    foreach (var f in (IEnumerable<ScriptFile>)fu.Identifier.Reference)
                    {
                        f.UnregisterDependant(this);
                    }
                }
            }
            m_fileUsings = new List<UsingData>();
            m_elements = new List<FileElement>();

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

        internal bool AddNamespaceUsing(int line, string namespaceOrType, string alias = null)
        {
            if (m_namespaceUsings.Select(u => u.Identifier.Name).FirstOrDefault(u => String.Equals(u, namespaceOrType, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                return false;
            }
            m_namespaceUsings.Add(new UsingData(line, alias, new IdentifierInfo(namespaceOrType, namespaceOrType, IdentifierType.UnresolvedType, null, null)));
            return true;
        }

        internal bool AddNamespaceUsing(int line, IIdentifierInfo identifier, string alias = null)
        {
            if (m_namespaceUsings.Select(u => u.Identifier.FullName).FirstOrDefault(u => String.Equals(u, identifier.FullName, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                return false;
            }
            m_namespaceUsings.Add(new UsingData(line, alias, identifier));
            return true;
        }

        internal bool AddFileUsing(int line, string file)
        {
            if (m_fileUsings.Select(u => u.Identifier.Name).FirstOrDefault(u => String.Equals(u, file, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                return false;
            }
            m_fileUsings.Add(new UsingData(line, file, IdentifierType.FileByName));
            return true;
        }

        private IValueContainerOwnerAccess TryGetVariable(int id)
        {
            foreach (var v in m_fileScopeVariables)
            {
                if (v.VariableOwnerAccess.Container.UniqueID == id)
                {
                    return v.VariableOwnerAccess;
                }
            }
            return null;    // Not found
        }

        internal int CreateOrGetFileVariable(
            string @namespace,
            AccessModifier access,
            string name,
            TypeReference datatype,
            bool @readonly,
            int line,
            int column,
            int codeHash,
            VariableContainerAction resetter = null,
            VariableContainerAction creator = null,
            VariableContainerAction initializer = null,
            PropertyBlock fileSetupData = null,
            PropertyBlock customSetupData = null)
        {
            var existing = m_fileScopeVariablesBefore.FirstOrDefault(
                v => (String.Equals(v.VariableOwnerAccess.Container.Name, name, StringComparison.InvariantCulture) &&
                        v.VariableOwnerAccess.Container.DataType.Type == datatype.Type));
            if (existing != null)
            {
                System.Diagnostics.Debug.WriteLine($"Using existing variable \"{name}\" (in {this.FileName}), with ID {existing.ID}");
                existing.VariableOwnerAccess.InitNeeded = (existing.VariableOwnerAccess.CodeHash != codeHash);
                m_fileScopeVariables.Add(existing);
                m_fileScopeVariablesBefore.RemoveAt(m_fileScopeVariablesBefore.IndexOf(existing));
                if (resetter != null)
                {
                    existing.VariableOwnerAccess.DataResetter = resetter;
                }
                if (creator != null)
                {
                    existing.VariableOwnerAccess.DataCreator = creator;
                }
                if (initializer != null)
                {
                    existing.VariableOwnerAccess.DataInitializer = initializer;
                }
                existing.VariableOwnerAccess.CodeHash = codeHash;
                if (fileSetupData != null)
                {
                    SetFileVariableAllData(existing, fileSetupData);
                }
                if (customSetupData != null)
                {
                    SetFileVariableCustomData(existing, customSetupData);
                }
                return existing.ID;
            }

            var vc = FileVariable.Create(this, access, @namespace, name, datatype, @readonly, line, column, codeHash, resetter, creator, initializer);
            if (fileSetupData != null)
            {
                SetFileVariableAllData(vc, fileSetupData);
            }
            if (customSetupData != null)
            {
                SetFileVariableCustomData(vc, customSetupData);
            }
            m_fileScopeVariables.Add(vc);
            this.ObjectContainerListChanged?.Invoke(this, EventArgs.Empty);
            return vc.ID;
        }

        internal static void SetFileVariableAllData(FileVariable variable, PropertyBlock data)
        {
            System.Diagnostics.Debug.Assert(variable.VariableOwnerAccess.Tags != null);
            variable.VariableOwnerAccess.Tags[VARIABLE_ALL_DATA_PROPS] = data;
        }
        internal static void SetFileVariableCustomData(FileVariable variable, PropertyBlock customSetupData)
        {
            System.Diagnostics.Debug.Assert(variable.VariableOwnerAccess.Tags != null);
            variable.VariableOwnerAccess.Tags[VARIABLE_CUSTOM_PROPS_TAG] = customSetupData;
        }
        internal static PropertyBlock GetFileVariableAllData(FileVariable variable)
        {
            System.Diagnostics.Debug.Assert(variable.VariableOwnerAccess.Tags != null);
            object value;
            if (variable.VariableOwnerAccess.Tags.TryGetValue(VARIABLE_ALL_DATA_PROPS, out value))
            {
                return value as PropertyBlock;
            }
            else return null;
        }
        internal static PropertyBlock GetFileVariableCustomData(FileVariable variable)
        {
            System.Diagnostics.Debug.Assert(variable.VariableOwnerAccess.Tags != null);
            object value;
            if (variable.VariableOwnerAccess.Tags.TryGetValue(VARIABLE_CUSTOM_PROPS_TAG, out value))
            {
                return value as PropertyBlock;
            }
            else return null;
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

        public IValueContainer<T> TryGetVariableContainer<T>(int id)
        {
            var t = typeof(T);
            foreach (var v in m_fileScopeVariables)
            {
                if (v.ID == id)
                {
                    if (v.VariableOwnerAccess.Container is IValueContainer<T>)
                    {
                        return v.VariableOwnerAccess.Container as IValueContainer<T>;
                    }
                    else
                    {
                        throw new ArgumentException($"The data type for the variable is not the same as the specified type ({t.Name}).");
                    }
                }
            }

            foreach (var uf in m_fileUsings)
            {
                var file = uf.Identifier.Reference as ScriptFile;
                var found = file.TryGetVariableContainer<T>(id);
                if (found != null) return found;
            }
            foreach (var un in m_namespaceUsings)
            {
                if (un.Identifier.Type == IdentifierType.FileNamespace && un.Identifier.Reference != null)
                {
                    var files = un.Identifier.Reference as List<ScriptFile>;
                    if (files != null)
                    {
                        foreach (var f in files)
                        {
                            var found = f.TryGetVariableContainer<T>(id);
                            if (found != null)
                            {
                                return found;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void SaveCurrentFileVariables()
        {
            m_fileScopeVariablesBefore = m_fileScopeVariables;
            m_fileScopeVariables = new List<FileVariable>();
        }

        public IEnumerable<IValueContainer> ListFileVariables()
        {
            foreach (var v in m_fileScopeVariables)
            {
                yield return v.VariableOwnerAccess.Container;
            }
        }

        public void InitializeFileVariables(ILogger logger)
        {
            foreach (var v in m_fileScopeVariables)
            {
                bool doInit = !v.VariableOwnerAccess.DataCreated || v.VariableOwnerAccess.InitNeeded;
                if (!v.VariableOwnerAccess.DataCreated)
                {
                    logger?.Log("Variable " + v.VariableOwnerAccess.Container.Name + " - Create data");
                    v.VariableOwnerAccess.DataCreator?.Invoke(v.VariableOwnerAccess, logger);
                    var obj = v.VariableOwnerAccess.Container.GetValue();
                    if (obj != null && obj is INameable && (obj as INameable).Name == null)
                    {
                        // Set the name of the object to the same as the container/variable;
                        (obj as INameable).Name = v.VariableOwnerAccess.Container.Name;
                    }
                }
                if (doInit)
                {
                    if (logger != null)
                    {
                        var text = "Reset and initialize";
                        var props = GetFileVariableAllData(v);
                        if (props != null)
                        {
                            text = $"{text}, data: {props.GetTestString()}";
                        }
                        logger.Log("Variable " + v.VariableOwnerAccess.Container.Name + " init: " + text);
                    }
                    v.VariableOwnerAccess.DataResetter?.Invoke(v.VariableOwnerAccess, logger);
                    v.VariableOwnerAccess.DataInitializer?.Invoke(v.VariableOwnerAccess, logger);
                }
            }
        }

        public void DisposeUnusedFileVariables(ILogger logger)
        {
            foreach (var v in m_fileScopeVariablesBefore)
            {
                logger?.Log("Variable " + v.VariableOwnerAccess.Container.Name + " - Dispose");
                v.VariableOwnerAccess.Dispose();
            }
            m_fileScopeVariablesBefore = null;
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
            foreach (var e in m_fileScopeVariables)
            {
                yield return e;
            }
            foreach (var e in m_elements)
            {
                yield return e;
            }
        }

        public IEnumerable<IFileElement> ListPublicElements(string userNamespace, bool onlyLocal = false)
        {
            AccessModifier access = AccessModifier.Public;
            if (String.Equals(userNamespace, m_namespace, StringComparison.InvariantCulture)) access = AccessModifier.Protected;
            foreach (var element in this.ListElements().Where(e => e.AccessLevel >= access)) yield return element;
        }

        public IFileElement this[string name]
        {
            get
            {
                return this.ListElements().FirstOrDefault(e => e.Name == name);
            }
        }

        public IEnumerable<UsingData> Usings
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
                if (m_namespaceUsings[i].Identifier.Type == IdentifierType.UnresolvedType)
                {
                    var resolved = resolver(m_namespaceUsings[i].Identifier.Name);
                    if (resolved != null)
                    {
                        m_namespaceUsings[i] = new UsingData(m_namespaceUsings[i].Line, resolved);
                    }
                    else
                    {
                        m_errors.UnresolvedUsing(m_fileUsings[i].Line, -1, m_fileUsings[i].Identifier.Name);
                    }
                }
            }
        }

        internal void ResolveFileUsings(Converter<string, IScriptFile> resolver)
        {
            var c = m_fileUsings.Count;
            for (int i = 0; i < c; i++)
            {
                if (m_fileUsings[i].Identifier.Reference == null)
                {
                    try
                    {
                        var resolved = resolver(m_fileUsings[i].Identifier.Name);
                        if (resolved != null)
                        {
                            m_fileUsings[i] = new UsingData(m_fileUsings[i].Line, m_fileUsings[i].Identifier.Name, IdentifierType.FileByName, resolved);
                            resolved.RegisterDependant(this);
                        }
                        else
                        {
                            m_errors.UnresolvedUsing(m_fileUsings[i].Line, -1, m_fileUsings[i].Identifier.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        m_errors.InternalError(m_fileUsings[i].Line, -1, "Exception thrown when resolving using. Exception: " + ex.GetType().Name + ", " + ex.Message);
                    }
                }
            }
        }

        internal void UpdateRootIdentifiers()
        {
            if (m_rootIdentifiers == null) m_rootIdentifiers = new Dictionary<string, List<IIdentifierInfo>>();
            else m_rootIdentifiers.Clear();

            foreach (var element in this.ListElements())
            {
                this.AddRootIdentifier(element.Name, element);
            }

            foreach (var fu in this.ListResolvedFileUsings())
            {
                foreach (var element in fu.ListPublicElements(m_namespace))
                {
                    this.AddRootIdentifier(element.Name, element);
                }
            }
            foreach (var nu in this.ListResolvedNamespaceUsings())
            {
                switch (nu.Type)
                {
                    case IdentifierType.DotNetNamespace:
                        {
                            var ns = nu.Reference as NamespaceList;
                            foreach (var sub in ns.ListSubNamespaces(false))
                            {
                                this.AddRootIdentifier(sub.Name, sub);
                            }
                            foreach (var type in ns.ListTypes(false))
                            {
                                this.AddRootIdentifier(type.Name, new IdentifierInfo(type.Name, type.FullName, IdentifierType.DotNetType, new TypeReference(type), null));
                            }
                        }
                        break;
                    case IdentifierType.DotNetType:
                        {
                            var types = nu.DataType.Type.GetNestedTypes();
                            foreach (var t in types)
                            {
                                this.AddRootIdentifier(t.Name, new IdentifierInfo(t.Name, t.FullName, IdentifierType.DotNetType, new TypeReference(t), null));
                            }
                            var methods = nu.DataType.Type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            foreach (var m in methods)
                            {
                                if (!m.IsSpecialName)
                                {
                                    this.AddRootIdentifier(m.Name, new IdentifierInfo(m.Name, m.Name, IdentifierType.DotNetMethod, null, m));
                                }
                            }
                        }
                        break;
                    case IdentifierType.FileByName:
                        throw new NotImplementedException();

                    case IdentifierType.FileNamespace:
                        foreach (var file in ((IEnumerable<ScriptFile>)nu.Reference))
                        {
                            foreach (var element in file.ListPublicElements(m_namespace))
                            {
                                this.AddRootIdentifier(element.Name, element);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            foreach (var na in ListResolvedAliasUsings())
            {
                throw new NotImplementedException();
            }
        }

        private void AddRootIdentifier(string name, IIdentifierInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"AddRootIdentifier {this.FileName}: {name}");
            if (!m_rootIdentifiers.ContainsKey(name))
            {
                var list = new List<IIdentifierInfo>();
                list.Add(info);
                m_rootIdentifiers[name] = list;
            }
            else
            {
                m_rootIdentifiers[name].Add(info);
            }
        }

        public List<IIdentifierInfo> LookupIdentifier(string identifier)
        {
            List<IIdentifierInfo> result = null;
            if (m_rootIdentifiers != null)
            {
                m_rootIdentifiers.TryGetValue(identifier, out result);
            }
            else
            {
                result = this.ListElements().Where(e => e.Name.Equals(identifier, StringComparison.InvariantCulture)).Cast<IIdentifierInfo>().ToList();
                if (result.Count == 0) return null;
            }
            return result;
        }

        internal IEnumerable<IIdentifierInfo> ListResolvedNamespaceUsings()
        {
            return m_namespaceUsings.Where(e => e.Identifier.Type != IdentifierType.UnresolvedType && !e.IsAlias).Select(nu => nu.Identifier);
        }
        internal IEnumerable<Tuple<string, IIdentifierInfo>> ListResolvedAliasUsings()
        {
            return m_namespaceUsings.Where(e => e.Identifier.Type != IdentifierType.UnresolvedType && e.IsAlias).
                Select(nu => new Tuple<string, IIdentifierInfo>(nu.Alias, nu.Identifier));
        }

        internal IEnumerable<ScriptFile> ListResolvedFileUsings()
        {
            foreach (var u in m_fileUsings)
            {
                if (u.Identifier.Reference != null && u.Identifier.Type == IdentifierType.FileByName) yield return u.Identifier.Reference as ScriptFile;
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
            if (m_fileScopeVariables != null)
            {
                foreach (var fv in m_fileScopeVariables)
                {
                    yield return fv.VariableOwnerAccess.Container;
                }
            }
        }
    }
}
