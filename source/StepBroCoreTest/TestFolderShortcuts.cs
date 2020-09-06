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
            Assert.AreEqual(@"myfile.txt", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), "myfile.txt"));
            Assert.AreEqual(@"c:\temp\myfile.txt", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), @"c:\temp\myfile.txt"));
        }

        [TestMethod]
        public void NoRelative()
        {
            Assert.AreEqual(@"C:\temp", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), "[Anna]"));
            Assert.AreEqual(@"C:\temp\sub", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), "[Christina]"));
        }

        [TestMethod]
        public void RelativePath()
        {
            Assert.AreEqual(@"C:\temp\f1\file.txt", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), @"[Anna]\f1\file.txt"));
            Assert.AreEqual(@"C:\temp\sub\f1\file.txt", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), @"[Christina]\f1\file.txt"));

            Assert.AreEqual(@"C:\temp\..\f2\file.txt", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), @"[Anna]\..\f2\file.txt"));
            Assert.AreEqual(@"C:\temp\sub\..\f2\file.txt", FileReferenceUtils.ResolveShortcutPath(m_collection.ListShortcuts(), @"[Christina]\..\f2\file.txt"));

            Assert.AreEqual(@"C:\f2\file.txt", FileReferenceUtils.GetFullPath(m_collection.ListShortcuts(), @"[Anna]\..\f2\file.txt"));
            Assert.AreEqual(@"C:\temp\f2\file.txt", FileReferenceUtils.GetFullPath(m_collection.ListShortcuts(), @"[Christina]\..\f2\file.txt"));
        }
    }
}
