using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Store;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Unit tests for <see cref="StoreIdentity"/> (issue #261). Covers the pure resolver contract
    /// (DisplayName primary, FilePath fallback, documented sentinel, casing preserved) and the COM
    /// convenience overload, which is exercised with a <c>Mock&lt;Outlook.Store&gt;</c> only (never a
    /// live Outlook process) and never touches the filesystem.
    /// </summary>
    [TestClass]
    public class StoreIdentityTests
    {
        [TestMethod]
        public void Resolve_WhenDisplayNamePresent_ReturnsDisplayName()
        {
            // Arrange / Act
            var identity = StoreIdentity.Resolve("Mailbox", @"C:\Data\mailbox.ost");

            // Assert: DisplayName is primary and wins over the fallback.
            identity.Value.Should().Be("Mailbox");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Resolve_WhenDisplayNameNullOrWhitespaceAndFallbackPresent_ReturnsFallback(
            string displayName
        )
        {
            // Arrange / Act
            var identity = StoreIdentity.Resolve(displayName, @"C:\Data\mailbox.ost");

            // Assert: the fallback is used only when DisplayName is null/whitespace.
            identity.Value.Should().Be(@"C:\Data\mailbox.ost");
        }

        [TestMethod]
        [DataRow(null, null)]
        [DataRow("", "   ")]
        [DataRow("   ", "")]
        public void Resolve_WhenBothAbsent_ReturnsDocumentedSentinel(
            string displayName,
            string fallback
        )
        {
            // Arrange / Act
            var identity = StoreIdentity.Resolve(displayName, fallback);

            // Assert: an unresolvable store resolves only to the documented sentinel, which is not
            // string.Empty.
            identity.Value.Should().Be(StoreIdentity.UnresolvedSentinel);
            identity.Value.Should().NotBe(string.Empty);
        }

        [TestMethod]
        public void Resolve_PreservesCasingOfResolvedValue()
        {
            // Arrange / Act
            var identity = StoreIdentity.Resolve("MixedCaseStore");

            // Assert: the resolved value preserves original casing (case-insensitivity is applied by
            // the collections that hold identities, not by Resolve).
            identity.Value.Should().Be("MixedCaseStore");
        }

        [TestMethod]
        public void ResolveStore_WhenFilePathAccessThrows_StillReturnsDisplayName()
        {
            // Arrange: a store whose DisplayName is available but whose FilePath read throws (the
            // blocking-COM guard the epic prohibits). The guarded read must be swallowed.
            var store = new Mock<Outlook.Store>();
            store.SetupGet(x => x.DisplayName).Returns("Mailbox");
            store
                .SetupGet(x => x.FilePath)
                .Throws(new InvalidOperationException("FilePath unavailable"));

            // Act
            var identity = StoreIdentity.Resolve(store.Object);

            // Assert
            identity.Value.Should().Be("Mailbox");
        }

        [TestMethod]
        public void ResolveStore_WhenDisplayNameAndFilePathThrow_ReturnsSentinel()
        {
            // Arrange: both COM reads throw, so neither a DisplayName nor a FilePath is available.
            var store = new Mock<Outlook.Store>();
            store.SetupGet(x => x.DisplayName).Throws(new InvalidOperationException("no name"));
            store.SetupGet(x => x.FilePath).Throws(new InvalidOperationException("no path"));

            // Act
            var identity = StoreIdentity.Resolve(store.Object);

            // Assert
            identity.Value.Should().Be(StoreIdentity.UnresolvedSentinel);
        }

        [TestMethod]
        public void ResolveStore_WhenDisplayNameEmptyAndFilePathPresent_ReturnsFilePath()
        {
            // Arrange: DisplayName is whitespace, so the guarded FilePath is used as the fallback.
            var store = new Mock<Outlook.Store>();
            store.SetupGet(x => x.DisplayName).Returns("   ");
            store.SetupGet(x => x.FilePath).Returns(@"C:\Data\fallback.ost");

            // Act
            var identity = StoreIdentity.Resolve(store.Object);

            // Assert
            identity.Value.Should().Be(@"C:\Data\fallback.ost");
        }
    }
}
