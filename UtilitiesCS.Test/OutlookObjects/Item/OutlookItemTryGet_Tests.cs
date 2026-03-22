using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemTryGet_Tests
    {
        [TestMethod]
        public void Constructor_SetsInternalItem()
        {
            // Arrange
            var olItem = new OutlookItem(new object());

            // Act
            var tryGet = new OutlookItemTryGet(olItem);

            // Assert
            tryGet.Should().NotBeNull();
        }

        [TestMethod]
        public void TryGet_WhenGetterSucceeds_ReturnsTrueAndValue()
        {
            // Arrange
            var olItem = new OutlookItem(new object());
            var tryGet = new OutlookItemTryGet(olItem);

            // Act
            var success = tryGet.TryGet(() => "value", out string result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be("value");
        }

        [TestMethod]
        public void TryGet_WhenGetterThrows_ReturnsFalseAndDefault()
        {
            // Arrange
            var olItem = new OutlookItem(new object());
            var tryGet = new OutlookItemTryGet(olItem);

            // Act
            var success = tryGet.TryGet<string>(
                () => throw new InvalidOperationException(),
                out string result
            );

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [TestMethod]
        public void TrySet_WhenSetterSucceeds_ReturnsTrue()
        {
            // Arrange
            var olItem = new OutlookItem(new object());
            var tryGet = new OutlookItemTryGet(olItem);
            string captured = null;

            // Act
            var success = tryGet.TrySet<string>(v => captured = v, "hello");

            // Assert
            success.Should().BeTrue();
            captured.Should().Be("hello");
        }

        [TestMethod]
        public void TrySet_WhenSetterThrows_ReturnsFalse()
        {
            // Arrange
            var olItem = new OutlookItem(new object());
            var tryGet = new OutlookItemTryGet(olItem);

            // Act
            var success = tryGet.TrySet<string>(
                v => throw new InvalidOperationException(),
                "hello"
            );

            // Assert
            success.Should().BeFalse();
        }

        [TestMethod]
        public void Subject_WithNonOutlookObject_ReturnsFalse()
        {
            // Arrange — OutlookItem wraps a plain object, no Subject property
            var olItem = new OutlookItem(new object());
            var tryGet = new OutlookItemTryGet(olItem);

            // Act
            var success = tryGet.Subject(out string result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [TestMethod]
        public void InnerObject_ReturnsTrue()
        {
            // Arrange
            var wrappedObj = new object();
            var olItem = new OutlookItem(wrappedObj);
            var tryGet = new OutlookItemTryGet(olItem);

            // Act
            var success = tryGet.InnerObject(out object result);

            // Assert
            success.Should().BeTrue();
            result.Should().BeSameAs(wrappedObj);
        }
    }
}
