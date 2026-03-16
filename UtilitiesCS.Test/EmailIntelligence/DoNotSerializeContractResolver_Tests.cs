using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class DoNotSerializeContractResolver_Tests
    {
        [TestMethod]
        public void CreateProperties_ReturnsAllPropertiesWhenNoNamesAreExcluded()
        {
            // Arrange
            var resolver = new TestableResolver();

            // Act
            var properties = resolver.CreatePropertiesFor(typeof(SampleSettings), MemberSerialization.OptOut);

            // Assert
            properties.Select(property => property.PropertyName)
                .Should().Contain(new[] { nameof(SampleSettings.Visible), nameof(SampleSettings.Hidden) });
        }

        [TestMethod]
        public void CreateProperties_ExcludesConfiguredPropertyNames()
        {
            // Arrange
            var resolver = new TestableResolver(nameof(SampleSettings.Hidden));

            // Act
            var properties = resolver.CreatePropertiesFor(typeof(SampleSettings), MemberSerialization.OptOut);

            // Assert
            properties.Select(property => property.PropertyName).Should().ContainSingle().Which.Should().Be(nameof(SampleSettings.Visible));
        }

        [TestMethod]
        public void CreateProperties_WithNullType_ThrowsArgumentNullException()
        {
            // Arrange
            var resolver = new TestableResolver();

            // Act
            Action act = () => resolver.CreatePropertiesFor(null, MemberSerialization.OptOut);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void JsonSerialization_UsesResolverToExcludeConfiguredProperties()
        {
            // Arrange
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DoNotSerializeContractResolver(nameof(SampleSettings.Hidden)),
                Formatting = Formatting.None
            };
            var value = new SampleSettings { Visible = "shown", Hidden = "secret" };

            // Act
            var json = JsonConvert.SerializeObject(value, settings);
            var roundTrip = JsonConvert.DeserializeObject<SampleSettings>(json);

            // Assert
            json.Should().Contain("Visible");
            json.Should().NotContain("Hidden");
            roundTrip.Should().NotBeNull();
            roundTrip!.Visible.Should().Be("shown");
            roundTrip.Hidden.Should().BeNull();
        }

        private sealed class TestableResolver : DoNotSerializeContractResolver
        {
            public TestableResolver(params string[] propertyNames)
                : base(propertyNames)
            {
            }

            public System.Collections.Generic.IList<JsonProperty> CreatePropertiesFor(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization);
            }
        }

        private sealed class SampleSettings
        {
            public string Visible { get; set; }

            public string Hidden { get; set; }
        }
    }
}