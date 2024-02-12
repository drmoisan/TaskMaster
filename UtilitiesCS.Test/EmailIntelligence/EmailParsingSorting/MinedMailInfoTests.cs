using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence.Bayesian;
using Microsoft.Office.Interop.Outlook;
using FluentAssertions;
using Newtonsoft.Json;
using System;

namespace UtilitiesCS.Test.EmailIntelligence.EmailParsingSorting
{
    [TestClass]
    public class MinedMailInfoTests
    {
        private MockRepository mockRepository;
        private Mock<IItemInfo> mockIItemInfo;
        private Mock<IFolderInfo> mockOlFolderInfo;
        private Mock<Folder> mockFolder;
        private Mock<Folder> mockFolderRoot;
        private Mock<RecipientInfo> mockSender;
        private RecipientInfo[] ccRecipients;
        private RecipientInfo[] toRecipients;
        private string categories;
        private string[] tokens;
        private string conversationId;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockFolder = this.CreateMockFolder("FolderL1\\FolderL2\\FolderL3\\FolderName");
            this.mockFolderRoot = this.CreateMockFolder("FolderL1");
            this.mockOlFolderInfo = this.CreateMockOlFolderInfo(this.mockFolder.Object, this.mockFolderRoot.Object);
            this.categories = "Category1, Category2, Category3";
            this.tokens = ["Token1", "Token2", "Token3"];
            this.conversationId = "1A2B3C4DMockConversationId5E6F7G";
            this.mockSender = this.CreateMockRecipientInfo("sender", "sender@domain.com");
            (this.ccRecipients, this.toRecipients) = this.CreateMockRecipients();
            this.mockIItemInfo = CreateMockIItemInfo(this.mockOlFolderInfo.Object, this.mockSender.Object, 
                this.ccRecipients, this.toRecipients, this.conversationId);
        }

        private (RecipientInfo[] ccRecipients, RecipientInfo[] toRecipients) CreateMockRecipients()
        {
            var ccRecipients = new RecipientInfo[]
            {
                this.CreateMockRecipientInfo("cc1", "cc1@domain1.com").Object,
                this.CreateMockRecipientInfo("cc2", "cc1@domain2.com").Object,
            };

            var toRecipients = new RecipientInfo[]
            {
                this.CreateMockRecipientInfo("to1", "cc1@domain1.com").Object,
                this.CreateMockRecipientInfo("to2", "cc1@domain2.com").Object,
            };
            return (ccRecipients, toRecipients);
        }
        
        private Mock<RecipientInfo> CreateMockRecipientInfo(string name, string address)
        {
            var recipientInfo = this.mockRepository.Create<RecipientInfo>();
            recipientInfo.SetupAllProperties();
            recipientInfo.Setup(x => x.Name).Returns(name);
            recipientInfo.Setup(x => x.Address).Returns(address);
            recipientInfo.Setup(x => x.Html).Returns($"<a href=\"mailto:{address}\">{name}</a>");
            return recipientInfo;
        }
        
        private Mock<Folder> CreateMockFolder(string folderPath)
        {
            var folder = this.mockRepository.Create<Folder>();
            folder.SetupAllProperties();
            folder.Setup(x => x.FolderPath).Returns(folderPath);
            var folderName = folderPath.Substring(folderPath.LastIndexOf("\\") + 1);
            folder.Setup(x => x.Name).Returns(folderName);
            return folder;
        }
        
        private Mock<IFolderInfo> CreateMockOlFolderInfo(Folder folder, Folder folderRoot)
        {
            var item = this.mockRepository.Create<IFolderInfo>();
            item.SetupAllProperties();
            item.Setup(x => x.OlFolder).Returns(folder);
            item.Setup(x => x.FolderSize).Returns(1000000);
            item.Setup(x => x.ItemCount).Returns(100);
            item.Setup(x => x.Name).Returns(folder.Name);            
            item.Setup(x => x.OlRoot).Returns(folderRoot);            
            var relativePath = folder.FolderPath.Replace(folderRoot.FolderPath, "");
            while (relativePath.StartsWith("\\")) { relativePath = relativePath.Substring(1); }
            item.Setup(x => x.RelativePath).Returns(relativePath);
            item.Setup(x => x.Selected).Returns(true);
            item.Setup(x => x.SubscriptionStatus).Returns(IFolderInfo.PropertyEnum.All);
            return item;
        }

        private Mock<IItemInfo> CreateMockIItemInfo(
            IFolderInfo folderInfo, RecipientInfo sender, RecipientInfo[] ccRecipients, 
            RecipientInfo[] toRecipients, string conversationId)
        {
            var itemInfo = this.mockRepository.Create<IItemInfo>();
            itemInfo.SetupAllProperties();
            itemInfo.Setup(x => x.Categories).Returns("Category1, Category2, Category3");
            itemInfo.Setup(x => x.Tokens).Returns(["Token1", "Token2", "Token3" ]);
            itemInfo.Setup(x => x.FolderInfo).Returns(folderInfo);
            itemInfo.Setup(x => x.Body).Returns("Body Text");
            itemInfo.Setup(x => x.CcRecipients).Returns(ccRecipients);
            itemInfo.Setup(x => x.ToRecipients).Returns(toRecipients);
            itemInfo.Setup(x => x.ConversationID).Returns(conversationId);
            itemInfo.Setup(x => x.Sender).Returns(sender);
            return itemInfo;
        }

        [TestMethod]
        public void ConstructorTest()
        {
            // Arrange
            var expected = new MinedMailInfo();
            expected.CcRecipients = this.ccRecipients;
            expected.ToRecipients = this.toRecipients;
            expected.Categories = this.categories;
            expected.Tokens = this.tokens;
            expected.FolderInfo = this.mockOlFolderInfo.Object;
            expected.ConversationId = this.conversationId;
            expected.Sender = this.mockSender.Object; 

            // Act
            var actual = new MinedMailInfo(this.mockIItemInfo.Object);
            Console.WriteLine($"Expected Object:\n{JsonConvert.SerializeObject(expected)}");
            Console.WriteLine($"Actual Object:\n{JsonConvert.SerializeObject(actual)}");

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
