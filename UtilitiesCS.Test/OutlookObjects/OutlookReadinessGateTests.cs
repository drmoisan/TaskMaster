using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookStore = Microsoft.Office.Interop.Outlook.Store;

namespace UtilitiesCS.Test.OutlookObjects
{
    /// <summary>
    /// Tests for the store-scoped <see cref="OutlookReadinessGate.IsReady(Store)"/> overload
    /// added by F3 (issue #263). The overload is a cheap, non-throwing probe of a specific store's
    /// default-inbox reachability, reusing the same transient-HRESULT classification as the
    /// pre-existing parameterless probe. All cases are COM-mocked via Moq; no live Outlook.
    /// </summary>
    [TestClass]
    public sealed class OutlookReadinessGateTests
    {
        private static OutlookReadinessGate CreateGate()
        {
            // The store-scoped overload does not touch the Application; a bare mock satisfies the
            // constructor's non-null guard.
            return new OutlookReadinessGate(new Mock<Application>().Object);
        }

        [TestMethod]
        public void IsReady_Store_WhenDefaultInboxReachable_ReturnsTrue()
        {
            // Arrange
            var gate = CreateGate();
            var store = new Mock<OutlookStore>(MockBehavior.Strict);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Returns(new Mock<OutlookFolder>().Object);

            // Act
            var result = gate.IsReady(store.Object);

            // Assert
            result.Should().BeTrue("a reachable default inbox means the store is ready");
        }

        [TestMethod]
        public void IsReady_Store_WhenGetDefaultFolderThrowsTransientComException_ReturnsFalseAndIsTransient()
        {
            // Arrange
            var gate = CreateGate();
            var transient = new COMException(
                "store not ready",
                unchecked((int)OutlookReadinessGate.TransientStoreNotReadyHResult)
            );
            var store = new Mock<OutlookStore>(MockBehavior.Strict);
            store.Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox)).Throws(transient);

            // Act
            var result = gate.IsReady(store.Object);

            // Assert
            result.Should().BeFalse("a transient COMException means not-ready, never throws");
            gate.IsTransientError(transient)
                .Should()
                .BeTrue("the thrown HRESULT is a known transient not-ready code");
        }

        [TestMethod]
        public void IsReady_Store_WhenGetDefaultFolderThrowsNonTransientComException_ReturnsFalseAndIsNotTransient()
        {
            // Arrange
            var gate = CreateGate();
            var nonTransient = new COMException("permanent failure", unchecked((int)0x80004005));
            var store = new Mock<OutlookStore>(MockBehavior.Strict);
            store
                .Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderInbox))
                .Throws(nonTransient);

            // Act
            var result = gate.IsReady(store.Object);

            // Assert
            result.Should().BeFalse("any COMException means not-ready, never throws");
            gate.IsTransientError(nonTransient)
                .Should()
                .BeFalse("E_FAIL is not a known transient not-ready code");
        }

        [TestMethod]
        public void IsReady_Store_WhenStoreIsNull_ReturnsFalse()
        {
            // Arrange
            var gate = CreateGate();

            // Act
            var result = gate.IsReady((OutlookStore)null);

            // Assert
            result.Should().BeFalse("a null store is not ready and the probe never throws");
        }
    }
}
