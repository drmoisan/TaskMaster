using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Unit tests for the pure static seam
    /// <see cref="AppFileSystemFolderPaths.MatchBestSpecialFolder(IReadOnlyDictionary{string, string}, string)"/>.
    ///
    /// Purpose:
    ///     This branch was previously deferred (Flag-and-Stop) because the instance method read
    ///     the live filesystem-backed <c>SpecialFolders</c> dictionary. Phase 5 extracts the
    ///     matching logic into a pure static helper that accepts the folder collection as a
    ///     parameter, so the matching semantics can be exercised deterministically with an
    ///     in-memory dictionary and no filesystem access.
    ///
    /// Semantics under test (preserved byte-for-byte from the original instance method):
    ///     null/empty collection returns null; entries whose value is contained in the path
    ///     (ordinal, case-sensitive <c>string.Contains</c>) are candidates; the candidate with
    ///     the longest value wins; the matched key is returned, or null when none match.
    ///
    /// Constraints: MSTest + FluentAssertions; AAA; no filesystem access; no temp files; no
    /// LoadFolders; deterministic.
    /// </summary>
    [TestClass]
    public class AppFileSystemFolderPathsMatchBestSpecialFolderTests
    {
        // ---- positive ----

        [TestMethod]
        public void MatchBestSpecialFolder_PathContainsKnownValue_ReturnsThatKey()
        {
            // Arrange
            var folders = new Dictionary<string, string>
            {
                ["AppData"] = @"C:\Users\Test\AppData\Local",
            };

            // Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(
                folders,
                @"C:\Users\Test\AppData\Local\TaskMaster\file.txt"
            );

            // Assert
            result.Should().Be("AppData", "the only folder value is contained in the path");
        }

        // ---- best-match / longest-prefix ----

        [TestMethod]
        public void MatchBestSpecialFolder_TwoCandidatesContained_LongerValueKeyWins()
        {
            // Arrange: both values are contained in the path; the longer value's key must win.
            var folders = new Dictionary<string, string>
            {
                ["Root"] = @"C:\Users",
                ["AppData"] = @"C:\Users\Test\AppData",
            };

            // Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(
                folders,
                @"C:\Users\Test\AppData\Local\file.txt"
            );

            // Assert
            result
                .Should()
                .Be("AppData", "the longest contained value wins the descending-length ordering");
        }

        // ---- case sensitivity (ordinal Contains) ----

        [TestMethod]
        public void MatchBestSpecialFolder_CaseMismatch_DoesNotMatch_ReturnsNull()
        {
            // Arrange: string.Contains is ordinal/case-sensitive, so a case mismatch is not a match.
            var folders = new Dictionary<string, string> { ["AppData"] = @"C:\USERS\TEST" };

            // Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(
                folders,
                @"C:\users\test\file.txt"
            );

            // Assert
            result.Should().BeNull("ordinal Contains treats differing case as a non-match");
        }

        // ---- trailing separator (substring match is unaffected) ----

        [TestMethod]
        public void MatchBestSpecialFolder_ValueWithoutTrailingSeparator_StillMatchesAsSubstring()
        {
            // Arrange: the stored value has no trailing separator; it is still a substring of the
            // path, so the match succeeds (no normalization is performed by the method).
            var folders = new Dictionary<string, string> { ["MyDocuments"] = @"D:\Docs" };

            // Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(
                folders,
                @"D:\Docs\report.pdf"
            );

            // Assert
            result
                .Should()
                .Be(
                    "MyDocuments",
                    "the value is a substring of the path regardless of trailing separator"
                );
        }

        // ---- no-match / null / empty ----

        [TestMethod]
        public void MatchBestSpecialFolder_NoValueContained_ReturnsNull()
        {
            // Arrange
            var folders = new Dictionary<string, string> { ["AppData"] = @"C:\Users\Test\AppData" };

            // Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(
                folders,
                @"E:\Unrelated\path\file.txt"
            );

            // Assert
            result.Should().BeNull("no folder value is contained in the path");
        }

        [TestMethod]
        public void MatchBestSpecialFolder_NullCollection_ReturnsNull()
        {
            // Arrange / Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(null, @"C:\anything");

            // Assert
            result.Should().BeNull("a null collection short-circuits to null");
        }

        [TestMethod]
        public void MatchBestSpecialFolder_EmptyCollection_ReturnsNull()
        {
            // Arrange
            var folders = new Dictionary<string, string>();

            // Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(folders, @"C:\anything");

            // Assert
            result.Should().BeNull("an empty collection short-circuits to null");
        }

        [TestMethod]
        public void MatchBestSpecialFolder_EmptyPath_NoValueContained_ReturnsNull()
        {
            // Arrange: an empty path cannot contain any non-empty folder value.
            var folders = new Dictionary<string, string> { ["AppData"] = @"C:\Users\Test\AppData" };

            // Act
            var result = AppFileSystemFolderPaths.MatchBestSpecialFolder(folders, string.Empty);

            // Assert
            result.Should().BeNull("an empty path contains no non-empty folder value");
        }

        [TestMethod]
        public void MatchBestSpecialFolder_NullPath_ThrowsNullReferenceException()
        {
            // Arrange: the method invokes path.Contains, so a null path throws (documented
            // behavior preserved exactly from the original instance method).
            var folders = new Dictionary<string, string> { ["AppData"] = @"C:\Users\Test\AppData" };

            // Act
            Action act = () => AppFileSystemFolderPaths.MatchBestSpecialFolder(folders, null);

            // Assert
            act.Should()
                .Throw<NullReferenceException>("path.Contains dereferences the null path argument");
        }
    }
}
