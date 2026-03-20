using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class AbstractCloneable_Tests
    {
        [TestMethod]
        public void Clone_WithNestedObject_ReturnsDistinctDeepCopy()
        {
            // Arrange
            var original = new CloneableNode
            {
                Name = "root",
                Child = new CloneableNode { Name = "child" },
                Tags = new List<string> { "one", "two" },
            };

            // Act
            var clone = original.CloneTyped();

            // Assert
            clone.Should().NotBeSameAs(original);
            clone.Name.Should().Be("root");
            clone.Child.Should().NotBeNull();
            clone.Child.Should().NotBeSameAs(original.Child);
            clone.Child.Name.Should().Be("child");
            clone.Tags.Should().NotBeSameAs(original.Tags);
            clone.Tags.Should().Equal("one", "two");
        }

        [TestMethod]
        public void Clone_WithNullReferenceMembers_PreservesNulls()
        {
            // Arrange
            var original = new CloneableNode
            {
                Name = "root",
                Child = null,
                Tags = null,
            };

            // Act
            var clone = original.CloneTyped();

            // Assert
            clone.Should().NotBeSameAs(original);
            clone.Name.Should().Be("root");
            clone.Child.Should().BeNull();
            clone.Tags.Should().BeNull();
        }

        [TestMethod]
        public void Clone_WithCollection_ModifyingCloneDoesNotAffectOriginal()
        {
            // Arrange
            var original = new CloneableNode
            {
                Name = "root",
                Tags = new List<string> { "one", "two" },
            };

            // Act
            var clone = original.CloneTyped();
            clone.Tags.Add("three");

            // Assert
            original.Tags.Should().Equal("one", "two");
            clone.Tags.Should().Equal("one", "two", "three");
        }

        [TestMethod]
        public void Clone_ViaICloneableInterface_ReturnsDistinctCopy()
        {
            // Arrange
            var original = new CloneableNode { Name = "test" };
            ICloneable cloneable = original;

            // Act
            var clone = cloneable.Clone() as CloneableNode;

            // Assert
            clone.Should().NotBeNull();
            clone.Should().NotBeSameAs(original);
            clone.Name.Should().Be("test");
        }

        [TestMethod]
        public void Clone_WithPrimitiveFields_CopiesPrimitiveValues()
        {
            // Arrange
            var original = new CloneableNode
            {
                Name = "primitive-test",
                Child = null,
                Tags = new List<string> { "tag1" },
            };

            // Act
            var clone1 = original.CloneTyped();
            var clone2 = original.CloneTyped();

            // Assert
            clone1.Name.Should().Be("primitive-test");
            clone2.Name.Should().Be("primitive-test");
            clone1.Should().NotBeSameAs(clone2);
        }

        private sealed class CloneableNode : AbstractCloneable
        {
            public string Name { get; set; }

            public CloneableNode Child { get; set; }

            public List<string> Tags { get; set; }

            public CloneableNode CloneTyped() => (CloneableNode)Clone();

            protected override void HandleCloned(AbstractCloneable clone)
            {
                base.HandleCloned(clone);

                var typedClone = (CloneableNode)clone;
                typedClone.Child = Child is null ? null : Child.CloneTyped();
                typedClone.Tags = Tags is null ? null : new List<string>(Tags);
            }
        }
    }
}
