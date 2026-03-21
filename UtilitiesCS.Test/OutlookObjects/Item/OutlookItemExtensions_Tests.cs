using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemExtensions_Tests
    {
        [TestMethod]
        public void Try_ReturnsOutlookItemTry()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            var tryItem = OutlookItemExtensions.Try(item);

            // Assert
            tryItem.Should().NotBeNull();
            tryItem.Should().BeOfType<OutlookItemTry>();
        }

        [TestMethod]
        public void TryGet_ReturnsOutlookItemTryGet()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            var tryGet = OutlookItemExtensions.TryGet(item);

            // Assert
            tryGet.Should().NotBeNull();
            tryGet.Should().BeOfType<OutlookItemTryGet>();
        }

        [TestMethod]
        public void IsValid_NullItem_ReturnsFalse()
        {
            // Arrange
            OutlookItem item = null;

            // Act
            var result = OutlookItemExtensions.IsValid(item);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_ItemWithNullInnerObject_ReturnsFalse()
        {
            // Arrange
            var item = new OutlookItem(null);

            // Act
            var result = item.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_NonOutlookInnerObject_ReturnsFalse()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            var result = item.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetOlItemType_NonOutlookItem_ThrowsInvalidOperationException()
        {
            // Arrange
            var mock = new Mock<IOutlookItem>();
            mock.SetupGet(x => x.InnerObject).Returns(new object());

            // Act
            System.Action act = () => OutlookItemExtensions.GetOlItemType(mock.Object);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
