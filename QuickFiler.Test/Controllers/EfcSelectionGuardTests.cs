using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace QuickFiler.Test.Controllers
{
    /// <summary>
    /// Issue #614 tests for <see cref="EfcSelectionGuard"/> (defect D9) and its remediation
    /// cycle corrections: the filing predicate and the folder-creation predicate. Both predicates
    /// are pure, so no collaborator requires mocking and Moq is deliberately not used here.
    /// </summary>
    [TestClass]
    public class EfcSelectionGuardTests
    {
        #region Filing predicate

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
            // Arrange / Act / Assert: D1/D9 protection - a store root is not under the archive.
            EfcSelectionGuard
                .IsValidFilingSelection(@"\\mailbox@example.com")
                .Should()
                .BeFalse("a store-rooted Outlook path is not an archive-relative filing stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_SingleSeparatorLeadingSelection_IsRejected()
        {
            // Arrange / Act / Assert: a leading separator makes the value a full Outlook path,
            // which is rejected as such at the filing guard.
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive\Clients")
                .Should()
                .BeFalse("a rooted value is not an archive-relative filing stem");
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
        public void IsValidFilingSelection_ValidRelativeStem_IsAccepted()
        {
            // Arrange / Act / Assert
            EfcSelectionGuard.IsValidFilingSelection(@"Clients\North").Should().BeTrue();
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
                    .IsValidFilingSelection(name)
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
                .IsValidFilingSelection("A")
                .Should()
                .BeTrue("filing to the archive folder 'A' must remain possible");
        }

        [TestMethod]
        public void IsValidFilingSelection_RootedTargetAboveArchiveRoot_IsRejected()
        {
            // D1 guard rail: narrowing the guard for CR-2 must not admit an above-root path.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\External\Clients")
                .Should()
                .BeFalse("a rooted path is not an archive-relative filing stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_CrossStoreRootedTarget_IsRejected()
        {
            // D4 guard rail: a cross-store path still fails the resolution test.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\\other-mailbox@example.com\Archive\Clients")
                .Should()
                .BeFalse("a cross-store rooted path is not an archive-relative filing stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_SeparatorBoundaryNearMiss_IsRejected()
        {
            // D9 guard rail: a separator-leading sibling value remains a full Outlook path.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive2\Clients")
                .Should()
                .BeFalse("a separator-leading value is not an archive-relative filing stem");
        }

        [TestMethod]
        public void IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsRejected()
        {
            // RC-1 inversion: rooted values are never filing stems here; normalization is deferred to issue #637.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\aRcHiVe\Clients\North")
                .Should()
                .BeFalse(
                    "a rooted value is never a filing stem at this surface and producer-side normalization is deferred to issue #637"
                );
        }

        [TestMethod]
        public void IsValidFilingSelection_ArchiveRootExactTarget_IsRejected()
        {
            // RC-1 inversion: the archive root itself is not an archive-relative filing stem.
            // Arrange / Act / Assert
            EfcSelectionGuard
                .IsValidFilingSelection(@"\Archive")
                .Should()
                .BeFalse("the archive root itself is not an archive-relative filing stem");
        }

        [TestMethod]
        public void Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary()
        {
            // RC-1 composition guard: every accepted filing value must survive the D4 ResolvePaths boundary.
            // Arrange
            string[] candidates =
            {
                @"Clients\North",
                "HR",
                "A",
                @"\aRcHiVe\Clients\North",
                @"\Archive",
                @"\Archive\Clients",
                @"\External\Clients",
                @"\\mailbox@example.com",
                @"C:\Users\testuser\OneDrive - Contoso",
                "==== SUGGESTIONS ====",
            };
            int evaluated = 0;

            foreach (string candidate in candidates)
            {
                if (!EfcSelectionGuard.IsValidFilingSelection(candidate))
                {
                    continue;
                }

                var config = new EmailFilerConfig
                {
                    Globals = null,
                    OlAncestor = @"\\mailbox@example.com\Archive",
                    DestinationOlStem = candidate,
                    FsAncestorEquivalent = @"C:\Mail",
                };
                evaluated++;

                // Act
                System.Action act = () => config.ResolvePaths();

                // Assert
                act.Should()
                    .NotThrow($"the filing predicate accepted '{candidate}' as a filing stem");
            }

            evaluated
                .Should()
                .BeGreaterThan(0, "the candidate matrix must exercise at least one accepted value");
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
    }
}
