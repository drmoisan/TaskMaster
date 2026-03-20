using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.OutlookObjects.Recipient
{
    [TestClass]
    public class RecipientStaticTests
    {
        [TestMethod]
        public void ConvertRecipientToHtml_ShouldFormatDisplayNameAndMailToLink()
        {
            // Arrange / Act
            string html = RecipientStatic.ConvertRecipientToHtml("Ada Lovelace", "ada@example.com");

            // Assert
            html.Should()
                .Be("Ada Lovelace &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;");
        }

        [DataTestMethod]
        [DataRow("ada@example.com", "ada", null, "example.com")]
        [DataRow("ada.lovelace@example.com", "ada", "lovelace", "example.com")]
        [DataRow("a.b.charles.babbage@example.com", "charles", "babbage", "example.com")]
        public void ExtractNameFromAddress_ShouldReturnExpectedNameParts(
            string address,
            string expectedFirstName,
            string expectedLastName,
            string expectedDomain
        )
        {
            // Arrange / Act
            (string firstName, string lastName, string domain) =
                RecipientStatic.ExtractNameFromAddress(address);

            // Assert
            firstName.Should().Be(expectedFirstName);
            lastName.Should().Be(expectedLastName);
            domain.Should().Be(expectedDomain);
        }

        [TestMethod]
        public void ExtractNameFromAddress_WithMalformedAddress_ReturnsNullParts()
        {
            // Arrange / Act
            (string firstName, string lastName, string domain) =
                RecipientStatic.ExtractNameFromAddress("not-an-email");

            // Assert
            firstName.Should().BeNull();
            lastName.Should().BeNull();
            domain.Should().BeNull();
        }

        [TestMethod]
        public void GetSenderInfo_WithNullMailItem_ThrowsArgumentNullException()
        {
            // Arrange
            InteropMailItem mailItem = null;

            // Act
            System.Action act = () => mailItem.GetSenderInfo();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetSenderInfo_WithEmptyMeetingSenderName_ReturnsBlankRecipientInfo()
        {
            // Arrange
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.SenderName).Returns(string.Empty);

            // Act
            var result = meeting.Object.GetSenderInfo();

            // Assert
            result.Name.Should().BeEmpty();
            result.Address.Should().BeEmpty();
            result.Html.Should().BeEmpty();
        }

        [TestMethod]
        public void GetSenderInfo_WithNullMailSender_ReturnsBlankRecipientInfo()
        {
            // Arrange
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns((AddressEntry)null);

            // Act
            var result = mail.Object.GetSenderInfo();

            // Assert
            result.Name.Should().BeEmpty();
            result.Address.Should().BeEmpty();
            result.Html.Should().BeEmpty();
        }

        [TestMethod]
        public void GetSenderInfo_WithNameSpaceAndResolvedSender_ReturnsResolvedRecipientInfo()
        {
            // Arrange
            var sender = new Mock<AddressEntry>();
            var mail = new Mock<InteropMailItem>();
            var nameSpace = new Mock<NameSpace>();
            var resolvedRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();

            sender.SetupGet(x => x.Name).Returns("Ada Lovelace");
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            nameSpace
                .Setup(x => x.CreateRecipient("Ada Lovelace"))
                .Returns(resolvedRecipient.Object);
            resolvedRecipient.Setup(x => x.Resolve()).Returns(true);
            resolvedRecipient.SetupGet(x => x.Name).Returns("Ada Lovelace");
            resolvedRecipient.SetupGet(x => x.Address).Returns("ada@example.com");

            // Act
            var result = mail.Object.GetSenderInfo(nameSpace.Object);

            // Assert
            result.Name.Should().Be("Ada Lovelace");
            result.Address.Should().Be("ada@example.com");
            result
                .Html.Should()
                .Be("Ada Lovelace &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;");
        }

        [TestMethod]
        public void GetSenderInfo_WithNameSpaceAndUnresolvedSender_FallsBackToMailSenderValues()
        {
            // Arrange
            var sender = new Mock<AddressEntry>();
            var mail = new Mock<InteropMailItem>();
            var nameSpace = new Mock<NameSpace>();
            var unresolvedRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();

            sender.SetupGet(x => x.Name).Returns("Ada Lovelace");
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderName).Returns("Ada Lovelace");
            mail.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            nameSpace
                .Setup(x => x.CreateRecipient("Ada Lovelace"))
                .Returns(unresolvedRecipient.Object);
            unresolvedRecipient.Setup(x => x.Resolve()).Returns(false);

            // Act
            var result = mail.Object.GetSenderInfo(nameSpace.Object);

            // Assert
            result.Name.Should().Be("Ada Lovelace");
            result.Address.Should().Be("ada@example.com");
            result
                .Html.Should()
                .Be("Ada Lovelace &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;");
        }

        [TestMethod]
        public void ToResolvedRecipient_WhenRecipientDoesNotResolve_ReturnsOriginalRecipientAfterResolveAttempt()
        {
            // Arrange
            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var nameSpace = new Mock<NameSpace>();
            var createdRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();

            recipient.SetupGet(x => x.Name).Returns("Ada Lovelace");
            recipient.Setup(x => x.Resolve()).Returns(true);
            nameSpace
                .Setup(x => x.CreateRecipient("Ada Lovelace"))
                .Returns(createdRecipient.Object);
            createdRecipient.Setup(x => x.Resolve()).Returns(false);

            // Act
            var result = recipient.Object.ToResolvedRecipient(nameSpace.Object);

            // Assert
            result.Should().BeSameAs(recipient.Object);
            recipient.Verify(x => x.Resolve(), Times.Once);
        }

        [TestMethod]
        public void GetSenderName_ForMailItem_ShouldReturnSenderName()
        {
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.SenderName).Returns("Ada Lovelace");

            var result = mail.Object.GetSenderName();

            result.Should().Be("Ada Lovelace");
        }

        [TestMethod]
        public void GetSenderName_ForMeetingItem_ShouldReturnSenderName()
        {
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.SenderName).Returns("Charles Babbage");

            var result = meeting.Object.GetSenderName();

            result.Should().Be("Charles Babbage");
        }

        [TestMethod]
        public void GetSenderAddress_ForMailItem_ShouldReturnSenderEmailAddress()
        {
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");

            var result = mail.Object.GetSenderAddress();

            result.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetSenderAddress_ForMeetingItem_ShouldReturnSenderEmailAddress()
        {
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.SenderEmailAddress).Returns("charles@example.com");

            var result = meeting.Object.GetSenderAddress();

            result.Should().Be("charles@example.com");
        }

        [TestMethod]
        public void GetSenderInfo_ForMeetingWithValidSender_ShouldReturnPopulatedRecipientInfo()
        {
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.SenderName).Returns("Ada Lovelace");
            meeting.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");

            var result = meeting.Object.GetSenderInfo();

            result.Name.Should().Be("Ada Lovelace");
            result.Address.Should().Be("ada@example.com");
            result.Html.Should().Contain("ada@example.com");
        }

        [TestMethod]
        public void GetSenderInfo_ForMailWithValidSender_ShouldReturnPopulatedRecipientInfo()
        {
            var sender = new Mock<AddressEntry>();
            var mail = new Mock<InteropMailItem>();
            var accessor = new Mock<PropertyAccessor>();

            sender.SetupGet(x => x.PropertyAccessor).Returns(accessor.Object);
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderName).Returns("Ada Lovelace");
            mail.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");

            var result = mail.Object.GetSenderInfo();

            result.Name.Should().Be("Ada Lovelace");
            result.Address.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetRecipientInfo_ShouldReturnNameAndAddress()
        {
            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            recipient.SetupGet(x => x.Name).Returns("Ada");
            recipient.SetupGet(x => x.Address).Returns("ada@example.com");

            var (name, address) = RecipientStatic.GetRecipientInfo(recipient.Object);

            name.Should().Be("Ada");
            address.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetSenderInfo_ForNullMeetingItem_ShouldThrowArgumentNullException()
        {
            MeetingItem meeting = null;

            System.Action act = () => meeting.GetSenderInfo();

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ToResolvedRecipient_AddressEntry_WhenResolves_ShouldReturnResolvedRecipient()
        {
            var addressEntry = new Mock<AddressEntry>();
            var nameSpace = new Mock<NameSpace>();
            var resolvedRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();

            addressEntry.SetupGet(x => x.Name).Returns("Ada");
            nameSpace.Setup(x => x.CreateRecipient("Ada")).Returns(resolvedRecipient.Object);
            resolvedRecipient.Setup(x => x.Resolve()).Returns(true);

            var result = addressEntry.Object.ToResolvedRecipient(nameSpace.Object);

            result.Should().BeSameAs(resolvedRecipient.Object);
        }

        [TestMethod]
        public void ToResolvedRecipient_AddressEntry_WhenNotResolved_ShouldReturnDefault()
        {
            var addressEntry = new Mock<AddressEntry>();
            var nameSpace = new Mock<NameSpace>();
            var unresolvedRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();

            addressEntry.SetupGet(x => x.Name).Returns("Ghost");
            nameSpace.Setup(x => x.CreateRecipient("Ghost")).Returns(unresolvedRecipient.Object);
            unresolvedRecipient.Setup(x => x.Resolve()).Returns(false);

            var result = addressEntry.Object.ToResolvedRecipient(nameSpace.Object);

            result.Should().BeNull();
        }
    }
}
