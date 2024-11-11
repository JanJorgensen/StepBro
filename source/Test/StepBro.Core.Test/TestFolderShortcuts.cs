using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.File;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestFolderShortcuts
    {
        FolderShortcutCollection m_collection = null;

        [TestInitialize]
        public void Setup()
        {
            m_collection = new FolderShortcutCollection(FolderShortcutOrigin.HostApplication);
            m_collection.AddShortcut("Anna", Path.Join("C:", "temp"));
            m_collection.AddShortcut("Betina", @"[Anna]");
            m_collection.AddShortcut("Christina", Path.Join(@"[Anna]", "sub"));
        }

        [TestMethod]
        public void NoShortcut()
        {
            string error = null;
            Assert.AreEqual(@"myfile.txt", m_collection.ListShortcuts().ResolveShortcutPath("myfile.txt", ref error));
            Assert.AreEqual(Path.Join("C:", "temp", "myfile.txt"), m_collection.ListShortcuts().ResolveShortcutPath(Path.Join("C:", "temp", "myfile.txt"), ref error));
        }

        [TestMethod]
        public void NoRelative()
        {
            string error = null;
            string pathToTemp = Path.Join("C:", "temp");
            string pathToSub = Path.Join(pathToTemp, "sub");
            Assert.AreEqual(pathToTemp, m_collection.ListShortcuts().ResolveShortcutPath("[Anna]", ref error));
            Assert.AreEqual(pathToSub, m_collection.ListShortcuts().ResolveShortcutPath("[Christina]", ref error));
        }

        [TestMethod]
        public void RelativePath()
        {
            string error = null;
            string pathToTemp = Path.Join("C:", "temp");
            string pathToSub = Path.Join(pathToTemp, "sub");

            Assert.AreEqual(Path.Join(pathToTemp, "f1", "file.txt"), m_collection.ListShortcuts().ResolveShortcutPath(Path.Join("[Anna]", "f1", "file.txt"), ref error));
            Assert.AreEqual(Path.Join(pathToSub, "f1", "file.txt"), m_collection.ListShortcuts().ResolveShortcutPath(Path.Join("[Christina]", "f1", "file.txt"), ref error));

            Assert.AreEqual(Path.Join(pathToTemp, "..", "f2", "file.txt"), m_collection.ListShortcuts().ResolveShortcutPath(Path.Join("[Anna]", "..", "f2", "file.txt"), ref error));
            Assert.AreEqual(Path.Join(pathToSub, "..", "f2", "file.txt"), m_collection.ListShortcuts().ResolveShortcutPath(Path.Join("[Christina]", "..", "f2", "file.txt"), ref error));

            Assert.AreEqual(Path.Join("C:", "f2", "file.txt"), m_collection.ListShortcuts().GetFullPath(Path.Join("[Anna]", "..", "f2", "file.txt"), ref error));
            Assert.AreEqual(Path.Join(pathToTemp, "f2", "file.txt"), m_collection.ListShortcuts().GetFullPath(Path.Join("[Christina]", "..", "f2", "file.txt"), ref error));
        }

        //[TestMethod]
        //public void TestFolderFun()
        //{
        //    string path = @"c:\temp\anders\henry";
        //    string root = System.IO.Path.GetPathRoot(path);
        //    string parent = System.IO.Directory.GetParent(path).FullName;
        //    parent = System.IO.Path.GetDirectoryName(path);
        //    Assert.AreEqual("", parent);
        //}
    }
}
