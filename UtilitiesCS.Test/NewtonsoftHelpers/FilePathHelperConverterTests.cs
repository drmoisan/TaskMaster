using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class FilePathHelperConverterTests
    {
        private MockRepository mockRepository;
        private Mock<IFileSystemFolderPaths> mockFileSystemFolderPaths;
        private Mock<JsonReader> mockJsonReader;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockFileSystemFolderPaths = this.mockRepository.Create<IFileSystemFolderPaths>();
            this.mockJsonReader = this.mockRepository.Create<JsonReader>();
        }

        private FilePathHelperConverter CreateFilePathHelperConverter()
        {
            return new FilePathHelperConverter(this.mockFileSystemFolderPaths.Object);
        }

        [TestMethod]
        public void ReadPropertyName_ValidInput_Success()
        {
            // Arrange
            var expected = "property";
            var filePathHelperConverter = this.CreateFilePathHelperConverter();
            //mockJsonReader.Setup(x => x.Read()).Returns(true);
            mockJsonReader.Setup(x => x.TokenType).Returns(JsonToken.PropertyName);
            mockJsonReader.Setup(x => x.Value).Returns(expected);

            // Act
            var actual = filePathHelperConverter.ReadPropertyName(mockJsonReader.Object);

            // Assert
            actual.Should().BeEquivalentTo(expected);
            this.mockJsonReader.Verify(x => x.TokenType, Times.Once());
            this.mockJsonReader.Verify(x => x.Value, Times.Once());
        }

        [TestMethod]
        public void ReadPropertyName_NullValue_Failure()
        {
            // Arrange
            string expected = null;
            var filePathHelperConverter = this.CreateFilePathHelperConverter();
            mockJsonReader.Setup(x => x.Read()).Returns(true);
            mockJsonReader.Setup(x => x.TokenType).Returns(JsonToken.PropertyName);
            mockJsonReader.Setup(x => x.Value).Returns(expected);

            // Act
            Action act = () => filePathHelperConverter.ReadPropertyName(mockJsonReader.Object);

            // Assert
            act.Should().Throw<JsonReaderException>();
            this.mockJsonReader.Verify(x => x.TokenType, Times.AtLeastOnce());
            this.mockJsonReader.Verify(x => x.Value, Times.Once());
        }

        [TestMethod]
        public void ReadPropertyName_WrongType_Failure()
        {
            // Arrange
            string expected = "property";
            var filePathHelperConverter = this.CreateFilePathHelperConverter();
            mockJsonReader.Setup(x => x.Read()).Returns(true);
            mockJsonReader.Setup(x => x.TokenType).Returns(JsonToken.Boolean);
            mockJsonReader.Setup(x => x.Value).Returns(expected);

            // Act
            Action act = () => filePathHelperConverter.ReadPropertyName(mockJsonReader.Object);

            // Assert
            act.Should().Throw<JsonReaderException>();
            this.mockJsonReader.Verify(x => x.TokenType, Times.AtLeastOnce());
        }

        [TestMethod]
        public void ReadPropertyValue_ValidInput_Success()
        {
            // Arrange
            var expected = "value";
            var filePathHelperConverter = this.CreateFilePathHelperConverter();
            //mockJsonReader.Setup(x => x.Read()).Returns(true);
            mockJsonReader.Setup(x => x.TokenType).Returns(JsonToken.String);
            mockJsonReader.Setup(x => x.Value).Returns(expected);

            // Act
            var actual = filePathHelperConverter.ReadPropertyValue(mockJsonReader.Object);

            // Assert
            actual.Should().BeEquivalentTo(expected);
            this.mockJsonReader.Verify(x => x.TokenType, Times.Once());
            this.mockJsonReader.Verify(x => x.Value, Times.Once());
        }

        [TestMethod]
        public void ReadPropertyValue_WrongType_Failure()
        {
            // Arrange
            string expected = "property";
            var filePathHelperConverter = this.CreateFilePathHelperConverter();
            mockJsonReader.Setup(x => x.Read()).Returns(true);
            mockJsonReader.Setup(x => x.TokenType).Returns(JsonToken.Boolean);
            mockJsonReader.Setup(x => x.Value).Returns(expected);

            // Act
            Action act = () => filePathHelperConverter.ReadPropertyValue(mockJsonReader.Object);

            // Assert
            act.Should().Throw<JsonReaderException>();
            this.mockJsonReader.Verify(x => x.TokenType, Times.AtLeastOnce());
        }

        //[TestMethod]
        //public void ReadJson_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var filePathHelperConverter = this.CreateFilePathHelperConverter();
        //    JsonReader reader = null;
        //    Type objectType = null;
        //    FilePathHelper existingValue = null;
        //    bool hasExistingValue = false;
        //    JsonSerializer serializer = null;

        //    // Act
        //    var result = filePathHelperConverter.ReadJson(
        //        reader,
        //        objectType,
        //        existingValue,
        //        hasExistingValue,
        //        serializer);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void WriteJson_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var filePathHelperConverter = this.CreateFilePathHelperConverter();
        //    JsonWriter writer = null;
        //    FilePathHelper value = null;
        //    JsonSerializer serializer = null;

        //    // Act
        //    filePathHelperConverter.WriteJson(
        //        writer,
        //        value,
        //        serializer);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        [TestMethod]
        public void ExtractFolderPath_WithKnownSpecialFolder_CombinesRelativePath()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var specialFolders = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string> { { "AppData", @"C:\Users\Test\AppData" } }
            );
            this.mockFileSystemFolderPaths.Setup(x => x.SpecialFolders).Returns(specialFolders);
            var info = new Dictionary<string, string>
            {
                { "SpecialFolderName", "AppData" },
                { "RelativePath", "SubDir" },
                { "FileName", "test.json" },
            };

            // Act
            var result = converter.ExtractFolderPath(info);

            // Assert
            result.Should().Be(System.IO.Path.Combine(@"C:\Users\Test\AppData", "SubDir"));
        }

        [TestMethod]
        public void ExtractFolderPath_WithUnknownSpecialFolder_ReturnsNull()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var specialFolders = new ConcurrentDictionary<string, string>();
            this.mockFileSystemFolderPaths.Setup(x => x.SpecialFolders).Returns(specialFolders);
            var info = new Dictionary<string, string>
            {
                { "SpecialFolderName", "Unknown" },
                { "FileName", "test.json" },
            };

            // Act
            var result = converter.ExtractFolderPath(info);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void ExtractFolderPath_WithMissingSpecialFolderName_ReturnsNull()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var info = new Dictionary<string, string> { { "FileName", "test.json" } };

            // Act
            var result = converter.ExtractFolderPath(info);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void ExtractFileName_WhenFileNamePresent_ReturnsValue()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var info = new Dictionary<string, string> { { "FileName", "data.json" } };

            // Act
            var result = converter.ExtractFileName(info);

            // Assert
            result.Should().Be("data.json");
        }

        [TestMethod]
        public void ExtractFileName_WhenFileNameMissing_ThrowsJsonReaderException()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var info = new Dictionary<string, string>();

            // Act
            System.Action act = () => converter.ExtractFileName(info);

            // Assert
            act.Should().Throw<JsonReaderException>().WithMessage("*FileName*");
        }

        [TestMethod]
        public void GetSerializablePath_WithMatchingSpecialFolder_ReturnsNameAndRelativePath()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var specialFolders = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string> { { "AppData", @"C:\Users\Test\AppData" } }
            );
            this.mockFileSystemFolderPaths.Setup(x => x.SpecialFolders).Returns(specialFolders);

            // Act
            var (name, relativePath) = converter.GetSerializablePath(
                @"C:\Users\Test\AppData\SubDir\file.json"
            );

            // Assert
            name.Should().Be("AppData");
            relativePath.Should().Be(@"\SubDir\file.json");
        }

        [TestMethod]
        public void GetSerializablePath_WithNoMatch_ReturnsNotFoundAndFullPath()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var specialFolders = new ConcurrentDictionary<string, string>();
            this.mockFileSystemFolderPaths.Setup(x => x.SpecialFolders).Returns(specialFolders);

            // Act
            var (name, relativePath) = converter.GetSerializablePath(@"D:\data\file.json");

            // Assert
            name.Should().Be("Not Found");
            relativePath.Should().Be(@"D:\data\file.json");
        }

        [TestMethod]
        public void ExtractFolderPath_StringOverload_WithNoneSpecialFolder_ReturnsRelativePath()
        {
            // Arrange
            var converter = this.CreateFilePathHelperConverter();
            var specialFolders = new ConcurrentDictionary<string, string>();
            this.mockFileSystemFolderPaths.Setup(x => x.SpecialFolders).Returns(specialFolders);

            // Act
            var result = converter.ExtractFolderPath("None", @"C:\some\path");

            // Assert
            result.Should().Be(@"C:\some\path");
        }
    }
}
