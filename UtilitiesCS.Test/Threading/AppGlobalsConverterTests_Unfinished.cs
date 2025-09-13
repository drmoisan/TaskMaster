using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using UtilitiesCS;

namespace Z.Disabled.UtilitiesCS.Test.Threading
{
    [TestClass]
    public class AppGlobalsConverterTests_Unfinished
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockApplicationGlobals;
        private Mock<IFileSystemFolderPaths> mockFileSystemsFolderPaths;

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
        public void WriteJson_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var appGlobalsConverter = this.CreateAppGlobalsConverter();
            //JsonWriter writer = this.mockJsonWriter.Object;
            //JsonWriter writer = null;
            IApplicationGlobals value = this.mockApplicationGlobals.Object;
            //var serializer = this.mockJsonSerializer.Object;
            var serializer = new JsonSerializer();

            // Act
            //appGlobalsConverter.WriteJson(
            //    writer,
            //    value,
            //    serializer);

            // Assert
            //this.mockJsonSerializer.Verify(x => x.Serialize(It.IsAny<JsonWriter>(), It.IsAny<object>()), Times.Once);
            //this.mockJsonSerializer.Verify(x => x.Serialize(It.Is<JsonWriter>(x => x == writer), It.Is<string>(y => y == "default")));
            //this.mockJsonSerializer.Verify(x => x.Serialize(writer, "default"), Times.Once);

        }
    }
}
