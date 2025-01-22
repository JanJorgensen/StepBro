using StepBro.Core.File;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    /// <summary>
    /// Abstract file system interface, for listing and opening textual files (typicaly script files, data files and configuration files).
    /// </summary>
    public interface ITextFileSystem
    {
        IEnumerable<string> ListFiles(string path);
        IEnumerable<string> ListFolders(string path);

        DateTime GetFileChangeTime(string path);
        StreamReader OpenFileStream(string path);
        string ReadTextFile(string path);

        /// <summary>
        /// Search for the specified file based on the specified search path. 
        /// The search will continue in the base path folders all down to the root folder, or until a config file (<see cref="StepBro.Core.Api.Constants.STEPBRO_FOLDER_CONFIG_FILE"/>) is found with the root flaq set (<see cref="StepBro.Core.Api.Constants.FOLDER_CONFIG_FILE_ROOT_FLAG"/>);
        /// </summary>
        /// <param name="startpath">The base path where the search starts.</param>
        /// <param name="name">The name of the file to find.</param>
        /// <param name="shortcuts">Interface to a folder shortcut source.</param>
        /// <returns>The full path of the found file, or <code>null</code> if no matching file was found.</returns>
        string SearchFile(string startpath, string name, IFolderShortcutsSource shortcuts = null);
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>true if the caller has the required permissions and path contains the name of an existing file; otherwise, false.</returns>
        bool FileExists(string path);
        /// <summary>
        /// Determins whether the specified folder/directory exists.
        /// </summary>
        /// <param name="path">The directory to check.</param>
        /// <returns>true if the caller has the required permissions and path contains the name of an existing directory; otherwise, false.</returns>
        bool DirectoryExists(string path);
    }

    public class TextFileSystem : ServiceBase<ITextFileSystem, TextFileSystem>, ITextFileSystem
    {

        public TextFileSystem(out IService serviceAccess) :
            base("TextFileSystem", out serviceAccess, typeof(ILogger), typeof(IConfigurationFileManager))
        {
        }

        public IEnumerable<string> ListFiles(string path)
        {
            return System.IO.Directory.GetFiles(path);
        }

        public IEnumerable<string> ListFolders(string path)
        {
            return System.IO.Directory.GetDirectories(path);
        }

        public DateTime GetFileChangeTime(string path)
        {
            return System.IO.File.GetLastWriteTime(path);
        }

        public StreamReader OpenFileStream(string path)
        {
            return System.IO.File.OpenText(path);
        }

        public string ReadTextFile(string path)
        {
            return System.IO.File.ReadAllText(path);
        }


        public string SearchFile(string startpath, string name, IFolderShortcutsSource shortcuts)
        {
            return SearchFile(this, startpath, name, shortcuts);
        }

        public bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        public static string SearchFile(ITextFileSystem fileSystem, string startpath, string name, IFolderShortcutsSource shortcuts)
        {
            throw new NotImplementedException();
        }
    }
}
