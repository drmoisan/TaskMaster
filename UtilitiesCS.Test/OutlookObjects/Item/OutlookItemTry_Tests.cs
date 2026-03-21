using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemTry_Tests
    {
        private Mock<IOutlookItem> CreateMockOlItem()
        {
            var mock = new Mock<IOutlookItem>();
            mock.SetupGet(x => x.InnerObject).Returns(new object());
            mock.SetupGet(x => x.ItemType).Returns(typeof(object));
            mock.SetupGet(x => x.Args).Returns(new object[0]);
            return mock;
        }

        [TestMethod]
        public void Constructor_SetsInternalFields()
        {
            // Arrange
            var mock = CreateMockOlItem();

            // Act
            var tryItem = new OutlookItemTry(mock.Object);

            // Assert
            tryItem.InnerObject.Should().NotBeNull();
        }

        [TestMethod]
        public void TryGet_WhenGetterSucceeds_ReturnsValue()
        {
            // Arrange
            var mock = CreateMockOlItem();
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            var result = tryItem.TryGet(() => "hello");

            // Assert
            result.Should().Be("hello");
        }

        [TestMethod]
        public void TryGet_WhenGetterThrowsSystemException_ReturnsDefault()
        {
            // Arrange
            var mock = CreateMockOlItem();
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            var result = tryItem.TryGet<string>(() => throw new InvalidOperationException());

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void TrySet_WhenSetterSucceeds_InvokesSetter()
        {
            // Arrange
            var mock = CreateMockOlItem();
            var tryItem = new OutlookItemTry(mock.Object);
            string captured = null;

            // Act
            tryItem.TrySet<string>(v => captured = v, "test");

            // Assert
            captured.Should().Be("test");
        }

        [TestMethod]
        public void TrySet_WhenSetterThrowsSystemException_DoesNotThrow()
        {
            // Arrange
            var mock = CreateMockOlItem();
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            System.Action act = () =>
                tryItem.TrySet<string>(v => throw new InvalidOperationException(), "test");

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void TryCall_WhenActionSucceeds_Executes()
        {
            // Arrange
            var mock = CreateMockOlItem();
            var tryItem = new OutlookItemTry(mock.Object);
            bool called = false;

            // Act
            tryItem.TryCall(() => called = true);

            // Assert
            called.Should().BeTrue();
        }

        [TestMethod]
        public void TryCall_WhenActionThrows_DoesNotThrow()
        {
            // Arrange
            var mock = CreateMockOlItem();
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            System.Action act = () => tryItem.TryCall(() => throw new InvalidOperationException());

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Subject_DelegatesTo_OlItem()
        {
            // Arrange
            var mock = CreateMockOlItem();
            mock.SetupGet(x => x.Subject).Returns("Test Subject");
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            var subject = tryItem.Subject;

            // Assert
            subject.Should().Be("Test Subject");
        }

        [TestMethod]
        public void Subject_WhenOlItemThrows_ReturnsDefault()
        {
            // Arrange
            var mock = CreateMockOlItem();
            mock.SetupGet(x => x.Subject).Throws<InvalidOperationException>();
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            var subject = tryItem.Subject;

            // Assert
            subject.Should().BeNull();
        }

        [TestMethod]
        public void EntryID_DelegatesTo_OlItem()
        {
            // Arrange
            var mock = CreateMockOlItem();
            mock.SetupGet(x => x.EntryID).Returns("ABC123");
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            var entryId = tryItem.EntryID;

            // Assert
            entryId.Should().Be("ABC123");
        }

        [TestMethod]
        public void Body_Set_DelegatesTo_OlItem()
        {
            // Arrange
            var mock = CreateMockOlItem();
            mock.SetupSet(x => x.Body = It.IsAny<string>());
            var tryItem = new OutlookItemTry(mock.Object);

            // Act
            tryItem.Body = "new body";

            // Assert
            mock.VerifySet(x => x.Body = "new body", Times.Once());
        }
    }
}
