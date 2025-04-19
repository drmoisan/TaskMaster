using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using UtilitiesCS;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class RecipientStaticTests
    {
        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }


        [TestMethod]
        public void ConvertRecipientToHtml_StateUnderTest_ExpectedBehavior()
        {
            // Arrange            
            string name = null;
            string address = null;

            // Act
            var result = RecipientStatic.ConvertRecipientToHtml(
                name,
                address);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetSenderName_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            
            MailItem olMail = null;

            // Act
            var result = RecipientStatic.GetSenderName(
                olMail);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetSenderAddress_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            
            MailItem olMail = null;

            // Act
            var result = RecipientStatic.GetSenderAddress(
                olMail);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetSenderInfo_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            
            MailItem olMail = null;

            // Act
            var result = RecipientStatic.GetSenderInfo(
                olMail);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetRecipients_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            
            MailItem olMail = null;

            // Act
            var result = RecipientStatic.GetRecipients(
                olMail);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetInfo_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
           
            IEnumerable<Recipient> recipients = null;

            // Act
            var result = RecipientStatic.GetInfo(
                recipients);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetInfo_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            
            Recipient recipient = null;
            SegmentStopWatch sw = null;

            // Act
            var result = RecipientStatic.GetInfo(
                recipient,
                sw);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetToRecipientsInHtml_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            
            MailItem olMail = null;

            // Act
            var result = RecipientStatic.GetToRecipientsInHtml(
                olMail);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetToRecipients_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            
            MailItem olMail = null;

            // Act
            var result = RecipientStatic.GetToRecipients(
                olMail);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetCcRecipients_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            
            MailItem olMail = null;

            // Act
            var result = RecipientStatic.GetCcRecipients(
                olMail);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
