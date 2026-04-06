using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.Dialogs;
using OutlookApplication = Microsoft.Office.Interop.Outlook.Application;

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

        [TestMethod]
        public void LoadJunkCertain_KeepsStoredValue_WhenReplacementSelectionIsCancelled()
        {
            var originalSetting = Properties.Settings.Default.OlJunkCertain;
            var dialogInvokerProperty = typeof(MyBox).GetProperty(
                "DialogInvoker",
                BindingFlags.Static | BindingFlags.NonPublic
            )!;
            var originalDialogInvoker = dialogInvokerProperty.GetValue(null);
            var expected = "Missing\\Junk Email";
            var namespaceMapi = mockRepository.Create<NameSpace>();
            namespaceMapi.Setup(x => x.PickFolder()).Returns((MAPIFolder)null);
            var application = mockRepository.Create<OutlookApplication>();
            application.SetupGet(x => x.Application).Returns(application.Object);
            application.Setup(x => x.GetNamespace("MAPI")).Returns(namespaceMapi.Object);
            var root = CreateRootFolder();
            var sut = new AppOlObjects(application.Object, Mock.Of<IApplicationGlobals>());
            SetPrivateField(sut, "_root", root.Object);

            try
            {
                Properties.Settings.Default.OlJunkCertain = expected;
                dialogInvokerProperty.SetValue(
                    null,
                    new Func<MyBoxViewer, System.Windows.Forms.DialogResult>(_ =>
                        System.Windows.Forms.DialogResult.OK
                    )
                );

                sut.LoadJunkCertain().Should().BeNull();
                AppOlObjects.ReadJunkCertainSetting().Should().Be(expected);
            }
            finally
            {
                Properties.Settings.Default.OlJunkCertain = originalSetting;
                dialogInvokerProperty.SetValue(null, originalDialogInvoker);
            }
        }

        private Mock<Folder> CreateRootFolder()
        {
            var folders = mockRepository.Create<Folders>();
            folders.SetupGet(x => x.Count).Returns(0);
            folders
                .As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(Array.Empty<MAPIFolder>().GetEnumerator());

            var root = mockRepository.Create<Folder>();
            root.SetupGet(x => x.Name).Returns("Mailbox");
            root.SetupGet(x => x.FolderPath).Returns(@"\\Mailbox");
            root.SetupGet(x => x.Folders).Returns(folders.Object);
            return root;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            typeof(AppOlObjects)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(target, value);
        }
    }
}
