using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItem_Tests
    {
        [TestMethod]
        public void Constructor_WithNullItem_DoesNotThrow()
        {
            // Arrange & Act
            System.Action act = () => new OutlookItem(null);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Constructor_WithObject_SetsItemAndType()
        {
            // Arrange
            var testObj = "test string";

            // Act
            var item = new OutlookItem(testObj);

            // Assert
            item.ItemType.Should().Be(typeof(string));
            item.Args.Should().NotBeNull();
            item.Args.Should().BeEmpty();
        }

        [TestMethod]
        public void InnerObject_ReturnsWrappedItem()
        {
            // Arrange
            var testObj = new object();
            var item = new OutlookItem(testObj);

            // Assert
            item.InnerObject.Should().BeSameAs(testObj);
        }

        [TestMethod]
        public void Subject_Get_WithNonOutlookItem_ReturnsNull()
        {
            // Arrange — non-Outlook object has no Subject property
            var item = new OutlookItem(new object());

            // Act
            var subject = item.Subject;

            // Assert
            subject.Should().BeNull();
        }

        [TestMethod]
        public void EntryID_Get_WithNonOutlookItem_ReturnsNull()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            var entryId = item.EntryID;

            // Assert
            entryId.Should().BeNull();
        }

        [TestMethod]
        public void Body_Get_WithNonOutlookItem_ReturnsNull()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            var body = item.Body;

            // Assert
            body.Should().BeNull();
        }

        [TestMethod]
        public void Categories_Get_WithNonOutlookItem_ReturnsNull()
        {
            // Arrange
            var item = new OutlookItem(new object());

            // Act
            var categories = item.Categories;

            // Assert
            categories.Should().BeNull();
        }
    }
}
