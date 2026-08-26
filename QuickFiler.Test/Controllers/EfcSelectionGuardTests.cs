using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #614 tests for <see cref="EfcSelectionGuard"/> (defect D9) and its remediation
    /// cycle 1 corrections: the filing predicate, the folder-creation predicate, and the
    /// throw-tolerant archive-root resolver. The predicates are pure and the resolver takes
    /// delegate seams supplied inline, so no collaborator requires mocking and Moq is
    /// deliberately not used here.
    /// </summary>
    [TestClass]
    public class EfcSelectionGuardTests
    {
        #region Filing predicate

        [TestMethod]
        public void IsValidFilingSelection_NullSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection(null, @"\Archive").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_EmptySelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection(string.Empty, @"\Archive").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_WhitespaceSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection("    ", @"\Archive").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_BannerSentinel_IsRejected()
        {
            // Arrange / Act / Assert: the suggestion banner is not a filing destination.
            EfcSelectionGuard
                .IsValidFilingSelection("==== SUGGESTIONS ====", @"\Archive")
                .Should()
                .BeFalse();
        }

        [TestMethod]
        public void IsValidFilingSelection_StoreRootedSelection_IsRejected()
        {
            // Arrange / Act / Assert: D1/D9 protection - a store root is not under the archive.
            EfcSelectionGuard
                .IsValidFilingSelection(@"\\mailbox@example.com", @"\Archive")
                .Should()
                .BeFalse("a store-rooted Outlook path is not resolvable against the archive root");
        }

        [TestMethod]
        public void IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected()
        {
            // Arrange / Act / Assert: with no resolvable root every rooted value is rejected,
            // which is the ResolveArchiveRootOrEmpty degrade path.
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive\Clients", null)
                .Should()
                .BeFalse("a rooted value cannot be resolved when no archive root is available");
        }

        [TestMethod]
        public void IsValidFilingSelection_DriveRootedSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"C:\Users\testuser\OneDrive - Contoso", @"\Archive")
                .Should()
                .BeFalse("a drive-rooted filesystem path is not an archive-relative stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_ValidRelativeStem_IsAccepted()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"Clients\North", @"\Archive")
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted()
        {
            // CR-1 regression: filing to a two-character archive folder worked before #614 and
            // must keep working. The minimum-length rule belongs to folder creation only.
            foreach (string name in new[] { "HR", "IT", "PR", "QA", "Q1" })
            {
                // Arrange / Act / Assert
                EfcSelectionGuard
                    .IsValidFilingSelection(name, @"\Archive")
                    .Should()
                    .BeTrue($"filing to the archive folder '{name}' must remain possible");
            }
        }

        [TestMethod]
        public void IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted()
        {
            // CR-1 regression: the shortest possible relative stem is still a valid destination.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection("A", @"\Archive")
                .Should()
                .BeTrue("filing to the archive folder 'A' must remain possible");
        }

        [TestMethod]
        public void IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsAccepted()
        {
            // CR-2 regression: the guard must agree with BreadcrumbBridgeRouter.SelectRow, which
            // is scope-pinned to pass an at-or-under-root rooted target through verbatim. This is
            // the same case-insensitive value asserted by
            // Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\aRcHiVe\Clients\North", @"\Archive")
                .Should()
                .BeTrue("a rooted target under the archive root resolves and is selectable");
        }

        [TestMethod]
        public void IsValidFilingSelection_ArchiveRootExactTarget_IsAccepted()
        {
            // CR-2 recorded consequence: TryMakeArchiveRelative returns true for the exact root,
            // so the root itself is a resolvable filing target at this surface.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive", @"\Archive")
                .Should()
                .BeTrue("the archive root resolves against itself");
        }

        [TestMethod]
        public void IsValidFilingSelection_RootedTargetAboveArchiveRoot_IsRejected()
        {
            // D1 guard rail: narrowing the guard for CR-2 must not admit an above-root path.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\External\Clients", @"\Archive")
                .Should()
                .BeFalse("a path outside the archive root does not resolve against it");
        }

        [TestMethod]
        public void IsValidFilingSelection_CrossStoreRootedTarget_IsRejected()
        {
            // D4 guard rail: a cross-store path still fails the resolution test.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\\other-mailbox@example.com\Archive\Clients", @"\Archive")
                .Should()
                .BeFalse("a cross-store path does not resolve against the local archive root");
        }

        [TestMethod]
        public void IsValidFilingSelection_SeparatorBoundaryNearMiss_IsRejected()
        {
            // D9 guard rail: the resolution test is separator-terminated, so a sibling that
            // merely extends the root name is not under the root.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive2\Clients", @"\Archive")
                .Should()
                .BeFalse("Archive2 is a sibling of Archive, not a child of it");
        }

        [TestMethod]
        public void IsValidFilingSelection_RootedTargetWithUnavailableRoot_IsRejected()
        {
            // Degrade-path guard rail: an empty root is what ResolveArchiveRootOrEmpty yields
            // when the archive root cannot be resolved, and it must reject every rooted value.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive\Clients", string.Empty)
                .Should()
                .BeFalse("no rooted value can be resolved without an archive root");
        }

        #endregion

        #region Folder-creation predicate

        [TestMethod]
        public void IsValidCreationSelection_NullSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidCreationSelection(null).Should().BeFalse();
        }

        [TestMethod]
        public void IsValidCreationSelection_EmptySelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidCreationSelection(string.Empty).Should().BeFalse();
        }

        [TestMethod]
        public void IsValidCreationSelection_WhitespaceSelection_IsRejected()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidCreationSelection("    ").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidCreationSelection_BannerSentinel_IsRejected()
        {
            // Arrange / Act / Assert: the suggestion banner is not a creation parent.
            EfcSelectionGuard.IsValidCreationSelection("==== SUGGESTIONS ====").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidCreationSelection_TwoCharacterSelection_IsRejected()
        {
            // CR-1: the minimum-length rule is a folder-creation rule and lives only here.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidCreationSelection("AB")
                .Should()
                .BeFalse("the creation path keeps its minimum-length strictness");
        }

        [TestMethod]
        public void IsValidCreationSelection_SingleCharacterSelection_IsRejected()
        {
            // Arrange / Act / Assert: below the creation minimum.
            EfcSelectionGuard.IsValidCreationSelection("A").Should().BeFalse();
        }

        [TestMethod]
        public void IsValidCreationSelection_MinimumLengthSelection_IsAccepted()
        {
            // Arrange / Act / Assert: exactly at the creation minimum, the accepted boundary.
            EfcSelectionGuard
                .IsValidCreationSelection("ABC")
                .Should()
                .BeTrue("three characters is the shortest accepted creation name");
        }

        [TestMethod]
        public void IsValidCreationSelection_RootedSelection_IsRejected()
        {
            // Arrange / Act / Assert: creation concatenates beneath the root, so a rooted value
            // is never a valid creation stem.
            EfcSelectionGuard
                .IsValidCreationSelection(@"\Archive\Clients")
                .Should()
                .BeFalse("a rooted value is not a creation stem");
        }

        [TestMethod]
        public void IsValidCreationSelection_ValidRelativeStem_IsAccepted()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidCreationSelection(@"Clients\North").Should().BeTrue();
        }

        #endregion

        #region Archive-root resolver

        [TestMethod]
        public void ResolveArchiveRootOrEmpty_AccessorSucceeds_ReturnsRootAndLogsNothing()
        {
            // A resolvable archive root is returned verbatim and emits no degrade diagnostic.
            // Arrange
            var diagnostics = new List<string>();

            // Act
            string root = EfcSelectionGuard.ResolveArchiveRootOrEmpty(
                () => @"\Archive",
                message => diagnostics.Add(message)
            );

            // Assert
            root.Should().Be(@"\Archive");
            diagnostics.Should().BeEmpty("a successful read must not emit a degrade diagnostic");
        }

        [TestMethod]
        public void ResolveArchiveRootOrEmpty_AccessorThrowsInvalidOperation_DegradesToEmpty()
        {
            // The one documented accessor failure degrades to an empty root plus a fixed,
            // value-free diagnostic instead of tearing down the OK-button path.
            // Arrange
            var diagnostics = new List<string>();

            // Act
            string root = EfcSelectionGuard.ResolveArchiveRootOrEmpty(
                () => throw new InvalidOperationException("archive root unresolvable"),
                message => diagnostics.Add(message)
            );

            // Assert
            root.Should().BeEmpty("the degrade path yields an empty root");
            diagnostics
                .Should()
                .ContainSingle()
                .Which.Should()
                .Be(EfcSelectionGuard.RootUnavailableDiagnostic);
        }

        #endregion
    }
}
