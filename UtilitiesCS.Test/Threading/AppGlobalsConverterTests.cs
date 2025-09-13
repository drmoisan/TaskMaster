using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using UtilitiesCS;
using Newtonsoft.Json;
using FluentAssertions;

namespace UtilitiesCS.Test.Threading
{
    [TestClass]
    public class AppGlobalsConverterTests
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockApplicationGlobals;
        //private Mock<IAppStagingFilenames> mockStagingFilenames;
        private Mock<IFileSystemFolderPaths> mockFileSystemsFolderPaths;
        private Mock<JsonReader> mockJsonReader;
        private Mock<Type> mockType;
        private Mock<JsonWriter> mockJsonWriter;
        //private Mock<JsonSerializerSub> mockJsonSerializer;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockFileSystemsFolderPaths = this.mockRepository.Create<IFileSystemFolderPaths>();
            //this.mockFileSystemsFolderPaths.SetupGet(x => x.FldrPythonStaging).Returns("Working");
            this.mockApplicationGlobals = this.mockRepository.Create<IApplicationGlobals>();
            this.mockApplicationGlobals.SetupAllProperties();
            this.mockApplicationGlobals.SetupGet(x => x.FS).Returns(this.mockFileSystemsFolderPaths.Object);
            this.mockJsonReader = this.mockRepository.Create<JsonReader>();
            this.mockJsonReader.SetupAllProperties();
            this.mockType = this.mockRepository.Create<Type>();
            this.mockType.SetupAllProperties();
            this.mockJsonWriter = this.mockRepository.Create<JsonWriter>();
            this.mockJsonWriter.SetupAllProperties();
            //this.mockJsonSerializer = this.mockRepository.Create<JsonSerializerSub>();
            //this.mockJsonSerializer.SetupAllProperties();
            //this.mockJsonSerializer.Setup(x => x.Serialize(It.IsAny<JsonWriter>(), It.IsAny<object>()))
            //    .Callback<JsonWriter, object>((writer, obj) =>
            //{
            //    Console.WriteLine($"JsonSerializer.Serialize called with {writer} and {obj}");
            //});
        }

        #region Helper Methods and Classes

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

        

        #endregion Helper Methods and Classes

        [TestMethod]
        public void SimpleConversionTest() 
        { 
            // Arrange
            var settings = this.CreateJsonSerializerSettings();
            var appGlobals = this.mockApplicationGlobals.Object;
            var expected = new SampleClass() { Name = "Test", Age = 47, AppGlobals = appGlobals };
            
            // Act
            var json = JsonConvert.SerializeObject(expected, settings);
            Console.WriteLine("JSON Object");
            Console.WriteLine(json);
            var actual = JsonConvert.DeserializeObject<SampleClass>(json, settings);
            
            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
        
        [TestMethod]
        public void ReadJson_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var appGlobalsConverter = this.CreateAppGlobalsConverter();
            JsonReader reader = this.mockJsonReader.Object;
            Type objectType = this.mockType.Object;
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
            result.Should().BeEquivalentTo(this.mockApplicationGlobals.Object);
        }

        
    }
}
