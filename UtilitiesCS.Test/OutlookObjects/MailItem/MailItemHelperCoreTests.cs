using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.EmailParsing;
using InteropMailItem = Microsoft.Office.Interop.Outlook.MailItem;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.MailItem
{
    [TestClass]
    public class MailItemHelperCoreTests
    {
        [TestMethod]
        public void ResolveFolderRoot_WhenFolderPathContainsArchiveRoot_ShouldReturnArchiveRoot()
        {
            var archiveRoot = new Mock<OutlookFolder>();
            var inbox = new Mock<OutlookFolder>();
            var globals = CreateGlobals(archiveRoot.Object, inbox.Object, "\\Archive");

            var result = MailItemHelper.ResolveFolderRoot(globals.Object, "\\Archive\\Projects");

            result.Should().BeSameAs(archiveRoot.Object);
        }

        [TestMethod]
        public void CompressPlainText_ShouldStripConfiguredSectionsAndAppendEndMarker()
        {
            const string text =
                "WARNING\r\nHello <https://example.test>\r\nFrom: Person\r\nSubject: Re: Status\r\nOlder content";

            var result = MailItemHelper.CompressPlainText(
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
        public void GetHtml_ShouldInjectEmailHeaderIntoBodyMarkup()
        {
            var mailItem = new Mock<InteropMailItem>();
            mailItem
                .SetupGet(x => x.HTMLBody)
                .Returns("<html><head></head><body>Original</body></html>");
            var helper = CreateHelper();
            SetField(helper, "_item", mailItem.Object);
            SetLazyField(helper, "_html", "<html><head></head><body>Original</body></html>");
            SetLazyField(helper, "_senderHtml", "Sender");
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_toRecipientsHtml", "To User");
            SetLazyField(helper, "_ccRecipientsHtml", "Cc User");
            SetLazyField(helper, "_subject", "Planning");

            var result = helper.GetHtml("ignored");

            result.Should().Contain("<b>From:</b>Sender");
            result.Should().Contain("Original");
        }

        [TestMethod]
        public void GetHtml_ShouldRewriteCidReferenceToVirtualHostUrl_WhenAttachmentContentIdMatches()
        {
            var mailItem = new Mock<InteropMailItem>();
            mailItem
                .SetupGet(x => x.HTMLBody)
                .Returns("<html><head></head><body><img src=\"cid:logo1\"></body></html>");
            var helper = CreateHelper();
            SetField(helper, "_item", mailItem.Object);
            SetLazyField(helper, "_senderHtml", "Sender");
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_toRecipientsHtml", "To User");
            SetLazyField(helper, "_ccRecipientsHtml", "Cc User");
            SetLazyField(helper, "_subject", "Planning");
            SetLazyField(
                helper,
                "_attachmentsInfo",
                new IAttachment[] { new AttachmentSerializable() { ContentId = "logo1" } }
            );

            var result = helper.GetHtml();

            result.Should().Contain("src=\"https://cid.quickfiler.local/logo1\"");
        }

        [TestMethod]
        public void RecipientsEquivalent_ShouldHandleNullAndMismatchedArrays()
        {
            var helper = CreateHelper();
            var recipient = new Mock<IRecipientInfo>().Object;

            helper.RecipientsEquivalent(null, null).Should().BeTrue();
            helper.RecipientsEquivalent(new[] { recipient }, null).Should().BeFalse();
            helper
                .RecipientsEquivalent(new[] { recipient }, Array.Empty<IRecipientInfo>())
                .Should()
                .BeFalse();
        }

        [TestMethod]
        public void CompressPlainText_collapses_runs_of_whitespace()
        {
            var result = MailItemHelper.CompressPlainText("a   b\r\n\r\n c", string.Empty);
            result.Should().Contain("a b c");
        }

        [TestMethod]
        public void CompressPlainText_returns_safe_value_for_null_or_empty_input()
        {
            MailItemHelper.CompressPlainText(null, string.Empty).Should().NotBeNull();
            MailItemHelper.CompressPlainText(string.Empty, string.Empty).Should().NotBeNull();
        }

        [TestMethod]
        public async Task FromMailItemAsync_MaterializesTokenizationDependenciesBeforeBackgroundTokenAccess()
        {
            var mailItem = new Mock<InteropMailItem>();
            var archiveRoot = new Mock<OutlookFolder>();
            var inbox = new Mock<OutlookFolder>();
            var globals = CreateGlobals(archiveRoot.Object, inbox.Object, "\\Archive");
            var sender = CreateSenderMock("Ada Sender", "ada@example.com");
            var toRecipient = CreateRecipientMock(
                "To User",
                "to@example.com",
                (int)OlMailRecipientType.olTo
            );
            var ccRecipient = CreateRecipientMock(
                "Cc User",
                "cc@example.com",
                (int)OlMailRecipientType.olCC
            );
            var recipients = CreateRecipientsMock(toRecipient.Object, ccRecipient.Object);
            var attachments = CreateAttachmentsMock();

            var subjectReads = 0;
            var bodyReads = 0;
            var htmlBodyReads = 0;
            var senderReads = 0;
            var recipientsReads = 0;
            var attachmentsReads = 0;

            mailItem.SetupGet(x => x.Subject).Callback(() => subjectReads++).Returns("Subject");
            mailItem.SetupGet(x => x.Body).Callback(() => bodyReads++).Returns("Body");
            mailItem
                .SetupGet(x => x.HTMLBody)
                .Callback(() => htmlBodyReads++)
                .Returns("<html><body>Body</body></html>");
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Sender");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.EntryID).Returns("entry-1");
            mailItem.SetupGet(x => x.Sender).Callback(() => senderReads++).Returns(sender.Object);
            mailItem
                .SetupGet(x => x.Recipients)
                .Callback(() => recipientsReads++)
                .Returns(recipients.Object);
            mailItem
                .SetupGet(x => x.Attachments)
                .Callback(() => attachmentsReads++)
                .Returns(attachments.Object);

            var helper = await MailItemHelper.FromMailItemAsync(
                mailItem.Object,
                globals.Object,
                CancellationToken.None,
                loadAll: false
            );

            subjectReads.Should().BeGreaterThan(0);
            bodyReads.Should().BeGreaterThan(0);
            htmlBodyReads.Should().BeGreaterThan(0);
            senderReads.Should().BeGreaterThan(0);
            recipientsReads.Should().BeGreaterThan(0);
            attachmentsReads.Should().BeGreaterThan(0);

            var subjectReadsAfterMaterialization = subjectReads;
            var bodyReadsAfterMaterialization = bodyReads;
            var htmlBodyReadsAfterMaterialization = htmlBodyReads;
            var senderReadsAfterMaterialization = senderReads;
            var recipientsReadsAfterMaterialization = recipientsReads;
            var attachmentsReadsAfterMaterialization = attachmentsReads;

            var tokenizer = new Mock<IEmailTokenizer>();
            tokenizer
                .Setup(x => x.Tokenize(It.IsAny<IItemInfo>()))
                .Returns(
                    (IItemInfo info) =>
                    {
                        _ = info.Subject;
                        _ = info.Body;
                        _ = info.HTMLBody;
                        _ = info.Sender;
                        _ = info.ToRecipients;
                        _ = info.CcRecipients;
                        _ = info.AttachmentsInfo;
                        return new[] { "token" };
                    }
                );
            SetField(helper, "_tokenizer", tokenizer.Object);

            var tokens = await Task.Run(() => helper.Tokens);

            tokens.Should().Equal("token");
            subjectReads.Should().Be(subjectReadsAfterMaterialization);
            bodyReads.Should().Be(bodyReadsAfterMaterialization);
            htmlBodyReads.Should().Be(htmlBodyReadsAfterMaterialization);
            senderReads.Should().Be(senderReadsAfterMaterialization);
            recipientsReads.Should().Be(recipientsReadsAfterMaterialization);
            attachmentsReads.Should().Be(attachmentsReadsAfterMaterialization);
        }

        [TestMethod]
        public void TryProjectMailItemMembers_UsesMaterializedProjectionValues()
        {
            var projection = MailItemHelper.TryProjectMailItemMembers(
                new { Subject = "Subject", EntryID = "entry-1" }
            );

            projection.Subject.Should().Be("Subject");
            projection.EntryId.Should().Be("entry-1");
        }

        [TestMethod]
        public async Task FromDfAfterResolved_LoadsPriorityProjectionAndRecipientStrings()
        {
            var mailItem = new Mock<InteropMailItem>();
            var archiveRoot = new Mock<OutlookFolder>();
            archiveRoot.SetupGet(x => x.FolderPath).Returns("\\Archive");
            var inboxRoot = new Mock<OutlookFolder>();
            inboxRoot.SetupGet(x => x.FolderPath).Returns("\\Inbox");
            var currentFolder = new Mock<OutlookFolder>();
            currentFolder.SetupGet(x => x.FolderPath).Returns("\\Archive\\Projects");

            var globals = CreateGlobals(archiveRoot.Object, inboxRoot.Object, "\\Archive");
            var sender = CreateSenderMock("Ada Sender", "ada@example.com");
            var toRecipient = CreateRecipientMock(
                "To User",
                "to@example.com",
                (int)OlMailRecipientType.olTo
            );
            var ccRecipient = CreateRecipientMock(
                "Cc User",
                "cc@example.com",
                (int)OlMailRecipientType.olCC
            );
            var recipients = CreateRecipientsMock(toRecipient.Object, ccRecipient.Object);
            var attachments = CreateAttachmentsMock();

            mailItem.SetupGet(x => x.Subject).Returns("Subject");
            mailItem.SetupGet(x => x.Body).Returns("Body");
            mailItem.SetupGet(x => x.HTMLBody).Returns("<html><body>Body</body></html>");
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Sender");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.EntryID).Returns("entry-1");
            mailItem.SetupGet(x => x.Sender).Returns(sender.Object);
            mailItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            mailItem.SetupGet(x => x.Attachments).Returns(attachments.Object);
            mailItem.SetupGet(x => x.Parent).Returns(currentFolder.Object);

            var helper = await MailItemHelper.FromMailItemAsync(
                mailItem.Object,
                globals.Object,
                CancellationToken.None,
                loadAll: false
            );

            SetLazyField(helper, "_triage", string.Empty);
            SetLazyField(helper, "_categories", string.Empty);
            SetLazyField(helper, "_sentOn", "5/2/2026 12:00 AM");
            SetLazyField(helper, "_actionable", string.Empty);
            SetLazyField(helper, "_conversationID", "conv-1");

            var result = await helper.FromDfAfterResolved();

            result.Should().BeSameAs(helper);
            helper.FolderInfo.OlRoot.Should().BeSameAs(archiveRoot.Object);
            helper.ToRecipientsName.Should().Contain("To User");
            helper.CcRecipientsName.Should().Contain("Cc User");
            helper.Html.Should().NotBeNull();
        }

        private static Mock<IApplicationGlobals> CreateGlobals(
            OutlookFolder archiveRoot,
            OutlookFolder inbox,
            string archiveRootPath
        )
        {
            var olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(x => x.ArchiveRoot).Returns(archiveRoot);
            olObjects.SetupGet(x => x.Inbox).Returns(inbox);
            olObjects.SetupGet(x => x.ArchiveRootPath).Returns(archiveRootPath);
            olObjects.SetupGet(x => x.EmailPrefixToStrip).Returns(string.Empty);

            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
            return globals;
        }

        private static Mock<AddressEntry> CreateSenderMock(string name, string address)
        {
            var propertyAccessor = new Mock<PropertyAccessor>();
            var sender = new Mock<AddressEntry>();

            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            sender.SetupGet(x => x.Name).Returns(name);
            sender.SetupGet(x => x.Address).Returns(address);
            sender.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            return sender;
        }

        private static Mock<Microsoft.Office.Interop.Outlook.Recipient> CreateRecipientMock(
            string name,
            string address,
            int type
        )
        {
            var propertyAccessor = new Mock<PropertyAccessor>();
            var addressEntry = new Mock<AddressEntry>();
            var recipient = new Mock<Microsoft.Office.Interop.Outlook.Recipient>();

            addressEntry
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            addressEntry.SetupGet(x => x.Name).Returns(name);
            recipient.SetupGet(x => x.Name).Returns(name);
            recipient.SetupGet(x => x.Address).Returns(address);
            recipient.SetupGet(x => x.Type).Returns(type);
            recipient.SetupGet(x => x.AddressEntry).Returns(addressEntry.Object);
            recipient.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            return recipient;
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

        private static Mock<Attachments> CreateAttachmentsMock()
        {
            var attachments = new Mock<Attachments>();
            var attachmentList = Array.Empty<Microsoft.Office.Interop.Outlook.Attachment>();

            attachments.SetupGet(x => x.Count).Returns(0);
            attachments
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)attachmentList).GetEnumerator());

            return attachments;
        }

        private static MailItemHelper CreateHelper()
        {
#pragma warning disable SYSLIB0050
            return (MailItemHelper)FormatterServices.GetUninitializedObject(typeof(MailItemHelper));
#pragma warning restore SYSLIB0050
        }

        private static void SetField(MailItemHelper helper, string fieldName, object value)
        {
            var field =
                typeof(MailItemHelper).GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic
                ) ?? throw new MissingFieldException(typeof(MailItemHelper).FullName, fieldName);
            field.SetValue(helper, value);
        }

        private static void SetLazyField<T>(MailItemHelper helper, string fieldName, T value)
        {
            SetField(helper, fieldName, new Lazy<T>(() => value));
        }
    }
}
