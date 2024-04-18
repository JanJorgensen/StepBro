using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StepBro.VISA
{
    /// <summary>
    /// Class used to load .NET Framework assemblies located in GAC from .NET 5+
    /// Requred only for expiremental using VISA.NET library in .NET 5+
    /// </summary>
    internal class GacLoader : StepBro.VISA.Compatibility.IGacLoader
    {
        /// <summary>
        /// Load an assembly from the GAC.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns>Loaded assembly</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Assembly Load(AssemblyName assemblyName)
        {
            var gacPaths = new[]
            {
               $"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\\Microsoft.NET\\assembly\\GAC_MSIL\\{assemblyName.Name}",
               $"{Environment.GetFolderPath(Environment.SpecialFolder.Windows)}\\assembly\\GAC_MSIL\\{assemblyName.Name}",
            };

            foreach (var folder in gacPaths.Where(f => Directory.Exists(f)))
            {
                foreach (var subfolder in Directory.EnumerateDirectories(folder))
                {
                    if (subfolder.Contains(Convert.ToHexString(assemblyName.GetPublicKeyToken()), StringComparison.OrdinalIgnoreCase)
                        && subfolder.Contains(assemblyName.Version.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        var assemblyPath = Path.Combine(subfolder, assemblyName.Name + ".dll");
                        if (File.Exists(assemblyPath))
                            return Assembly.LoadFrom(assemblyPath);
                    }
                }
            }
            throw new FileNotFoundException($"Assembly {assemblyName} not found.");
        }
    }
}