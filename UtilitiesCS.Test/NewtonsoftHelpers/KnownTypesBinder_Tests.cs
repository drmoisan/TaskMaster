using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.NewtonsoftHelpers;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class KnownTypesBinder_Tests
    {
        [TestMethod]
        public void BindToType_ShouldReturnRegisteredType_WhenTypeNameMatches()
        {
            // Arrange
            var binder = new KnownTypesBinder
            {
                KnownTypes = new[] { typeof(BinderDog), typeof(BinderCat) },
            };

            // Act
            Type result = binder.BindToType("ignored", nameof(BinderCat));

            // Assert
            result.Should().Be(typeof(BinderCat));
        }

        [TestMethod]
        public void BindToType_ShouldReturnNull_ForUnknownOrNullTypeName()
        {
            // Arrange
            var binder = new KnownTypesBinder { KnownTypes = new[] { typeof(BinderDog) } };

            // Act
            Type unknownType = binder.BindToType("ignored", "MissingType");
            Type nullType = binder.BindToType("ignored", null);

            // Assert
            unknownType.Should().BeNull();
            nullType.Should().BeNull();
        }

        [TestMethod]
        public void BindToName_ShouldEmitSimpleTypeName_AndNullAssemblyName()
        {
            // Arrange
            var binder = new KnownTypesBinder();

            // Act
            binder.BindToName(typeof(BinderDog), out string assemblyName, out string typeName);

            // Assert
            assemblyName.Should().BeNull();
            typeName.Should().Be(nameof(BinderDog));
        }

        [TestMethod]
        public void JsonSerializer_ShouldRoundTripRegisteredType()
        {
            // Arrange
            var binder = new KnownTypesBinder { KnownTypes = new[] { typeof(BinderDog) } };
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = binder,
            };
            object original = new BinderDog { Name = "Rex" };

            // Act
            string json = JsonConvert.SerializeObject(original, settings);
            object roundTrip = JsonConvert.DeserializeObject<object>(json, settings);

            // Assert
            json.Should().Contain(nameof(BinderDog));
            roundTrip.Should().BeOfType<BinderDog>();
            ((BinderDog)roundTrip).Name.Should().Be("Rex");
        }

        private sealed class BinderDog
        {
            public string Name { get; set; }
        }

        private sealed class BinderCat
        {
            public string Name { get; set; }
        }
    }
}
