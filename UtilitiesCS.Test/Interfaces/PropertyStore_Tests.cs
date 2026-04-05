using System;
using System.Drawing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Interfaces;

namespace UtilitiesCS.Test.Interfaces
{
    [TestClass]
    public class PropertyStore_Tests
    {
        [TestMethod]
        public void CreateKey_ReturnsSequentialKeys()
        {
            // Act
            var key1 = PropertyStore.CreateKey();
            var key2 = PropertyStore.CreateKey();

            // Assert
            key2.Should().Be(key1 + 1);
        }

        [TestMethod]
        public void SetAndGetInteger_RoundTrips()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act
            store.SetInteger(key, 42);
            var result = store.GetInteger(key);

            // Assert
            result.Should().Be(42);
        }

        [TestMethod]
        public void ContainsInteger_WhenSet_ReturnsTrue()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            store.SetInteger(key, 10);

            // Act / Assert
            store.ContainsInteger(key).Should().BeTrue();
        }

        [TestMethod]
        public void ContainsInteger_WhenNotSet_ReturnsFalse()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act / Assert
            store.ContainsInteger(key).Should().BeFalse();
        }

        [TestMethod]
        public void SetAndGetObject_RoundTrips()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            var obj = "test-value";

            // Act
            store.SetObject(key, obj);
            var result = store.GetObject(key);

            // Assert
            result.Should().Be("test-value");
        }

        [TestMethod]
        public void ContainsObject_WhenSet_ReturnsTrue()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            store.SetObject(key, "value");

            // Act / Assert
            store.ContainsObject(key).Should().BeTrue();
        }

        [TestMethod]
        public void ContainsObject_WhenNotSet_ReturnsFalse()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act / Assert
            store.ContainsObject(key).Should().BeFalse();
        }

        [TestMethod]
        public void GetColor_WhenNotSet_ReturnsEmptyColor()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act
            var result = store.GetColor(key);

            // Assert
            result.Should().Be(Color.Empty);
        }

        [TestMethod]
        public void SetAndGetColor_RoundTrips()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act
            store.SetColor(key, Color.Red);
            var result = store.GetColor(key);

            // Assert
            result.Should().Be(Color.Red);
        }

        [TestMethod]
        public void GetPadding_WhenNotSet_ReturnsEmptyPadding()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act
            var result = store.GetPadding(key);

            // Assert
            result.Should().Be(System.Windows.Forms.Padding.Empty);
        }

        [TestMethod]
        public void SetAndGetPadding_RoundTrips()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            var padding = new System.Windows.Forms.Padding(5, 10, 15, 20);

            // Act
            store.SetPadding(key, padding);
            var result = store.GetPadding(key);

            // Assert
            result.Should().Be(padding);
        }

        [TestMethod]
        public void GetRectangle_WhenNotSet_ReturnsEmptyRectangle()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act
            var result = store.GetRectangle(key);

            // Assert
            result.Should().Be(Rectangle.Empty);
        }

        [TestMethod]
        public void SetAndGetRectangle_RoundTrips()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            var rect = new Rectangle(10, 20, 30, 40);

            // Act
            store.SetRectangle(key, rect);
            var result = store.GetRectangle(key);

            // Assert
            result.Should().Be(rect);
        }

        [TestMethod]
        public void RemoveInteger_WhenKeyExists_DoesNotThrow()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            store.SetInteger(key, 99);

            // Act
            Action act = () => store.RemoveInteger(key);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RemoveObject_WhenKeyExists_RemovesIt()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            store.SetObject(key, "value");

            // Act
            store.RemoveObject(key);

            // Assert
            store.ContainsObject(key).Should().BeFalse();
        }

        [TestMethod]
        public void MultipleKeys_StoreIndependently()
        {
            // Arrange
            var store = new PropertyStore();
            var key1 = PropertyStore.CreateKey();
            var key2 = PropertyStore.CreateKey();

            // Act
            store.SetInteger(key1, 10);
            store.SetInteger(key2, 20);

            // Assert
            store.GetInteger(key1).Should().Be(10);
            store.GetInteger(key2).Should().Be(20);
        }

        [TestMethod]
        public void GetInteger_WithOutBool_IndicatesFoundStatus()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            store.SetInteger(key, 7);

            // Act
            var value = store.GetInteger(key, out bool found);

            // Assert
            found.Should().BeTrue();
            value.Should().Be(7);
        }

        [TestMethod]
        public void GetObject_WithOutBool_WhenNotSet_FoundIsFalse()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act
            var value = store.GetObject(key, out bool found);

            // Assert
            found.Should().BeFalse();
            value.Should().BeNull();
        }

        [TestMethod]
        public void GetSize_WhenNotSet_ReturnsEmptySizeAndFalse()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();

            // Act
            var value = store.GetSize(key, out bool found);

            // Assert
            found.Should().BeFalse();
            value.Should().Be(Size.Empty);
        }

        [TestMethod]
        public void SetAndGetSize_RoundTrips()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            var size = new Size(12, 34);

            // Act
            store.SetSize(key, size);
            var value = store.GetSize(key, out bool found);

            // Assert
            found.Should().BeTrue();
            value.Should().Be(size);
        }

        [TestMethod]
        public void WrapperSetters_WhenExistingObjectValueIsNull_CreateNewWrappers()
        {
            // Arrange
            var store = new PropertyStore();
            var colorKey = PropertyStore.CreateKey();
            var paddingKey = PropertyStore.CreateKey();
            var rectangleKey = PropertyStore.CreateKey();
            var sizeKey = PropertyStore.CreateKey();

            store.SetObject(colorKey, null);
            store.SetObject(paddingKey, null);
            store.SetObject(rectangleKey, null);
            store.SetObject(sizeKey, null);

            // Act
            store.SetColor(colorKey, Color.Blue);
            store.SetPadding(paddingKey, new System.Windows.Forms.Padding(1, 2, 3, 4));
            store.SetRectangle(rectangleKey, new Rectangle(5, 6, 7, 8));
            store.SetSize(sizeKey, new Size(9, 10));

            // Assert
            store.GetColor(colorKey).Should().Be(Color.Blue);
            store.GetPadding(paddingKey).Should().Be(new System.Windows.Forms.Padding(1, 2, 3, 4));
            store.GetRectangle(rectangleKey).Should().Be(new Rectangle(5, 6, 7, 8));
            store.GetSize(sizeKey, out _).Should().Be(new Size(9, 10));
        }

        [TestMethod]
        public void WrapperSetters_WhenWrapperAlreadyExists_UpdateWrappedValues()
        {
            // Arrange
            var store = new PropertyStore();
            var colorKey = PropertyStore.CreateKey();
            var paddingKey = PropertyStore.CreateKey();
            var rectangleKey = PropertyStore.CreateKey();
            var sizeKey = PropertyStore.CreateKey();

            store.SetColor(colorKey, Color.Red);
            store.SetPadding(paddingKey, new System.Windows.Forms.Padding(1));
            store.SetRectangle(rectangleKey, new Rectangle(1, 1, 1, 1));
            store.SetSize(sizeKey, new Size(1, 1));

            // Act
            store.SetColor(colorKey, Color.Green);
            store.SetPadding(paddingKey, new System.Windows.Forms.Padding(2, 3, 4, 5));
            store.SetRectangle(rectangleKey, new Rectangle(2, 3, 4, 5));
            store.SetSize(sizeKey, new Size(6, 7));

            // Assert
            store.GetColor(colorKey).Should().Be(Color.Green);
            store.GetPadding(paddingKey).Should().Be(new System.Windows.Forms.Padding(2, 3, 4, 5));
            store.GetRectangle(rectangleKey).Should().Be(new Rectangle(2, 3, 4, 5));
            store.GetSize(sizeKey, out _).Should().Be(new Size(6, 7));
        }

        [TestMethod]
        public void IntegerGroupOperations_CoverAllElementSlotsAndSelectiveRemoval()
        {
            // Arrange
            var store = new PropertyStore();
            var key0 = PropertyStore.CreateKey();
            var key1 = PropertyStore.CreateKey();
            var key2 = PropertyStore.CreateKey();
            var key3 = PropertyStore.CreateKey();

            // Act
            store.SetInteger(key0, 10);
            store.SetInteger(key1, 20);
            store.SetInteger(key2, 30);
            store.SetInteger(key3, 40);
            store.RemoveInteger(key1);

            // Assert
            store.GetInteger(key0).Should().Be(10);
            store.ContainsInteger(key1).Should().BeFalse();
            store.GetInteger(key2).Should().Be(30);
            store.GetInteger(key3).Should().Be(40);
        }

        [TestMethod]
        public void ObjectGroupOperations_CoverAllElementSlotsAndSelectiveRemoval()
        {
            // Arrange
            var store = new PropertyStore();
            var key0 = PropertyStore.CreateKey();
            var key1 = PropertyStore.CreateKey();
            var key2 = PropertyStore.CreateKey();
            var key3 = PropertyStore.CreateKey();

            // Act
            store.SetObject(key0, "zero");
            store.SetObject(key1, "one");
            store.SetObject(key2, "two");
            store.SetObject(key3, "three");
            store.RemoveObject(key2);

            // Assert
            store.GetObject(key0).Should().Be("zero");
            store.GetObject(key1).Should().Be("one");
            store.ContainsObject(key2).Should().BeFalse();
            store.GetObject(key3).Should().Be("three");
        }

        [TestMethod]
        public void RemoveInteger_WhenSiblingSlotWasNeverSet_LeavesExistingValueUntouched()
        {
            // Arrange
            var store = new PropertyStore();
            var key0 = PropertyStore.CreateKey();
            var key1 = PropertyStore.CreateKey();
            store.SetInteger(key0, 99);

            // Act
            store.RemoveInteger(key1);

            // Assert
            store.GetInteger(key0).Should().Be(99);
            store.ContainsInteger(key1).Should().BeFalse();
        }

        [TestMethod]
        public void RemoveObject_WhenSingleEntryExists_ClearsTheStore()
        {
            // Arrange
            var store = new PropertyStore();
            var key = PropertyStore.CreateKey();
            store.SetObject(key, "value");

            // Act
            store.RemoveObject(key);

            // Assert
            store.ContainsObject(key).Should().BeFalse();
            store.GetObject(key).Should().BeNull();
        }

        [TestMethod]
        public void LargeIntegerCollection_UsesBinarySearchPathForLookupsAndRemovals()
        {
            // Arrange
            var store = new PropertyStore();
            var keys = new int[20];

            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = PropertyStore.CreateKey();
                store.SetInteger(keys[i], i * 10);
            }

            // Act
            var middleValue = store.GetInteger(keys[12], out bool middleFound);
            store.RemoveInteger(keys[18]);

            // Assert
            middleFound.Should().BeTrue();
            middleValue.Should().Be(120);
            store.ContainsInteger(keys[18]).Should().BeFalse();
            store.GetInteger(keys[3]).Should().Be(30);
        }

        [TestMethod]
        public void LargeObjectCollection_UsesBinarySearchPathForLookupsAndRemovals()
        {
            // Arrange
            var store = new PropertyStore();
            var keys = new int[20];

            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = PropertyStore.CreateKey();
                store.SetObject(keys[i], $"value-{i}");
            }

            // Act
            var middleValue = store.GetObject(keys[11], out bool middleFound);
            store.RemoveObject(keys[17]);

            // Assert
            middleFound.Should().BeTrue();
            middleValue.Should().Be("value-11");
            store.ContainsObject(keys[17]).Should().BeFalse();
            store.GetObject(keys[2]).Should().Be("value-2");
        }

        [TestMethod]
        public void SetInteger_WhenInsertingBetweenExistingEntryKeys_PreservesSortedLookupBehavior()
        {
            // Arrange
            var store = new PropertyStore();

            // Act
            store.SetInteger(400, 1);
            store.SetInteger(800, 2);
            store.SetInteger(600, 3);

            // Assert
            store.GetInteger(400).Should().Be(1);
            store.GetInteger(600).Should().Be(3);
            store.GetInteger(800).Should().Be(2);
        }

        [TestMethod]
        public void SetObject_WhenInsertingBetweenExistingEntryKeys_PreservesSortedLookupBehavior()
        {
            // Arrange
            var store = new PropertyStore();

            // Act
            store.SetObject(400, "first");
            store.SetObject(800, "third");
            store.SetObject(600, "second");

            // Assert
            store.GetObject(400).Should().Be("first");
            store.GetObject(600).Should().Be("second");
            store.GetObject(800).Should().Be("third");
        }

        [TestMethod]
        public void LargeIntegerCollection_WhenKeyIsMissing_ReportsFalseAcrossBinarySearchMissPaths()
        {
            // Arrange
            var store = new PropertyStore();

            for (var i = 0; i < 20; i++)
            {
                store.SetInteger(400 + (i * 4), i);
            }

            // Act
            var lowValue = store.GetInteger(396, out bool lowFound);
            var highValue = store.GetInteger(500, out bool highFound);

            // Assert
            lowFound.Should().BeFalse();
            highFound.Should().BeFalse();
            lowValue.Should().Be(0);
            highValue.Should().Be(0);
        }

        [TestMethod]
        public void LargeObjectCollection_WhenKeyIsMissing_ReportsFalseAcrossBinarySearchMissPaths()
        {
            // Arrange
            var store = new PropertyStore();

            for (var i = 0; i < 20; i++)
            {
                store.SetObject(400 + (i * 4), $"value-{i}");
            }

            // Act
            var lowValue = store.GetObject(396, out bool lowFound);
            var highValue = store.GetObject(500, out bool highFound);

            // Assert
            lowFound.Should().BeFalse();
            highFound.Should().BeFalse();
            lowValue.Should().BeNull();
            highValue.Should().BeNull();
        }

        [TestMethod]
        public void LargeDistinctIntegerEntries_WhenExistingKeyIsRequested_UsesBinarySearchFoundPath()
        {
            // Arrange
            var store = new PropertyStore();

            for (var i = 0; i < 20; i++)
            {
                store.SetInteger(400 + (i * 4), i * 10);
            }

            // Act
            var value = store.GetInteger(448, out bool found);

            // Assert
            found.Should().BeTrue();
            value.Should().Be(120);
        }

        [TestMethod]
        public void LargeDistinctObjectEntries_WhenExistingKeyIsRequested_UsesBinarySearchFoundPath()
        {
            // Arrange
            var store = new PropertyStore();

            for (var i = 0; i < 20; i++)
            {
                store.SetObject(400 + (i * 4), $"value-{i}");
            }

            // Act
            var value = store.GetObject(448, out bool found);

            // Assert
            found.Should().BeTrue();
            value.Should().Be("value-12");
        }

        [TestMethod]
        public void RemoveInteger_WhenRemovingMiddleEntry_RebuildsArrayWithoutLosingNeighbors()
        {
            // Arrange
            var store = new PropertyStore();
            store.SetInteger(400, 1);
            store.SetInteger(800, 2);
            store.SetInteger(1200, 3);

            // Act
            store.RemoveInteger(800);

            // Assert
            store.GetInteger(400).Should().Be(1);
            store.ContainsInteger(800).Should().BeFalse();
            store.GetInteger(1200).Should().Be(3);
        }

        [TestMethod]
        public void RemoveObject_WhenRemovingMiddleEntry_RebuildsArrayWithoutLosingNeighbors()
        {
            // Arrange
            var store = new PropertyStore();
            store.SetObject(400, "first");
            store.SetObject(800, "middle");
            store.SetObject(1200, "last");

            // Act
            store.RemoveObject(800);

            // Assert
            store.GetObject(400).Should().Be("first");
            store.ContainsObject(800).Should().BeFalse();
            store.GetObject(1200).Should().Be("last");
        }

        [TestMethod]
        public void RemoveInteger_WhenRemovingDifferentElementSlots_ClearsOnlyRequestedValues()
        {
            // Arrange
            var store = new PropertyStore();
            store.SetInteger(100, 10);
            store.SetInteger(101, 20);
            store.SetInteger(102, 30);
            store.SetInteger(103, 40);

            // Act
            store.RemoveInteger(100);
            store.RemoveInteger(102);
            store.RemoveInteger(103);

            // Assert
            store.ContainsInteger(100).Should().BeFalse();
            store.GetInteger(101).Should().Be(20);
            store.ContainsInteger(102).Should().BeFalse();
            store.ContainsInteger(103).Should().BeFalse();
        }

        [TestMethod]
        public void RemoveInteger_WhenRemovingElementOneWhileGroupRemains_ClearsOnlySecondValue()
        {
            // Arrange
            var store = new PropertyStore();
            store.SetInteger(100, 10);
            store.SetInteger(101, 20);
            store.SetInteger(102, 30);

            // Act
            store.RemoveInteger(101);

            // Assert
            store.GetInteger(100).Should().Be(10);
            store.ContainsInteger(101).Should().BeFalse();
            store.GetInteger(102).Should().Be(30);
        }

        [TestMethod]
        public void RemoveObject_WhenSiblingSlotWasNeverSet_ReturnsWithoutMutatingStoredValue()
        {
            // Arrange
            var store = new PropertyStore();
            store.SetObject(100, "first");

            // Act
            store.RemoveObject(101);

            // Assert
            store.GetObject(100).Should().Be("first");
            store.ContainsObject(101).Should().BeFalse();
        }

        [TestMethod]
        public void RemoveObject_WhenRemovingDifferentElementSlots_ClearsOnlyRequestedValues()
        {
            // Arrange
            var store = new PropertyStore();
            store.SetObject(100, "zero");
            store.SetObject(101, "one");
            store.SetObject(102, "two");
            store.SetObject(103, "three");

            // Act
            store.RemoveObject(101);
            store.RemoveObject(103);

            // Assert
            store.GetObject(100).Should().Be("zero");
            store.ContainsObject(101).Should().BeFalse();
            store.GetObject(102).Should().Be("two");
            store.ContainsObject(103).Should().BeFalse();
        }
    }
}
