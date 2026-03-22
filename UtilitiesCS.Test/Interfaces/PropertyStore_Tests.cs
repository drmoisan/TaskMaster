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
    }
}
