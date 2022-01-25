using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using StepBro.Core.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Test
{
    internal class FileParsingSetup
    {
        public IAddonManager Addons { get; set; }
        public IDynamicObjectManager DynamicObjects { get; set; }
        public ILoadedFilesManager LoadedFiles { get; set; }
        public ServiceManager.IServiceManagerAdministration Services { get; }
        public TextFileSystemMock FileSystem { get; }

        public FileParsingSetup(Assembly testAssembly)
        {
            this.Services = ServiceManager.Create();
            //LastServiceManager = services;

            IService service;
            this.Addons = new AddonManager(null, out service);
            this.Services.Manager.Register(service);
            this.DynamicObjects = new DynamicObjectManager(out service);
            this.Services.Manager.Register(service);
            var configFileManager = new Mocks.ConfigurationFileManagerMock(out service);
            this.Services.Manager.Register(service);
            this.LoadedFiles = new LoadedFilesManager(out service);
            this.Services.Manager.Register(service);
            var mainLogger = new Logger("", false, "StepBro", "Main logger created in CreateFileParsingSetup");
            this.Services.Manager.Register(mainLogger.RootScopeService);
            var taskManager = new TaskManager(out service);
            this.Services.Manager.Register(service);

            TaskContextDummy taskContext = new TaskContextDummy();
            this.Services.StartServices(taskContext);

            this.Addons.AddAssembly(typeof(System.Convert).Assembly, false);
            this.Addons.AddAssembly(typeof(Math).Assembly, false);
            this.Addons.AddAssembly(typeof(Enumerable).Assembly, false);
            this.Addons.AddAssembly(AddonManager.StepBroCoreAssembly, true);

            if (testAssembly != null) this.Addons.AddAssembly(testAssembly, false);

            //foreach (var f in files)
            //{
            //    loadedFiles.RegisterLoadedFile(f);
            //}
        }
    }
}
