using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StepBro.Core.Data;
using StepBro.Core.Tasks;

namespace StepBro.Core.File
{
    public enum FolderShortcutOrigin
    {
        Root,
        Environment,
        HostApplication,
        Configuration,
        User,
        Project,
        ScriptFile,
        //Context
    }

    public interface IFolderShortcut
    {
        FolderShortcutOrigin Origin { get; }
        string Name { get; }
        string Path { get; }
    }

    public interface IFolderShortcutsSource
    {
        /// <summary>
        /// The list of folder shortcuts defined in the environment (OS), the host application, the configuration and the current script file.
        /// </summary>
        /// <returns>An enumeration of the available shortcuts.</returns>
        IEnumerable<IFolderShortcut> ListShortcuts();
    }

    public class FolderShortcut : IFolderShortcut
    {
        private FolderShortcutOrigin m_origin;
        private string m_name;
        private string m_path;

        public FolderShortcut(FolderShortcutOrigin origin, string name, string path)
        {
            m_origin = origin;
            m_name = name;
            m_path = path;
        }

        public FolderShortcutOrigin Origin
        {
            get
            {
                return m_origin;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public string Path
        {
            get
            {
                return m_path;
            }
        }

        public override string ToString()
        {
            return $"Shortcut {Name}: {Path}";
        }
    }

    public static class FileReferenceUtils
    {
        public static NamedString SplitPath(this string source)
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

        public static string ResolveShortcutPath(this IEnumerable<IFolderShortcut> shortcuts, string path, ref string errorMessage)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Empty path argument.");
            }

            var splittedPath = FileReferenceUtils.SplitPath(path);

            if (String.IsNullOrEmpty(splittedPath.Name))
            {
                return path;
            }
            else
            {
                IFolderShortcut shortcut = shortcuts.FirstOrDefault(s => s.Name == splittedPath.Name);
                if (shortcut != null)
                {
                    string p = ResolveShortcutPath(shortcuts, shortcut.Path, ref errorMessage);

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

    public class FolderCollection : IFolderShortcutsSource
    {
        FolderShortcutOrigin m_origin;
        List<IFolderShortcutsSource> m_collections = new List<IFolderShortcutsSource>();
        List<IFolderShortcut> m_shortcuts = new List<IFolderShortcut>();

        public FolderCollection(FolderShortcutOrigin origin, params IFolderShortcutsSource[] collections)
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

        public IFolderShortcut AddShortcut(string name, string path)
        {
            FolderShortcut shortcut = new FolderShortcut(m_origin, name, path);
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
        private FolderCollection m_collections = null;
        private FolderCollection m_environmentShortcuts = new FolderCollection(FolderShortcutOrigin.Environment);
        private FolderCollection m_configurationShortcuts = new FolderCollection(FolderShortcutOrigin.Configuration);

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
                var sf = (System.Environment.SpecialFolder)Enum.Parse(typeof(System.Environment.SpecialFolder),sfname);
                var folder = System.Environment.GetFolderPath(sf);
                if (!String.IsNullOrEmpty(folder))
                {
                    m_environmentShortcuts.AddShortcut(sfname, folder);
                }
            }
            foreach (DictionaryEntry ev in Environment.GetEnvironmentVariables())
            {
                var value = ev.Value as string;
                if (!String.IsNullOrEmpty(value) && value.Contains(System.IO.Path.DirectorySeparatorChar))
                {
                    var key = ev.Key as string;
                    if (!m_environmentShortcuts.ListShortcuts().Any(k => String.Equals(k.Name, key, StringComparison.InvariantCultureIgnoreCase)) && System.IO.Directory.Exists(value))
                    {
                        m_environmentShortcuts.AddShortcut(key, value);
                    }
                }
            }
            m_collections = new FolderCollection(FolderShortcutOrigin.Root, m_environmentShortcuts, m_configurationShortcuts);
        }
    }
}
