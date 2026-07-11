using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.ReusableTypeClasses;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookMailItem = Microsoft.Office.Interop.Outlook.MailItem;
using OutlookRecipient = Microsoft.Office.Interop.Outlook.Recipient;

namespace UtilitiesCS.Test.OutlookObjects.MailItemCoverage
{
    [TestClass]
    public class EmailDetailsWrapperTests
    {
        [TestMethod]
        public void ProjectionHelpers_WhenMailHasBasicValues_ReturnExpectedSenderActionAndTriageData()
        {
            // Arrange
            var wrapper = new EmailDetailsWrapper();
            var mailItem = new Mock<OutlookMailItem>();
            var sender = new Mock<AddressEntry>();
            var triageProperty = new Mock<UserProperty>();
            var userProperties = new Mock<UserProperties>();

            mailItem.SetupGet(x => x.IsMarkedAsTask).Returns(true);
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Lovelace");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.Sender).Returns(sender.Object);
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            userProperties.Setup(x => x.Find("Triage", true)).Returns(triageProperty.Object);
            triageProperty.SetupGet(x => x.Value).Returns("A");

            // Act / Assert
            wrapper.GetActionTaken(mailItem.Object).Should().Be("Task");
            wrapper.GetSenderName(mailItem.Object).Should().Be("Ada Lovelace");
            wrapper.GetSenderAddress(mailItem.Object).Should().Be("ada@example.com");
            wrapper.GetTriage(mailItem.Object).Should().Be("A");

            IRecipientInfo senderInfo = wrapper.GetSenderInfo(mailItem.Object);
            senderInfo.Name.Should().Be("Ada Lovelace");
            senderInfo.Address.Should().Be("ada@example.com");
            senderInfo
                .Html.Should()
                .Be("Ada Lovelace &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;");
        }

        [TestMethod]
        public void GetRecipients_WhenMailRecipientsAreNull_ReturnsEmptyStrings()
        {
            // Arrange
            var wrapper = new EmailDetailsWrapper();
            var mailItem = new Mock<OutlookMailItem>();
            mailItem.SetupGet(x => x.Recipients).Returns((Recipients)null);

            // Act
            (string recipientsTo, string recipientsCC) result = wrapper.GetRecipients(
                mailItem.Object
            );

            // Assert
            result.recipientsTo.Should().BeEmpty();
            result.recipientsCC.Should().BeEmpty();
        }

        [TestMethod]
        public void RecipientHelpers_FilterProjectAndConvertRecipients()
        {
            // Arrange
            var wrapper = new EmailDetailsWrapper();
            var toRecipient = CreateRecipient(
                "Grace Hopper",
                "grace@example.com",
                OlMailRecipientType.olTo
            );
            var ccRecipient = CreateRecipient(
                "Alan Turing",
                "alan@example.com",
                OlMailRecipientType.olCC
            );
            var recipients = CreateRecipientsCollection(toRecipient.Object, ccRecipient.Object);
            var mailItem = new Mock<OutlookMailItem>();
            mailItem.SetupGet(x => x.Recipients).Returns(recipients.Object);

            // Act
            (string recipientsTo, string recipientsCC) recipientsText = wrapper.GetRecipients(
                mailItem.Object
            );
            OutlookRecipient[] toRecipients = wrapper.GetToRecipients(mailItem.Object).ToArray();
            OutlookRecipient[] ccRecipients = wrapper.GetCcRecipients(mailItem.Object).ToArray();
            RecipientInfo[] infos = wrapper
                .GetInfo(new[] { toRecipient.Object, ccRecipient.Object })
                .ToArray();
            RecipientInfo singleInfo = wrapper.GetInfo(toRecipient.Object);

            // Assert
            recipientsText.recipientsTo.Should().Be("grace@example.com");
            recipientsText.recipientsCC.Should().Be("alan@example.com");
            toRecipients.Should().Equal(toRecipient.Object);
            ccRecipients.Should().Equal(ccRecipient.Object);
            infos.Select(x => x.Address).Should().Equal("grace@example.com", "alan@example.com");
            singleInfo.Name.Should().Be("Grace Hopper");
            singleInfo.Address.Should().Be("grace@example.com");
        }

        [TestMethod]
        public void Details_WhenMailHasDeterministicValues_ProjectsArrayAndAppliesFolderRemap()
        {
            // Arrange
            var wrapper = new EmailDetailsWrapper();
            var sender = new Mock<AddressEntry>();
            var triageProperty = new Mock<UserProperty>();
            var userProperties = new Mock<UserProperties>();
            var attachments = new Mock<Attachments>();
            var parentFolder = new Mock<OutlookFolder>();
            var toRecipient = CreateRecipient(
                "Grace Hopper",
                "grace@example.com",
                OlMailRecipientType.olTo
            );
            var ccRecipient = CreateRecipient(
                "Alan Turing",
                "alan@example.com",
                OlMailRecipientType.olCC
            );
            var recipients = CreateRecipientsCollection(toRecipient.Object, ccRecipient.Object);
            var mailItem = new Mock<OutlookMailItem>();

            mailItem.SetupGet(x => x.IsMarkedAsTask).Returns(true);
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Lovelace");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.Sender).Returns(sender.Object);
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);
            mailItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            mailItem.SetupGet(x => x.SentOn).Returns(new DateTime(2026, 3, 14, 9, 30, 0));
            mailItem.SetupGet(x => x.Subject).Returns("Weekly Report");
            mailItem.SetupGet(x => x.Body).Returns("Body text");
            mailItem.SetupGet(x => x.ConversationID).Returns("conversation-id");
            mailItem.SetupGet(x => x.EntryID).Returns("entry-id");
            mailItem.SetupGet(x => x.Attachments).Returns(attachments.Object);
            mailItem.SetupGet(x => x.Parent).Returns(parentFolder.Object);

            userProperties.Setup(x => x.Find("Triage", true)).Returns(triageProperty.Object);
            triageProperty.SetupGet(x => x.Value).Returns("A");
            attachments.SetupGet(x => x.Count).Returns(0);
            parentFolder.SetupGet(x => x.FolderPath).Returns("\\\\Inbox\\Projects");

            var dictRemap = new ScoDictionaryNew<string, string>(
                new Dictionary<string, string> { ["Projects"] = "Archive Projects" }
            );

            // Act
            string[] result = wrapper.Details(mailItem.Object, "\\\\Inbox", dictRemap);

            // Assert
            result[1].Should().Be("A");
            result[2].Should().Be("Archive Projects");
            result[4].Should().Be("ada@example.com");
            result[5].Should().Be("grace@example.com");
            result[6].Should().Be("alan@example.com");
            result[7].Should().Be("Weekly Report");
            result[8].Should().Be("Body text");
            result[9].Should().Be("@example.com");
            result[10].Should().Be("conversation-id");
            result[11].Should().Be("entry-id");
            result[12].Should().BeEmpty();
            result[13].Should().Be("Task");
        }

        private static Mock<OutlookRecipient> CreateRecipient(
            string name,
            string address,
            OlMailRecipientType recipientType
        )
        {
            var recipient = new Mock<OutlookRecipient>();
            var addressEntry = new Mock<AddressEntry>();

            recipient.SetupGet(x => x.Name).Returns(name);
            recipient.SetupGet(x => x.Address).Returns(address);
            recipient.SetupGet(x => x.Type).Returns((int)recipientType);
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            addressEntry
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);

            return recipient;
        }

        private static Mock<Recipients> CreateRecipientsCollection(
            params OutlookRecipient[] recipients
        )
        {
            var collection = new ArrayList(recipients);
            var recipientsCollection = new Mock<Recipients>();
            recipientsCollection
                .Setup(x => x.GetEnumerator())
                .Returns(() => collection.GetEnumerator());
            return recipientsCollection;
        }
    }
}
