using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Runtime.InteropServices;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public class AppOlObjectsTests
    {
        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsNull_WhenAddressEntryIsNull()
        {
            // Arrange
            AddressEntry addressEntry = null;

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsExchangePrimarySmtpAddress_WhenAvailable()
        {
            // Arrange
            var expectedAddress = "user@contoso.com";
            var exchangeUser = mockRepository.Create<ExchangeUser>();
            exchangeUser.SetupGet(x => x.PrimarySmtpAddress).Returns(expectedAddress);

            var addressEntry = mockRepository.Create<AddressEntry>();
            addressEntry.Setup(x => x.GetExchangeUser()).Returns(exchangeUser.Object);

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry.Object);

            // Assert
            result.Should().Be(expectedAddress);
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsAddressProperty_WhenExchangeUserIsUnavailable()
        {
            // Arrange
            var expectedAddress = "fallback@contoso.com";
            var addressEntry = mockRepository.Create<AddressEntry>();
            addressEntry.Setup(x => x.GetExchangeUser()).Returns((ExchangeUser)null);
            addressEntry.SetupGet(x => x.Address).Returns(expectedAddress);

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry.Object);

            // Assert
            result.Should().Be(expectedAddress);
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TryGetSmtpAddress_ReturnsNull_WhenOutlookInteropCallsThrowComException()
        {
            // Arrange
            var addressEntry = mockRepository.Create<AddressEntry>();
            addressEntry.Setup(x => x.GetExchangeUser()).Throws(new COMException("The operation failed."));
            addressEntry.SetupGet(x => x.Address).Throws(new COMException("The operation failed."));

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry.Object);

            // Assert
            result.Should().BeNull();
            mockRepository.VerifyAll();
        }
    }
}
