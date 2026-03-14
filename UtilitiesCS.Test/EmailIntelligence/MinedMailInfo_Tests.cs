using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class MinedMailInfo_Tests
    {
        [TestMethod]
        public void Constructor_WithNoArguments_LeavesPropertiesAtDefaults()
        {
            // Arrange
            var minedMailInfo = new MinedMailInfo();

            // Assert
            minedMailInfo.Categories.Should().BeNull();
            minedMailInfo.Tokens.Should().BeNull();
            minedMailInfo.FolderInfo.Should().BeNull();
            minedMailInfo.ToRecipients.Should().BeNull();
            minedMailInfo.CcRecipients.Should().BeNull();
            minedMailInfo.Sender.Should().BeNull();
            minedMailInfo.ConversationId.Should().BeNull();
            minedMailInfo.EntryId.Should().BeNull();
            minedMailInfo.StoreId.Should().BeNull();
            minedMailInfo.Subject.Should().BeNull();
            minedMailInfo.Actionable.Should().BeNull();
            minedMailInfo.GroupingKey.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithItemInfo_MapsAllSupportedProperties()
        {
            // Arrange
            var folder = Mock.Of<IFolderWrapper>();
            var sender = Mock.Of<IRecipientInfo>();
            var toRecipients = new[] { Mock.Of<IRecipientInfo>(), Mock.Of<IRecipientInfo>() };
            var ccRecipients = new[] { Mock.Of<IRecipientInfo>() };
            var tokens = new[] { "alpha", "beta" };

            var itemInfo = new Mock<IItemInfo>(MockBehavior.Strict);
            itemInfo.SetupGet(x => x.Categories).Returns("CatA;CatB");
            itemInfo.SetupGet(x => x.Tokens).Returns(tokens);
            itemInfo.SetupGet(x => x.FolderInfo).Returns(folder);
            itemInfo.SetupGet(x => x.ToRecipients).Returns(toRecipients);
            itemInfo.SetupGet(x => x.CcRecipients).Returns(ccRecipients);
            itemInfo.SetupGet(x => x.Sender).Returns(sender);
            itemInfo.SetupGet(x => x.ConversationID).Returns("conversation-id");
            itemInfo.SetupGet(x => x.EntryId).Returns("entry-id");
            itemInfo.SetupGet(x => x.StoreId).Returns("store-id");
            itemInfo.SetupGet(x => x.Subject).Returns("Subject line");
            itemInfo.SetupGet(x => x.Actionable).Returns("Yes");

            // Act
            var minedMailInfo = new MinedMailInfo(itemInfo.Object);

            // Assert
            minedMailInfo.Categories.Should().Be("CatA;CatB");
            minedMailInfo.Tokens.Should().BeSameAs(tokens);
            minedMailInfo.FolderInfo.Should().BeSameAs(folder);
            minedMailInfo.ToRecipients.Should().BeSameAs(toRecipients);
            minedMailInfo.CcRecipients.Should().BeSameAs(ccRecipients);
            minedMailInfo.Sender.Should().BeSameAs(sender);
            minedMailInfo.ConversationId.Should().Be("conversation-id");
            minedMailInfo.EntryId.Should().Be("entry-id");
            minedMailInfo.StoreId.Should().Be("store-id");
            minedMailInfo.Subject.Should().Be("Subject line");
            minedMailInfo.Actionable.Should().Be("Yes");
        }

        [TestMethod]
        public void Clone_ReturnsDistinctInstanceWithSharedReferenceMembers()
        {
            // Arrange
            var original = CreatePopulatedMinedMailInfo();

            // Act
            var clone = (MinedMailInfo)original.Clone();

            // Assert
            clone.Should().NotBeSameAs(original);
            clone.Categories.Should().Be(original.Categories);
            clone.Tokens.Should().BeSameAs(original.Tokens);
            clone.FolderInfo.Should().BeSameAs(original.FolderInfo);
            clone.ToRecipients.Should().BeSameAs(original.ToRecipients);
            clone.CcRecipients.Should().BeSameAs(original.CcRecipients);
            clone.Sender.Should().BeSameAs(original.Sender);
            clone.ConversationId.Should().Be(original.ConversationId);
            clone.EntryId.Should().Be(original.EntryId);
            clone.StoreId.Should().Be(original.StoreId);
            clone.Subject.Should().Be(original.Subject);
            clone.Actionable.Should().Be(original.Actionable);
        }

        [TestMethod]
        public void DeepCopy_CreatesNewArraysWhilePreservingValues()
        {
            // Arrange
            var original = CreatePopulatedMinedMailInfo();
            var originalToRecipient = original.ToRecipients[0];
            var originalCcRecipient = original.CcRecipients[0];

            // Act
            var copy = original.DeepCopy();
            copy.Tokens[0] = "changed-token";
            copy.ToRecipients[0] = Mock.Of<IRecipientInfo>();
            copy.CcRecipients[0] = Mock.Of<IRecipientInfo>();

            // Assert
            copy.Should().NotBeSameAs(original);
            copy.Tokens.Should().NotBeSameAs(original.Tokens);
            copy.ToRecipients.Should().NotBeSameAs(original.ToRecipients);
            copy.CcRecipients.Should().NotBeSameAs(original.CcRecipients);
            copy.FolderInfo.Should().BeSameAs(original.FolderInfo);
            copy.Sender.Should().BeSameAs(original.Sender);
            original.Tokens[0].Should().Be("alpha");
            original.ToRecipients[0].Should().BeSameAs(originalToRecipient);
            original.CcRecipients[0].Should().BeSameAs(originalCcRecipient);
            copy.Categories.Should().Be(original.Categories);
            copy.ConversationId.Should().Be(original.ConversationId);
            copy.EntryId.Should().Be(original.EntryId);
            copy.StoreId.Should().Be(original.StoreId);
            copy.Subject.Should().Be(original.Subject);
            copy.Actionable.Should().Be(original.Actionable);
            copy.GroupingKey.Should().Be(original.GroupingKey);
        }

        [TestMethod]
        public void JsonRoundTrip_WithNullFields_PreservesSerializableState()
        {
            // Arrange
            var original = new MinedMailInfo
            {
                Categories = null,
                Tokens = null,
                FolderInfo = null,
                ToRecipients = null,
                CcRecipients = null,
                Sender = null,
                ConversationId = "conversation-id",
                EntryId = "entry-id",
                StoreId = "store-id",
                Subject = "Subject line",
                Actionable = null,
            };

            // Act
            var json = JsonConvert.SerializeObject(original);
            var roundTrip = JsonConvert.DeserializeObject<MinedMailInfo>(json);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip.Categories.Should().BeNull();
            roundTrip.Tokens.Should().BeNull();
            roundTrip.FolderInfo.Should().BeNull();
            roundTrip.ToRecipients.Should().BeNull();
            roundTrip.CcRecipients.Should().BeNull();
            roundTrip.Sender.Should().BeNull();
            roundTrip.ConversationId.Should().Be("conversation-id");
            roundTrip.EntryId.Should().Be("entry-id");
            roundTrip.StoreId.Should().Be("store-id");
            roundTrip.Subject.Should().Be("Subject line");
            roundTrip.Actionable.Should().BeNull();
        }

        private static MinedMailInfo CreatePopulatedMinedMailInfo()
        {
            var folder = Mock.Of<IFolderWrapper>();
            var sender = Mock.Of<IRecipientInfo>();
            var ccRecipient = Mock.Of<IRecipientInfo>();

            return new MinedMailInfo
            {
                Categories = "CatA;CatB",
                Tokens = new[] { "alpha", "beta" },
                FolderInfo = folder,
                ToRecipients = new[] { sender },
                CcRecipients = new[] { ccRecipient },
                Sender = sender,
                ConversationId = "conversation-id",
                EntryId = "entry-id",
                StoreId = "store-id",
                Subject = "Subject line",
                Actionable = "Yes",
                GroupingKey = "group-key",
            };
        }
    }
}