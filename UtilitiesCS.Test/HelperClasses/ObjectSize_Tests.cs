using System.Collections;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ObjectSize_Tests
    {
        [TestMethod]
        public void GetObjectSize_ReturnsZeroForNullAndObjectsWithoutFields()
        {
            // Arrange
            object nullReference = null;
            var emptyObject = new EmptyObject();

            // Act
            var nullSize = ObjectSize.GetObjectSize(nullReference);
            var emptyObjectSize = ObjectSize.GetObjectSize(emptyObject);

            // Assert
            nullSize.Should().Be(0);
            emptyObjectSize.Should().Be(0);
        }

        [TestMethod]
        public void GetObjectSize_ReturnsMarshalSizeForPrimitiveValueTypes()
        {
            // Arrange
            var integerValue = 42;
            var doubleValue = 123.5d;

            // Act
            var integerSize = ObjectSize.GetObjectSize(integerValue);
            var doubleSize = ObjectSize.GetObjectSize(doubleValue);

            // Assert
            integerSize.Should().Be(Marshal.SizeOf(integerValue));
            doubleSize.Should().Be(Marshal.SizeOf(doubleValue));
        }

        [TestMethod]
        public void GetObjectSize_ReturnsCharacterCountTimesTwoForStrings()
        {
            // Arrange
            const string value = "héllo";

            // Act
            var size = ObjectSize.GetObjectSize(value);

            // Assert
            size.Should().Be(value.Length * sizeof(char));
        }

        [TestMethod]
        public void GetObjectSize_SumsCollectionItemsAndAvoidsDoubleCountingSharedReferences()
        {
            // Arrange
            var shared = new ObjectWithFields { Number = 7, Text = "same" };
            ICollection collection = new ArrayList
            {
                shared,
                shared,
                5,
                "xy"
            };

            var expected = Marshal.SizeOf(5)
                + ("xy".Length * sizeof(char))
                + Marshal.SizeOf(shared.Number)
                + (shared.Text.Length * sizeof(char));

            // Act
            var size = ObjectSize.GetObjectSize(collection);

            // Assert
            size.Should().Be(expected);
        }

        [TestMethod]
        public void GetObjectSize_TraversesComplexReferenceGraphs()
        {
            // Arrange
            var nested = new ObjectWithFields { Number = 9, Text = "nest" };
            var root = new ContainerObject
            {
                Count = 3,
                Label = "root",
                Child = nested
            };

            var expected = Marshal.SizeOf(root.Count)
                + (root.Label.Length * sizeof(char))
                + Marshal.SizeOf(nested.Number)
                + (nested.Text.Length * sizeof(char));

            // Act
            var size = ObjectSize.GetObjectSize(root);

            // Assert
            size.Should().Be(expected);
        }

        private sealed class EmptyObject
        {
        }

        private sealed class ContainerObject
        {
            public int Count;
            public string Label;
            public ObjectWithFields Child;
        }

        private sealed class ObjectWithFields
        {
            public int Number;
            public string Text;
        }
    }
}