using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses;
using OutlookMailItem = Microsoft.Office.Interop.Outlook.MailItem;
using OutlookMeetingItem = Microsoft.Office.Interop.Outlook.MeetingItem;

namespace UtilitiesCS.Test.OutlookObjects.MailItemCoverage
{
    [TestClass]
    public class EmailDetailsTests
    {
        [TestMethod]
        public void GetActionTaken_WhenMailIsMarkedAsTask_ReturnsTask()
        {
            // Arrange
            var mailItem = new Mock<OutlookMailItem>();
            mailItem.SetupGet(x => x.IsMarkedAsTask).Returns(true);

            // Act
            string result = EmailDetails.GetActionTaken(mailItem.Object);

            // Assert
            result.Should().Be("Task");
        }

        [DataTestMethod]
        [DataRow(102)]
        [DataRow(103)]
        [DataRow(104)]
        public void GetActionTaken_WhenLastVerbExecutedIsReplyOrForward_ReturnsActed(
            int lastVerbExecuted
        )
        {
            // Arrange
            var propertyAccessor = new Mock<PropertyAccessor>();
            var mailItem = new Mock<OutlookMailItem>();
            mailItem.SetupGet(x => x.IsMarkedAsTask).Returns(false);
            mailItem.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);
            propertyAccessor
                .Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10810003"))
                .Returns(lastVerbExecuted);

            // Act
            string result = EmailDetails.GetActionTaken(mailItem.Object);

            // Assert
            result.Should().Be("Acted");
        }

        [TestMethod]
        public void GetActionTaken_WhenVerbLookupThrowsOrReturnsUnknown_ReturnsNone()
        {
            // Arrange
            var throwingAccessor = new Mock<PropertyAccessor>();
            var throwingMailItem = new Mock<OutlookMailItem>();
            throwingMailItem.SetupGet(x => x.IsMarkedAsTask).Returns(false);
            throwingMailItem.SetupGet(x => x.PropertyAccessor).Returns(throwingAccessor.Object);
            throwingAccessor
                .Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10810003"))
                .Throws(new System.InvalidOperationException("No property"));

            var unknownAccessor = new Mock<PropertyAccessor>();
            var unknownMailItem = new Mock<OutlookMailItem>();
            unknownMailItem.SetupGet(x => x.IsMarkedAsTask).Returns(false);
            unknownMailItem.SetupGet(x => x.PropertyAccessor).Returns(unknownAccessor.Object);
            unknownAccessor
                .Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10810003"))
                .Returns(999);

            // Act / Assert
            EmailDetails.GetActionTaken(throwingMailItem.Object).Should().Be("None");
            EmailDetails.GetActionTaken(unknownMailItem.Object).Should().Be("None");
        }

        [TestMethod]
        public void GetTriage_WhenUserPropertyIsMissing_ReturnsEmptyStringForMailAndMeeting()
        {
            // Arrange
            var mailProperties = new Mock<UserProperties>();
            var meetingProperties = new Mock<UserProperties>();
            var mailItem = new Mock<OutlookMailItem>();
            var meetingItem = new Mock<OutlookMeetingItem>();

            mailItem.SetupGet(x => x.UserProperties).Returns(mailProperties.Object);
            meetingItem.SetupGet(x => x.UserProperties).Returns(meetingProperties.Object);
            mailProperties.Setup(x => x.Find("Triage", true)).Returns((UserProperty)null);
            meetingProperties.Setup(x => x.Find("Triage", true)).Returns((UserProperty)null);

            // Act / Assert
            mailItem.Object.GetTriage().Should().BeEmpty();
            meetingItem.Object.GetTriage().Should().BeEmpty();
        }

        [TestMethod]
        public void GetTriage_WhenUserPropertyExists_ReturnsStoredValueForMailAndMeeting()
        {
            // Arrange
            var mailProperty = new Mock<UserProperty>();
            var meetingProperty = new Mock<UserProperty>();
            var mailProperties = new Mock<UserProperties>();
            var meetingProperties = new Mock<UserProperties>();
            var mailItem = new Mock<OutlookMailItem>();
            var meetingItem = new Mock<OutlookMeetingItem>();

            mailProperty.SetupGet(x => x.Value).Returns("A");
            meetingProperty.SetupGet(x => x.Value).Returns("B");
            mailProperties.Setup(x => x.Find("Triage", true)).Returns(mailProperty.Object);
            meetingProperties.Setup(x => x.Find("Triage", true)).Returns(meetingProperty.Object);
            mailItem.SetupGet(x => x.UserProperties).Returns(mailProperties.Object);
            meetingItem.SetupGet(x => x.UserProperties).Returns(meetingProperties.Object);

            // Act / Assert
            mailItem.Object.GetTriage().Should().Be("A");
            meetingItem.Object.GetTriage().Should().Be("B");
        }

        [TestMethod]
        public void Details_WhenUsingMailItemHelper_ProjectsDeterministicFieldsAndAppliesFolderRemap()
        {
            // Arrange
            var helper = new TestMailItemHelper
            {
                Triage = "A",
                SentOn = "2026-03-14T9:30:00+00:00",
                Sender = new RecipientInfo("Ada Lovelace", "ada@example.com", "<a>Ada</a>"),
                Subject = "Weekly Report",
                Body = "Body text",
                ConversationID = "conversation-id",
                EntryId = "entry-id",
                Item = CreateTaskMailItem().Object,
            };

            var folderInfo = new Mock<IFolderWrapper>();
            folderInfo.SetupGet(x => x.RelativePath).Returns(@"Inbox\Projects");
            helper.FolderInfo = folderInfo.Object;
            helper.SetRecipients(
                [new RecipientInfo("Grace Hopper", "grace@example.com", null)],
                [new RecipientInfo("Alan Turing", "alan@example.com", null)]
            );
            helper.SetAttachments([CreateAttachment("report.pdf"), CreateAttachment("chart.png")]);
            var dictRemap = new ScoDictionary<string, string>(
                new Dictionary<string, string> { [@"Inbox\Projects"] = @"Archive\Projects" }
            );

            // Act
            string[] result = helper.Details(dictRemap);

            // Assert
            result.Length.Should().Be(14);
            result[1].Should().Be("A");
            result[2].Should().Be(@"Archive\Projects");
            result[3].Should().Be("2026-03-14T9:30:00+00:00");
            result[4].Should().Be("ada@example.com");
            result[5].Should().Be("grace@example.com");
            result[6].Should().Be("alan@example.com");
            result[7].Should().Be("Weekly Report");
            result[8].Should().Be("Body text");
            result[9].Should().Be("@example.com");
            result[10].Should().Be("conversation-id");
            result[11].Should().Be("entry-id");
            result[12].Should().Be("report.pdf; chart.png");
            result[13].Should().Be("Task");
        }

        private static Mock<OutlookMailItem> CreateTaskMailItem()
        {
            var mailItem = new Mock<OutlookMailItem>();
            mailItem.SetupGet(x => x.IsMarkedAsTask).Returns(true);
            return mailItem;
        }

        private static IAttachment CreateAttachment(string fileName)
        {
            var attachment = new Mock<IAttachment>();
            attachment.SetupProperty(x => x.FileName, fileName);
            return attachment.Object;
        }

        private sealed class TestMailItemHelper : MailItemHelper
        {
            public void SetRecipients(IRecipientInfo[] toRecipients, IRecipientInfo[] ccRecipients)
            {
                ToRecipients = toRecipients;
                CcRecipients = ccRecipients;
            }

            public void SetAttachments(IAttachment[] attachments)
            {
                AttachmentsInfo = attachments;
            }
        }
    }
}
