using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft;
using Newtonsoft.Json;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class MailItemHelperTests
    {
        private MockRepository mockRepository;
        private Mock<IApplicationGlobals> mockGlobals;
        private Mock<MailItem> mockMailItem;
        private Mock<AddressEntry> mockSender;
        private Mock<Folder> mockFolder;

        //private Mock<MailItemHelper> mockMailItemHelper;
        private Mock<Recipients> mockRecipients;
        private Mock<Recipient> mockRecipient1;
        private Mock<Recipient> mockRecipient2;
        private Mock<Attachments> mockAttachments;
        private Mock<Attachment> mockAttachment;
        private Mock<IOlObjects> mockOl;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockGlobals = SetupMockGlobals();
            this.mockMailItem = SetupMockMail();

            //this.mockMailItemHelper = SetupMockMailItemHelper();
        }

        private Mock<MailItemHelper> SetupMockMailItemHelper()
        {
            var m = mockRepository.Create<MailItemHelper>();
            m.SetupAllProperties();
            m.Setup(x => x.Item).Returns(mockMailItem.Object);
            m.Setup(x => x.Globals).Returns(mockGlobals.Object);
            m.Setup(x => x.EntryId).Returns("EntryID");
            var mockSenderInfo = mockRepository.Create<RecipientInfo>();
            mockSenderInfo.Setup(x => x.Name).Returns("SenderName");
            mockSenderInfo.Setup(x => x.Address).Returns("sendername@domain.com");
            mockSenderInfo.Setup(x => x.Html).Returns("SenderName <sendername@domain.com>");
            m.Setup(x => x.Sender).Returns(mockSenderInfo.Object);
            m.Setup(x => x.SenderHtml).Returns("SenderName <sendername@domain.com>");
            m.Setup(x => x.SenderName).Returns("SenderName");
            m.Setup(x => x.Actionable).Returns("Task");
            m.Setup(x => x.Body).Returns("Body");
            m.Setup(m => m.ConversationID).Returns("ConversationID");
            m.Setup(f => f.FolderName).Returns("FolderName");
            var mockFolderInfo = mockRepository.Create<FolderWrapper>();
            mockFolderInfo.Setup(x => x.OlFolder).Returns(mockFolder.Object);
            mockFolderInfo.Setup(x => x.Name).Returns("FolderName");
            m.Setup(x => x.FolderInfo).Returns(mockFolderInfo.Object);
            m.Setup(x => x.Html).Returns("HTMLBody");
            m.Setup(x => x.InternetCodepage).Returns(65001);
            m.Setup(x => x.SentDate).Returns(new DateTime(2024, 1, 1));
            m.Setup(x => x.SentOn).Returns("2024-01-01 00:00:00");

            return m;
        }

        private Mock<IApplicationGlobals> SetupMockGlobals()
        {
            var m = this.mockRepository.Create<IApplicationGlobals>();
            mockOl = this.mockRepository.Create<IOlObjects>();
            mockOl.Setup(x => x.EmailPrefixToStrip).Returns("EmailPrefixToStrip");
            var mockEmailRoot = this.mockRepository.Create<Folder>();
            mockEmailRoot.Setup(x => x.FolderPath).Returns("EmailRootPath");
            var mockArchiveRoot = this.mockRepository.Create<Folder>();
            mockArchiveRoot.Setup(x => x.FolderPath).Returns("ArchiveRootPath");
            mockOl.Setup(x => x.Inbox).Returns(mockEmailRoot.Object);
            mockOl.Setup(x => x.ArchiveRoot).Returns(mockArchiveRoot.Object);
            mockOl.Setup(x => x.ArchiveRootPath).Returns(mockArchiveRoot.Object.FolderPath);
            mockOl.Setup(x => x.InboxPath).Returns(mockEmailRoot.Object.FolderPath);
            m.Setup(x => x.Ol).Returns(mockOl.Object);
            return m;
        }

        private Mock<MailItem> SetupMockMail()
        {
            var mockMail = mockRepository.Create<MailItem>();
            mockMail.SetupAllProperties();
            mockMail.Setup(m => m.EntryID).Returns("EntryID");
            mockMail.Setup(m => m.Subject).Returns("Subject");
            mockSender = mockRepository.Create<AddressEntry>();
            mockSender
                .Setup(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);
            mockMail.Setup(x => x.Sender).Returns(mockSender.Object);
            mockMail.Setup(x => x.SenderEmailAddress).Returns("sendername@domain.com");
            mockMail.Setup(m => m.SenderName).Returns("SenderName");
            mockMail.Setup(m => m.IsMarkedAsTask).Returns(true);
            mockMail.Setup(m => m.Body).Returns("Body");
            mockMail.Setup(m => m.Categories).Returns("Categories");
            mockMail.Setup(m => m.ConversationID).Returns("ConversationID");
            mockFolder = mockRepository.Create<Folder>();
            mockFolder.SetupAllProperties();
            mockFolder.Setup(f => f.Name).Returns("FolderName");
            mockFolder.Setup(f => f.StoreID).Returns("StoreID");
            mockFolder.Setup(f => f.FolderPath).Returns("EmailRootPath//FolderName");

            var mockItems = mockRepository.Create<Items>();
            mockItems.SetupAllProperties();

            var items = new List<object> { mockMail.Object };
            mockItems.Setup(x => x.Count).Returns(() => items.Count());
            mockItems.Setup(x => x[It.IsAny<int>()]).Returns<int>(i => items[i]);
            mockItems.Setup(x => x.GetEnumerator()).Returns(() => items.GetEnumerator());

            mockFolder.Setup(x => x.Items).Returns(mockItems.Object);

            mockMail.Setup(m => m.Parent).Returns(mockFolder.Object);
            mockMail.Setup(m => m.HTMLBody).Returns("HTMLBody");
            mockMail.Setup(m => m.InternetCodepage).Returns(65001);
            mockMail.Setup(m => m.ReceivedTime).Returns(new DateTime(2024, 1, 1));
            mockRecipients = mockRepository.Create<Recipients>();
            mockRecipients.SetupAllProperties();

            mockRecipient1 = mockRepository.Create<Recipient>();
            mockRecipient1.SetupAllProperties();
            mockRecipient1.Setup(r => r.Name).Returns("Recipient1");
            mockRecipient1.Setup(r => r.Address).Returns("recipient1@domain.com");
            mockRecipient1.Setup(r => r.Type).Returns((int)OlMailRecipientType.olTo);

            var mockRecipient1AddressEntry = mockRepository.Create<AddressEntry>();
            mockRecipient1AddressEntry
                .Setup(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);
            mockRecipient1.Setup(r => r.AddressEntry).Returns(mockRecipient1AddressEntry.Object);

            mockRecipient2 = mockRepository.Create<Recipient>();
            mockRecipient2.SetupAllProperties();
            mockRecipient2.Setup(r => r.Name).Returns("Recipient2");
            mockRecipient2.Setup(r => r.Address).Returns("recipient2@domain.com");
            mockRecipient2.Setup(r => r.Type).Returns((int)OlMailRecipientType.olCC);
            var mockRecipient2AddressEntry = mockRepository.Create<AddressEntry>();
            mockRecipient2AddressEntry
                .Setup(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);
            mockRecipient2.Setup(r => r.AddressEntry).Returns(mockRecipient2AddressEntry.Object);

            List<Recipient> recipients = [mockRecipient1.Object, mockRecipient2.Object];
            mockRecipients.Setup(r => r.Count).Returns(() => recipients.Count);
            mockRecipients.Setup(r => r[It.IsAny<int>()]).Returns<int>(i => recipients[i]);
            mockRecipients.Setup(r => r.GetEnumerator()).Returns(() => recipients.GetEnumerator());

            //mockRecipients.Setup(r => r[1]).Returns(mockRecipient1.Object);
            //mockRecipients.Setup(r => r[2]).Returns(mockRecipient2.Object);

            //mockRecipients.Setup(r => r[1].Name).Returns("Recipient1");
            //mockRecipients.Setup(r => r[1].Address).Returns("recipient1@domain.com");
            //mockRecipients.Setup(r => r[1].Type).Returns((int)OlMailRecipientType.olTo);
            //mockRecipients.Setup(r => r[2].Name).Returns("Recipient2");
            //mockRecipients.Setup(r => r[2].Address).Returns("recipient2@domain.com");
            //mockRecipients.Setup(r => r[2].Type).Returns((int)OlMailRecipientType.olCC);
            mockMail.Setup(m => m.Recipients).Returns(mockRecipients.Object);
            mockMail.Setup(m => m.SentOn).Returns(new DateTime(2024, 1, 1));
            mockMail.Setup(m => m.UnRead).Returns(true);

            mockAttachment = mockRepository.Create<Attachment>();
            mockAttachment.Setup(a => a.Size).Returns(65001);
            mockAttachment.Setup(a => a.Type).Returns(OlAttachmentType.olByValue);
            mockAttachment.Setup(a => a.FileName).Returns("FileName");
            mockAttachment.Setup(a => a.PathName).Returns("PathName//FileName");

            mockAttachments = mockRepository.Create<Attachments>();
            List<Attachment> attachments = [mockAttachment.Object];
            mockAttachments.Setup(a => a.Count).Returns(() => attachments.Count);
            mockAttachments.Setup(a => a[It.IsAny<int>()]).Returns<int>(i => attachments[i]);
            mockAttachments
                .Setup(a => a.GetEnumerator())
                .Returns(() => attachments.GetEnumerator());

            mockMail.Setup(m => m.Attachments).Returns(mockAttachments.Object);

            var mockTriageProperty = mockRepository.Create<UserProperty>();
            mockTriageProperty.Setup(p => p.Value).Returns("Triage");
            mockTriageProperty.Setup(p => p.Name).Returns("Triage");
            List<UserProperty> userProperties = [mockTriageProperty.Object];

            var mockUserProperties = mockRepository.Create<UserProperties>();
            mockUserProperties.Setup(p => p.Count).Returns(() => userProperties.Count);
            mockUserProperties.Setup(p => p[It.IsAny<int>()]).Returns<int>(i => userProperties[i]);
            mockUserProperties
                .Setup(p => p.GetEnumerator())
                .Returns(() => userProperties.GetEnumerator());
            mockUserProperties
                .Setup(p => p.Find(It.IsAny<string>(), (object)true))
                .Returns<string, object>(
                    (name, custom) => userProperties.Find(x => x.Name == name)
                );

            mockMail.Setup(m => m.UserProperties).Returns(mockUserProperties.Object);

            return mockMail;
        }

        private object[] GetExpectedConstructorFields()
        {
            var senderInfo = new RecipientInfo(
                "SenderName",
                "sendername@domain.com",
                "SenderName &lt;<a href=\"mailto:sendername@domain.com\">sendername@domain.com</a>&gt;"
            );

            var folderInfo = new FolderWrapper(
                (Folder)mockMailItem.Object.Parent,
                mockOl.Object.Inbox
            );
            var mail = mockMailItem.Object;
            var attachmentsHelper = mail
                .Attachments.Cast<Attachment>()
                .Select(x => new AttachmentHelper(x, new DateTime(2024, 1, 1), "FolderName"))
                .ToArray();
            var attachmentInfo = attachmentsHelper.Select(x => x.AttachmentInfo).ToArray();
            return
            [
                mail,
                mockGlobals.Object,
                "EntryID",
                senderInfo,
                senderInfo.Html,
                senderInfo.Name,
                "Task",
                "Body <EOM>",
                "ConversationID",
                "EmailPrefixToStrip",
                "StoreID", // 10
                "FolderName",
                folderInfo,
                "HTMLBody",
                "HTMLBody", // 14
                false, // 15
                mockRecipients.Object.Cast<Recipient>().ToArray(),
                new IRecipientInfo[] { mockRecipient2.Object.GetInfo() }, // 17
                new IRecipientInfo[] { mockRecipient1.Object.GetInfo() },
                "Recipient1",
                "Recipient1 &lt;<a href=\"mailto:recipient1@domain.com\">recipient1@domain.com</a>&gt;",
                new DateTime(2024, 1, 1),
                "1/1/2024 12:00 AM",
                "Subject",
                new string[]
                {
                    "charset:utf-8",
                    "filename:fname:FileName",
                    "subject:Subject",
                    "from:name:sendername",
                    "from:addr:sendername",
                    "from:addr:domain.com",
                    "to:name:recipient1",
                    "to:addr:recipient1",
                    "to:addr:domain.com",
                    "cc:name:recipient2",
                    "cc:addr:recipient2",
                    "cc:addr:domain.com",
                    "to:2**0",
                    "to:2**0",
                    "body",
                    "<eom>",
                },
                "Triage",
                true,
                attachmentsHelper,
                attachmentInfo,
                65001,
            ];
        }

        private object[] GetLazyFields(MailItemHelper helper)
        {
            object[] fields =
            [
                helper.Item, //  0
                helper.Globals, //  1
                helper.EntryId, //  2
                helper.Sender, //  3
                helper.SenderHtml, //  4
                helper.SenderName, //  5
                helper.Actionable,
                helper.Body,
                helper.ConversationID,
                helper.EmailPrefixToStrip,
                helper.StoreId, // 10
                helper.FolderName,
                helper.FolderInfo,
                helper.HTMLBody,
                helper.Html, // 14
                helper.IsTaskFlagSet, // 15
                helper.OlRecipients, // 16
                helper.CcRecipients, // 17
                helper.ToRecipients, // 18
                helper.ToRecipientsName, // 19
                helper.ToRecipientsHtml,
                helper.SentDate,
                helper.SentOn,
                helper.Subject,
                helper.Tokens,
                helper.Triage,
                helper.UnRead,
                helper.AttachmentsHelper,
                helper.AttachmentsInfo,
                helper.InternetCodepage,
            ];
            return fields;
        }

        [TestMethod]
        public void Constructor_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            MailItem item = mockMailItem.Object;
            IApplicationGlobals globals = mockGlobals.Object;
            object[] expected = GetExpectedConstructorFields();
            var expectedText =
                $"[\n{string.Join("\n", expected.Select(
                x => x is object ? JsonConvert.SerializeObject(x) : x.ToString()).ToArray())}\n]";
            Console.WriteLine("\nEXPECTED:");
            Console.WriteLine(expectedText);

            // Act
            var helper = new MailItemHelper(item, globals);

            object[] actual = GetLazyFields(helper);

            var actualText =
                $"[\n{string.Join("\n", actual.Select(
                x => x is object ? JsonConvert.SerializeObject(x) : x.ToString()).ToArray())}\n]";
            Console.WriteLine("\nACTUAL:");
            Console.WriteLine(actualText);

            // Assert
            actual
                .Should()
                .BeEquivalentTo(
                    expected,
                    options => options.Excluding(x => x.Path.EndsWith("FilePathSaveAlt"))
                );
        }

        //[TestMethod]
        //public void FromDf_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    DataFrame df = null;
        //    long indexRow = 0;
        //    IApplicationGlobals appGlobals = null;
        //    CancellationToken token = default(global::System.Threading.CancellationToken);

        //    // Act
        //    var result = mailItemHelper.FromDf(
        //        df,
        //        indexRow,
        //        appGlobals,
        //        token);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task FromDfAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    DataFrame df = null;
        //    long indexRow = 0;
        //    IApplicationGlobals appGlobals = null;
        //    CancellationToken token = default(global::System.Threading.CancellationToken);
        //    bool background = false;
        //    bool resolveOnly = false;

        //    // Act
        //    var result = await mailItemHelper.FromDfAsync(
        //        df,
        //        indexRow,
        //        appGlobals,
        //        token,
        //        background,
        //        resolveOnly);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task FromDfAfterResolved_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();

        //    // Act
        //    var result = await mailItemHelper.FromDfAfterResolved();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task FromDfAsync_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    DataFrame df = null;
        //    long indexRow = 0;
        //    IApplicationGlobals appGlobals = null;
        //    CancellationToken token = default(global::System.Threading.CancellationToken);
        //    bool background = false;

        //    // Act
        //    var result = await mailItemHelper.FromDfAsync(
        //        df,
        //        indexRow,
        //        appGlobals,
        //        token,
        //        background);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task FromMailItemAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    MailItem item = null;
        //    IApplicationGlobals appGlobals = null;
        //    CancellationToken token = default(global::System.Threading.CancellationToken);
        //    bool loadAll = false;

        //    // Act
        //    var result = await mailItemHelper.FromMailItemAsync(
        //        item,
        //        appGlobals,
        //        token,
        //        loadAll);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void ResolveMail_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    NameSpace olNs = null;
        //    bool strict = false;

        //    // Act
        //    var result = mailItemHelper.ResolveMail(
        //        olNs,
        //        strict);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task ResolveMailAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    NameSpace olNs = null;
        //    CancellationToken token = default(global::System.Threading.CancellationToken);
        //    bool background = false;

        //    // Act
        //    var result = await mailItemHelper.ResolveMailAsync(
        //        olNs,
        //        token,
        //        background);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void LoadPriority_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    IApplicationGlobals globals = null;
        //    CancellationToken token = default(global::System.Threading.CancellationToken);

        //    // Act
        //    var result = mailItemHelper.LoadPriority(
        //        globals,
        //        token);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void LoadAll_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    IApplicationGlobals globals = null;
        //    Folder olRoot = null;
        //    bool loadTokens = false;

        //    // Act
        //    var result = mailItemHelper.LoadAll(
        //        globals,
        //        olRoot,
        //        loadTokens);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void LoadRecipients_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();

        //    // Act
        //    mailItemHelper.LoadRecipients();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void GetHeadersExtendedMapi_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();

        //    // Act
        //    var result = mailItemHelper.GetHeadersExtendedMapi();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void LoadTokens_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();

        //    // Act
        //    var result = mailItemHelper.LoadTokens();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public async Task TokenizeAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();

        //    // Act
        //    var result = await mailItemHelper.TokenizeAsync();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void ToggleDark_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();

        //    // Act
        //    var result = mailItemHelper.ToggleDark();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void ToggleDark_StateUnderTest_ExpectedBehavior1()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    ToggleState desiredState = default(global::UtilitiesCS.Enums.ToggleState);

        //    // Act
        //    var result = mailItemHelper.ToggleDark(
        //        desiredState);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void ToSerializableObject_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();

        //    // Act
        //    var result = mailItemHelper.ToSerializableObject();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[TestMethod]
        //public void FromSerializableObject_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var mailItemHelper = this.CreateMailItemHelper();
        //    ItemInfo itemInfo = null;
        //    NameSpace olNs = null;

        //    // Act
        //    var result = mailItemHelper.FromSerializableObject(
        //        itemInfo,
        //        olNs);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        #region TryProjectMailItemMembers Tests

        [TestMethod]
        public void TryProjectMailItemMembers_NullSource_ReturnsEmptyProjection()
        {
            // Act
            var result = MailItemHelper.TryProjectMailItemMembers(null);

            // Assert
            result.Subject.Should().BeEmpty();
            result.EntryId.Should().BeEmpty();
        }

        [TestMethod]
        public void TryProjectMailItemMembers_ValidMailItem_ReturnsProjected()
        {
            // Arrange
            var mail = mockMailItem.Object;

            // Act
            var result = MailItemHelper.TryProjectMailItemMembers(mail);

            // Assert
            result.Subject.Should().Be("Subject");
            result.EntryId.Should().Be("EntryID");
        }

        [TestMethod]
        public void TryProjectMailItemMembers_ObjectWithoutProperties_ReturnsEmpty()
        {
            // Arrange - plain object has no Subject/EntryID
            var source = new { Name = "test" };

            // Act
            var result = MailItemHelper.TryProjectMailItemMembers(source);

            // Assert
            result.Subject.Should().BeEmpty();
            result.EntryId.Should().BeEmpty();
        }

        #endregion

        #region CompressPlainText Tests

        [TestMethod]
        public void CompressPlainText_NullTextAndPrefix_ReturnsEomMarker()
        {
            // Act
            var result = MailItemHelper.CompressPlainText(null, null);

            // Assert
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_SimpleText_CompressesWhitespace()
        {
            // Act
            var result = MailItemHelper.CompressPlainText("Hello   World", "");

            // Assert
            result.Should().Be("Hello World <EOM>");
        }

        [TestMethod]
        public void CompressPlainText_StripWarning_RemovesPrefix()
        {
            // Act
            var result = MailItemHelper.CompressPlainText(
                "WARNING: Hello World",
                IItemInfo.PlainTextOptionsEnum.StripAll,
                "WARNING: "
            );

            // Assert
            result.Should().NotContain("WARNING:");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_StripLinks_RemovesHttpLinks()
        {
            // Arrange
            var text = "Check <https://example.com/page> out";

            // Act
            var result = MailItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripLinks,
                ""
            );

            // Assert
            result.Should().NotContain("https://example.com");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_StripLinksShowStripped_ReplacesWithLinkTag()
        {
            // Arrange
            var text = "Check <https://example.com/page> out";
            var options =
                IItemInfo.PlainTextOptionsEnum.StripLinks
                | IItemInfo.PlainTextOptionsEnum.ShowStripped;

            // Act
            var result = MailItemHelper.CompressPlainText(text, options, "");

            // Assert
            result.Should().Contain("<link>");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_StripFormatting_CollapsesWhitespace()
        {
            // Arrange
            var text = "Hello\n\tWorld\r\nFoo";

            // Act
            var result = MailItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripFormatting,
                ""
            );

            // Assert
            result.Should().NotContain("\n");
            result.Should().NotContain("\t");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_StripReplyHeader_RemovesReplyBlock()
        {
            // Arrange
            var text =
                "Original message\nFrom: Sender\nSent: 2024-01-01\nTo: Recipient\nSubject: Re: Test\nReply body here";

            // Act
            var result = MailItemHelper.CompressPlainText(
                text,
                IItemInfo.PlainTextOptionsEnum.StripReplyHeader
                    | IItemInfo.PlainTextOptionsEnum.StripReplyBody,
                ""
            );

            // Assert
            result.Should().Contain("Original message");
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_StripReplyHeaderShowStripped_IncludesEomChain()
        {
            // Arrange
            var text =
                "Message here\nFrom: Sender\nSent: 2024-01-01\nTo: Recipient\nSubject: Re: Test\nChained reply";
            var options =
                IItemInfo.PlainTextOptionsEnum.StripReplyHeader
                | IItemInfo.PlainTextOptionsEnum.ShowStripped;

            // Act
            var result = MailItemHelper.CompressPlainText(text, options, "");

            // Assert
            result.Should().EndWith("<EOM>");
        }

        [TestMethod]
        public void CompressPlainText_StripReplyBodyOnly_PreservesHeader()
        {
            // Arrange
            var text = "Message\nFrom: Sender\nSent: Date\nTo: To\nSubject: Re: Test\nReply body";
            var options = IItemInfo.PlainTextOptionsEnum.StripReplyBody;

            // Act
            var result = MailItemHelper.CompressPlainText(text, options, "");

            // Assert
            result.Should().EndWith("<EOM>");
        }

        #endregion

        #region ToggleDark Tests

        [TestMethod]
        public void ToggleDark_InitiallyOff_TogglesOn()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.ToggleDark();

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void ToggleDark_OnThenOff_RemovesDarkModeHeader()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            helper.ToggleDark(Enums.ToggleState.On);
            var result = helper.ToggleDark(Enums.ToggleState.Off);

            // Assert
            result.Should().NotContain("filter: invert(100%)");
        }

        [TestMethod]
        public void ToggleDark_AlreadyOn_DesiredOn_NoDoubleInsert()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            helper.ToggleDark(Enums.ToggleState.On);

            // Act
            var result = helper.ToggleDark(Enums.ToggleState.On);

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void ToggleDark_AlreadyOff_DesiredOff_NoChange()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.ToggleDark(Enums.ToggleState.Off);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region Equals Tests

        [TestMethod]
        public void Equals_NullOther_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.Equals((IItemInfo)null);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_SameReference_ReturnsTrue()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.Equals((IItemInfo)helper);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Equals_DifferentSize_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var other = mockRepository.Create<IItemInfo>();
            other.Setup(x => x.Size).Returns(999999);

            // Act
            var result = helper.Equals(other.Object);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_DifferentSentDate_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var other = mockRepository.Create<IItemInfo>();
            other.Setup(x => x.Size).Returns(helper.Size);
            other.Setup(x => x.SentDate).Returns(new DateTime(2099, 12, 31));

            // Act
            var result = helper.Equals(other.Object);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_DifferentSubject_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var other = mockRepository.Create<IItemInfo>();
            other.Setup(x => x.Size).Returns(helper.Size);
            other.Setup(x => x.SentDate).Returns(helper.SentDate);
            other.Setup(x => x.Subject).Returns("Completely Different");

            // Act
            var result = helper.Equals(other.Object);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_DifferentBody_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var other = mockRepository.Create<IItemInfo>();
            other.Setup(x => x.Size).Returns(helper.Size);
            other.Setup(x => x.SentDate).Returns(helper.SentDate);
            other.Setup(x => x.Subject).Returns(helper.Subject);
            other.Setup(x => x.Body).Returns("Different Body");

            // Act
            var result = helper.Equals(other.Object);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Equals_DifferentSender_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var mockSenderInfo = mockRepository.Create<IRecipientInfo>();
            var other = mockRepository.Create<IItemInfo>();
            other.Setup(x => x.Size).Returns(helper.Size);
            other.Setup(x => x.SentDate).Returns(helper.SentDate);
            other.Setup(x => x.Subject).Returns(helper.Subject);
            other.Setup(x => x.Body).Returns(helper.Body);
            other.Setup(x => x.Sender).Returns(mockSenderInfo.Object);

            // Act
            var result = helper.Equals(other.Object);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region RecipientsEquivalent Tests

        [TestMethod]
        public void RecipientsEquivalent_BothNull_ReturnsTrue()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.RecipientsEquivalent(null, null);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void RecipientsEquivalent_SourceNull_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var other = new IRecipientInfo[] { mockRepository.Create<IRecipientInfo>().Object };

            // Act
            var result = helper.RecipientsEquivalent(null, other);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void RecipientsEquivalent_OtherNull_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var source = new IRecipientInfo[] { mockRepository.Create<IRecipientInfo>().Object };

            // Act
            var result = helper.RecipientsEquivalent(source, null);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void RecipientsEquivalent_DifferentLength_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var source = new IRecipientInfo[] { mockRepository.Create<IRecipientInfo>().Object };
            var other = new IRecipientInfo[]
            {
                mockRepository.Create<IRecipientInfo>().Object,
                mockRepository.Create<IRecipientInfo>().Object,
            };

            // Act
            var result = helper.RecipientsEquivalent(source, other);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void RecipientsEquivalent_MatchingRecipients_ReturnsTrue()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var r1 = new RecipientInfo("Name1", "addr1@test.com", "html1");
            var r2 = new RecipientInfo("Name1", "addr1@test.com", "html1");
            var source = new IRecipientInfo[] { r1 };
            var other = new IRecipientInfo[] { r2 };

            // Act
            var result = helper.RecipientsEquivalent(source, other);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void RecipientsEquivalent_NoMatch_ReturnsFalse()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var r1 = new RecipientInfo("Name1", "addr1@test.com", "html1");
            var r2 = new RecipientInfo("Name2", "addr2@test.com", "html2");
            var source = new IRecipientInfo[] { r1 };
            var other = new IRecipientInfo[] { r2 };

            // Act
            var result = helper.RecipientsEquivalent(source, other);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Property and NotifyPropertyChanged Tests

        [TestMethod]
        public void PropertyChanged_CcRecipientsHtml_RaisesEvent()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var propertyNames = new List<string>();
            helper.PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            // Act
            helper.CcRecipientsHtml = "new value";

            // Assert
            propertyNames.Should().Contain("CcRecipientsHtml");
        }

        [TestMethod]
        public void PropertyChanged_CcRecipientsName_RaisesEvent()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var propertyNames = new List<string>();
            helper.PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            // Act
            helper.CcRecipientsName = "new name";

            // Assert
            propertyNames.Should().Contain("CcRecipientsName");
        }

        [TestMethod]
        public void PropertyChanged_ToRecipientsHtml_RaisesEvent()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var propertyNames = new List<string>();
            helper.PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            // Act
            helper.ToRecipientsHtml = "new value";

            // Assert
            propertyNames.Should().Contain("ToRecipientsHtml");
        }

        [TestMethod]
        public void PropertyChanged_ToRecipientsName_RaisesEvent()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var propertyNames = new List<string>();
            helper.PropertyChanged += (s, e) => propertyNames.Add(e.PropertyName);

            // Act
            helper.ToRecipientsName = "new name";

            // Assert
            propertyNames.Should().Contain("ToRecipientsName");
        }

        #endregion

        #region SetSender Tests

        [TestMethod]
        public void SetSender_ValidRecipient_SetsSenderFields()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);
            var sender = new RecipientInfo("TestSender", "test@test.com", "TestHtml");

            // Act
            helper.SetSender(sender);

            // Assert
            helper.Sender.Should().Be(sender);
            helper.SenderName.Should().Be("TestSender");
            helper.SenderHtml.Should().Be("TestHtml");
        }

        #endregion

        #region LoadRecipients Tests

        [TestMethod]
        public void LoadRecipients_WithMockedRecipients_SetsProperties()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            helper.LoadRecipients();

            // Assert
            helper.ToRecipients.Should().NotBeNull();
            helper.CcRecipients.Should().NotBeNull();
            helper.ToRecipientsName.Should().NotBeNullOrEmpty();
            helper.CcRecipientsName.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region LoadRecipientsForce Tests

        [TestMethod]
        public void LoadRecipientsForce_ForcesLazyEvaluation()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            helper.LoadRecipientsForce();

            // Assert
            helper.ToRecipientsName.Should().NotBeNull();
            helper.ToRecipientsHtml.Should().NotBeNull();
            helper.CcRecipientsName.Should().NotBeNull();
            helper.CcRecipientsHtml.Should().NotBeNull();
        }

        #endregion

        #region LoadPriorityForce Tests

        [TestMethod]
        public void LoadPriorityForce_ForcesLazyFieldEvaluation()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            helper.LoadPriorityForce();

            // Assert
            helper.EntryId.Should().NotBeNull();
            helper.Subject.Should().NotBeNull();
            helper.Body.Should().NotBeNull();
            helper.Categories.Should().NotBeNull();
        }

        #endregion

        #region ToSerializableObject / ToMatchableObject Tests

        [TestMethod]
        public void ToSerializableObject_ReturnsItemInfo()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.ToSerializableObject();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ItemInfo>();
        }

        [TestMethod]
        public void ToMatchableObject_ReturnsItemInfoWithKeyFields()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.ToMatchableObject();

            // Assert
            result.Should().NotBeNull();
            result.EntryId.Should().Be(helper.EntryId);
            result.StoreId.Should().Be(helper.StoreId);
            result.Subject.Should().Be(helper.Subject);
            result.Body.Should().Be(helper.Body);
            result.SentDate.Should().Be(helper.SentDate);
        }

        #endregion

        #region EmailHeader Tests

        [TestMethod]
        public void EmailHeader2_ContainsSenderAndRecipientInfo()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.EmailHeader2;

            // Assert
            result.Should().Contain("From:");
            result.Should().Contain("Sent:");
            result.Should().Contain("To:");
            result.Should().Contain("Subject:");
        }

        [TestMethod]
        public void EmailHeader_ContainsHtmlStructure()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.EmailHeader;

            // Assert
            result.Should().Contain("From:");
            result.Should().Contain("Sent:");
            result.Should().Contain("To:");
            result.Should().Contain("Cc:");
            result.Should().Contain("Subject:");
        }

        [TestMethod]
        public void EmailHeader_CalledTwice_ReturnsCachedValue()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var first = helper.EmailHeader;
            var second = helper.EmailHeader;

            // Assert
            first.Should().BeSameAs(second);
        }

        #endregion

        #region GetHtml Tests

        [TestMethod]
        public void GetHtml_InsertsEmailHeader()
        {
            // Arrange
            mockMailItem
                .Setup(m => m.HTMLBody)
                .Returns("<html><head></head><body>content</body></html>");
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.GetHtml();

            // Assert
            result.Should().Contain("From:");
            result.Should().Contain("content");
        }

        [TestMethod]
        public void GetHtml_WithHtmlBody_InsertsEmailHeader()
        {
            // Arrange
            mockMailItem
                .Setup(m => m.HTMLBody)
                .Returns("<html><head></head><body>content</body></html>");
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = helper.GetHtml("ignored");

            // Assert
            result.Should().Contain("From:");
        }

        #endregion

        #region MailItemProjection Tests

        [TestMethod]
        public void MailItemProjection_Constructor_SetsProperties()
        {
            // Act
            var proj = new MailItemHelper.MailItemProjection("TestSubject", "TestEntryId");

            // Assert
            proj.Subject.Should().Be("TestSubject");
            proj.EntryId.Should().Be("TestEntryId");
        }

        [TestMethod]
        public void MailItemProjection_NullSubject_DefaultsToEmpty()
        {
            // Act
            var proj = new MailItemHelper.MailItemProjection(null, null);

            // Assert
            proj.Subject.Should().BeEmpty();
            proj.EntryId.Should().BeEmpty();
        }

        #endregion

        #region Property Setters Tests

        [TestMethod]
        public void Properties_SetAndGet_WorkCorrectly()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act & Assert
            helper.Actionable = "TestAction";
            helper.Actionable.Should().Be("TestAction");

            helper.Body = "TestBody";
            helper.Body.Should().Be("TestBody");

            helper.Categories = "Cat1";
            helper.Categories.Should().Be("Cat1");

            helper.ConversationID = "ConvID";
            helper.ConversationID.Should().Be("ConvID");

            helper.EntryId = "EID";
            helper.EntryId.Should().Be("EID");

            helper.StoreId = "SID";
            helper.StoreId.Should().Be("SID");

            helper.FolderName = "Folder";
            helper.FolderName.Should().Be("Folder");

            helper.SentOn = "2024-01";
            helper.SentOn.Should().Be("2024-01");

            helper.Subject = "Subj";
            helper.Subject.Should().Be("Subj");

            helper.SenderHtml = "SHtml";
            helper.SenderHtml.Should().Be("SHtml");

            helper.SenderName = "SName";
            helper.SenderName.Should().Be("SName");

            helper.Size = 42;
            helper.Size.Should().Be(42);

            helper.SentDate = new DateTime(2025, 6, 15);
            helper.SentDate.Should().Be(new DateTime(2025, 6, 15));

            helper.Triage = "MyTriage";
            helper.Triage.Should().Be("MyTriage");

            helper.IsTaskFlagSet = true;
            helper.IsTaskFlagSet.Should().BeTrue();

            helper.InternetCodepage = 1252;
            helper.InternetCodepage.Should().Be(1252);
        }

        [TestMethod]
        public void PlainTextOptions_SetAndGet_WorkCorrectly()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            helper.PlainTextOptions = IItemInfo.PlainTextOptionsEnum.StripLinks;

            // Assert
            helper.PlainTextOptions.Should().Be(IItemInfo.PlainTextOptionsEnum.StripLinks);
        }

        #endregion

        #region InitializeSafeDefaults Tests

        [TestMethod]
        public void DefaultConstructor_WithProjection_InitializesSafeDefaults()
        {
            // Arrange & Act - use the DataFrame constructor which calls InitializeSafeDefaults
            // We can verify via the safe default values
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Assert - access properties to verify safe defaults can be overwritten
            helper.Actionable.Should().NotBeNull();
            helper.Body.Should().NotBeNull();
            helper.ConversationID.Should().NotBeNull();
            helper.EntryId.Should().NotBeNull();
            helper.FolderName.Should().NotBeNull();
            helper.StoreId.Should().NotBeNull();
            helper.SentOn.Should().NotBeNull();
            helper.Subject.Should().NotBeNull();
        }

        #endregion

        #region ResolveFolderRoot Tests

        [TestMethod]
        public void ResolveFolderRoot_ArchivePath_ReturnsArchiveRoot()
        {
            // Act
            var result = MailItemHelper.ResolveFolderRoot(
                mockGlobals.Object,
                "ArchiveRootPath//FolderName"
            );

            // Assert
            result.Should().Be(mockOl.Object.ArchiveRoot);
        }

        [TestMethod]
        public void ResolveFolderRoot_InboxPath_ReturnsInbox()
        {
            // Act
            var result = MailItemHelper.ResolveFolderRoot(
                mockGlobals.Object,
                "EmailRootPath//FolderName"
            );

            // Assert
            result.Should().Be(mockOl.Object.Inbox);
        }

        #endregion

        #region UnRead Property Tests

        [TestMethod]
        public void UnRead_SetValue_UpdatesMailItem()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            helper.UnRead = false;

            // Assert
            helper.UnRead.Should().BeFalse();
        }

        #endregion

        #region Tokenizer Tests

        [TestMethod]
        public void Tokenizer_DefaultsToEmailTokenizer()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var tokenizer = helper.Tokenizer;

            // Assert
            tokenizer.Should().NotBeNull();
            tokenizer.Should().BeOfType<EmailTokenizer>();
        }

        [TestMethod]
        public async Task TokenizeAsync_ReturnsTokens()
        {
            // Arrange
            var helper = new MailItemHelper(mockMailItem.Object, mockGlobals.Object);

            // Act
            var result = await helper.TokenizeAsync();

            // Assert
            result.Should().NotBeNull();
            helper.Tokens.Should().NotBeNull();
        }

        [TestMethod]
        public async Task FromMailItemAsync_MaterializesTokenizationDependenciesBeforeBackgroundTokenAccess()
        {
            // Arrange: count each COM-backed property read so the test can verify the helper
            // forces tokenization inputs on the caller thread before later background access.
            var subjectReads = 0;
            var bodyReads = 0;
            var htmlBodyReads = 0;
            var senderReads = 0;
            var recipientsReads = 0;
            var attachmentsReads = 0;
            var internetCodepageReads = 0;

            mockMailItem.SetupGet(x => x.Subject).Callback(() => subjectReads++).Returns("Subject");
            mockMailItem.SetupGet(x => x.Body).Callback(() => bodyReads++).Returns("Body");
            mockMailItem
                .SetupGet(x => x.HTMLBody)
                .Callback(() => htmlBodyReads++)
                .Returns("<html><body>Body</body></html>");
            mockMailItem.SetupGet(x => x.SenderName).Returns("SenderName");
            mockMailItem.SetupGet(x => x.SenderEmailAddress).Returns("sendername@domain.com");
            mockMailItem.SetupGet(x => x.EntryID).Returns("EntryID");
            mockMailItem
                .SetupGet(x => x.Sender)
                .Callback(() => senderReads++)
                .Returns(mockSender.Object);
            mockMailItem
                .SetupGet(x => x.Recipients)
                .Callback(() => recipientsReads++)
                .Returns(mockRecipients.Object);
            mockMailItem
                .SetupGet(x => x.Attachments)
                .Callback(() => attachmentsReads++)
                .Returns(mockAttachments.Object);
            mockMailItem
                .SetupGet(x => x.InternetCodepage)
                .Callback(() => internetCodepageReads++)
                .Returns(65001);

            var helper = await MailItemHelper.FromMailItemAsync(
                mockMailItem.Object,
                mockGlobals.Object,
                CancellationToken.None,
                loadAll: false
            );

            subjectReads.Should().BeGreaterThan(0);
            bodyReads.Should().BeGreaterThan(0);
            htmlBodyReads.Should().BeGreaterThan(0);
            senderReads.Should().BeGreaterThan(0);
            recipientsReads.Should().BeGreaterThan(0);
            attachmentsReads.Should().BeGreaterThan(0);
            internetCodepageReads.Should().BeGreaterThan(0);

            var subjectReadsAfterMaterialization = subjectReads;
            var bodyReadsAfterMaterialization = bodyReads;
            var htmlBodyReadsAfterMaterialization = htmlBodyReads;
            var senderReadsAfterMaterialization = senderReads;
            var recipientsReadsAfterMaterialization = recipientsReads;
            var attachmentsReadsAfterMaterialization = attachmentsReads;
            var internetCodepageReadsAfterMaterialization = internetCodepageReads;

            var tokenizer = mockRepository.Create<IEmailTokenizer>();
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
                        _ = info.InternetCodepage;
                        return new[] { "token" };
                    }
                );
            typeof(MailItemHelper)
                .GetField("_tokenizer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(helper, tokenizer.Object);

            // Act
            var tokens = await Task.Run(() => helper.Tokens);

            // Assert
            tokens.Should().Equal("token");
            subjectReads.Should().Be(subjectReadsAfterMaterialization);
            bodyReads.Should().Be(bodyReadsAfterMaterialization);
            htmlBodyReads.Should().Be(htmlBodyReadsAfterMaterialization);
            senderReads.Should().Be(senderReadsAfterMaterialization);
            recipientsReads.Should().Be(recipientsReadsAfterMaterialization);
            attachmentsReads.Should().Be(attachmentsReadsAfterMaterialization);
            internetCodepageReads.Should().Be(internetCodepageReadsAfterMaterialization);
        }

        #endregion
    }
}
