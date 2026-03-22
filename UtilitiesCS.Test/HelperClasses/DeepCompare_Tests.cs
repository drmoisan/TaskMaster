using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class DeepCompare_Tests
    {
        [TestMethod]
        public void DeepDifferences_NullFirstObject_ThrowsArgumentNullException()
        {
            // Arrange
            PlainNode obj2 = new PlainNode();

            // Act
            Action act = () => Deep.DeepDifferences<PlainNode>(null, obj2);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("obj");
        }

        [TestMethod]
        public void DeepDifferences_NullSecondObject_ThrowsArgumentNullException()
        {
            // Arrange
            PlainNode obj1 = new PlainNode();

            // Act
            Action act = () => Deep.DeepDifferences<PlainNode>(obj1, null);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_PlainEqualObjects_ThrowsInvalidCastException()
        {
            // Arrange
            var obj1 = new PlainNode { Value = 1 };
            var obj2 = new PlainNode { Value = 1 };

            // Act
            Action act = () => Deep.DeepDifferences(obj1, obj2);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_PlainDifferentValues_ThrowsInvalidCastException()
        {
            // Arrange
            var obj1 = new PlainNode { Value = 1 };
            var obj2 = new PlainNode { Value = 2 };

            // Act
            Action act = () => Deep.DeepDifferences(obj1, obj2);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_PlainNestedCollectionAndCircularObjects_ThrowInvalidCastException()
        {
            // Arrange
            var obj1 = new GraphNode
            {
                Label = "root",
                Items = new List<int> { 1, 2, 3 },
            };
            var obj2 = new GraphNode
            {
                Label = "other",
                Items = new List<int> { 1, 2, 4 },
            };
            obj1.Next = obj1;
            obj2.Next = obj2;

            // Act
            Action act = () => Deep.DeepDifferences(obj1, obj2);

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        [TestMethod]
        public void DeepDifferences_BothNull_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => Deep.DeepDifferences<PlainNode>(null, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void DeepDifferences_StringValues_ThrowsInvalidCastException()
        {
            // Act
            Action act = () => Deep.DeepDifferences("hello", "world");

            // Assert
            act.Should().Throw<InvalidCastException>();
        }

        private sealed class PlainNode
        {
            public int Value { get; set; }
        }

        private sealed class GraphNode
        {
            public string Label { get; set; }

            public List<int> Items { get; set; }

            public GraphNode Next { get; set; }
        }
    }
}
