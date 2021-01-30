using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace StepBroCoreTest.Generators
{
    [TestClass]
    public class LargeFileGenerators
    {
        private static string GetTestFolder()
        {
            var binfolder = Path.GetDirectoryName(typeof(LargeFileGenerators).Assembly.Location);
            return Path.GetFullPath(Path.Combine(binfolder, "..", "..", "test files"));
        }

        [TestMethod]
        public void CreateWideCallTreeScenario()
        {
            const int l2width = 100;
            const int l3width = 100;
            var folder = GetTestFolder();
            var filepath = Path.Combine(folder, "wide procedure hierarchy." + Main.StepBroFileExtension);
            var lines = new List<string>();

            var secondlevel = new Func<int, string>(a => "ProcL2" + StepBro.Core.Data.AlphaID.Create(a, 4));
            var thirdlevel = new Func<int, string>(a => "ProcL3" + StepBro.Core.Data.AlphaID.Create(a, 4));

            for (int i = 0; i < l3width; i++)
            {
                lines.Add($"procedure void {thirdlevel(i)}()");
                lines.Add("{");
                lines.Add($"    log (\"Here in ProcL3{thirdlevel(i)}\");");
                lines.Add("}");
            }
            for (int i = 0; i < l2width; i++)
            {
                lines.Add($"procedure void {secondlevel(i)}()");
                lines.Add("{");
                lines.Add($"    log (\"Here in ProcL3{secondlevel(i)}\");");
                for (int j = 0; j < l3width; j++)
                {
                    lines.Add($"    {thirdlevel(j)}();");
                }
                lines.Add("}");
            }
            lines.Add($"procedure void Main()");
            lines.Add("{");
            lines.Add($"    log (\"Here in Main\");");
            for (int i = 0; i < l2width; i++)
            {
                lines.Add($"    {secondlevel(i)}();");
            }
            lines.Add("}");
            File.WriteAllLines(filepath, lines);
        }
    }
}
