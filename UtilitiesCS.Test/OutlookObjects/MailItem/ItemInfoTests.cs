using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.OutlookObjects.MailItemCoverage
{
    [TestClass]
    public class ItemInfoTests
    {
        [TestMethod]
        public void Constructor_WithNoArguments_LeavesPropertiesAtDefaults()
        {
            // Arrange
            var itemInfo = new ItemInfo();

            // Assert
            itemInfo.Actionable.Should().BeNull();
            itemInfo.AttachmentsInfo.Should().BeNull();
            itemInfo.Body.Should().BeNull();
            itemInfo.Categories.Should().BeNull();
            itemInfo.ConversationID.Should().BeNull();
            itemInfo.EntryId.Should().BeNull();
            itemInfo.StoreId.Should().BeNull();
            itemInfo.FolderName.Should().BeNull();
            itemInfo.FolderInfo.Should().BeNull();
            itemInfo.Html.Should().BeNull();
            itemInfo.HTMLBody.Should().BeNull();
            itemInfo.InternetCodepage.Should().Be(0);
            itemInfo.IsTaskFlagSet.Should().BeFalse();
            itemInfo.PlainTextOptions.Should().Be(default(IItemInfo.PlainTextOptionsEnum));
            itemInfo.Size.Should().Be(0);
            itemInfo.Sender.Should().BeNull();
            itemInfo.CcRecipients.Should().BeNull();
            itemInfo.ToRecipients.Should().BeNull();
            itemInfo.SentDate.Should().Be(default);
            itemInfo.SentOn.Should().BeNull();
            itemInfo.Subject.Should().BeNull();
            itemInfo.Tokens.Should().BeNull();
            itemInfo.Triage.Should().BeNull();
            itemInfo.UnRead.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_WithItemInfo_CopiesMappedProperties()
        {
            // Arrange
            var sender = new RecipientInfo("Sender", "sender@example.com", "<p>sender</p>");
            var toRecipients = new IRecipientInfo[] { new RecipientInfo("To", "to@example.com", null) };
            var ccRecipients = new IRecipientInfo[] { new RecipientInfo("Cc", "cc@example.com", null) };
            var attachments = new[] { Mock.Of<IAttachment>() };
            var folderInfo = Mock.Of<IFolderWrapper>();
            var source = new Mock<IItemInfo>(MockBehavior.Strict);
            source.SetupGet(x => x.Actionable).Returns("Yes");
            source.SetupGet(x => x.AttachmentsInfo).Returns(attachments);
            source.SetupGet(x => x.Body).Returns("Body");
            source.SetupGet(x => x.Categories).Returns("Blue");
            source.SetupGet(x => x.ConversationID).Returns("conversation");
            source.SetupGet(x => x.EmailPrefixToStrip).Returns("RE:");
            source.SetupGet(x => x.EntryId).Returns("entry-id");
            source.SetupGet(x => x.StoreId).Returns("store-id");
            source.SetupGet(x => x.FolderName).Returns("Inbox");
            source.SetupGet(x => x.FolderInfo).Returns(folderInfo);
            source.SetupGet(x => x.Html).Returns("<b>Body</b>");
            source.SetupGet(x => x.HTMLBody).Returns("<html>Body</html>");
            source.SetupGet(x => x.InternetCodepage).Returns(65001);
            source.SetupGet(x => x.IsTaskFlagSet).Returns(true);
            source.SetupGet(x => x.PlainTextOptions).Returns(IItemInfo.PlainTextOptionsEnum.StripLinks);
            source.SetupGet(x => x.Size).Returns(42);
            source.SetupGet(x => x.Sender).Returns(sender);
            source.SetupGet(x => x.CcRecipients).Returns(ccRecipients);
            source.SetupGet(x => x.ToRecipients).Returns(toRecipients);
            source.SetupGet(x => x.SentDate).Returns(new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc));
            source.SetupGet(x => x.SentOn).Returns("Friday");
            source.SetupGet(x => x.Subject).Returns("Subject");
            source.SetupGet(x => x.Tokens).Returns(new[] { "alpha", "beta" });
            source.SetupGet(x => x.Triage).Returns("Later");
            source.SetupGet(x => x.UnRead).Returns(true);

            // Act
            var itemInfo = new ItemInfo(source.Object);

            // Assert
            itemInfo.Actionable.Should().Be("Yes");
            itemInfo.AttachmentsInfo.Should().BeSameAs(attachments);
            itemInfo.Body.Should().Be("Body");
            itemInfo.Categories.Should().Be("Blue");
            itemInfo.ConversationID.Should().Be("conversation");
            itemInfo.EmailPrefixToStrip.Should().Be("RE:");
            itemInfo.EntryId.Should().Be("entry-id");
            itemInfo.StoreId.Should().Be("store-id");
            itemInfo.FolderName.Should().Be("Inbox");
            itemInfo.FolderInfo.Should().BeSameAs(folderInfo);
            itemInfo.Html.Should().Be("<b>Body</b>");
            itemInfo.HTMLBody.Should().Be("<html>Body</html>");
            itemInfo.InternetCodepage.Should().Be(65001);
            itemInfo.IsTaskFlagSet.Should().BeTrue();
            itemInfo.PlainTextOptions.Should().Be(IItemInfo.PlainTextOptionsEnum.StripLinks);
            itemInfo.Size.Should().Be(42);
            itemInfo.Sender.Should().BeSameAs(sender);
            itemInfo.CcRecipients.Should().BeSameAs(ccRecipients);
            itemInfo.ToRecipients.Should().BeSameAs(toRecipients);
            itemInfo.SentDate.Should().Be(new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc));
            itemInfo.SentOn.Should().Be("Friday");
            itemInfo.Subject.Should().Be("Subject");
            itemInfo.Tokens.Should().Equal("alpha", "beta");
            itemInfo.Triage.Should().Be("Later");
            itemInfo.UnRead.Should().BeTrue();
            itemInfo.Sw.Should().BeNull();
        }

        [TestMethod]
        public void Equals_ShouldReturnTrue_WhenCoreFieldsAndRecipientsMatch()
        {
            // Arrange
            var sentDate = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc);
            var left = CreateItemInfo(sentDate, "Subject", "Body", "sender@example.com", new[] { "to@example.com" }, new[] { "cc@example.com" });
            var right = CreateItemInfo(sentDate, "Subject", "Body", "sender@example.com", new[] { "to@example.com" }, new[] { "cc@example.com" });

            // Act
            bool result = left.Equals(right);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_ShouldReturnFalse_WhenOtherIsNullOrAnyCoreFieldDiffers()
        {
            // Arrange
            var sentDate = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc);
            var baseline = CreateItemInfo(sentDate, "Subject", "Body", "sender@example.com", new[] { "to@example.com" }, new[] { "cc@example.com" });
            var differentSubject = CreateItemInfo(sentDate, "Other", "Body", "sender@example.com", new[] { "to@example.com" }, new[] { "cc@example.com" });
            var differentRecipients = CreateItemInfo(sentDate, "Subject", "Body", "sender@example.com", new[] { "different@example.com" }, new[] { "cc@example.com" });

            // Act / Assert
            baseline.Equals(null).Should().BeFalse();
            baseline.Equals(differentSubject).Should().BeFalse();
            baseline.Equals(differentRecipients).Should().BeFalse();
        }

        [TestMethod]
        public void GetHashCode_ShouldMatchForEquivalentItems_AndChangeWhenSubjectDiffers()
        {
            // Arrange
            var sentDate = new DateTime(2026, 3, 13, 12, 0, 0, DateTimeKind.Utc);
            var baseline = CreateItemInfo(sentDate, "Subject", "Body", "sender@example.com", new[] { "to@example.com" }, new[] { "cc@example.com" });
            var equivalent = CreateItemInfo(sentDate, "Subject", "Body", "sender@example.com", new[] { "to@example.com" }, new[] { "cc@example.com" });
            var differentSubject = CreateItemInfo(sentDate, "Other", "Body", "sender@example.com", new[] { "to@example.com" }, new[] { "cc@example.com" });

            // Act
            int baselineHash = baseline.GetHashCode();
            int equivalentHash = equivalent.GetHashCode();
            int differentSubjectHash = differentSubject.GetHashCode();

            // Assert
            baselineHash.Should().Be(equivalentHash);
            differentSubjectHash.Should().NotBe(baselineHash);
        }

        private static ItemInfo CreateItemInfo(
            DateTime sentDate,
            string subject,
            string body,
            string senderAddress,
            string[] toRecipients,
            string[] ccRecipients)
        {
            return new ItemInfo
            {
                SentDate = sentDate,
                Subject = subject,
                Body = body,
                Sender = new RecipientInfo("Sender", senderAddress, null),
                ToRecipients = Array.ConvertAll(toRecipients, address => (IRecipientInfo)new RecipientInfo(address, address, null)),
                CcRecipients = Array.ConvertAll(ccRecipients, address => (IRecipientInfo)new RecipientInfo(address, address, null)),
            };
        }
    }
}