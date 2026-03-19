using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookExtensions;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.MailItemCoverage
{
    [TestClass]
    public class MailItemExtensionsTests
    {
        [TestMethod]
        public void ToMIME_WhenPropertyAccessorReturnsBytes_ReturnsMimeContent()
        {
            // Arrange
            var expected = new byte[] { 1, 2, 3 };
            var accessor = new Mock<PropertyAccessor>();
            var mailItem = new Mock<InteropMailItem>();
            accessor
                .Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10130102"))
                .Returns(expected);
            mailItem.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);

            // Act
            byte[] result = mailItem.Object.ToMIME();

            // Assert
            result.Should().Equal(expected);
        }

        [TestMethod]
        public void ToMIME_WhenPropertyAccessorReturnsNonByteValue_ReturnsNull()
        {
            // Arrange
            var accessor = new Mock<PropertyAccessor>();
            var mailItem = new Mock<InteropMailItem>();
            accessor
                .Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10130102"))
                .Returns("not-bytes");
            mailItem.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);

            // Act
            byte[] result = mailItem.Object.ToMIME();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task TryMoveAsync_WhenMailItemIsNull_ReturnsNull()
        {
            // Arrange
            var folder = Mock.Of<OutlookFolder>();

            // Act
            object result = await MailItemExtensions.TryMoveAsync(null, folder);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task TryMoveAsync_WhenFolderIsNull_ReturnsNull()
        {
            // Arrange
            var mailItem = Mock.Of<InteropMailItem>();

            // Act
            object result = await mailItem.TryMoveAsync(null);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task TryMoveAsync_WhenMoveSucceeds_ReturnsMoveResult()
        {
            // Arrange
            var expected = new object();
            var folder = new Mock<OutlookFolder>();
            var mailItem = new Mock<InteropMailItem>();
            mailItem.Setup(x => x.Move(folder.Object)).Returns(expected);

            // Act
            object result = await mailItem.Object.TryMoveAsync(folder.Object);

            // Assert
            result.Should().BeSameAs(expected);
            mailItem.Verify(x => x.Move(folder.Object), Times.Once);
        }

        [TestMethod]
        public async Task TryMoveAsync_WhenMoveFailsWithoutRetries_ReturnsNull()
        {
            // Arrange
            var folder = new Mock<OutlookFolder>();
            var mailItem = new Mock<InteropMailItem>();
            mailItem
                .Setup(x => x.Move(folder.Object))
                .Throws(new InvalidOperationException("boom"));

            // Act
            object result = await mailItem.Object.TryMoveAsync(folder.Object);

            // Assert
            result.Should().BeNull();
            mailItem.Verify(x => x.Move(folder.Object), Times.Once);
        }

        [TestMethod]
        public async Task TryMoveAsync_WhenMoveFailsThenRetrySucceeds_ReturnsSuccessfulResult()
        {
            // Arrange
            var expected = new object();
            var folder = new Mock<OutlookFolder>();
            var mailItem = new Mock<InteropMailItem>();
            mailItem
                .SetupSequence(x => x.Move(folder.Object))
                .Throws(new InvalidOperationException("transient"))
                .Returns(expected);

            // Act
            object result = await mailItem.Object.TryMoveAsync(folder.Object, retries: 1);

            // Assert
            result.Should().BeSameAs(expected);
            mailItem.Verify(x => x.Move(folder.Object), Times.Exactly(2));
        }
    }
}
