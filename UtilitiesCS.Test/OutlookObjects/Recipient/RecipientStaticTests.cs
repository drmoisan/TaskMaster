using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            result.Should().Be(" &lt;<a href=\"mailto:\"></a>&gt;");
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetSenderName_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            MailItem olMail = null;

            // Assert
            System.Action act = () => RecipientStatic.GetSenderName(olMail);
            act.Should().Throw<NullReferenceException>();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetSenderAddress_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            MailItem olMail = null;

            // Assert
            System.Action act = () => RecipientStatic.GetSenderAddress(olMail);
            act.Should().Throw<NullReferenceException>();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetSenderInfo_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            MailItem olMail = null;

            // Assert
            System.Action act = () => RecipientStatic.GetSenderInfo(olMail);
            act.Should().Throw<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetRecipients_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            MailItem olMail = null;

            // Assert
            System.Action act = () => RecipientStatic.GetRecipients(olMail);
            act.Should().Throw<NullReferenceException>();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetInfo_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            IEnumerable<Recipient> recipients = null;

            // Assert
            System.Action act = () => RecipientStatic.GetInfo(recipients).ToList();
            act.Should().Throw<ArgumentNullException>();
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetInfo_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange

            Recipient recipient = null;
            SegmentStopWatch sw = null;

            // Assert
            System.Action act = () => RecipientStatic.GetInfo(recipient, sw);
            act.Should().Throw<NullReferenceException>();
            this.mockRepository.VerifyAll();
        }
    }
}
