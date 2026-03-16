using System;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.AppointmentItemCoverage
{
    [TestClass]
    public class MeetingItemHelperTests
    {
        [TestMethod]
        public void ResolveFolderRoot_WhenFolderPathContainsArchiveRoot_ShouldReturnArchiveRoot()
        {
            var archiveRoot = new Mock<OutlookFolder>();
            var inbox = new Mock<OutlookFolder>();
            var globals = CreateGlobals(archiveRoot.Object, inbox.Object, "\\Archive");

            var result = MeetingItemHelper.ResolveFolderRoot(globals.Object, "\\Archive\\Projects");

            result.Should().BeSameAs(archiveRoot.Object);
        }

        [TestMethod]
        public void ResolveFolderRoot_WhenFolderPathDoesNotContainArchiveRoot_ShouldReturnInbox()
        {
            var archiveRoot = new Mock<OutlookFolder>();
            var inbox = new Mock<OutlookFolder>();
            var globals = CreateGlobals(archiveRoot.Object, inbox.Object, "\\Archive");

            var result = MeetingItemHelper.ResolveFolderRoot(globals.Object, "\\Inbox\\Actionable");

            result.Should().BeSameAs(inbox.Object);
        }

        [TestMethod]
        public void CompressPlainText_ShouldStripConfiguredSectionsAndAppendEndMarker()
        {
            const string text = "WARNING\r\nHello <https://example.test>\r\nFrom: Person\r\nSubject: Re: Status\r\nOlder content";

            var result = MeetingItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripWarning |
                IItemInfo.PlainTextOptionsEnum.StripLinks |
                IItemInfo.PlainTextOptionsEnum.StripReplyBody |
                IItemInfo.PlainTextOptionsEnum.StripFormatting,
                "WARNING");

            result.Should().StartWith("Hello");
            result.Should().NotContain("WARNING");
            result.Should().NotContain("https://example.test");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void ToSerializableObject_ShouldProjectCurrentFieldValues()
        {
            var sender = new Mock<IRecipientInfo>().Object;
            var cc = new[] { new Mock<IRecipientInfo>().Object };
            var to = new[] { new Mock<IRecipientInfo>().Object };
            var folderInfo = new Mock<IFolderWrapper>().Object;
            var helper = CreateHelper();
            SetLazyField(helper, "_actionable", "Respond");
            SetLazyField(helper, "_body", "Meeting summary");
            SetLazyField(helper, "_conversationID", "conv-42");
            SetLazyField(helper, "_emailPrefixToStrip", "WARNING");
            SetLazyField(helper, "_entryId", "entry");
            SetLazyField(helper, "_storeId", "store");
            SetLazyField(helper, "_folderName", "Calendar");
            SetLazyField(helper, "_folderInfo", folderInfo);
            SetLazyField(helper, "_html", "<html><head></head><body>Body</body></html>");
            SetLazyField(helper, "_htmlBody", "<html><head></head><body>Body</body></html>");
            SetLazyField(helper, "_internetCodepage", 65001);
            SetLazyField(helper, "_isTaskFlagSet", true);
            SetField(helper, "_plainTextOptions", IItemInfo.PlainTextOptionsEnum.StripAll);
            SetLazyField(helper, "_sender", sender);
            SetLazyField(helper, "_ccRecipients", cc);
            SetLazyField(helper, "_toRecipients", to);
            SetLazyField(helper, "_sentDate", new DateTime(2026, 5, 2));
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_size", 128);
            SetLazyField(helper, "_subject", "Planning");
            SetLazyField(helper, "_tokens", new[] { "planning", "meeting" });
            SetLazyField(helper, "_triage", "Now");
            SetLazyField(helper, "_unread", true);

            var result = helper.ToSerializableObject();

            result.Actionable.Should().Be("Respond");
            result.Body.Should().Be("Meeting summary");
            result.ConversationID.Should().Be("conv-42");
            result.EntryId.Should().Be("entry");
            result.StoreId.Should().Be("store");
            result.FolderName.Should().Be("Calendar");
            result.FolderInfo.Should().BeSameAs(folderInfo);
            result.Html.Should().Be("<html><head></head><body>Body</body></html>");
            result.Sender.Should().BeSameAs(sender);
            result.CcRecipients.Should().HaveCount(1);
            result.CcRecipients[0].Should().BeSameAs(cc[0]);
            result.ToRecipients.Should().HaveCount(1);
            result.ToRecipients[0].Should().BeSameAs(to[0]);
            result.Subject.Should().Be("Planning");
            result.Tokens.Should().Equal("planning", "meeting");
        }

        [TestMethod]
        public void ToggleDark_ShouldInjectAndThenRemoveDarkModeHeader()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_html", "<html><head></head><body>Body</body></html>");

            var darkHtml = helper.ToggleDark();
            var restoredHtml = helper.ToggleDark();

            darkHtml.Should().Contain("filter: invert(100%)");
            restoredHtml.Should().Be("<html><head></head><body>Body</body></html>");
        }

        [TestMethod]
        public void ToMatchableObject_ShouldProjectMatchRelevantFields()
        {
            var sender = new Mock<IRecipientInfo>().Object;
            var cc = new[] { new Mock<IRecipientInfo>().Object };
            var to = new[] { new Mock<IRecipientInfo>().Object };
            var helper = CreateHelper();
            SetLazyField(helper, "_sender", sender);
            SetLazyField(helper, "_ccRecipients", cc);
            SetLazyField(helper, "_toRecipients", to);
            SetLazyField(helper, "_subject", "Planning");
            SetLazyField(helper, "_body", "Summary");
            SetLazyField(helper, "_entryId", "entry");
            SetLazyField(helper, "_storeId", "store");
            SetLazyField(helper, "_sentDate", new DateTime(2026, 5, 2));
            SetLazyField(helper, "_size", 128);

            var result = helper.ToMatchableObject();

            result.Sender.Should().BeSameAs(sender);
            result.CcRecipients.Should().HaveCount(1);
            result.CcRecipients[0].Should().BeSameAs(cc[0]);
            result.ToRecipients.Should().HaveCount(1);
            result.ToRecipients[0].Should().BeSameAs(to[0]);
            result.Subject.Should().Be("Planning");
            result.Body.Should().Be("Summary");
            result.EntryId.Should().Be("entry");
            result.StoreId.Should().Be("store");
            result.SentDate.Should().Be(new DateTime(2026, 5, 2));
            result.Size.Should().Be(128);
        }

        private static Mock<IApplicationGlobals> CreateGlobals(OutlookFolder archiveRoot, OutlookFolder inbox, string archiveRootPath)
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot);
            olObjects.SetupGet(x => x.Inbox).Returns(inbox);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(archiveRootPath);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static MeetingItemHelper CreateHelper()
        {
#pragma warning disable SYSLIB0050
            return (MeetingItemHelper)FormatterServices.GetUninitializedObject(typeof(MeetingItemHelper));
#pragma warning restore SYSLIB0050
        }

        private static void SetField(MeetingItemHelper helper, string fieldName, object value)
        {
            var field = typeof(MeetingItemHelper).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingFieldException(typeof(MeetingItemHelper).FullName, fieldName);
            field.SetValue(helper, value);
        }

        private static void SetLazyField<T>(MeetingItemHelper helper, string fieldName, T value)
        {
            SetField(helper, fieldName, new Lazy<T>(() => value));
        }
    }
}