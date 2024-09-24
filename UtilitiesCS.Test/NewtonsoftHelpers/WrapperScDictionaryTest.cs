using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using UtilitiesCS.NewtonsoftHelpers;
using FluentAssertions;
using UtilitiesCS.ReusableTypeClasses;
using Moq;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class WrapperScDictionaryTest
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

        private class TestDerived : NewScDictionary<string, int>
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
                
        private class RemainingObjectClass
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

        private class RemainingObjectClass1
        {            
            public SmartSerializableConfig Config { get; set; }
        }

        private class DerivedTest2: NewScDictionary<string, string>
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

        private DerivedTest2 GetDerivedTest2()
        {
            var dt = new DerivedTest2();
            dt.TryAdd("key1", "value1");
            dt.TryAdd("key2", "value2");
            globals = new TaskMaster.ApplicationGlobals(mockApplication.Object);
            dt.Config = ConfigInitializer.InitConfig(new NewSmartSerializableConfig(), globals);
            return dt;
        }

        private class RemainingObjectClass2
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

        private WrapperScDictionary<DerivedTest2, string, string> GetWrapperComposedTest2()
        {            
            var wrapper = new WrapperScDictionary<DerivedTest2, string, string>();
            wrapper.ConcurrentDictionary.TryAdd("key1", "value1");
            wrapper.ConcurrentDictionary.TryAdd("key2", "value2");
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
                config.Disk.FolderPath = globals.FS.FldrAppData;
                config.NetDisk.FileName = "testdict.json";
                config.NetDisk.FolderPath = globals.FS.FldrAppData;
                config.LocalDisk = config.Disk;
                config.JsonSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
                config.JsonSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                config.JsonSettings.Converters.Add(new AppGlobalsConverter(globals));
                config.JsonSettings.Converters.Add(new FilePathHelperConverter(globals.FS));
                config.JsonSettings.Converters.Add(new ScDictionaryConverter<NewScDictionary<string, string>, string, string>());
                return config;
            }
        }

        [TestMethod]
        public void ToComposition_ShouldExtractScDictionaryAndFields() 
        { 
            // Arrange
            var derived = GetDerivedTest2();
            var expected = GetWrapperComposedTest2();

            // Act
            var actual = new WrapperScDictionary<DerivedTest2, string, string>().ToComposition(derived);

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
        }

        [TestMethod]
        public void ToComposition_ShouldExtractDictionaryAndAdditionalFields()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);

            var wrapper = new WrapperScDictionary<TestDerived, string, int>();

            // Act
            wrapper.ToComposition(derivedInstance);

            // Assert
            Assert.AreEqual(2, wrapper.ConcurrentDictionary.Count);
            Assert.AreEqual(1, wrapper.ConcurrentDictionary["key1"]);
            Assert.AreEqual(2, wrapper.ConcurrentDictionary["key2"]);

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
            var composedInstance = new WrapperScDictionary<TestDerived, string, int>();
            composedInstance.ConcurrentDictionary.TryAdd("key1", 1);
            composedInstance.ConcurrentDictionary.TryAdd("key2", 2);
            composedInstance.RemainingObject = new RemainingObjectClass();
            var expected = new TestDerived();
            expected.TryAdd("key1", 1);
            expected.TryAdd("key2", 2);

            // Act            
            var recreatedInstance = composedInstance.ToDerived(composedInstance);

            // Assert
            expected.Should().BeEquivalentTo(recreatedInstance);
        }

        [TestMethod]
        public void EndToEnd_ShouldRecreateDerivedInstance()
        {
            // Arrange
            var derivedInstance = new TestDerived();
            derivedInstance.TryAdd("key1", 1);
            derivedInstance.TryAdd("key2", 2);


            // Act
            var wrapper = new WrapperScDictionary<TestDerived, string, int>().ToComposition(derivedInstance);
            var recreatedInstance = wrapper.ToDerived();

            // Assert
            derivedInstance.Should().BeEquivalentTo(recreatedInstance);
        }

        [TestMethod]
        public void CompileType_ShouldCreateTypeWithAdditionalFields()
        {
            // Arrange
            var wrapper = new WrapperScDictionary<TestDerived, string, int>();

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
            var expected = new WrapperScDictionary<TestDerived, string, int>();
            expected.ConcurrentDictionary.TryAdd("key1", 1);
            expected.ConcurrentDictionary.TryAdd("key2", 2);
            expected.RemainingObject = new RemainingObjectClass();

            // Act
            var wrapper = new WrapperScDictionary<TestDerived, string, int>().ToComposition(derivedInstance);

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
