using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.HelperClasses;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookRecipient = Microsoft.Office.Interop.Outlook.Recipient;

namespace UtilitiesCS.Test.OutlookObjects.AppointmentItemCoverage
{
    [TestClass]
    public partial class MeetingItemHelperTests
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
            const string text =
                "WARNING\r\nHello <https://example.test>\r\nFrom: Person\r\nSubject: Re: Status\r\nOlder content";

            var result = MeetingItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripWarning
                    | IItemInfo.PlainTextOptionsEnum.StripLinks
                    | IItemInfo.PlainTextOptionsEnum.StripReplyBody
                    | IItemInfo.PlainTextOptionsEnum.StripFormatting,
                "WARNING"
            );

            result.Should().StartWith("Hello");
            result.Should().NotContain("WARNING");
            result.Should().NotContain("https://example.test");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_WithNullInput_ReturnsEndMarkerOnly()
        {
            var result = MeetingItemHelper.CompressPlainText(null, string.Empty);

            result.Should().Be(" <EOM>");
        }

        [TestMethod]
        public void CompressPlainText_WithShowStrippedLinks_ReplacesLinksWithPlaceholder()
        {
            var result = MeetingItemHelper.CompressPlainText(
                "Review <https://example.test> now",
                IItemInfo.PlainTextOptionsEnum.StripLinks
                    | IItemInfo.PlainTextOptionsEnum.ShowStripped,
                string.Empty
            );

            result.Should().Contain("<link>");
            result.Should().NotContain("https://example.test");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_WithReplyHeaderPreserved_KeepsHeaderAndAppendsEndMarker()
        {
            const string text =
                "Intro\r\nFrom: Person\r\nSent: Friday\r\nTo: Team\r\nSubject: Re: Status\r\nOlder content";

            var result = MeetingItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripReplyBody,
                string.Empty
            );

            result.Should().Contain("From: Person");
            result.Should().Contain("Subject: Re: Status");
            result.Should().NotContain("Older content");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void Constructor_WithoutArguments_ShouldInitializeAttachmentsInfoLazyField()
        {
            var helper = new MeetingItemHelper();

            GetPrivateFieldValue(helper, "_attachmentsInfo").Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_WithMeetingItemAndGlobals_ShouldInitializeLazyFields()
        {
            var meetingItem = new Mock<MeetingItem>();
            var globals = CreateGlobals(
                new Mock<OutlookFolder>().Object,
                new Mock<OutlookFolder>().Object,
                "\\Archive"
            );

            var helper = new MeetingItemHelper(meetingItem.Object, globals.Object);

            GetPrivateFieldValue(helper, "_globals").Should().NotBeNull();
            GetPrivateFieldValue(helper, "_entryId").Should().NotBeNull();
            GetPrivateFieldValue(helper, "_appointment").Should().NotBeNull();
        }

        [TestMethod]
        public void ProtectedConstructor_WithItemInfo_ShouldSurfaceCurrentUnreadProjectionBehavior()
        {
            var sender = new RecipientInfo("Ada Lovelace", "ada@example.com", "Ada Html");
            var ccRecipients = new IRecipientInfo[]
            {
                new RecipientInfo("Alan Turing", "alan@example.com", "Alan Html"),
            };
            var toRecipients = new IRecipientInfo[]
            {
                new RecipientInfo("Grace Hopper", "grace@example.com", "Grace Html"),
            };
            var attachmentsInfo = Array.Empty<IAttachment>();
            var folderInfo = new Mock<IFolderWrapper>().Object;
            var itemInfo = new Mock<IItemInfo>();
            itemInfo.SetupGet(x => x.Actionable).Returns("Respond");
            itemInfo.SetupGet(x => x.Body).Returns("Meeting summary");
            itemInfo.SetupGet(x => x.ConversationID).Returns("conversation-id");
            itemInfo.SetupGet(x => x.EmailPrefixToStrip).Returns("WARNING");
            itemInfo.SetupGet(x => x.EntryId).Returns("entry-id");
            itemInfo.SetupGet(x => x.StoreId).Returns("store-id");
            itemInfo.SetupGet(x => x.FolderName).Returns("Calendar");
            itemInfo.SetupGet(x => x.FolderInfo).Returns(folderInfo);
            itemInfo.SetupGet(x => x.Html).Returns("<html></html>");
            itemInfo.SetupGet(x => x.IsTaskFlagSet).Returns(true);
            itemInfo
                .SetupGet(x => x.PlainTextOptions)
                .Returns(IItemInfo.PlainTextOptionsEnum.StripAll);
            itemInfo.SetupGet(x => x.Sender).Returns(sender);
            itemInfo.SetupGet(x => x.CcRecipients).Returns(ccRecipients);
            itemInfo.SetupGet(x => x.ToRecipients).Returns(toRecipients);
            itemInfo.SetupGet(x => x.SentDate).Returns(new DateTime(2026, 5, 2));
            itemInfo.SetupGet(x => x.SentOn).Returns("5/2/2026 9:30 AM");
            itemInfo.SetupGet(x => x.Subject).Returns("Planning");
            itemInfo.SetupGet(x => x.Tokens).Returns(new[] { "planning", "meeting" });
            itemInfo.SetupGet(x => x.Triage).Returns("Now");
            itemInfo.SetupGet(x => x.UnRead).Returns(true);
            itemInfo.SetupGet(x => x.AttachmentsInfo).Returns(attachmentsInfo);

            System.Action act = () => _ = new MeetingItemHelperCopyProbe(itemInfo.Object);

            act.Should().Throw<NullReferenceException>();
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
        public void ToggleDark_WhenAlreadyOn_DoesNotDuplicateDarkModeHeader()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_html", "<html><head></head><body>Body</body></html>");

            var initialDarkHtml = helper.ToggleDark(Enums.ToggleState.On);
            var repeatedDarkHtml = helper.ToggleDark(Enums.ToggleState.On);

            repeatedDarkHtml.Should().Be(initialDarkHtml);
            CountOccurrences(repeatedDarkHtml, helper.DarkModeHeader).Should().Be(1);
        }

        [TestMethod]
        public void ToggleDark_WhenAlreadyOff_LeavesHtmlUnchanged()
        {
            const string html = "<html><head></head><body>Body</body></html>";
            var helper = CreateHelper();
            SetLazyField(helper, "_html", html);

            var result = helper.ToggleDark(Enums.ToggleState.Off);

            result.Should().Be(html);
        }

        [TestMethod]
        public void SetSender_ShouldPopulateSenderCaches()
        {
            var helper = CreateHelper();
            var sender = new RecipientInfo(
                "Ada Lovelace",
                "ada@example.com",
                "Ada &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;"
            );

            helper.SetSender(sender);

            helper.Sender.Should().BeSameAs(sender);
            helper.SenderName.Should().Be("Ada Lovelace");
            helper
                .SenderHtml.Should()
                .Be("Ada &lt;<a href=\"mailto:ada@example.com\">ada@example.com</a>&gt;");
        }

        [TestMethod]
        public void GetHtml_ShouldInjectEmailHeaderInsideBodyTag()
        {
            var meetingItem = new Mock<MeetingItem>();
            meetingItem
                .SetupGet(x => x.RTFBody)
                .Returns("<html><head></head><body>Original</body></html>");
            var helper = CreateHelper();
            SetField(helper, "_item", meetingItem.Object);
            SetLazyField(helper, "_senderHtml", "Sender");
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_toRecipientsHtml", "To User");
            SetLazyField(helper, "_ccRecipientsHtml", "Cc User");
            SetLazyField(helper, "_subject", "Planning");

            var result = helper.GetHtml("ignored");

            result.Should().Contain("<body>\r\n    <div>");
            result.Should().Contain("<b>From:</b>Sender");
            result.Should().Contain("Original");
        }

        [TestMethod]
        public void GetHtml_WithoutArgument_ShouldInjectEmailHeaderInsideBodyTag()
        {
            var meetingItem = new Mock<MeetingItem>();
            meetingItem
                .SetupGet(x => x.RTFBody)
                .Returns("<html><head></head><body>Original</body></html>");
            var helper = CreateHelper();
            SetField(helper, "_item", meetingItem.Object);
            SetLazyField(helper, "_senderHtml", "Sender");
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_toRecipientsHtml", "To User");
            SetLazyField(helper, "_ccRecipientsHtml", "Cc User");
            SetLazyField(helper, "_subject", "Planning");
            helper.Sw = new SegmentStopWatch().Start();

            var result = helper.GetHtml();

            result.Should().Contain("<body>\r\n    <div>");
            result.Should().Contain("<b>Subject:</b>Planning");
            result.Should().Contain("Original");
        }

        [TestMethod]
        public void LoadRecipients_ShouldPopulateToAndCcRecipientFields()
        {
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
            var meetingItem = new Mock<MeetingItem>();
            meetingItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            var helper = CreateHelper();
            SetField(helper, "_item", meetingItem.Object);

            helper.LoadRecipients();

            helper.ToRecipients.Should().ContainSingle();
            helper.ToRecipients[0].Name.Should().Be("Grace Hopper");
            helper.ToRecipientsName.Should().Be("Grace Hopper");
            helper.ToRecipientsHtml.Should().Contain("mailto:grace@example.com");
            helper.CcRecipients.Should().ContainSingle();
            helper.CcRecipients[0].Name.Should().Be("Alan Turing");
            helper.CcRecipientsName.Should().Be("Alan Turing");
            helper.CcRecipientsHtml.Should().Contain("mailto:alan@example.com");
        }

        [TestMethod]
        public void LoadAll_WhenItemIsNull_ShouldThrowArgumentNullException()
        {
            var helper = CreateHelper();
            var globals = CreateGlobals(
                new Mock<OutlookFolder>().Object,
                new Mock<OutlookFolder>().Object,
                "\\Archive"
            );
            var root = new Mock<OutlookFolder>();

            System.Action act = () => helper.LoadAll(globals.Object, root.Object);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void LoadAll_WhenLoadTokensIsRequested_ShouldInitializePriorityRecipientsAndTokens()
        {
            var fixture = CreateConfiguredLoadAllFixture();
            var tokenizer = new Mock<IEmailTokenizer>();
            tokenizer
                .Setup(x => x.Tokenize(It.IsAny<IItemInfo>()))
                .Returns(new[] { "planning", "meeting" });
            SetField(fixture.Helper, "_tokenizer", tokenizer.Object);

            var result = fixture.Helper.LoadAll(
                fixture.Globals.Object,
                fixture.ArchiveRoot.Object,
                true
            );

            result.Should().BeSameAs(fixture.Helper);
            fixture.Helper.FolderInfo.OlRoot.Should().BeSameAs(fixture.ArchiveRoot.Object);
            fixture.Helper.SenderName.Should().Be("Ada Lovelace");
            fixture.Helper.ToRecipientsName.Should().Be("Grace Hopper");
            fixture.Helper.CcRecipientsName.Should().Be("Alan Turing");
            fixture.Helper.Body.Should().EndWith("<EOM>");
            fixture.Helper.Tokens.Should().Equal("planning", "meeting");
        }

        [TestMethod]
        public void EmailHeader2_ShouldIncludeProjectedTextFields()
        {
            var helper = CreateHelper();
            SetLazyField(helper, "_senderName", "Ada Lovelace");
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_toRecipientsName", "Grace Hopper");
            SetLazyField(helper, "_subject", "Planning");

            var result = helper.EmailHeader2;

            result.Should().Contain("From:");
            result.Should().Contain("Ada Lovelace");
            result.Should().Contain("Grace Hopper");
            result.Should().Contain("Planning");
        }
    }
}
