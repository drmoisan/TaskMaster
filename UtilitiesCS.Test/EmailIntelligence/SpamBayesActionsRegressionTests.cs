using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class SpamBayesActionsRegressionTests
    {
        [TestMethod]
        public void GetDestinationFolder_WhenSpamTrueAndJunkCertainExists_ReturnsConfiguredJunkCertain()
        {
            var configuredJunkCertain = new Mock<Folder>(MockBehavior.Loose);
            var spamBayes = CreateSpamBayes(configuredJunkCertain.Object);
            var currentParent = new Mock<Folder>(MockBehavior.Loose);
            var mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem.SetupGet(item => item.Parent).Returns(currentParent.Object);

            var result = spamBayes.GetDestinationFolder(mailItem.Object, isSpam: true);

            result.Should().BeSameAs(configuredJunkCertain.Object);
        }

        [TestMethod]
        public void GetDestinationFolder_WhenSpamTrueAndJunkCertainIsNull_ReturnsCurrentParent()
        {
            var spamBayes = CreateSpamBayes(junkCertain: null);
            var currentParent = new Mock<Folder>(MockBehavior.Loose);
            var mailItem = new Mock<MailItem>(MockBehavior.Loose);
            mailItem.SetupGet(item => item.Parent).Returns(currentParent.Object);

            var result = spamBayes.GetDestinationFolder(mailItem.Object, isSpam: true);

            result.Should().BeSameAs(currentParent.Object);
        }

        private static SpamBayes CreateSpamBayes(Folder junkCertain)
        {
            var outlookObjects = new Mock<IOlObjects>(MockBehavior.Loose);
            outlookObjects.SetupGet(objects => objects.JunkCertain).Returns(junkCertain);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            globals
                .SetupGet(applicationGlobals => applicationGlobals.Ol)
                .Returns(outlookObjects.Object);
            return new SpamBayes(globals.Object);
        }
    }
}
