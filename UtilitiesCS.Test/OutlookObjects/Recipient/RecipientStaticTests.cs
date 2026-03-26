using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;
using InteropStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects.Recipient
{
    [TestClass]
    public class RecipientStaticTests
    {
        private const string SmtpAddressProperty =
            "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        [TestMethod]
        public void ConvertRecipientToHtml_ShouldFormatDisplayNameAndMailToLink()
        {
            string html = RecipientStatic.ConvertRecipientToHtml("Ada Lovelace", "ada@example.com");

            html.Should()
                .Be("Ada Lovelace &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;");
        }

        [TestMethod]
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
            (string firstName, string lastName, string domain) =
                RecipientStatic.ExtractNameFromAddress(address);

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
        public void GetSenderInfo_WhenExchangeUserPropertiesThrowComException_FallsBackToMailValues()
        {
            // Arrange
            var exchangeUser = new Mock<ExchangeUser>();
            var sender = new Mock<AddressEntry>();
            var mail = new Mock<InteropMailItem>();

            exchangeUser
                .SetupGet(x => x.FirstName)
                .Throws(new System.Runtime.InteropServices.COMException("Boom"));
            exchangeUser
                .SetupGet(x => x.LastName)
                .Throws(new System.Runtime.InteropServices.COMException("Boom"));
            exchangeUser
                .SetupGet(x => x.PrimarySmtpAddress)
                .Throws(new System.Runtime.InteropServices.COMException("Boom"));

            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olExchangeUserAddressEntry);
            sender.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
            sender.SetupGet(x => x.Address).Returns("mdlz@jobalerts.mdlz.com");
            sender.SetupGet(x => x.Name).Returns("Mondelēz International, Inc.");

            mail.SetupGet(x => x.Sender).Returns(sender.Object);
            mail.SetupGet(x => x.SenderName).Returns("Mondelēz International, Inc.");
            mail.SetupGet(x => x.SenderEmailAddress).Returns("mdlz@jobalerts.mdlz.com");

            // Act
            var result = mail.Object.GetSenderInfo();

            // Assert
            result.Name.Should().Be("Mondelēz International, Inc.");
            result.Address.Should().Be("mdlz@jobalerts.mdlz.com");
            result
                .Html.Should()
                .Be(
                    "Mondelēz International, Inc. &lt;<a href=\"mailto:mdlz@jobalerts.mdlz.com\">mdlz@jobalerts.mdlz.com</a>&gt;"
                );
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
        public void GetSenderInfo_ForNullMeetingItem_ShouldThrowArgumentNullException()
        {
            MeetingItem meeting = null;

            System.Action act = () => meeting.GetSenderInfo();

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetGlobalAddressList_WithNullStore_ThrowsArgumentNullException()
        {
            // Arrange
            InteropStore store = null;
            var application = new Mock<Application>();

            // Act
            System.Action act = () => store.GetGlobalAddressList(application.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetRecipients_ForMailItemWithNullRecipients_ReturnsBlankValues()
        {
            // Arrange
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Recipients).Returns((Recipients)null);

            // Act
            var (recipientsTo, recipientsCc) = mail.Object.GetRecipients();

            // Assert
            recipientsTo.Should().BeEmpty();
            recipientsCc.Should().BeEmpty();
        }

        [TestMethod]
        public void GetRecipients_ForMailItem_ReturnsToAndCcAddressesUsingFallbacks()
        {
            // Arrange
            var toRecipient = CreateRecipientMock(
                name: "Ada Lovelace",
                address: "",
                type: (int)OlMailRecipientType.olTo,
                hasPropertyAccessorValue: true,
                smtpAddressFromPropertyAccessor: "ada@example.com"
            );
            var ccRecipient = CreateRecipientMock(
                name: "Grace Hopper",
                address: "",
                type: (int)OlMailRecipientType.olCC,
                propertyAccessorThrows: true
            );

            var recipients = CreateRecipientsMock(toRecipient.Object, ccRecipient.Object);
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            var (recipientsTo, recipientsCc) = mail.Object.GetRecipients();

            // Assert
            recipientsTo.Should().Be("ada@example.com");
            recipientsCc.Should().Be("Grace Hopper");
        }

        [TestMethod]
        public void GetRecipients_ForMeetingItem_ReturnsToAndCcAddresses()
        {
            // Arrange
            var toRecipient = CreateRecipientMock(
                name: "Ada Lovelace",
                address: "ada@example.com",
                type: (int)OlMailRecipientType.olTo
            );
            var ccRecipient = CreateRecipientMock(
                name: "Grace Hopper",
                address: "grace@example.com",
                type: (int)OlMailRecipientType.olCC
            );

            var recipients = CreateRecipientsMock(toRecipient.Object, ccRecipient.Object);
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            var (recipientsTo, recipientsCc) = meeting.Object.GetRecipients();

            // Assert
            recipientsTo.Should().Be("ada@example.com");
            recipientsCc.Should().Be("grace@example.com");
        }

        [TestMethod]
        public void GetRecipients_ForMailItemWithNamespace_UsesResolvedRecipients()
        {
            // Arrange
            var originalRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var resolvedRecipient = CreateRecipientMock(
                name: "Ada Lovelace",
                address: "ada@example.com",
                type: (int)OlMailRecipientType.olTo
            );
            var nameSpace = new Mock<NameSpace>();
            var recipients = CreateRecipientsMock(originalRecipient.Object);
            var mail = new Mock<InteropMailItem>();

            originalRecipient.SetupGet(x => x.Name).Returns("Ada Lovelace");
            nameSpace
                .Setup(x => x.CreateRecipient("Ada Lovelace"))
                .Returns(resolvedRecipient.Object);
            resolvedRecipient.Setup(x => x.Resolve()).Returns(true);
            mail.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            var (recipientsTo, recipientsCc) = mail.Object.GetRecipients(nameSpace.Object);

            // Assert
            recipientsTo.Should().Be("ada@example.com");
            recipientsCc.Should().BeEmpty();
        }

        [TestMethod]
        public void GetRecipients_ForMeetingItemWithNamespace_UsesResolvedRecipients()
        {
            // Arrange
            var originalRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var resolvedRecipient = CreateRecipientMock(
                name: "Grace Hopper",
                address: "grace@example.com",
                type: (int)OlMailRecipientType.olCC
            );
            var nameSpace = new Mock<NameSpace>();
            var recipients = CreateRecipientsMock(originalRecipient.Object);
            var meeting = new Mock<MeetingItem>();

            originalRecipient.SetupGet(x => x.Name).Returns("Grace Hopper");
            nameSpace
                .Setup(x => x.CreateRecipient("Grace Hopper"))
                .Returns(resolvedRecipient.Object);
            resolvedRecipient.Setup(x => x.Resolve()).Returns(true);
            meeting.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            var (recipientsTo, recipientsCc) = meeting.Object.GetRecipients(nameSpace.Object);

            // Assert
            recipientsTo.Should().BeEmpty();
            recipientsCc.Should().Be("grace@example.com");
        }

        [TestMethod]
        public void ToResolvedRecipient_WhenCreatedRecipientResolves_ReturnsResolvedRecipient()
        {
            // Arrange
            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var createdRecipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();
            var nameSpace = new Mock<NameSpace>();

            recipient.SetupGet(x => x.Name).Returns("Ada Lovelace");
            nameSpace
                .Setup(x => x.CreateRecipient("Ada Lovelace"))
                .Returns(createdRecipient.Object);
            createdRecipient.Setup(x => x.Resolve()).Returns(true);

            // Act
            var result = recipient.Object.ToResolvedRecipient(nameSpace.Object);

            // Assert
            result.Should().BeSameAs(createdRecipient.Object);
        }

        [TestMethod]
        public void GetInfo_WithStoresWrapper_UsesExchangeNameAndPropertyAccessorFallback()
        {
            // Arrange
            var recipient = CreateRecipientMock(
                name: "Ada Display",
                address: string.Empty,
                type: (int)OlMailRecipientType.olTo,
                userType: OlAddressEntryUserType.olExchangeUserAddressEntry,
                hasPropertyAccessorValue: true,
                smtpAddressFromPropertyAccessor: "ada@example.com",
                hasExchangeUser: true,
                exchangeFirstName: "Ada",
                exchangeLastName: "Lovelace",
                exchangePrimarySmtpAddress: string.Empty
            );

            var recipients = new[] { recipient.Object };

            // Act
            var result = RecipientStatic.GetInfo(recipients, null).Single();

            // Assert
            result.Name.Should().Be("Ada Lovelace");
            result.Address.Should().Be("ada@example.com");
            result
                .Html.Should()
                .Be("Ada Lovelace &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;");
        }

        [TestMethod]
        public void GetInfo_WithStoresWrapper_WhenExchangePropertiesThrowComException_FallsBackToRecipientValues()
        {
            // Arrange
            var recipient = CreateRecipientMock(
                name: "Mondelēz International, Inc.",
                address: "mdlz@jobalerts.mdlz.com",
                type: (int)OlMailRecipientType.olTo,
                userType: OlAddressEntryUserType.olExchangeUserAddressEntry,
                hasExchangeUser: true,
                exchangeNameThrowsComException: true,
                exchangePrimarySmtpThrowsComException: true
            );

            // Act
            var result = RecipientStatic.GetInfo(new[] { recipient.Object }, null).Single();

            // Assert
            result.Name.Should().Be("Mondelēz International, Inc.");
            result.Address.Should().Be("mdlz@jobalerts.mdlz.com");
            result
                .Html.Should()
                .Be(
                    "Mondelēz International, Inc. &lt;<a href=\"mailto:mdlz@jobalerts.mdlz.com\">mdlz@jobalerts.mdlz.com</a>&gt;"
                );
        }

        [TestMethod]
        public void GetInfo_ForRecipientSequence_ProjectsEachRecipient()
        {
            // Arrange
            var firstRecipient = CreateRecipientMock(
                name: "Ada",
                address: "ada@example.com",
                type: (int)OlMailRecipientType.olTo
            );
            var secondRecipient = CreateRecipientMock(
                name: "Grace",
                address: "grace@example.com",
                type: (int)OlMailRecipientType.olCC
            );

            // Act
            var result = new[] { firstRecipient.Object, secondRecipient.Object }
                .GetInfo()
                .ToArray();

            // Assert
            result.Select(x => x.Name).Should().Equal("Ada", "Grace");
            result.Select(x => x.Address).Should().Equal("ada@example.com", "grace@example.com");
        }

        [TestMethod]
        public void GetToRecipientsInHtml_ReturnsOnlyToRecipientsAsHtml()
        {
            // Arrange
            var toRecipient = CreateRecipientMock(
                name: "Ada Lovelace",
                address: "ada@example.com",
                type: (int)OlMailRecipientType.olTo,
                userType: OlAddressEntryUserType.olExchangeUserAddressEntry,
                hasExchangeUser: true,
                exchangeFirstName: "Ada",
                exchangeLastName: "Lovelace",
                exchangePrimarySmtpAddress: "ada@example.com"
            );
            var ccRecipient = CreateRecipientMock(
                name: "Grace Hopper",
                address: "grace@example.com",
                type: (int)OlMailRecipientType.olCC
            );

            var recipients = CreateRecipientsMock(toRecipient.Object, ccRecipient.Object);
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            var result = RecipientStatic.GetToRecipientsInHtml(mail.Object);

            // Assert
            result.Should().Contain("Ada Lovelace");
            result.Should().Contain("ada@example.com");
            result.Should().NotContain("Grace Hopper");
        }

        [TestMethod]
        public void GetToRecipients_ForMailItem_ReturnsOnlyToRecipients()
        {
            // Arrange
            var recipients = CreateRecipientsMock(
                CreateRecipientMock(
                    name: "Ada",
                    address: "ada@example.com",
                    type: (int)OlMailRecipientType.olTo
                ).Object,
                CreateRecipientMock(
                    name: "Grace",
                    address: "grace@example.com",
                    type: (int)OlMailRecipientType.olCC
                ).Object
            );
            var mail = new Mock<InteropMailItem>();
            mail.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            var result = mail.Object.GetToRecipients().Select(x => x.Name).ToArray();

            // Assert
            result.Should().Equal("Ada");
        }

        [TestMethod]
        public void GetCcRecipients_ForMeetingItem_ReturnsOnlyCcRecipients()
        {
            // Arrange
            var recipients = CreateRecipientsMock(
                CreateRecipientMock(
                    name: "Ada",
                    address: "ada@example.com",
                    type: (int)OlMailRecipientType.olTo
                ).Object,
                CreateRecipientMock(
                    name: "Grace",
                    address: "grace@example.com",
                    type: (int)OlMailRecipientType.olCC
                ).Object
            );
            var meeting = new Mock<MeetingItem>();
            meeting.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            var result = meeting.Object.GetCcRecipients().Select(x => x.Name).ToArray();

            // Assert
            result.Should().Equal("Grace");
        }

        private static Mock<Recipients> CreateRecipientsMock(
            params Microsoft.Office.Interop.Outlook.Recipient[] recipients
        )
        {
            var recipientsMock = new Mock<Recipients>();
            var recipientList = recipients.ToList();

            recipientsMock.SetupGet(x => x.Count).Returns(recipientList.Count);
            recipientsMock
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)recipientList).GetEnumerator());

            return recipientsMock;
        }

        private static Mock<Microsoft.Office.Interop.Outlook.Recipient> CreateRecipientMock(
            string name,
            string address,
            int type,
            OlAddressEntryUserType userType = OlAddressEntryUserType.olSmtpAddressEntry,
            bool hasPropertyAccessorValue = false,
            string smtpAddressFromPropertyAccessor = "",
            bool propertyAccessorThrows = false,
            bool hasExchangeUser = false,
            string exchangeFirstName = "",
            string exchangeLastName = "",
            string exchangePrimarySmtpAddress = "",
            bool exchangeNameThrowsComException = false,
            bool exchangePrimarySmtpThrowsComException = false
        )
        {
            var propertyAccessor = new Mock<PropertyAccessor>();
            var addressEntry = new Mock<AddressEntry>();
            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();

            if (propertyAccessorThrows)
            {
                propertyAccessor
                    .Setup(x => x.GetProperty(SmtpAddressProperty))
                    .Throws(new InvalidOperationException("Property lookup failed."));
            }
            else
            {
                propertyAccessor
                    .Setup(x => x.GetProperty(SmtpAddressProperty))
                    .Returns(
                        hasPropertyAccessorValue ? (object)smtpAddressFromPropertyAccessor : null
                    );
            }

            addressEntry.SetupGet(x => x.AddressEntryUserType).Returns(userType);
            addressEntry.SetupGet(x => x.Name).Returns(name);

            if (
                userType == OlAddressEntryUserType.olExchangeUserAddressEntry
                || userType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry
            )
            {
                if (!hasExchangeUser)
                {
                    addressEntry.Setup(x => x.GetExchangeUser()).Returns((ExchangeUser)null);
                }
                else
                {
                    var exchangeUser = new Mock<ExchangeUser>();
                    if (exchangeNameThrowsComException)
                    {
                        exchangeUser
                            .SetupGet(x => x.FirstName)
                            .Throws(new System.Runtime.InteropServices.COMException("Boom"));
                        exchangeUser
                            .SetupGet(x => x.LastName)
                            .Throws(new System.Runtime.InteropServices.COMException("Boom"));
                    }
                    else
                    {
                        exchangeUser.SetupGet(x => x.FirstName).Returns(exchangeFirstName);
                        exchangeUser.SetupGet(x => x.LastName).Returns(exchangeLastName);
                    }

                    if (exchangePrimarySmtpThrowsComException)
                    {
                        exchangeUser
                            .SetupGet(x => x.PrimarySmtpAddress)
                            .Throws(new System.Runtime.InteropServices.COMException("Boom"));
                    }
                    else
                    {
                        exchangeUser
                            .SetupGet(x => x.PrimarySmtpAddress)
                            .Returns(exchangePrimarySmtpAddress);
                    }

                    addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);
                }
            }

            recipient.SetupGet(x => x.Name).Returns(name);
            recipient.SetupGet(x => x.Address).Returns(address);
            recipient.SetupGet(x => x.Type).Returns(type);
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            recipient.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            return recipient;
        }
    }
}
