using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            addressEntry
                .Setup(x => x.GetExchangeUser())
                .Throws(new COMException("The operation failed."));
            addressEntry.SetupGet(x => x.Address).Throws(new COMException("The operation failed."));

            // Act
            var result = AppOlObjects.TryGetSmtpAddress(addressEntry.Object);

            // Assert
            result.Should().BeNull();
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void ReadJunkPotentialSetting_ReturnsJunkPotentialValue()
        {
            // Arrange
            var original = Properties.Settings.Default.JunkPotential;
            var expected = "Inbox\\Junk Suspects SB";
            Properties.Settings.Default.JunkPotential = expected;

            try
            {
                // Act
                var result = AppOlObjects.ReadJunkPotentialSetting();

                // Assert
                result.Should().Be(expected);
            }
            finally
            {
                Properties.Settings.Default.JunkPotential = original;
            }
        }

        [TestMethod]
        public void WriteJunkPotentialSetting_UpdatesJunkPotentialValue()
        {
            // Arrange
            var original = Properties.Settings.Default.JunkPotential;
            var expected = "Inbox\\Junk Suspects SB";

            try
            {
                // Act
                AppOlObjects.WriteJunkPotentialSetting(expected);

                // Assert
                Properties.Settings.Default.JunkPotential.Should().Be(expected);
            }
            finally
            {
                Properties.Settings.Default.JunkPotential = original;
            }
        }
    }
}
