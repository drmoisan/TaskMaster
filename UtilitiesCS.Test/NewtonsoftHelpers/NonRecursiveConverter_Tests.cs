using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class NonRecursiveConverter_Tests
    {
        /// <summary>
        /// Concrete test implementation of the abstract NonRecursiveConverter.
        /// </summary>
        private class TestConverter : NonRecursiveConverter<TestConverter>
        {
            public Func<JsonReader, Type, object, JsonSerializer, object> OnReadAction { get; set; }
            public Action<JsonWriter, object, JsonSerializer> OnWriteAction { get; set; }

            public override bool CanConvert(Type objectType) => objectType == typeof(string);

            protected override object OnReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer
            )
            {
                return OnReadAction?.Invoke(reader, objectType, existingValue, serializer)
                    ?? serializer.Deserialize(reader, objectType);
            }

            protected override void OnWriteJson(
                JsonWriter writer,
                object value,
                JsonSerializer serializer
            )
            {
                if (OnWriteAction != null)
                    OnWriteAction(writer, value, serializer);
                else
                    serializer.Serialize(writer, value);
            }
        }

        [TestMethod]
        public void CanRead_WhenNotReading_ReturnsTrue()
        {
            // Arrange
            var converter = new TestConverter();

            // Act
            var result = converter.CanRead;

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void CanWrite_WhenNotWriting_ReturnsTrue()
        {
            // Arrange
            var converter = new TestConverter();

            // Act
            var result = converter.CanWrite;

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void CanConvert_StringType_ReturnsTrue()
        {
            // Arrange
            var converter = new TestConverter();

            // Act
            var result = converter.CanConvert(typeof(string));

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void CanConvert_NonStringType_ReturnsFalse()
        {
            // Arrange
            var converter = new TestConverter();

            // Act
            var result = converter.CanConvert(typeof(int));

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void ReadJson_ValidInput_DelegatesToOnReadJson()
        {
            // Arrange
            var converter = new TestConverter
            {
                OnReadAction = (reader, type, existing, serializer) =>
                {
                    return serializer.Deserialize(reader, type);
                },
            };
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);
            var json = "\"hello world\"";

            // Act
            var result = JsonConvert.DeserializeObject<string>(json, settings);

            // Assert
            result.Should().Be("hello world");
        }

        [TestMethod]
        public void WriteJson_ValidInput_DelegatesToOnWriteJson()
        {
            // Arrange
            var converter = new TestConverter
            {
                OnWriteAction = (writer, value, serializer) =>
                {
                    serializer.Serialize(writer, value);
                },
            };
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act
            var json = JsonConvert.SerializeObject("test value", settings);

            // Assert
            json.Should().Be("\"test value\"");
        }

        [TestMethod]
        public void ReadJson_ResetsGuardAfterRead_CanReadAgain()
        {
            // Arrange
            var converter = new TestConverter();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act - first read
            var result1 = JsonConvert.DeserializeObject<string>("\"first\"", settings);
            // After the read, CanRead should be true again
            converter.CanRead.Should().BeTrue();

            // Act - second read
            var result2 = JsonConvert.DeserializeObject<string>("\"second\"", settings);

            // Assert
            result1.Should().Be("first");
            result2.Should().Be("second");
        }

        [TestMethod]
        public void WriteJson_ResetsGuardAfterWrite_CanWriteAgain()
        {
            // Arrange
            var converter = new TestConverter();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act - first write
            var json1 = JsonConvert.SerializeObject("first", settings);
            converter.CanWrite.Should().BeTrue();

            // Act - second write
            var json2 = JsonConvert.SerializeObject("second", settings);

            // Assert
            json1.Should().Be("\"first\"");
            json2.Should().Be("\"second\"");
        }

        [TestMethod]
        public void ReadJson_WhenOnReadJsonThrows_GuardIsReset()
        {
            // Arrange
            var converter = new TestConverter
            {
                OnReadAction = (reader, type, existing, serializer) =>
                {
                    throw new InvalidOperationException("Test error");
                },
            };
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act & Assert
            Action act = () => JsonConvert.DeserializeObject<string>("\"test\"", settings);
            act.Should().Throw<JsonSerializationException>();

            // Guard should be reset after exception
            converter.CanRead.Should().BeTrue();
        }

        [TestMethod]
        public void WriteJson_WhenOnWriteJsonThrows_GuardIsReset()
        {
            // Arrange
            var converter = new TestConverter
            {
                OnWriteAction = (writer, value, serializer) =>
                {
                    throw new InvalidOperationException("Test error");
                },
            };
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act & Assert
            Action act = () => JsonConvert.SerializeObject("test", settings);
            act.Should().Throw<JsonSerializationException>();

            // Guard should be reset after exception
            converter.CanWrite.Should().BeTrue();
        }

        [TestMethod]
        public void ReadJson_NullExistingValue_HandledCorrectly()
        {
            // Arrange
            var converter = new TestConverter();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act
            var result = JsonConvert.DeserializeObject<string>("null", settings);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void WriteJson_NullValue_SerializesAsNull()
        {
            // Arrange
            var converter = new TestConverter();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(converter);

            // Act
            var json = JsonConvert.SerializeObject((string)null, settings);

            // Assert
            json.Should().Be("null");
        }
    }
}
