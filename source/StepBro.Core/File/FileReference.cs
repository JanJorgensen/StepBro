using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StepBro.Core.Data;

namespace StepBro.Core.File
{
    public enum FolderShortcutOrigin
    {
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
        string Name { get; set; }
        string Path { get; set; }
    }

    public interface IFolderShortcutsSource
    {
        /// <summary>
        /// The list of folder shortcuts defined in the environment (OS), the host application, the configuration and the current script file.
        /// </summary>
        /// <returns>An enumeration of the available shortcuts.</returns>
        IEnumerable<IFolderShortcut> GetFolders();
    }

    public class FolderShortcut : IFolderShortcut
    {
        private FolderShortcutOrigin m_origin;
        private string m_name;
        private string m_path;

        public FolderShortcut(FolderCollection parentCollection, FolderShortcutOrigin origin, string name, string path)
        {
            m_origin = parentCollection.Origin;
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

            set
            {
                m_name = value;
                // Notify!
            }
        }

        public string Path
        {
            get
            {
                return m_path;
            }

            set
            {
                m_path = value;
                // Notify!
            }
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

        public static string ResolveShortcutPath(this IEnumerable<IFolderShortcut> shortcuts, string path)
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
                    string p = ResolveShortcutPath(shortcuts, shortcut.Path);

                    if (String.IsNullOrEmpty(splittedPath.Value))
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
                    throw new ArgumentException("Unknown folder shortcut: '" + splittedPath.Name + "'.");
                }
            }
        }

        public static string GetFullPath(this IEnumerable<IFolderShortcut> shortcuts, string path)
        {
            string resolved = ResolveShortcutPath(shortcuts, path);
            return System.IO.Path.GetFullPath(resolved);
        }
    }

    //public interface IFolderManager
    //{
    //    FolderCollection CreateCollection(FolderShortcutOrigin origin);
    //    string GetFullPath(string path);
    //}

    //internal class FolderManager : IFolderManager
    //{
    //    List<FolderCollection> m_collections = new List<FolderCollection>();

    //    public FolderCollection CreateCollection(FolderShortcutOrigin origin)
    //    {
    //        FolderCollection collection = new FolderCollection(this, origin);
    //        m_collections.Add(collection);
    //        return collection;
    //    }

    //    private IEnumerable<IFolderShortcut> AllShortCuts()
    //    {
    //        foreach (var collection in m_collections)
    //        {
    //            foreach (var shortcut in collection.Shortcuts)
    //            {
    //                yield return shortcut;
    //            }
    //        }
    //    }

    //    public string GetFullPath(string path)
    //    {
    //        return GetFullPath(this.AllShortCuts(), path);
    //    }

    //}

    public class FolderCollection : IDisposable
    {
        FolderShortcutOrigin m_origin;
        List<FolderCollection> m_collections = new List<FolderCollection>();
        List<IFolderShortcut> m_shortcuts = new List<IFolderShortcut>();
        //FolderManager m_manager;

        public FolderCollection(FolderShortcutOrigin origin, params FolderCollection[] collections)
        {
            m_origin = origin;
            m_collections.AddRange(collections);
            //m_manager = manager;
        }


        public FolderShortcutOrigin Origin { get { return m_origin; } }

        public IEnumerable<IFolderShortcut> Shortcuts { get { return m_shortcuts; } }

        public IFolderShortcut AddShortcut(string name, string path)
        {
            FolderShortcut shortcut = new FolderShortcut(this, m_origin, name, path);
            m_shortcuts.Add(shortcut);
            //folder.FolderChanged += Folder_Changed;
            //if (this.FolderAdded != null) this.FolderAdded(this, EventArgs.Empty);
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

        //private void Folder_Changed(object sender, EventArgs e)
        //{
        //    if (this.FolderChanged != null) this.FolderChanged(this, e);
        //}

        public void Dispose()
        {
            // Remove from manager.
            throw new NotImplementedException();
        }

        //public event EventHandler FolderChanged;
        //public event EventHandler FolderAdded;
    }
}
