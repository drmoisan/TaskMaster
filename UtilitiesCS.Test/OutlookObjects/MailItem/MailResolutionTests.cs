using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.OutlookObjects.MailItemCoverage
{
    [TestClass]
    public class MailItemMailResolutionTests
    {
        [DataTestMethod]
        [DataRow("IPM.Note.SMIME")]
        [DataRow("IPM.Note.Secure")]
        [DataRow("IPM.Note.Secure.Sign")]
        [DataRow("IPM.Outlook.Recall")]
        public void IsMailUnReadable_WithKnownUnreadableMessageClass_ReturnsTrue(
            string messageClass
        )
        {
            // Arrange
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns(messageClass);

            // Act
            var result = mailItem.Object.IsMailUnReadable();

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsMailUnReadable_WithReadableMessageClass_ReturnsFalse()
        {
            // Arrange
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Note");

            // Act
            var result = mailItem.Object.IsMailUnReadable();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void TryResolveMailItem_WithReadableMailItem_ReturnsSameMailItem()
        {
            // Arrange
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Note");

            // Act
            var result = MailResolution.TryResolveMailItem(mailItem.Object);

            // Assert
            result.Should().BeSameAs(mailItem.Object);
        }

        [TestMethod]
        public void TryResolveMailItem_WithUnreadableMailItem_ReturnsNull()
        {
            // Arrange
            var mailItem = new Mock<InteropMailItem>();
            mailItem.SetupGet(x => x.MessageClass).Returns("IPM.Note.Secure");

            // Act
            var result = MailResolution.TryResolveMailItem(mailItem.Object);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void TryResolveMailItem_WithNullObject_ReturnsNull()
        {
            // Act
            var result = MailResolution.TryResolveMailItem(null);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void TryResolveMailItem_WithNonMailObject_ReturnsNull()
        {
            // Arrange
            var notMail = new object();

            // Act
            var result = MailResolution.TryResolveMailItem(notMail);

            // Assert
            result.Should().BeNull();
        }
    }
}
