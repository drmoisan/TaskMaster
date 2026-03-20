using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.NewtonsoftHelpers;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class ScDictionaryConverter_Tests
    {
        private MockRepository mockRepository;
        private Mock<Microsoft.Office.Interop.Outlook.Application> mockApplication;
        private IApplicationGlobals globals;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            mockRepository = new MockRepository(MockBehavior.Loose);
            mockApplication = mockRepository.Create<Microsoft.Office.Interop.Outlook.Application>();
            globals = new TaskMaster.ApplicationGlobals(mockApplication.Object, true);
        }

        [TestMethod]
        public void Constructor_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var converter = new ScDictionaryConverter<ScDictionary<string, int>, string, int>();

            // Assert
            converter.Should().NotBeNull();
        }

        [TestMethod]
        public void WriteJson_DictionaryWithEntries_ProducesJson()
        {
            // Arrange
            var converter = new ScDictionaryConverter<ScDictionary<string, int>, string, int>();
            var dict = new ScDictionary<string, int>();
            dict.TryAdd("key1", 1);
            dict.TryAdd("key2", 2);

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { converter },
            };

            // Act
            var json = JsonConvert.SerializeObject(dict, settings);

            // Assert
            json.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void WriteJson_EmptyDictionary_ProducesJson()
        {
            // Arrange
            var converter = new ScDictionaryConverter<ScDictionary<string, int>, string, int>();
            var dict = new ScDictionary<string, int>();

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { converter },
            };

            // Act
            var json = JsonConvert.SerializeObject(dict, settings);

            // Assert
            json.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void ReadJson_NullWrapper_ReturnsNull()
        {
            // Arrange
            var converter = new ScDictionaryConverter<ScDictionary<string, int>, string, int>();
            var settings = new JsonSerializerSettings { Converters = { converter } };

            // Act
            var result = JsonConvert.DeserializeObject<ScDictionary<string, int>>("null", settings);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void RoundTrip_DictionaryWithEntries_PreservesData()
        {
            // Arrange
            var converter = new ScDictionaryConverter<ScDictionary<string, int>, string, int>();
            var original = new ScDictionary<string, int>();
            original.TryAdd("alpha", 10);
            original.TryAdd("beta", 20);

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { converter },
            };

            // Act
            var json = JsonConvert.SerializeObject(original, settings);
            var restored = JsonConvert.DeserializeObject<ScDictionary<string, int>>(json, settings);

            // Assert
            restored.Should().NotBeNull();
            restored.Should().ContainKey("alpha");
            restored.Should().ContainKey("beta");
            restored["alpha"].Should().Be(10);
            restored["beta"].Should().Be(20);
        }
    }
}
