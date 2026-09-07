using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    /// <summary>
    /// Unit tests for <see cref="ArchiveStemProjection"/> and its single member
    /// <c>ToDisplayStem</c>, the lenient display projection introduced by issue #799.
    /// The projection yields the archive-relative stem
    /// only when the path is strictly under the configured root, and returns its input unchanged in
    /// every other case. The boundary cases pinned here are the #614 cases the strict
    /// <see cref="ArchiveStemContract"/> already enforces, restated at the display boundary because
    /// the fallback there is the opposite one: show the caller's own text rather than nothing.
    /// Pure assertions only: no mocks, no COM, no filesystem, and no temporary file.
    /// </summary>
    [TestClass]
    public sealed class ArchiveStemProjectionTests
    {
        private const string ArchiveRoot = "\\\\Mailbox - User\\Archive";
        private const string UnderRoot = "\\\\Mailbox - User\\Archive\\Clients\\Acme";
        private const string ExpectedStem = "Clients\\Acme";

        /// <summary>The ordinary case: a path strictly under the root projects to its stem.</summary>
        [TestMethod]
        public void ToDisplayStem_PathStrictlyUnderRoot_ReturnsArchiveRelativeStem()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(UnderRoot, ArchiveRoot);

            // Assert
            projected.Should().Be(ExpectedStem);
        }

        /// <summary>
        /// A path EQUAL to the root is not projectable: the strict contract reports success with an
        /// empty stem, and an empty display row is worse than the full path, so the input is
        /// returned unchanged.
        /// </summary>
        [TestMethod]
        public void ToDisplayStem_PathEqualsRoot_ReturnsInputUnchanged()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(ArchiveRoot, ArchiveRoot);

            // Assert
            projected
                .Should()
                .Be(ArchiveRoot, "an empty display row is worse than the unprojected path");
        }

        /// <summary>
        /// The #614 false-prefix case. A sibling folder named Archive2 tested against a root ending
        /// in Archive yields the character 2 at the root's length, which is not a separator, so the
        /// path is NOT under the root and is not projected.
        /// </summary>
        [TestMethod]
        public void ToDisplayStem_FalsePrefixSiblingArchive2_ReturnsInputUnchanged()
        {
            // Arrange
            const string sibling = "\\\\Mailbox - User\\Archive2\\Clients";

            // Act
            var projected = ArchiveStemProjection.ToDisplayStem(sibling, ArchiveRoot);

            // Assert
            projected.Should().Be(sibling, "the separator-boundary test rejects a false prefix");
        }

        /// <summary>A root supplied with one trailing separator projects identically.</summary>
        [TestMethod]
        public void ToDisplayStem_RootWithOneTrailingSeparator_ReturnsArchiveRelativeStem()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(UnderRoot, ArchiveRoot + "\\");

            // Assert
            projected.Should().Be(ExpectedStem);
        }

        /// <summary>A root supplied with two trailing separators projects identically.</summary>
        [TestMethod]
        public void ToDisplayStem_RootWithTwoTrailingSeparators_ReturnsArchiveRelativeStem()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(UnderRoot, ArchiveRoot + "\\\\");

            // Assert
            projected.Should().Be(ExpectedStem);
        }

        /// <summary>
        /// AC4 of issue #799: an empty archive root leaves the input unchanged. The previous
        /// per-site logic stripped one leading separator in this case, which produced a path that
        /// was neither a valid full path nor a valid archive-relative stem.
        /// </summary>
        [TestMethod]
        public void ToDisplayStem_EmptyRoot_ReturnsInputUnchanged()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(UnderRoot, string.Empty);

            // Assert
            projected
                .Should()
                .Be(UnderRoot, "#799 AC4 removed the empty-root one-separator strip");
        }

        /// <summary>A whitespace-only root is treated exactly as an empty root.</summary>
        [TestMethod]
        public void ToDisplayStem_WhitespaceOnlyRoot_ReturnsInputUnchanged()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(UnderRoot, "   ");

            // Assert
            projected.Should().Be(UnderRoot);
        }

        /// <summary>A null path is returned unchanged rather than throwing.</summary>
        [TestMethod]
        public void ToDisplayStem_NullPath_ReturnsNull()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(null, ArchiveRoot);

            // Assert
            projected.Should().BeNull();
        }

        /// <summary>An empty path is returned unchanged rather than projected.</summary>
        [TestMethod]
        public void ToDisplayStem_EmptyPath_ReturnsInputUnchanged()
        {
            // Arrange, Act
            var projected = ArchiveStemProjection.ToDisplayStem(string.Empty, ArchiveRoot);

            // Assert
            projected.Should().Be(string.Empty);
        }

        /// <summary>
        /// Forward-slash separators on both parameters project exactly as backslash separators do,
        /// because the underlying contract treats both characters as separators.
        /// </summary>
        [TestMethod]
        public void ToDisplayStem_ForwardSlashSeparators_ReturnsArchiveRelativeStem()
        {
            // Arrange
            const string root = "//Mailbox - User/Archive";
            const string path = "//Mailbox - User/Archive/Clients/Acme";

            // Act
            var projected = ArchiveStemProjection.ToDisplayStem(path, root);

            // Assert
            projected.Should().Be("Clients/Acme");
        }

        /// <summary>
        /// The prefix comparison is ordinal case-insensitive, so a mixed-case root still projects.
        /// </summary>
        [TestMethod]
        public void ToDisplayStem_MixedCaseRoot_ReturnsArchiveRelativeStem()
        {
            // Arrange
            const string mixedCaseRoot = "\\\\mailbox - USER\\aRcHiVe";

            // Act
            var projected = ArchiveStemProjection.ToDisplayStem(UnderRoot, mixedCaseRoot);

            // Assert
            projected.Should().Be(ExpectedStem);
        }
    }
}
