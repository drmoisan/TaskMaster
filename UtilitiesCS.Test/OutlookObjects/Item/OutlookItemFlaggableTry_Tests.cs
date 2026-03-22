using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OutlookItemFlaggableTry_Tests
    {
        private Mock<IOutlookItemFlaggable> CreateMockFlaggable()
        {
            var mock = new Mock<IOutlookItemFlaggable>();
            mock.SetupGet(x => x.InnerObject).Returns(new object());
            mock.SetupGet(x => x.ItemType).Returns(typeof(object));
            mock.SetupGet(x => x.Args).Returns(new object[0]);
            return mock;
        }

        [TestMethod]
        public void Constructor_SetsOlItem()
        {
            // Arrange
            var mock = CreateMockFlaggable();

            // Act
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Assert
            tryFlaggable.Should().NotBeNull();
        }

        [TestMethod]
        public void Complete_Get_DelegatesToOlItem()
        {
            // Arrange
            var mock = CreateMockFlaggable();
            mock.SetupGet(x => x.Complete).Returns(true);
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Act
            var result = tryFlaggable.Complete;

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Complete_Get_WhenThrows_ReturnsFalse()
        {
            // Arrange
            var mock = CreateMockFlaggable();
            mock.SetupGet(x => x.Complete).Throws<InvalidOperationException>();
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Act
            var result = tryFlaggable.Complete;

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Complete_Set_DelegatesToOlItem()
        {
            // Arrange
            var mock = CreateMockFlaggable();
            mock.SetupSet(x => x.Complete = It.IsAny<bool>());
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Act
            tryFlaggable.Complete = true;

            // Assert
            mock.VerifySet(x => x.Complete = true, Times.Once());
        }

        [TestMethod]
        public void DueDate_Get_DelegatesToOlItem()
        {
            // Arrange
            var expected = new DateTime(2026, 1, 1);
            var mock = CreateMockFlaggable();
            mock.SetupGet(x => x.DueDate).Returns(expected);
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Act
            var result = tryFlaggable.DueDate;

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod]
        public void DueDate_Get_WhenThrows_ReturnsDefault()
        {
            // Arrange
            var mock = CreateMockFlaggable();
            mock.SetupGet(x => x.DueDate).Throws<InvalidOperationException>();
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Act
            var result = tryFlaggable.DueDate;

            // Assert
            result.Should().Be(default(DateTime));
        }

        [TestMethod]
        public void TaskSubject_Get_DelegatesToOlItem()
        {
            // Arrange
            var mock = CreateMockFlaggable();
            mock.SetupGet(x => x.TaskSubject).Returns("Task 1");
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Act
            var result = tryFlaggable.TaskSubject;

            // Assert
            result.Should().Be("Task 1");
        }

        [TestMethod]
        public void TotalWork_GetSet_DelegatesToOlItem()
        {
            // Arrange
            var mock = CreateMockFlaggable();
            mock.SetupGet(x => x.TotalWork).Returns(120);
            var tryFlaggable = new OutlookItemFlaggableTry(mock.Object);

            // Act
            var result = tryFlaggable.TotalWork;

            // Assert
            result.Should().Be(120);
        }
    }
}
