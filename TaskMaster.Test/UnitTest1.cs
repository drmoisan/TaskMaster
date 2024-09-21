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
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using SDILReader;

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
        public async Task ConvertSpam()
        {
            var appGlobals = new ApplicationGlobals(mockApplication.Object);
            //var af = new AppAutoFileObjects(mockGlobals.Object);
            var af = new AppAutoFileObjects(appGlobals);
            var manager = af.LoadManager();
            
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
                spam.SerializeThreadSafe(spam.Disk.FilePath);
                //spam.Serialize();
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
            ILGlobals.LoadOpCodes();
            dict["key1"] = "value1";
            dict.Config.Disk.FileName = "testdict.json";
            dict.Config.Disk.FolderPath = appGlobals.FS.FldrAppData;
            dict.Config.NetDisk.FileName = "testdict.json";
            dict.Config.NetDisk.FolderPath = appGlobals.FS.FldrAppData;
            dict.Config.LocalDisk = dict.Config.Disk;
            dict.Config.JsonSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            dict.Config.JsonSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            dict.Config.JsonSettings.Converters.Add(new AppGlobalsConverter(appGlobals));
            dict.Config.JsonSettings.Converters.Add(new FilePathHelperConverter(appGlobals.FS));
            dict.Config.JsonSettings.Converters.Add(new ScDictionaryConverter<NewScDictionary<string, string>, string, string>());
            //dict.Config.JsonSettings.TraceWriter = new NLogTraceWriter();
            Action<string, Exception> action = (msg, ex) => Console.WriteLine(msg);
            dict.Config.JsonSettings.TraceWriter = new NConsoleTraceWriter() { Log = action };

            dict.SerializeThreadSafe(dict.Config.Disk.FilePath);

            Assert.IsTrue(System.IO.File.Exists(dict.Config.Disk.FilePath));
        }
        

        private static string SerializeObjectInternal(object value, Type type, JsonSerializer jsonSerializer)
        {
            StringBuilder sb = new StringBuilder(256);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
            }

            return sw.ToString();
        }


    }
}
