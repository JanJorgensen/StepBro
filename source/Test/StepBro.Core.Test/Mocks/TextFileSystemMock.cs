using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Test.Mocks
{
    internal class TextFileSystemMock : ServiceBase<ITextFileSystem, TextFileSystemMock>, ITextFileSystem
    {
        private string m_basePath;
        private List<NamedData<Tuple<string,DateTime>>> m_files = new List<NamedData<Tuple<string, DateTime>>>();

        public TextFileSystemMock(out IService serviceAccess) :
            base("TextFileSystem", out serviceAccess, typeof(ILogger), typeof(IConfigurationFileManager))
        {
        }

        public void AddFile(string path, string content)
        {
            m_files.Add(new NamedData<Tuple<string, DateTime>>(path, new Tuple<string, DateTime>(content, DateTime.Now)));
        }

        private NamedData<Tuple<string, DateTime>> TryGetFile(string path)
        {
            foreach (var fileData in m_files)
            {
                string fileFullPath = System.IO.Path.Combine(m_basePath, fileData.Name);
                if (String.Equals(path, fileFullPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    return fileData;
                }
            }
            return NamedData<Tuple<string, DateTime>>.Empty;
        }

        public bool FileExists(string path)
        {
            return this.TryGetFile(path).IsEmpty() == false;
        }

        public DateTime GetFileChangeTime(string path)
        {
            var data = this.TryGetFile(path);
            if (data.IsEmpty()) throw new ArgumentException("File not found.");
            return data.Value.Item2;
        }

        public IEnumerable<string> ListFiles(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ListFolders(string path)
        {
            throw new NotImplementedException();
        }

        public StreamReader OpenFileStream(string path)
        {
            var data = this.TryGetFile(path);
            if (data.IsEmpty()) throw new ArgumentException("File not found.");
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data.Value.Item1));
            return new StreamReader(stream);
        }

        public string SearchFile(string startpath, string name, IFolderShortcutsSource shortcuts)
        {
            return TextFileSystem.SearchFile(this, startpath, name, shortcuts);
        }
    }
}
