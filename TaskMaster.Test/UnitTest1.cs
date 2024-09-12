using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TaskMaster;
using UtilitiesCS;
using Moq;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;
using TaskMaster.AppGlobals;
using System.Collections.Generic;
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
                var config = await SmartSerializableLoader.DeserializeAsync(appGlobals, configBin);
                spam.Config.Disk = config.Config.Disk;
                spam.Config.NetDisk = config.Config.NetDisk;
                spam.Config.LocalDisk = config.Config.LocalDisk;
                spam.Config.LocalJsonSettings = config.Config.LocalJsonSettings;
                spam.Config.NetJsonSettings = config.Config.NetJsonSettings;
                spam.Config.JsonSettings = config.Config.JsonSettings;
                spam.Config.ClassifierActivated = config.Activated;
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
    }
}
