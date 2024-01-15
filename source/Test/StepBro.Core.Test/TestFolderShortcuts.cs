using System;
using System.Collections.Generic;
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
        FolderCollection m_collection = null;

        [TestInitialize]
        public void Setup()
        {
            m_collection = new FolderCollection(FolderShortcutOrigin.HostApplication);
            m_collection.AddShortcut("Anna", @"C:\temp");
            m_collection.AddShortcut("Betina", @"[Anna]");
            m_collection.AddShortcut("Christina", @"[Anna]\sub");
        }

        [TestMethod]
        public void NoShortcut()
        {
            string error = null;
            Assert.AreEqual(@"myfile.txt", m_collection.ListShortcuts().ResolveShortcutPath("myfile.txt", ref error));
            Assert.AreEqual(@"c:\temp\myfile.txt", m_collection.ListShortcuts().ResolveShortcutPath(@"c:\temp\myfile.txt", ref error));
        }

        [TestMethod]
        public void NoRelative()
        {
            string error = null;
            Assert.AreEqual(@"C:\temp", m_collection.ListShortcuts().ResolveShortcutPath("[Anna]", ref error));
            Assert.AreEqual(@"C:\temp\sub", m_collection.ListShortcuts().ResolveShortcutPath("[Christina], ref error", ref error));
        }

        [TestMethod]
        public void RelativePath()
        {
            string error = null;
            Assert.AreEqual(@"C:\temp\f1\file.txt", m_collection.ListShortcuts().ResolveShortcutPath(@"[Anna]\f1\file.txt", ref error));
            Assert.AreEqual(@"C:\temp\sub\f1\file.txt", m_collection.ListShortcuts().ResolveShortcutPath(@"[Christina]\f1\file.txt", ref error));

            Assert.AreEqual(@"C:\temp\..\f2\file.txt", m_collection.ListShortcuts().ResolveShortcutPath(@"[Anna]\..\f2\file.txt", ref error));
            Assert.AreEqual(@"C:\temp\sub\..\f2\file.txt", m_collection.ListShortcuts().ResolveShortcutPath(@"[Christina]\..\f2\file.txt", ref error));

            Assert.AreEqual(@"C:\f2\file.txt", m_collection.ListShortcuts().GetFullPath(@"[Anna]\..\f2\file.txt", ref error));
            Assert.AreEqual(@"C:\temp\f2\file.txt", m_collection.ListShortcuts().GetFullPath(@"[Christina]\..\f2\file.txt", ref error));
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
