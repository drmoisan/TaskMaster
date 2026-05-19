using System;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;

namespace UtilitiesCS.Test.OutlookObjects.Recipient
{
    /// <summary>
    /// Tests for the robust Exchange directory resolution paths in GetRecipientInfo,
    /// GetSenderName, and GetSenderAddress. These cover the Exchange user lookup, COM-safe
    /// fallback chains, and Exchange DN address handling that were absent in the previous
    /// rudimentary implementations.
    /// </summary>
    [TestClass]
    public class RecipientStaticSenderResolverTests
    {
        private const string SmtpAddressProperty =
            "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        // ── GetRecipientInfo ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void GetRecipientInfo_WithExchangeUser_ReturnsExchangeNameAndSmtpAddress()
        {
            // Arrange: Exchange recipient whose raw Address is empty (Exchange DN not exposed),
            // but whose Exchange directory entry carries both the display name and primary SMTP.
            var exchangeUser = new Mock<ExchangeUser>();
            exchangeUser.SetupGet(x => x.FirstName).Returns("Ada");
            exchangeUser.SetupGet(x => x.LastName).Returns("Lovelace");
            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns("ada@exchange.example.com");

            var propertyAccessor = new Mock<PropertyAccessor>();
            propertyAccessor
                .Setup(x => x.GetProperty(SmtpAddressProperty))
                .Returns((object)"ada@exchange.example.com");

            var addressEntry = new Mock<AddressEntry>();
            addressEntry
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);

            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            recipient.SetupGet(x => x.Name).Returns("Ada Display Name");
            recipient.SetupGet(x => x.Address).Returns(string.Empty);
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            recipient.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            // Act
            var (name, address) = RecipientStatic.GetRecipientInfo(recipient.Object);

            // Assert: Exchange directory values take precedence over the raw recipient properties.
            name.Should().Be("Ada Lovelace");
            address.Should().Be("ada@exchange.example.com");
        }

        [TestMethod]
        public void GetRecipientInfo_WithNonExchangeRecipient_ReturnsNameAndAddress()
        {
            // Arrange: standard SMTP recipient — no Exchange directory involved.
            var propertyAccessor = new Mock<PropertyAccessor>();
            propertyAccessor
                .Setup(x => x.GetProperty(SmtpAddressProperty))
                .Returns((object)"grace@example.com");

            var addressEntry = new Mock<AddressEntry>();
            addressEntry
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);

            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            recipient.SetupGet(x => x.Name).Returns("Grace Hopper");
            recipient.SetupGet(x => x.Address).Returns("grace@example.com");
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            recipient.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            // Act
            var (name, address) = RecipientStatic.GetRecipientInfo(recipient.Object);

            // Assert
            name.Should().Be("Grace Hopper");
            address.Should().Be("grace@example.com");
        }

        // ── GetSenderName (MailItem) ──────────────────────────────────────────────────────

        [TestMethod]
        public void GetSenderName_ForMailItemWithNullSender_ReturnsSenderName()
        {
            // Arrange: Sender AddressEntry not available; fall back to the stored SenderName.
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns((AddressEntry)null);
            mail.SetupGet(x => x.SenderName).Returns("Ada Lovelace");

            // Act
            var result = mail.Object.GetSenderName();

            // Assert
            result.Should().Be("Ada Lovelace");
        }

        [TestMethod]
        public void GetSenderName_ForMailItemWithExchangeUser_ReturnsExchangeDirectoryName()
        {
            // Arrange: Exchange sender whose SenderName differs from the directory first/last pair.
            // The Exchange directory name is the authoritative source and must take precedence.
            var exchUser = new Mock<ExchangeUser>();
            exchUser.SetupGet(x => x.FirstName).Returns("Ada");
            exchUser.SetupGet(x => x.LastName).Returns("Lovelace");

            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            sender.Setup(x => x.GetExchangeUser()).Returns(exchUser.Object);

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderName).Returns("ada.lovelace@example.com");

            // Act
            var result = mail.Object.GetSenderName();

            // Assert: directory name preferred over the mail-item SenderName field.
            result.Should().Be("Ada Lovelace");
        }

        [TestMethod]
        public void GetSenderName_ForMailItemWhenGetExchangeUserReturnsNull_FallsBackToMailSenderName()
        {
            // Arrange: Exchange user type but GetExchangeUser returns null; the method should
            // prefer the mail-item SenderName fallback instead of touching AddressEntry.Name.
            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            sender.Setup(x => x.GetExchangeUser()).Returns((ExchangeUser)null);
            sender.SetupGet(x => x.Name).Returns("Ada from AddressEntry");

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderName).Returns("Ada from SenderName");

            // Act
            var result = mail.Object.GetSenderName();

            // Assert: mail-item sender data is the primary safe fallback.
            result.Should().Be("Ada from SenderName");
        }

        [TestMethod]
        public void GetSenderName_ForMailItemWhenExchangeLookupThrowsAndAddressEntryNameThrows_FallsBackToSenderName()
        {
            // Arrange: Exchange directory lookup and AddressEntry.Name both fail; the method
            // must not propagate the exception and must still return SenderName.
            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            sender
                .Setup(x => x.GetExchangeUser())
                .Throws(new InvalidOperationException("COM call failed"));
            sender
                .SetupGet(x => x.Name)
                .Throws(new InvalidOperationException("Name lookup failed"));

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderName).Returns("Ada Lovelace");

            // Act — must not throw
            var result = mail.Object.GetSenderName();

            // Assert
            result.Should().Be("Ada Lovelace");
        }

        [TestMethod]
        public void GetSenderAddress_ForMailItemWhenSenderAddressThrows_UsesPropertyAccessorFallback()
        {
            // Arrange: the Exchange lookup fails and AddressEntry.Address also fails, so the
            // method must continue to the property-accessor SMTP fallback instead of crashing.
            var propertyAccessor = new Mock<PropertyAccessor>();
            propertyAccessor
                .Setup(x => x.GetProperty(SmtpAddressProperty))
                .Returns((object)"ada@example.com");

            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            sender
                .Setup(x => x.GetExchangeUser())
                .Throws(new InvalidOperationException("Exchange lookup failed"));
            sender.SetupGet(x => x.Address).Throws(new InvalidOperationException("Address failed"));
            sender.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderEmailAddress).Returns(string.Empty);
            mail.SetupGet(x => x.SenderName).Returns("Ada Lovelace");

            // Act
            var result = mail.Object.GetSenderAddress();

            // Assert
            result.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetRecipientInfo_WhenExchangeLookupFails_UsesSafeRecipientFallbacks()
        {
            // Arrange: recipient Exchange lookup fails, Name/Address getters both fail, and the
            // helper must still degrade safely to the PR_SMTP_ADDRESS property accessor value.
            var propertyAccessor = new Mock<PropertyAccessor>();
            propertyAccessor
                .Setup(x => x.GetProperty(SmtpAddressProperty))
                .Returns((object)"ada.recipient@example.com");

            var addressEntry = new Mock<AddressEntry>();
            addressEntry
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            addressEntry
                .Setup(x => x.GetExchangeUser())
                .Throws(new InvalidOperationException("Exchange lookup failed"));

            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            recipient.SetupGet(x => x.Name).Throws(new InvalidOperationException("Name failed"));
            recipient
                .SetupGet(x => x.Address)
                .Throws(new InvalidOperationException("Address failed"));
            recipient.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            // Act
            var (name, address) = RecipientStatic.GetRecipientInfo(recipient.Object);

            // Assert
            name.Should().Be("ada.recipient@example.com");
            address.Should().Be("ada.recipient@example.com");
        }

        // ── GetSenderAddress (MailItem) ────────────────────────────────────────────────

        [TestMethod]
        public void GetSenderAddress_ForMailItemWithNullSender_ReturnsSenderEmailAddress()
        {
            // Arrange
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns((AddressEntry)null);
            mail.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");

            // Act
            var result = mail.Object.GetSenderAddress();

            // Assert
            result.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetSenderAddress_ForMailItemWithExchangeUser_ReturnsPrimarySmtpAddress()
        {
            // Arrange: Exchange sender with a valid primary SMTP in the directory.
            // The raw SenderEmailAddress may be an Exchange DN and must not be returned.
            var exchUser = new Mock<ExchangeUser>();
            exchUser.SetupGet(x => x.PrimarySmtpAddress).Returns("ada@exchange.example.com");

            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            sender.Setup(x => x.GetExchangeUser()).Returns(exchUser.Object);

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderEmailAddress).Returns("/o=ExchangeLabs/dn=Ada");

            // Act
            var result = mail.Object.GetSenderAddress();

            // Assert: primary SMTP from Exchange directory takes precedence over the raw field.
            result.Should().Be("ada@exchange.example.com");
        }

        [TestMethod]
        public void GetSenderAddress_ForMailItemWhenGetExchangeUserReturnsNull_FallsBackToSenderEmailAddress()
        {
            // Arrange: Exchange user type but no user object accessible; SenderEmailAddress
            // contains a valid SMTP address that should be used.
            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            sender.Setup(x => x.GetExchangeUser()).Returns((ExchangeUser)null);

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");

            // Act
            var result = mail.Object.GetSenderAddress();

            // Assert
            result.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetSenderAddress_ForMailItemWhenSenderEmailAddressEmpty_UsesPropertyAccessorFallback()
        {
            // Arrange: neither Exchange user nor SenderEmailAddress provides an SMTP address;
            // the PropertyAccessor carries the SMTP via the MAPI PR_SMTP_ADDRESS property.
            var propertyAccessor = new Mock<PropertyAccessor>();
            propertyAccessor
                .Setup(x => x.GetProperty(SmtpAddressProperty))
                .Returns((object)"ada@example.com");

            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            sender.SetupGet(x => x.Address).Returns(string.Empty);
            sender.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderEmailAddress).Returns(string.Empty);

            // Act
            var result = mail.Object.GetSenderAddress();

            // Assert: MAPI property accessor used as fallback when direct addresses are absent.
            result.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetSenderAddress_ForMailItemWhenAllFallbacksFail_ReturnsEmptyString()
        {
            // Arrange: no address is available from any source; final fallback must be "".
            var propertyAccessor = new Mock<PropertyAccessor>();
            propertyAccessor
                .Setup(x => x.GetProperty(SmtpAddressProperty))
                .Throws(new InvalidOperationException("property not found"));

            var sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            sender.SetupGet(x => x.Address).Returns(string.Empty);
            sender.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderEmailAddress).Returns(string.Empty);
            mail.SetupGet(x => x.SenderName).Returns(string.Empty);

            // Act
            var result = mail.Object.GetSenderAddress();

            // Assert
            result.Should().BeEmpty();
        }

        // ── GetSenderAddress (MeetingItem) ─────────────────────────────────────────────

        [TestMethod]
        public void GetSenderAddress_ForMeetingItemWithValidSmtpAddress_ReturnsSmtpAddress()
        {
            // Arrange: meeting sender with a normal SMTP address.
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");

            // Act
            var result = meeting.Object.GetSenderAddress();

            // Assert
            result.Should().Be("ada@example.com");
        }

        [TestMethod]
        public void GetSenderAddress_ForMeetingItemWithExchangeDnAddress_FallsBackToSenderName()
        {
            // Arrange: Exchange DN address stored in SenderEmailAddress cannot be used
            // as SMTP without a session reference; the method must fall back to SenderName.
            var meeting = new Mock<MeetingItem>();
            meeting
                .SetupGet(x => x.SenderEmailAddress)
                .Returns("/o=ExchangeLabs/ou=Exchange Administrative Group/cn=Ada Lovelace");
            meeting.SetupGet(x => x.SenderName).Returns("Ada Lovelace");

            // Act
            var result = meeting.Object.GetSenderAddress();

            // Assert
            result.Should().Be("Ada Lovelace");
        }

        [TestMethod]
        public void GetSenderAddress_ForMeetingItemWithEmptyAddress_FallsBackToSenderName()
        {
            // Arrange: empty SenderEmailAddress (e.g. externally organised meeting).
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.SenderEmailAddress).Returns(string.Empty);
            meeting.SetupGet(x => x.SenderName).Returns("Ada Lovelace");

            // Act
            var result = meeting.Object.GetSenderAddress();

            // Assert
            result.Should().Be("Ada Lovelace");
        }
    }
}
