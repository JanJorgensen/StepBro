using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.Core.ScriptData;
using System.Collections.Generic;
using System.Linq;

namespace StepBroCoreTest
{
    [TestClass]
    public class AddonManagerTest
    {
        // C:\work\StepBro\bin\modules\TestModule.dll

        private static string GetBasePath()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.Diagnostics.Debug.WriteLine("Base path: " + path);
            return path;
        }

        private static string GetTestModulePath()
        {
            return System.IO.Path.Combine(GetBasePath(), "modules\\TestModule.dll");
        }

        [TestMethod]
        [Ignore] // CI works in a way we do not understand yet
        public void TestLoadTestModule()
        {
            IAddonManager addons = AddonManager.Create();

            var testFile = GetTestModulePath();
            addons.LoadAssembly(testFile, false);

            var typeReference = addons.TryGetType(null, "TestModule.TestClassWithOnlyProperties");
            Assert.IsNotNull(typeReference);
            typeReference = addons.TryGetType(null, "TestModule.SubNamespace.Deep.VeryDeep.MuchDeeper.TestClassInDeepNamespace");
            Assert.IsNotNull(typeReference);
            typeReference = addons.TryGetType(null, "TestModule.SubNamespace.Deep.VeryDeep.MuchDeeper.TestClassInDeepNamespace.SubClass");
            Assert.IsNotNull(typeReference);

            var usings = new UsingData[] { new UsingData(-1, false, addons.Lookup(null, "TestModule")) };
            typeReference = addons.TryGetType(usings, "TestClassWithOnlyProperties");
            Assert.IsNotNull(typeReference);
            Assert.AreEqual("TestClassWithOnlyProperties", typeReference.Name);

            typeReference = addons.TryGetType(usings, "SubNamespace.Deep.VeryDeep.MuchDeeper.TestClassInDeepNamespace.SubClass");
            Assert.IsNotNull(typeReference);
            Assert.AreEqual("SubClass", typeReference.Name);

            usings = new UsingData[] { new UsingData(-1, false, addons.Lookup(null, "TestModule")), new UsingData(-1, false, addons.Lookup(null, "TestModule.SubNamespace.Deep")) };
            typeReference = addons.TryGetType(usings, "VeryDeep.MuchDeeper.TestClassInDeepNamespace.SubClass");
            Assert.IsNotNull(typeReference);
            Assert.AreEqual("SubClass", typeReference.Name);

            //var moduleClass = typeReference as ICodeModuleClass;
            //Assert.IsNotNull(moduleClass);
        }

        [TestMethod]
        public void TestPeakCANModuleLoad()
        {
            IAddonManager addons = AddonManager.Create();

            var baseDir = GetBasePath();

            var canFile = System.IO.Path.Combine(baseDir, "modules\\StepBro.CAN.dll");
            addons.LoadAssembly(canFile, true);

            var pcanFile = System.IO.Path.Combine(baseDir, "modules\\StepBro.PeakCANPlugin.dll");
            addons.LoadAssembly(pcanFile, true);
        }

        [TestMethod]
        public void TestExtensionMethodBrowsing()
        {
            var usings = new string[] { "System", "System.Linq" };
            IAddonManager addons = AddonManager.Create();
            addons.AddAssembly(typeof(System.Linq.Enumerable).Assembly, false);

            var methods = new List<System.Reflection.MethodInfo>(
                addons.ListExtensionMethods( typeof(IEnumerable<int>), "Select"));

            Assert.IsTrue(methods.Select(mi => mi.Name).Contains("Select"));


            methods = new List<System.Reflection.MethodInfo>(
                addons.ListExtensionMethods(typeof(List<int>), "Select"));

            Assert.IsTrue(methods.Select(mi => mi.Name).Contains("Select"));
        }

        [TestMethod]
        [Ignore]    // Enable again when some extension methods actually are added.
        public void TestExtensionMethods()
        {
            var usings = new string[] { "StepBro.Core.Execution" };
            IAddonManager addons = AddonManager.Create();
            addons.AddAssembly(AddonManager.StepBroCoreAssembly, false);

            var methods = addons.ListExtensionMethods(typeof(IProcedureReference), "DynamicInvoke").ToList();
            Assert.AreEqual(1, methods.Count);
            Assert.AreEqual("DynamicInvoke", methods[0].Name);
        }
    }
}
