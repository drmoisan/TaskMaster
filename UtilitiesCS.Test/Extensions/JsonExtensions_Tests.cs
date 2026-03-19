using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class JsonExtensions_Tests
    {
        [TestMethod]
        public void Deserialize_DeserializesValidJsonIntoReferenceType()
        {
            // Arrange
            var payload = Encoding.UTF8.GetBytes(
                "{\"Name\":\"alpha\",\"Child\":{\"Name\":\"beta\"}}"
            );

            // Act
            var result = JsonExtensions.Deserialize<SampleNode>(payload);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("alpha");
            result.Child.Should().NotBeNull();
            result.Child.Name.Should().Be("beta");
        }

        [TestMethod]
        public void Deserialize_ReturnsDefaultValuesForEmptyObjectAndThrowsForNullOrMalformedJson()
        {
            // Arrange
            var emptyObject = Encoding.UTF8.GetBytes("{}");
            byte[] nullPayload = null;
            var malformedPayload = Encoding.UTF8.GetBytes("{ invalid json }");

            // Act
            var emptyResult = JsonExtensions.Deserialize<SampleNode>(emptyObject);
            Action nullAction = () => JsonExtensions.Deserialize<SampleNode>(nullPayload);
            Action malformedAction = () => JsonExtensions.Deserialize<SampleNode>(malformedPayload);

            // Assert
            emptyResult.Should().NotBeNull();
            emptyResult.Name.Should().BeNull();
            emptyResult.Child.Should().BeNull();
            nullAction.Should().Throw<ArgumentNullException>();
            malformedAction.Should().Throw<JsonReaderException>();
        }

        [TestMethod]
        public void Deserialize_WhenTargetTypeDoesNotMatchJsonToken_ThrowsJsonSerializationException()
        {
            // Arrange
            var payload = Encoding.UTF8.GetBytes("[1,2,3]");

            // Act
            Action action = () => JsonExtensions.Deserialize<SampleNode>(payload);

            // Assert
            action.Should().Throw<JsonSerializationException>();
        }

        [TestMethod]
        public void ToJsonText_ReturnsCanonicalJsonForNestedStructures()
        {
            // Arrange
            using var textReader = new StringReader(
                "{\"items\":[1,{\"name\":\"value\"}],\"flag\":true}"
            );
            using var reader = new JsonTextReader(textReader);
            reader.Read();

            // Act
            var result = reader.ToJsonText();

            // Assert
            result.Should().Be("{\"items\":[1,{\"name\":\"value\"}],\"flag\":true}");
        }

        [TestMethod]
        public void ToJsonText_WhenReaderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            JsonReader reader = null;

            // Act
            Action action = () => reader.ToJsonText();

            // Assert
            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("reader");
        }

        private sealed class SampleNode
        {
            public string Name { get; set; }

            public SampleNode Child { get; set; }
        }
    }
}
