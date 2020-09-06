using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBroCoreTest
{
    [TestClass]
    public class AddonManagerTest
    {
        // C:\work\T#\bin\modules\TestModule.dll

        private static string GetBasePath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static string GetTestModulePath()
        {
            return System.IO.Path.Combine(GetBasePath(), "modules\\TestModule.dll");
        }

        [TestMethod]
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

            var usings = new IIdentifierInfo[] { addons.Lookup(null, "TestModule") };
            typeReference = addons.TryGetType(usings, "TestClassWithOnlyProperties");
            Assert.IsNotNull(typeReference);
            Assert.AreEqual("TestClassWithOnlyProperties", typeReference.Name);
            typeReference = null;

            typeReference = addons.TryGetType(usings, "SubNamespace.Deep.VeryDeep.MuchDeeper.TestClassInDeepNamespace.SubClass");
            Assert.IsNotNull(typeReference);
            Assert.AreEqual("SubClass", typeReference.Name);
            typeReference = null;

            usings = new IIdentifierInfo[] { addons.Lookup(null, "TestModule"), addons.Lookup(null, "TestModule.SubNamespace.Deep") };
            typeReference = addons.TryGetType(usings, "VeryDeep.MuchDeeper.TestClassInDeepNamespace.SubClass");
            Assert.IsNotNull(typeReference);
            Assert.AreEqual("SubClass", typeReference.Name);
            typeReference = null;

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
                addons.ListExtensionMethods(
                    typeof(IEnumerable<int>)).Where(mi => mi.Name == "Select"));

            Assert.IsTrue(methods.Select(mi => mi.Name).Contains("Select"));


            methods = new List<System.Reflection.MethodInfo>(
                addons.ListExtensionMethods(
                    typeof(List<int>)).Where(mi => mi.Name == "Select"));

            Assert.IsTrue(methods.Select(mi => mi.Name).Contains("Select"));
        }

        [TestMethod]
        [Ignore]    // Enable again when some extension methods actually are added.
        public void TestExtensionMethods()
        {
            var usings = new string[] { "StepBro.Core.Execution" };
            IAddonManager addons = AddonManager.Create();
            addons.AddAssembly(typeof(StepBro.Core.Data.Verdict).Assembly, false);

            var methods = addons.ListExtensionMethods(
                    typeof(IProcedureReference)).Where(mi => mi.Name == "DynamicInvoke").ToList();
            Assert.AreEqual(1, methods.Count);
            Assert.AreEqual("DynamicInvoke", methods[0].Name);
        }
    }
}
