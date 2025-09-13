using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using UtilitiesCS;
using Microsoft.Office.Interop.Outlook;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft;
using Newtonsoft.Json;
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
            mockSender.Setup(x => x.AddressEntryUserType).Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);
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
            mockMail.Setup(m => m.ReceivedTime).Returns(new DateTime(2024,1,1));
            mockRecipients = mockRepository.Create<Recipients>();
            mockRecipients.SetupAllProperties();
            
            mockRecipient1 = mockRepository.Create<Recipient>();
            mockRecipient1.SetupAllProperties();
            mockRecipient1.Setup(r => r.Name).Returns("Recipient1");
            mockRecipient1.Setup(r => r.Address).Returns("recipient1@domain.com");
            mockRecipient1.Setup(r => r.Type).Returns((int)OlMailRecipientType.olTo);
            
            var mockRecipient1AddressEntry = mockRepository.Create<AddressEntry>();
            mockRecipient1AddressEntry.Setup(x => x.AddressEntryUserType).Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);
            mockRecipient1.Setup(r => r.AddressEntry).Returns(mockRecipient1AddressEntry.Object);

            mockRecipient2 = mockRepository.Create<Recipient>();
            mockRecipient2.SetupAllProperties();
            mockRecipient2.Setup(r => r.Name).Returns("Recipient2");
            mockRecipient2.Setup(r => r.Address).Returns("recipient2@domain.com");
            mockRecipient2.Setup(r => r.Type).Returns((int)OlMailRecipientType.olCC);
            var mockRecipient2AddressEntry = mockRepository.Create<AddressEntry>();
            mockRecipient2AddressEntry.Setup(x => x.AddressEntryUserType).Returns(OlAddressEntryUserType.olOutlookContactAddressEntry);
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
            mockAttachments.Setup(a => a.GetEnumerator()).Returns(() => attachments.GetEnumerator());

            mockMail.Setup(m => m.Attachments).Returns(mockAttachments.Object);

            var mockTriageProperty = mockRepository.Create<UserProperty>();
            mockTriageProperty.Setup(p => p.Value).Returns("Triage");
            mockTriageProperty.Setup(p => p.Name).Returns("Triage");
            List<UserProperty> userProperties = [mockTriageProperty.Object];
            
            var mockUserProperties = mockRepository.Create<UserProperties>();
            mockUserProperties.Setup(p => p.Count).Returns(() => userProperties.Count);
            mockUserProperties.Setup(p => p[It.IsAny<int>()]).Returns<int>(i => userProperties[i]);
            mockUserProperties.Setup(p => p.GetEnumerator()).Returns(() => userProperties.GetEnumerator());
            mockUserProperties.Setup(p => p.Find(It.IsAny<string>(), (object)true)).Returns<string, object>((name,custom) => userProperties.Find(x => x.Name == name));

            mockMail.Setup(m => m.UserProperties).Returns(mockUserProperties.Object);

            return mockMail;
        }

        private object[] GetExpectedConstructorFields() 
        {
            var senderInfo = new RecipientInfo(
                "SenderName", 
                "sendername@domain.com", 
                "SenderName &lt;<a href=\"mailto:sendername@domain.com\">sendername@domain.com</a>&gt;");

            var folderInfo = new FolderWrapper((Folder)mockMailItem.Object.Parent, mockOl.Object.Inbox);
            var mail = mockMailItem.Object;
            var attachmentsHelper = mail.Attachments
                                        .Cast<Attachment>()
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
                "StoreID",                              // 10
                "FolderName",
                folderInfo,
                "HTMLBody",
                "HTMLBody",                             // 14
                false,                                  // 15
                mockRecipients.Object.Cast<Recipient>().ToArray(),
                new IRecipientInfo[] {mockRecipient2.Object.GetInfo() },        // 17
                new IRecipientInfo[] {mockRecipient1.Object.GetInfo() },
                "Recipient1",
                "Recipient1 &lt;<a href=\"mailto:recipient1@domain.com\">recipient1@domain.com</a>&gt;",
                new DateTime(2024, 1, 1),
                "1/1/2024 12:00 AM",
                "Subject",
                new string[] {"charset:utf-8","filename:fname:FileName","subject:Subject","from:name:sendername","from:addr:sendername","from:addr:domain.com","to:name:recipient1","to:addr:recipient1","to:addr:domain.com","cc:name:recipient2","cc:addr:recipient2","cc:addr:domain.com","to:2**0","to:2**0","body","<eom>" },
                "Triage",
                true,
                attachmentsHelper,
                attachmentInfo,
                65001
            ];
        }

        private object[] GetLazyFields(MailItemHelper helper)
        {
            object[] fields = 
            [
                helper.Item,                    //  0
                helper.Globals,                 //  1
                helper.EntryId,                 //  2
                helper.Sender,                  //  3
                helper.SenderHtml,              //  4
                helper.SenderName,              //  5
                helper.Actionable, 
                helper.Body, 
                helper.ConversationID, 
                helper.EmailPrefixToStrip, 
                helper.StoreId,                 // 10
                helper.FolderName, 
                helper.FolderInfo, 
                helper.HTMLBody, 
                helper.Html,                    // 14
                helper.IsTaskFlagSet,           // 15
                helper.OlRecipients,            // 16
                helper.CcRecipients,            // 17
                helper.ToRecipients,            // 18
                helper.ToRecipientsName,        // 19
                helper.ToRecipientsHtml,
                helper.SentDate, 
                helper.SentOn, 
                helper.Subject, 
                helper.Tokens, 
                helper.Triage, 
                helper.UnRead, 
                helper.AttachmentsHelper,
                helper.AttachmentsInfo, 
                helper.InternetCodepage
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
            var expectedText = $"[\n{string.Join("\n", expected.Select(
                x => x is object ? JsonConvert.SerializeObject(x) : x.ToString()).ToArray())}\n]";
            Console.WriteLine("\nEXPECTED:");
            Console.WriteLine(expectedText);

            // Act
            var helper = new MailItemHelper(item, globals);

            object[] actual = GetLazyFields(helper);
            
            var actualText = $"[\n{string.Join("\n",actual.Select(
                x => x is object ? JsonConvert.SerializeObject(x) : x.ToString()).ToArray())}\n]";
            Console.WriteLine("\nACTUAL:");
            Console.WriteLine(actualText);
            
            // Assert
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.Path.EndsWith("FilePathSaveAlt")));
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
    }
}
