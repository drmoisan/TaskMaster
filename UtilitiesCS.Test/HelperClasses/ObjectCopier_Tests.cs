using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ObjectCopier_Tests
    {
        [TestMethod]
        public void Clone_WhenSourceIsNull_ReturnsNull()
        {
            // Arrange
            SerializablePerson source = null;

            // Act
            var clone = ObjectCopier.Clone(source);

            // Assert
            clone.Should().BeNull();
        }

        [TestMethod]
        public void Clone_WhenTypeIsNotSerializable_ThrowsArgumentException()
        {
            // Arrange
            var source = new NonSerializablePerson { Name = "Ada" };

            // Act
            Action act = () => ObjectCopier.Clone(source);

            // Assert
            act.Should()
                .Throw<ArgumentException>()
                .WithMessage("*serializable*")
                .And.ParamName.Should()
                .Be("source");
        }

        [TestMethod]
        public void Clone_WhenSourceHasNestedObjects_CreatesDeepCopy()
        {
            // Arrange
            var source = new SerializablePerson
            {
                Name = "Ada",
                Address = new SerializableAddress { City = "Montreal" },
            };

            // Act
            var clone = ObjectCopier.Clone(source);
            clone.Address.City = "Quebec City";

            // Assert
            clone.Should().NotBeSameAs(source);
            clone.Name.Should().Be("Ada");
            clone.Address.Should().NotBeSameAs(source.Address);
            source.Address.City.Should().Be("Montreal");
            clone.Address.City.Should().Be("Quebec City");
        }

        [TestMethod]
        public void Clone_WhenSourceContainsCollection_CopiesCollectionIndependently()
        {
            // Arrange
            var source = new SerializablePerson
            {
                Name = "Ada",
                Aliases = new List<string> { "A", "B" },
            };

            // Act
            var clone = ObjectCopier.Clone(source);
            clone.Aliases.Add("C");

            // Assert
            clone.Aliases.Should().Equal("A", "B", "C");
            source.Aliases.Should().Equal("A", "B");
        }

        [TestMethod]
        public void Clone_WhenSourceContainsCircularReference_PreservesCycleInClone()
        {
            // Arrange
            var source = new CircularNode { Name = "root" };
            source.Next = source;

            // Act
            var clone = ObjectCopier.Clone(source);

            // Assert
            clone.Should().NotBeSameAs(source);
            clone.Next.Should().BeSameAs(clone);
            clone.Name.Should().Be("root");
        }

        [Serializable]
        private sealed class SerializablePerson
        {
            public string Name { get; set; }

            public SerializableAddress Address { get; set; }

            public List<string> Aliases { get; set; } = new List<string>();
        }

        [Serializable]
        private sealed class SerializableAddress
        {
            public string City { get; set; }
        }

        [Serializable]
        private sealed class CircularNode
        {
            public string Name { get; set; }

            public CircularNode Next { get; set; }
        }

        private sealed class NonSerializablePerson
        {
            public string Name { get; set; }
        }
    }
}
