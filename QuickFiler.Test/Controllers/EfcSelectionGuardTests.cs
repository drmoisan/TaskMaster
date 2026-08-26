using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #614 tests for <see cref="EfcSelectionGuard"/> (defect D9): the single predicate
    /// shared by the EFC OK action and its selection validation. The predicate is pure, so no
    /// collaborator requires mocking and Moq is deliberately not used here.
    /// </summary>
    [TestClass]
    public class EfcSelectionGuardTests
    {
        [TestMethod]
        public void IsValidFilingSelection_NullSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection(null).Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_EmptySelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection(string.Empty).Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_WhitespaceSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection("    ").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_BannerSentinel_IsRejected()
        {
            // Arrange / Act / Assert: the suggestion banner is not a filing destination.
            EfcSelectionGuard.IsValidFilingSelection("==== SUGGESTIONS ====").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_StoreRootedSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\\mailbox@example.com")
                .Should()
                .BeFalse("a store-rooted Outlook path is not an archive-relative stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive\Clients")
                .Should()
                .BeFalse("a rooted value is not an archive-relative stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_DriveRootedSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"C:\Users\testuser\OneDrive - Contoso")
                .Should()
                .BeFalse("a drive-rooted filesystem path is not an archive-relative stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_TwoCharacterSelection_IsRejected()
        {
            // Arrange / Act / Assert: preserves the prior validation path's strictness.
            EfcSelectionGuard.IsValidFilingSelection("AB").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_ValidRelativeStem_IsAccepted()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection(@"Clients\North").Should().BeTrue();
        }
    }
}
