using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using System;
using Microsoft.Office.Interop.Outlook;
using ToDoModel;
using FluentAssertions;

namespace QuickFiler.Test
{
	[TestClass]
    public class MailItemInfoTests
    {
        private MockRepository mockRepository;
        private Mock<MailItem> mockMailItem;
        private Mock<AddressEntry> mockSender;
        private Mock<UserProperty> mockTriage;
        private Mock<UserProperties> mockUserProperties;
        private DateTime now = DateTime.Now;




        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockMailItem = this.mockRepository.Create<MailItem>();
            
            // Setup sender
            this.mockSender = this.mockRepository.Create<AddressEntry>();
            this.mockSender.Setup(x => x.Type).Returns("normal");
            this.mockSender.Setup(x => x.Name).Returns("SenderName");
            // Add to mail item
            this.mockMailItem.Setup(x => x.Sender).Returns(this.mockSender.Object);

            this.mockMailItem.Setup(x => x.Subject).Returns("Subject");
            this.mockMailItem.Setup(x => x.Body).Returns("Body");

            // Create triage user property
            this.mockTriage = this.mockRepository.Create<UserProperty>();
            this.mockTriage.Setup(x => x.Value).Returns("Triage");

            // Create user properties and add triage to it
            this.mockUserProperties = this.mockRepository.Create<UserProperties>();
            this.mockUserProperties.Setup(x => x["Triage"]).Returns(this.mockTriage.Object);
            this.mockUserProperties.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                                   .Returns<string, object>((a, b) =>
            {
                if(a == "Triage") { return this.mockTriage.Object; }
                return null;
            });

            // Add user properties to mail item
            this.mockMailItem.Setup(x => x.UserProperties).Returns(this.mockUserProperties.Object);
            
            this.mockMailItem.Setup(x => x.SentOn).Returns(now);
            this.mockMailItem.Setup(x => x.Sent).Returns(true);
            this.mockMailItem.Setup(x => x.IsMarkedAsTask).Returns(true);
        }

        private MailItemInfo CreateMailItemInfo()
        {
            return new MailItemInfo(this.mockMailItem.Object);
        }

        [TestMethod]
        public void SenderName_Get_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mailItemInfo = this.CreateMailItemInfo();
            var expected = "SenderName";

            // Act
            var actual = mailItemInfo.Sender;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExtractBasics_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var mailItemInfo = this.CreateMailItemInfo();
            var expected = new MailItemInfo()
            {
                Item = this.mockMailItem.Object,
                Sender = "SenderName",
                Subject = "Subject",
                Body = "Body",
                Triage = "Triage",
                SentOn = now.ToString("g"),
                Actionable = "Triage"
            };

            // Act
            var result = mailItemInfo.ExtractBasics();
            var actual = mailItemInfo;

            // Assert
            Assert.IsTrue(result);
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
