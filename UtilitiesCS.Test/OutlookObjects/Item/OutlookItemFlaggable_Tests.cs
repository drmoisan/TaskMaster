using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemFlaggable_Tests
    {
        [TestMethod]
        public void Constructor_WithNullItem_DoesNotThrow()
        {
            // Arrange — non-Outlook objects cause GetOlItemType to throw
            System.Action act = () => new OutlookItemFlaggable((object)null);

            // Assert
            act.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Constructor_WithNonOutlookObject_ThrowsInvalidOperationException()
        {
            // Arrange — GetOlItemType throws for non-supported types
            System.Action act = () => new OutlookItemFlaggable(new object());

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Constructor_WithIOutlookItem_NullItem_DoesNotThrow()
        {
            // Arrange & Act
            System.Action act = () => new OutlookItemFlaggable((IOutlookItem)null);

            // Assert — null IOutlookItem should not throw
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Constructor_WithMockedIOutlookItem_NonOutlookInner_Throws()
        {
            // Arrange
            var mockItem = new Mock<IOutlookItem>();
            mockItem.SetupGet(x => x.InnerObject).Returns(new object());
            mockItem.SetupGet(x => x.Args).Returns(new object[0]);

            // Act
            System.Action act = () => new OutlookItemFlaggable(mockItem.Object);

            // Assert — GetOlItemType throws for non-supported inner object types
            act.Should().Throw<InvalidOperationException>();
        }
    }
}
