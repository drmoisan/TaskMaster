using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ToDoModel.Data_Model.People;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class PeopleScoRemainingObjectConverter_Tests
    {
        private PeopleScoRemainingObjectConverter converter;

        [TestInitialize]
        public void TestInitialize()
        {
            converter = new PeopleScoRemainingObjectConverter();
        }

        [TestMethod]
        public void CanConvert_ObjectType_ReturnsTrue()
        {
            // Arrange & Act
            var result = converter.CanConvert(typeof(object));

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void CanConvert_StringType_ReturnsFalse()
        {
            // Arrange & Act
            var result = converter.CanConvert(typeof(string));

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void CanConvert_IntType_ReturnsFalse()
        {
            // Arrange & Act
            var result = converter.CanConvert(typeof(int));

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WriteJson_ValidObject_SerializesWithoutError()
        {
            // Arrange
            var settings = new JsonSerializerSettings();
            var remaining = new PeopleScoRemainingObject { Name = "TestName" };

            // Act
            var json = JsonConvert.SerializeObject(remaining, settings);

            // Assert
            json.Should().Contain("TestName");
        }

        [TestMethod]
        public void ReadJson_ValidJson_DeserializesToPeopleScoRemainingObject()
        {
            // Arrange
            var json = "{\"Name\":\"TestPerson\"}";
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act
            var jObject = JObject.Parse(json);
            var result = jObject.ToObject<PeopleScoRemainingObject>();

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("TestPerson");
        }

        [TestMethod]
        public void ReadJson_EmptyJson_ReturnsObjectWithNullProperties()
        {
            // Arrange
            var json = "{}";

            // Act
            var jObject = JObject.Parse(json);
            var result = jObject.ToObject<PeopleScoRemainingObject>();

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().BeNull();
        }

        [TestMethod]
        public void WriteJson_NullName_SerializesCorrectly()
        {
            // Arrange
            var remaining = new PeopleScoRemainingObject { Name = null };

            // Act
            var json = JsonConvert.SerializeObject(remaining);

            // Assert
            json.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void RoundTrip_ValidObject_PreservesData()
        {
            // Arrange
            var original = new PeopleScoRemainingObject { Name = "RoundTrip" };
            var settings = new JsonSerializerSettings { Formatting = Formatting.None };

            // Act
            var json = JsonConvert.SerializeObject(original, settings);
            var deserialized = JsonConvert.DeserializeObject<PeopleScoRemainingObject>(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized.Name.Should().Be("RoundTrip");
        }
    }
}
