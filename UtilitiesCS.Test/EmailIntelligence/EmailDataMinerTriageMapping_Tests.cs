using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class EmailDataMinerTriageMapping_Tests
    {
        [DataTestMethod]
        [DataRow("A")]
        [DataRow("B")]
        [DataRow("C")]
        [DataRow(null)]
        public async Task ToMinedMail_TriageValue_PreservesValue(string triage)
        {
            // Arrange
            var item = new Mock<IItemInfo>(MockBehavior.Strict);
            item.SetupGet(value => value.Categories).Returns("Blue");
            item.SetupGet(value => value.Tokens).Returns(new[] { "alpha", "beta" });
            item.SetupGet(value => value.FolderInfo).Returns((IFolderWrapper)null);
            item.SetupGet(value => value.ToRecipients).Returns(Array.Empty<IRecipientInfo>());
            item.SetupGet(value => value.CcRecipients).Returns(Array.Empty<IRecipientInfo>());
            item.SetupGet(value => value.Sender).Returns((IRecipientInfo)null);
            item.SetupGet(value => value.ConversationID).Returns("conversation");
            item.SetupGet(value => value.EntryId).Returns("entry");
            item.SetupGet(value => value.StoreId).Returns("store");
            item.SetupGet(value => value.Subject).Returns("subject");
            item.SetupGet(value => value.Actionable).Returns("Yes");
            item.SetupGet(value => value.Triage).Returns(triage);
            var miner = new EmailDataMiner(
                new Mock<IApplicationGlobals>(MockBehavior.Loose).Object
            );

            // Act
            var result = await miner.ToMinedMail(new[] { item.Object });

            // Assert
            result.Should().ContainSingle();
            result[0].Triage.Should().Be(triage);
        }
    }

    public partial class EmailDataMiner_Tests
    {
        [TestMethod]
        public async Task ToMinedMail_WhenItemsProvided_ProjectsItemFieldsIntoSerializableModels()
        {
            // Arrange
            var folder = new FolderWrapper(true, 1, 10, "Inbox", "root/inbox");
            var item = new Mock<IItemInfo>(MockBehavior.Strict);
            item.SetupGet(value => value.Categories).Returns("Blue");
            item.SetupGet(value => value.Tokens).Returns(new[] { "alpha", "beta" });
            item.SetupGet(value => value.FolderInfo).Returns(folder);
            item.SetupGet(value => value.ToRecipients).Returns(Array.Empty<IRecipientInfo>());
            item.SetupGet(value => value.CcRecipients).Returns(Array.Empty<IRecipientInfo>());
            item.SetupGet(value => value.Sender).Returns((IRecipientInfo)null);
            item.SetupGet(value => value.ConversationID).Returns("conversation");
            item.SetupGet(value => value.EntryId).Returns("entry");
            item.SetupGet(value => value.StoreId).Returns("store");
            item.SetupGet(value => value.Subject).Returns("subject");
            item.SetupGet(value => value.Actionable).Returns("Yes");
            item.SetupGet(value => value.Triage).Returns("B");

            var miner = new EmailDataMiner(new StubGlobals());

            // Act
            var result = await miner.ToMinedMail(new[] { item.Object });

            // Assert
            result.Should().ContainSingle();
            result[0].FolderInfo.Should().BeSameAs(folder);
            result[0].Tokens.Should().Equal("alpha", "beta");
            result[0].Subject.Should().Be("subject");
            result[0].Actionable.Should().Be("Yes");
            result[0].Triage.Should().Be("B");
        }
    }
}
