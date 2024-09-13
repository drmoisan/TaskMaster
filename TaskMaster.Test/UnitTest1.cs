using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TaskMaster;
using UtilitiesCS;
using Moq;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;
using TaskMaster.AppGlobals;
using System.Collections.Generic;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.ReusableTypeClasses;
using TaskMaster.Properties;
using System.Reflection;
using Microsoft.Build.Evaluation;
using System.Linq;
using UtilitiesCS.NewtonsoftHelpers;

namespace TaskMaster.Test
{
    [TestClass]
    public class UnitTest1
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockGlobals;
        private Mock<Microsoft.Office.Interop.Outlook.Application> mockApplication;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            mockRepository = new MockRepository(MockBehavior.Strict);
            mockGlobals = mockRepository.Create<IApplicationGlobals>();
            mockApplication = mockRepository.Create<Microsoft.Office.Interop.Outlook.Application>();
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var appGlobals = new ApplicationGlobals(mockApplication.Object);
            //var af = new AppAutoFileObjects(mockGlobals.Object);
            var af = new AppAutoFileObjects(appGlobals);
            af.ResetLoadManager();
            
            var manager = await af.Manager2;
            Assert.IsNotNull(manager);
            //af.LoadManagerConfig();
            //var names = af.GetManifestResourceNames();
            //foreach (var name in names)
            //{
            //    Console.WriteLine(name);
            //}

        }

        [TestMethod]
        public async Task TestMethod2()
        {
            var appGlobals = new ApplicationGlobals(mockApplication.Object);
            //var af = new AppAutoFileObjects(mockGlobals.Object);
            var af = new AppAutoFileObjects(appGlobals);
            var manager = af.LoadManager();            
            //af.ResetLoadManager();
            //var manager = await af.Manager2;

            var spam = manager["Spam"];
            if (af.BinaryResources.TryGetValue("ConfigSpam", out byte[] configBin))
            {
                var config = await SmartSerializableConfig.DeserializeAsync(appGlobals, configBin);
                spam.Disk = config.Disk;
                spam.NetDisk = config.NetDisk;
                spam.LocalDisk = config.LocalDisk;
                spam.LocalJsonSettings = config.LocalJsonSettings;
                spam.NetJsonSettings = config.NetJsonSettings;
                spam.JsonSettings = config.JsonSettings;
                spam.ClassifierActivated = config.Activated;
                spam.Serialize();
            }
        }

        [TestMethod]
        public async Task TestMethod3()
        {
            var globals = new ApplicationGlobals(mockApplication.Object);
            var af = new AppAutoFileObjects(globals);
            af.ResetLoadManagerLazy();

            var spam = await af.ManagerLazy["Spam"];
            Assert.IsNotNull(spam);

        }

        [TestMethod]
        public void TestMethod4()
        {
            var config = new ManagerLazyConfig();
            config.Configurations = new List<ManagerLazyConfigStruct>() 
            { 
                new ManagerLazyConfigStruct() { ResourceName = "ConfigSpam", ClassifierName = "Spam", Active = true },
                new ManagerLazyConfigStruct() { ResourceName = "ConfigFolder", ClassifierName = "Folder", Active = true },
                new ManagerLazyConfigStruct() { ResourceName = "ConfigTriage", ClassifierName = "Triage", Active = true }
            };
            
        }

        [TestMethod]
        public void TestMethodConverter()
        {
            var appGlobals = new ApplicationGlobals(mockApplication.Object);
            var af = new AppAutoFileObjects(appGlobals);
            var manager = af.LoadManager();
            var managerNew = new NewScDictionary<string, BayesianClassifierGroup>(manager);
            managerNew.Config.Disk = manager.Disk;
            managerNew.Config.NetDisk = manager.NetDisk;
            managerNew.Config.LocalDisk = manager.LocalDisk;
            managerNew.Config.LocalJsonSettings = manager.LocalJsonSettings;
            managerNew.Config.NetJsonSettings = manager.NetJsonSettings;
            managerNew.Config.JsonSettings = manager.JsonSettings;
            managerNew.Config.JsonSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            managerNew.Config.JsonSettings.Converters.Add(new AppGlobalsConverter(appGlobals));
            managerNew.Config.JsonSettings.TraceWriter = new NLogTraceWriter();
            managerNew.Config.Disk.FileName = "managerConverted.json";

            managerNew.SerializeThreadSafe(managerNew.Config.Disk.FilePath);

            Assert.IsTrue(System.IO.File.Exists(managerNew.Config.Disk.FilePath));
        }

        [TestMethod]
        public void TestNewScDict()
        {
            var appGlobals = new ApplicationGlobals(mockApplication.Object);
            var af = new AppAutoFileObjects(appGlobals);
            var dict = new NewScDictionary<string, string>();
            dict["key1"] = "value1";
            dict.Config.Disk.FileName = "testdict.json";
            dict.Config.Disk.FolderPath = appGlobals.FS.FldrAppData;            
            dict.Config.NetDisk.FileName = "testdict.json";
            dict.Config.NetDisk.FolderPath = appGlobals.FS.FldrAppData;
            dict.Config.LocalDisk = dict.Config.Disk;
            dict.Config.JsonSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            dict.Config.JsonSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All;
            dict.Config.JsonSettings.Converters.Add(new AppGlobalsConverter(appGlobals));
            //dict.Config.JsonSettings.TraceWriter = new NLogTraceWriter();
            Action<string, Exception> action = (msg, ex) => Console.WriteLine(msg);
            dict.Config.JsonSettings.TraceWriter = new NConsoleTraceWriter() { Log = action };

            dict.SerializeThreadSafe(dict.Config.Disk.FilePath);

            Assert.IsTrue(System.IO.File.Exists(dict.Config.Disk.FilePath));
        }

        [TestMethod]
        public void PrintAssemblies()
        {
            var appGlobals = new ApplicationGlobals(mockApplication.Object);
            //var classes = appGlobals.GetClasses();
            //projects.Select(project => project.)
            //foreach (var c in classes)
            //{
            //    if (c.Name.Contains("ToDoObj"))
            //    {
            //        continue;
            //    }
            //    Console.WriteLine($"{c.Attributes} => {c.Name}");
            //}

            //var assemblies = AppDomain.CurrentDomain.GetAssemblies();            
            //foreach (var assembly in assemblies)
            //{
            //    Console.WriteLine(assembly.GetName().Name);
            //}
        }

        [TestMethod]
        public void GetBinder() 
        { 

        }

        




    }
}
