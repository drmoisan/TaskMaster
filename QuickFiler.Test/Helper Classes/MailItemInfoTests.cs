using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using UtilitiesCS;
using System;
using Microsoft.Office.Interop.Outlook;

using FluentAssertions;
using Deedle.Internal;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Core;

namespace Z.Unfinished.QuickFiler.Test
{
	[TestClass]
    public class MailItemInfoTests
    {
        private MockRepository mockRepository;
        private Mock<MailItem> mockMailItem;
        private Mock<PropertyAccessor> mockPropertyAccessor;
        private Mock<AddressEntry> mockSender;
        private Mock<UserProperty> mockTriage;
        private Mock<UserProperties> mockUserProperties;
        private DateTime now = DateTime.Now;
        private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";
        private Mock<Folder> mockFolder;
        private Mock<Recipients> mockRecipients;
        private Mock<Recipient> mockRecipient;
        private Mock<AddressEntry> mockRecipientAddress;
        private Mock<PropertyAccessor> mockRecipientPropertyAccessor;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockMailItem = this.mockRepository.Create<MailItem>();

            // Setup sender property assessor
            this.mockRecipientPropertyAccessor = this.mockRepository.Create<PropertyAccessor>();
            this.mockRecipientPropertyAccessor.Setup(x => x.GetProperty(PR_SMTP_ADDRESS)).Returns("recipient.address@domain.com");

            this.mockRecipientAddress = this.mockRepository.Create<AddressEntry>();
            this.mockRecipientAddress.Setup(x => x.Type).Returns("normal");
            this.mockRecipientAddress.Setup(x => x.Name).Returns("RecipientName");
            this.mockRecipientAddress.Setup(x => x.PropertyAccessor).Returns(this.mockRecipientPropertyAccessor.Object);

            // Setup mock recipient
            this.mockRecipient = this.mockRepository.Create<Recipient>();
            this.mockRecipient.Setup(x => x.Type).Returns((int)OlMailRecipientType.olTo);
            this.mockRecipient.Setup(x => x.Name).Returns("RecipientName");
            this.mockRecipient.Setup(x => x.AddressEntry).Returns(this.mockRecipientAddress.Object);
            this.mockRecipient.Setup(x => x.PropertyAccessor).Returns(this.mockRecipientPropertyAccessor.Object);
            List<Recipient> recipientList = new List<Recipient> { this.mockRecipient.Object };
            
            // Setup mock recipients and add to mail item
            this.mockRecipients = this.mockRepository.Create<Recipients>();
            this.mockRecipients.Setup(x => x.Count).Returns(recipientList.Count);
            this.mockRecipients.Setup(x => x[It.IsAny<int>()]).Returns<int>(i => recipientList.ElementAt(i));
            this.mockRecipients.Setup(x => x.GetEnumerator()).Returns(recipientList.GetEnumerator());
            this.mockMailItem.Setup(x => x.Recipients).Returns(this.mockRecipients.Object);

            // Setup mock folder and add to mail item
            this.mockFolder = this.mockRepository.Create<Folder>();
            this.mockFolder.Setup(x => x.Name).Returns("Folder Name");
            this.mockMailItem.Setup(x => x.Parent).Returns(this.mockFolder.Object);
            
            // Setup sender property assessor
            this.mockPropertyAccessor = this.mockRepository.Create<PropertyAccessor>();
            this.mockPropertyAccessor.Setup(x => x.GetProperty(PR_SMTP_ADDRESS)).Returns("sender.address@domain.com");

            // Setup sender
            this.mockSender = this.mockRepository.Create<AddressEntry>();
            this.mockSender.Setup(x => x.Type).Returns("To");
            this.mockSender.Setup(x => x.Name).Returns("SenderName");
            this.mockSender.Setup(x => x.PropertyAccessor).Returns(this.mockPropertyAccessor.Object);
            
            // Add Sender to mail item
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
            this.mockMailItem.Setup(x => x.EntryID).Returns("MockEntryIdNumber");
            this.mockMailItem.Setup(x => x.SentOn).Returns(now);
            this.mockMailItem.Setup(x => x.Sent).Returns(true);
            this.mockMailItem.Setup(x => x.IsMarkedAsTask).Returns(true);
            this.mockMailItem.Setup(x => x.HTMLBody).Returns("HTML Body");
            
        }

        private MailItemInfo CreateMailItemInfo()
        {
            return new MailItemInfo(this.mockMailItem.Object);
        }

        [TestMethod]
        public void SenderName_Get_StateUnderTest_ExpectedBehavior()
        {
            //TODO: Incomplete. Need to finish setting up the mail item mock
            //// Arrange
            //var mailItemInfo = this.CreateMailItemInfo();
            //var expected = "SenderName";

            //// Act
            //var actual = mailItemInfo.SenderName;

            //// Assert
            //Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExtractBasics_StateUnderTest_ExpectedBehavior()
        {
            //TODO: Incomplete. Need to finish setting up the mail item mock
            //// Arrange
            //var mailItemInfo = this.CreateMailItemInfo();
            //var emailPrefixToStrip = "";
            //var expected = new MailItemInfo()
            //{
            //    Item = this.mockMailItem.Object,
            //    SenderName = "SenderName",
            //    Subject = "Subject",
            //    Body = "Body <EOM>",
            //    Triage = "Triage",
            //    SentOn = now.ToString("g"),
            //    Actionable = "Task",
            //    CcRecipientsHtml = "",
            //    CcRecipientsName = "",
            //    ToRecipientsHtml = "",
            //    ToRecipientsName = "",
            //};

            //// Act
            //mailItemInfo.LoadPriority(emailPrefixToStrip);
            //var actual = mailItemInfo;

            //// Assert
            //actual.Should().BeEquivalentTo(expected);
        }
                
    }
}
