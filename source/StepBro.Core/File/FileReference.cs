﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Tasks;

namespace StepBro.Core.File
{
    /// <summary>
    /// The different basic categories for owners of folder shortcuts.
    /// </summary>
    public enum FolderShortcutOrigin
    {
        Root,
        Environment,
        HostApplication,
        Configuration,
        User,
        Project,
        ScriptFile
        //Context
    }

    /// <summary>
    /// Interface for a folder shortcut specification object.
    /// </summary>
    public interface IFolderShortcut
    {
        FolderShortcutOrigin Origin { get; }
        string Name { get; }
        string Path { get; }
        public bool IsResolved { get; }
        FilePath ResolvedPath { get; }
    }

    /// <summary>
    /// An interface for an object that has access to a list of foldet shortcuts (<seealso cref="IFolderShortcut"/> objects).
    /// </summary>
    public interface IFolderShortcutsSource
    {
        /// <summary>
        /// The list of folder shortcuts defined in the environment (OS), the host application, the configuration and the current script file.
        /// </summary>
        /// <returns>An enumeration of the available shortcuts.</returns>
        IEnumerable<IFolderShortcut> ListShortcuts();
    }

    /// <summary>
    /// A delegate for a function that enumerates a list of available folder shortcuts.
    /// </summary>
    /// <returns>An enumeration of folder shortcuts.</returns>
    public delegate IEnumerable<IFolderShortcut> FolderShortcutsDelegate();

    /// <summary>
    /// A helper class that "converts" a <seealso cref="FolderShortcutsDelegate"/> to a <seealso cref="IFolderShortcutsSource"/> interface.
    /// </summary>
    public class FolderShortcutsFromDelegate : IFolderShortcutsSource
    {
        private readonly FolderShortcutsDelegate m_delegate;
        public FolderShortcutsFromDelegate(FolderShortcutsDelegate @delegate)
        {
            if (@delegate == null) throw new ArgumentNullException();
            m_delegate = @delegate;
        }

        public IEnumerable<IFolderShortcut> ListShortcuts()
        {
            return m_delegate();
        }
    }

    /// <summary>
    /// A named file system folder reference, that can be used as basis for file path specifications. 
    /// </summary>
    public class FolderShortcut : IFolderShortcut, IIdentifierInfo
    {
        private FolderShortcutOrigin m_origin;
        private string m_name;
        private string m_path;
        private FilePath m_resolvedPath;

        public FolderShortcut(FolderShortcutOrigin origin, string name, string path, string resolved = null)
        {
            m_origin = origin;
            m_name = name;
            m_path = path;
            m_resolvedPath = (resolved != null) ? new FilePath(resolved) : null;
        }

        public FolderShortcutOrigin Origin
        {
            get
            {
                return m_origin;
            }
        }

        /// <summary>
        ///  The name of the shortcut.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// The specified file path for the shortcut.
        /// </summary>
        public string Path
        {
            get
            {
                return m_path;
            }
        }

        /// <summary>
        /// The resulting absolute filepath for the shortcut.
        /// </summary>
        public FilePath ResolvedPath
        {
            get
            {
                return m_resolvedPath;
            }
        }

        /// <summary>
        /// Whether the shortcut is successfully resolved.
        /// </summary>
        public bool IsResolved { get { return m_resolvedPath != null; } }

        string IIdentifierInfo.FullName => throw new NotImplementedException();

        IdentifierType IIdentifierInfo.Type => IdentifierType.ApplicationObject;

        TypeReference IIdentifierInfo.DataType => (TypeReference)typeof(FilePath);

        object IIdentifierInfo.Reference => m_resolvedPath;

        string IIdentifierInfo.SourceFile => null;

        int IIdentifierInfo.SourceLine => -1;

        /// <summary>
        /// Tries to resolve the specified shortcut path, using the specified list of other shortcuts and the specified base path for the shortcut owner.
        /// </summary>
        /// <param name="shortcuts"></param>
        /// <param name="basePath">The base path for the object that ownes the shortcut.</param>
        /// <param name="errorMessage">Either a short description of the reason for failing to resolve the specified path, or <code>null</code> if succeeded to resolve the path.</param>
        /// <returns>Whether the shortcut path was successfully resolved.</returns>
        public bool TryResolve(IEnumerable<IFolderShortcut> shortcuts, string basePath, ref string errorMessage)
        {
            string resolved = shortcuts.ResolveShortcutPath(m_path, ref errorMessage);
            if (resolved == null)
            {
                m_resolvedPath = null;
                return false;
            }
            else
            {
                m_resolvedPath = new FilePath(System.IO.Path.GetFullPath(resolved, basePath));
                return true;
            }
        }

        public override string ToString()
        {
            if (m_resolvedPath == null) return $"Shortcut {Name}: {Path}";
            else return $"Shortcut {Name}: {Path} ({m_resolvedPath})";
        }
    }

    public static class FileReferenceUtils
    {
        /// <summary>
        /// Splits a string into a base shortcut reference and a specified path.
        /// </summary>
        /// <remarks>If the specified path is not using </remarks>
        /// <param name="source">The file path string.</param>
        /// <returns>A named string, where the name part is the shortcut base (if any) and the value part is the specified relative or absolute file path contibution.</returns>
        public static NamedString SplitShortcutPath(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new NamedString("", "");
            }
            if (source[0] == '[')
            {
                int i = source.IndexOf(']');
                if (i < 2)
                {
                    return new NamedString("", "");
                }
                string baseName = source.Substring(1, i - 1);
                string path = source.Substring(i + 1).TrimStart(' ', '\\', '/', ':');
                return new NamedString(baseName, path);
            }
            else
            {
                return new NamedString("", source);
            }
        }

        /// <summary>
        /// Resolves the specified shortcut path to the absolute path.
        /// </summary>
        /// <remarks>This specified path can be a relative path, a path using another shortcut or an already absolute path.</remarks>
        /// <param name="shortcuts">The known folder shortcuts that can be used for resolving the specified path.</param>
        /// <param name="path">The path to resolve to an absolute path.</param>
        /// <param name="errorMessage">Either a short description of the reason for failing to resolve the specified path, or <code>null</code> if succeeded to resolve the path.</param>
        /// <returns>The resolved path string, or <code>null</code> if failing to resolve the specified path.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ResolveShortcutPath(this IEnumerable<IFolderShortcut> shortcuts, string path, ref string errorMessage)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Empty path argument.");
            }

            var splittedPath = FileReferenceUtils.SplitShortcutPath(path);

            if (String.IsNullOrEmpty(splittedPath.Name))
            {
                return path;
            }
            else
            {
                IFolderShortcut shortcut = shortcuts.FirstOrDefault(s => s.Name == splittedPath.Name);
                if (shortcut != null)
                {
                    string p = shortcut.IsResolved ? shortcut.ResolvedPath : ResolveShortcutPath(shortcuts, shortcut.Path, ref errorMessage);

                    if (p == null)
                    {
                        return null;
                    }
                    else if (String.IsNullOrEmpty(splittedPath.Value))
                    {
                        return p;
                    }
                    else
                    {
                        return System.IO.Path.Combine(p, splittedPath.Value);
                    }
                }
                else
                {
                    errorMessage = "Unknown folder shortcut: '" + splittedPath.Name + "'.";
                    return null;
                }
            }
        }

        public static string GetFullPath(this IEnumerable<IFolderShortcut> shortcuts, string path, ref string errorMessage)
        {
            string resolved = shortcuts.ResolveShortcutPath(path, ref errorMessage);
            if (resolved == null)
            {
                return null;
            }
            else
            {
                return System.IO.Path.GetFullPath(resolved);
            }
        }
    }

    public class FolderShortcutCollection : IFolderShortcutsSource
    {
        FolderShortcutOrigin m_origin;
        List<IFolderShortcutsSource> m_collections = new List<IFolderShortcutsSource>();
        List<IFolderShortcut> m_shortcuts = new List<IFolderShortcut>();

        public FolderShortcutCollection(FolderShortcutOrigin origin, params IFolderShortcutsSource[] collections)
        {
            m_origin = origin;
            m_collections.AddRange(collections);
        }


        public FolderShortcutOrigin Origin { get { return m_origin; } }

        public IEnumerable<IFolderShortcut> Shortcuts { get { return m_shortcuts; } }

        public void AddSource(IFolderShortcutsSource source)
        {
            m_collections.Add(source);
        }

        public IFolderShortcut AddShortcut(string name, string path, bool isResolved = false)
        {
            FolderShortcut shortcut = new FolderShortcut(m_origin, name, path, isResolved ? path : null);
            m_shortcuts.Add(shortcut);
            return shortcut;
        }

        public IEnumerable<IFolderShortcut> ListShortcuts()
        {
            foreach (var sc in m_shortcuts)
            {
                yield return sc;
            }
            foreach (var coll in m_collections)
            {
                foreach (var sc in coll.ListShortcuts())
                {
                    yield return sc;
                }
            }
        }

        static public IEnumerable<IFolderShortcut> ListShortcuts(params IFolderShortcutsSource[] sources)
        {
            foreach (var source in sources)
            {
                foreach (var sc in source.ListShortcuts())
                {
                    yield return sc;
                }
            }
        }
    }

    public interface IFolderManager : IFolderShortcutsSource
    {
        void AddSource(IFolderShortcutsSource source);
    }

    internal class FolderManager : ServiceBase<IFolderManager, FolderManager>, IFolderManager
    {
        private FolderShortcutCollection m_collections = null;
        private FolderShortcutCollection m_environmentShortcuts = new FolderShortcutCollection(FolderShortcutOrigin.Environment);
        private FolderShortcutCollection m_configurationShortcuts = new FolderShortcutCollection(FolderShortcutOrigin.Configuration);

        public FolderManager(out IService serviceAccess) :
            base("FolderManager", out serviceAccess)
        {
        }

        public void AddSource(IFolderShortcutsSource source)
        {
            m_collections.AddSource(source);
        }

        public IEnumerable<IFolderShortcut> ListShortcuts()
        {
            return m_collections.ListShortcuts();
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            foreach (var sfname in Enum.GetNames(typeof(System.Environment.SpecialFolder)))
            {
                var sf = (System.Environment.SpecialFolder)Enum.Parse(typeof(System.Environment.SpecialFolder), sfname);
                var folder = System.Environment.GetFolderPath(sf);
                if (!String.IsNullOrEmpty(folder))
                {
                    m_environmentShortcuts.AddShortcut(sfname, folder, isResolved: true);
                }
            }
            bool hasOutputFolderShortcut = false;
            foreach (DictionaryEntry ev in Environment.GetEnvironmentVariables())
            {
                var value = ev.Value as string;
                if (!String.IsNullOrEmpty(value) && value.Contains(System.IO.Path.DirectorySeparatorChar))
                {
                    var key = ev.Key as string;
                    if (!m_environmentShortcuts.ListShortcuts().Any(k => String.Equals(k.Name, key, StringComparison.InvariantCultureIgnoreCase)) && (System.IO.Directory.Exists(value) || System.IO.File.Exists(value)))
                    {
                        m_environmentShortcuts.AddShortcut(key, value, isResolved: true);
                    }
                    if (key.Equals(Constants.STEPBRO_OUTPUT_FOLDER_SHORTCUT, StringComparison.InvariantCulture))
                    {
                        hasOutputFolderShortcut = true;
                    }
                }
            }

            if (!hasOutputFolderShortcut)
            {
                // Not configured, so use the documents folder.
                m_environmentShortcuts.AddShortcut(Constants.STEPBRO_OUTPUT_FOLDER_SHORTCUT, System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), isResolved: true);
            }

            m_collections = new FolderShortcutCollection(FolderShortcutOrigin.Root, m_environmentShortcuts, m_configurationShortcuts);
        }
    }
}
