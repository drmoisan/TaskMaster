using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using UtilitiesCS;
using Newtonsoft.Json;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class AppGlobalsConverterTests
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockApplicationGlobals;
        private Mock<IAppStagingFilenames> mockStagingFilenames;
        private Mock<IFileSystemFolderPaths> mockFileSystemsFolderPaths;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockFileSystemsFolderPaths = this.mockRepository.Create<IFileSystemFolderPaths>();
            this.mockFileSystemsFolderPaths.SetupGet(x => x.FldrPythonStaging).Returns("Working");
            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
            this.mockApplicationGlobals.SetupAllProperties();
            this.mockApplicationGlobals.SetupGet(x => x.FS).Returns(this.mockFileSystemsFolderPaths.Object);
            
        }

        private JsonSerializerSettings CreateJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            settings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All;
            settings.Converters.Add(new AppGlobalsConverter(this.mockApplicationGlobals.Object));
            
            return settings;
        }

        private AppGlobalsConverter CreateAppGlobalsConverter()
        {
            return new AppGlobalsConverter(
                this.mockApplicationGlobals.Object);
        }

        public class SampleClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public IApplicationGlobals AppGlobals { get; set; }
        }

        [TestMethod]
        public void SimpleConversionTest() 
        { 
            // Arrange
            var settings = this.CreateJsonSerializerSettings();
            var appGlobals = this.mockApplicationGlobals.Object;
            var input = new SampleClass() { Name = "Test", Age = 47, AppGlobals = appGlobals };
            var expected = appGlobals.FS.FldrPythonStaging;

            
            // Act
            var json = JsonConvert.SerializeObject(input, settings);
            Console.WriteLine("JSON Object");
            Console.WriteLine(json);
            var actualObj = JsonConvert.DeserializeObject<SampleClass>(json, settings);
            var actual = actualObj.AppGlobals.FS.FldrPythonStaging;
            
            // Assert
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void ReadJson_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var appGlobalsConverter = this.CreateAppGlobalsConverter();
            JsonReader reader = null;
            Type objectType = null;
            IApplicationGlobals existingValue = null;
            bool hasExistingValue = false;
            JsonSerializer serializer = null;

            // Act
            var result = appGlobalsConverter.ReadJson(
                reader,
                objectType,
                existingValue,
                hasExistingValue,
                serializer);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void WriteJson_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var appGlobalsConverter = this.CreateAppGlobalsConverter();
            JsonWriter writer = null;
            IApplicationGlobals value = null;
            JsonSerializer serializer = null;

            // Act
            appGlobalsConverter.WriteJson(
                writer,
                value,
                serializer);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
