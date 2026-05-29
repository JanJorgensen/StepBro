using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System;

namespace StepBro.Core.File
{
    [Public]
    public class FilePath : IReconstructable
    {
        private string m_filepath;

        public FilePath()
        {
            m_filepath = String.Empty;
        }

        internal FilePath(string path)
        {
            m_filepath = path;
        }

        public FilePath([Implicit] IScriptFile home, string path)
        {
            m_filepath = GetFullPath(home, path);
        }

        public string Value { get { return m_filepath; } }

        public FilePath ParentFolder()
        {
            return new FilePath(System.IO.Directory.GetParent(m_filepath).FullName);
        }

        public override string ToString()
        {
            return m_filepath;
        }

        public static implicit operator string(FilePath fp) => fp.m_filepath;
        //public static explicit operator FilePath(string s) => new FilePath(s);

        public static FilePath operator +(FilePath a, FilePath b) => new FilePath(System.IO.Path.Combine(a, b));
        public static FilePath operator +(FilePath a, string b) => new FilePath(System.IO.Path.Combine(a, b));

        static public FilePath Create([Implicit] IScriptFile home, string path)
        {
            if (home == null) { throw new ArgumentNullException(nameof(home)); }
            return new FilePath(GetFullPath(home, path));
        }

        public static string GetFullPath(IScriptFile home, string filepath)
        {
            string error = null;
            var result = home.FolderShortcuts.ListShortcuts().GetFullPath(filepath, ref error);
            if (String.IsNullOrEmpty(result))
            {
                throw new ParsingErrorException("FilePath.GetFullPath failed: " + error);
            }
            return result;
        }

        Tuple<Type, Reconstructor, object> IReconstructable.GetReconstructor()
        {
            return new Tuple<Type, Reconstructor, object>(typeof(FilePath), Reconstruct, m_filepath);
        }

        private static object Reconstruct(IScriptCallContext context, object constructionData)
        {
            return new FilePath((string)constructionData);
        }
    }
}
