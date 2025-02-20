using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using UtilitiesCS.NewtonsoftHelpers;
using FluentAssertions;
using UtilitiesCS.ReusableTypeClasses;
using Moq;


namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class WrapperScoDictionaryTest
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockGlobals;
        private Mock<Microsoft.Office.Interop.Outlook.Application> mockApplication;
        private IApplicationGlobals globals;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            mockRepository = new MockRepository(MockBehavior.Strict);
            mockGlobals = mockRepository.Create<IApplicationGlobals>();
            mockApplication = mockRepository.Create<Microsoft.Office.Interop.Outlook.Application>();
        }

        internal class TestDerived : ScoDictionaryNew<string, int>
        {
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            private string _additionalField3;
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }

            public TestDerived()
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
            }

            public int GetAdditionalField2() => AdditionalField2;
        }

        internal class RemainingObjectClass
        {
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            public int GetAdditionalField2() => AdditionalField2;
            private string _additionalField3;
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }

            public RemainingObjectClass()
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
            }
        }

        internal class RemainingObjectClass1
        {
            public NewSmartSerializableConfig Config { get; set; }
        }

        internal class DerivedTest2 : ScoDictionaryNew<string, string>
        {
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            private string _additionalField3;
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }

            public DerivedTest2()
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
            }
            public int GetAdditionalField2() => AdditionalField2;
        }

        internal DerivedTest2 GetDerivedTest2()
        {
            var dt = new DerivedTest2();
            dt.TryAdd("key1", "value1");
            dt.TryAdd("key2", "value2");
            globals = new TaskMaster.ApplicationGlobals(mockApplication.Object, true);
            dt.Config = ConfigInitializer.InitConfig(new NewSmartSerializableConfig(), globals);
            return dt;
        }

        internal class RemainingObjectClass2
        {
            public NewSmartSerializableConfig Config { get; set; }
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            private string _additionalField3;
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }
            public int GetAdditionalField2() => AdditionalField2;
            public RemainingObjectClass2()
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
            }
        }

        internal WrapperScoDictionary<DerivedTest2, string, string> GetWrapperComposedTest2()
        {
            var wrapper = new WrapperScoDictionary<DerivedTest2, string, string>();
            wrapper.CoDictionary.TryAdd("key1", "value1");
            wrapper.CoDictionary.TryAdd("key2", "value2");
            var ro = new RemainingObjectClass2();
            ro.Config = ConfigInitializer.InitConfig(new NewSmartSerializableConfig(), globals);
            wrapper.RemainingObject = ro;
            return wrapper;
        }

        public static class ConfigInitializer
        {
            public static NewSmartSerializableConfig InitConfig(NewSmartSerializableConfig config, IApplicationGlobals globals)
            {
                config.Disk.FileName = "testdict.json";
                if (globals.FS.SpecialFolders.TryGetValue("AppData", out var appData))
                {
                    config.Disk.FolderPath = appData;
                    config.NetDisk.FileName = "testdict.json";
                    config.NetDisk.FolderPath = appData;
                    config.LocalDisk = config.Disk;
                    config.JsonSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
                    config.JsonSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                    config.JsonSettings.Converters.Add(new AppGlobalsConverter(globals));
                    config.JsonSettings.Converters.Add(new FilePathHelperConverter(globals.FS));
                    config.JsonSettings.Converters.Add(new ScDictionaryConverter<ScDictionary<string, string>, string, string>());
                    return config;
                }
                return null;

            }
        }

        [TestMethod]
        public void ToComposition_ShouldExtractScDictionaryAndFields()
        {
            // Arrange
            var derived = GetDerivedTest2();
            var expected = GetWrapperComposedTest2();

            // Act
            var actual = new WrapperScoDictionary<DerivedTest2, string, string>().ToComposition(derived);

            // Assert            
            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public void ToDerived_ShouldRecreateScDictionaryDerivative()
        {
            // Arrange
            var expected = GetDerivedTest2();
            var composed = GetWrapperComposedTest2();

            // Act
            var actual = composed.ToDerived();

            // Assert
            actual.Should().BeEquivalentTo(expected);
            actual.Config.Should().BeEquivalentTo(expected.Config);
            actual.AdditionalField3.Should().Be(expected.AdditionalField3);
            actual.AdditionalField1.Should().Be(expected.AdditionalField1);
            
        }

        [TestMethod]
        public void ToComposition_ShouldExtractDictionaryAndAdditionalFields()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);

            var wrapper = new WrapperScoDictionary<TestDerived, string, int>();

            // Act
            wrapper.ToComposition(derivedInstance);

            // Assert
            Assert.AreEqual(2, wrapper.CoDictionary.Count);
            Assert.AreEqual(1, wrapper.CoDictionary["key1"]);
            Assert.AreEqual(2, wrapper.CoDictionary["key2"]);

            var remainingObjectType = wrapper.RemainingObject.GetType();
            var additionalProperty1 = remainingObjectType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public);
            var additionalField2 = remainingObjectType.GetField("AdditionalField2", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(additionalProperty1);
            Assert.IsNotNull(additionalField2);
            Assert.AreEqual("Test", additionalProperty1.GetValue(wrapper.RemainingObject));
            Assert.AreEqual(42, additionalField2.GetValue(wrapper.RemainingObject));
        }

        [TestMethod]
        public void ToDerived_ShouldRecreateDerivedInstance()
        {
            // Arrange
            var composedInstance = new WrapperScoDictionary<TestDerived, string, int>();
            composedInstance.CoDictionary.TryAdd("key1", 1);
            composedInstance.CoDictionary.TryAdd("key2", 2);
            composedInstance.RemainingObject = new RemainingObjectClass();
            var expected = new TestDerived();
            expected.TryAdd("key1", 1);
            expected.TryAdd("key2", 2);

            // Act            
            var recreatedInstance = composedInstance.ToDerived(composedInstance);

            // Assert
            recreatedInstance.Should().BeEquivalentTo(expected);
            recreatedInstance.Config.Should().BeEquivalentTo(expected.Config);
            recreatedInstance.AdditionalField3.Should().Be(expected.AdditionalField3);
            recreatedInstance.AdditionalField1.Should().Be(expected.AdditionalField1);
        }

        [TestMethod]
        public void EndToEnd_ShouldRecreateDerivedInstance()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);


            // Act
            var wrapper = new WrapperScoDictionary<TestDerived, string, int>().ToComposition(derivedInstance);
            var recreatedInstance = wrapper.ToDerived();

            // Assert
            recreatedInstance.Should().BeEquivalentTo(derivedInstance);
            recreatedInstance.Config.Should().BeEquivalentTo(derivedInstance.Config);
            recreatedInstance.AdditionalField3.Should().Be(derivedInstance.AdditionalField3);
            recreatedInstance.AdditionalField1.Should().Be(derivedInstance.AdditionalField1);
        }

        [TestMethod]
        public void CompileType_ShouldCreateTypeWithAdditionalFields()
        {
            // Arrange
            var wrapper = new WrapperScoDictionary<TestDerived, string, int>();

            // Act
            var newType = wrapper.CompileType();

            // Assert
            Assert.IsNotNull(newType);
            var additionalProperty1 = newType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public);
            var additionalField2 = newType.GetField("AdditionalField2", BindingFlags.Instance | BindingFlags.NonPublic);
            var additionalProperty3 = newType.GetProperty("AdditionalField3", BindingFlags.Instance | BindingFlags.Public);
            var additionalField3 = newType.GetField("_additionalField3", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(additionalProperty1);
            Assert.IsNotNull(additionalField2);
            Assert.IsNotNull(additionalProperty3);
            Assert.IsNotNull(additionalField3);
        }

        [TestMethod]
        public void ConvertToNewClassInstance_ShouldCreateInstanceWithFields()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);
            var expected = new WrapperScoDictionary<TestDerived, string, int>();
            expected.CoDictionary.TryAdd("key1", 1);
            expected.CoDictionary.TryAdd("key2", 2);
            expected.RemainingObject = new RemainingObjectClass();

            // Act
            var wrapper = new WrapperScoDictionary<TestDerived, string, int>().ToComposition(derivedInstance);

            var newClassInstance = wrapper.RemainingObject;
            var newClassType = newClassInstance.GetType();

            // Assert
            Assert.IsNotNull(newClassInstance);
            var property = newClassType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public);
            var value = property.GetValue(newClassInstance);
            Assert.AreEqual("Test", newClassType.GetProperty("AdditionalField1", BindingFlags.Instance | BindingFlags.Public).GetValue(newClassInstance));
            Assert.AreEqual(42, newClassType.GetField("AdditionalField2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(newClassInstance));
            wrapper.Should().BeEquivalentTo(expected);
        }
    }
}
