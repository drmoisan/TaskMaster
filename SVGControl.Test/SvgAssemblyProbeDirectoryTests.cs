#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SVGControl.Test
{
    /// <summary>
    /// AC-8 coverage for the pure probe-directory helpers on <see cref="SvgAssemblyProbe"/>. These
    /// are the decision logic of the <c>AssemblyResolve</c> strategy-3 fallback, extracted to a
    /// stateless type so they can be exercised directly without triggering a live assembly bind.
    /// No test asserts anything about the process-wide resolver handler itself: it is installed once
    /// per AppDomain and is never removed, so its presence is not an observable per-test state.
    /// </summary>
    [TestClass]
    public class SvgAssemblyProbeDirectoryTests
    {
        [TestMethod]
        public void TryGetDirectoryFromCodeBase_WithAValidFileUri_ReturnsTheContainingDirectory()
        {
            // Arrange — the shape an assembly's CodeBase actually takes.
            string codeBase = "file:///C:/probe/root/SVGControl.dll";

            // Act
            string? directory = SvgAssemblyProbe.TryGetDirectoryFromCodeBase(codeBase);

            // Assert
            directory
                .Should()
                .NotBeNull("a well-formed file:// code base must yield a usable directory");
            directory
                .Should()
                .EndWith(
                    "root",
                    "the helper returns the directory containing the assembly, not the assembly path"
                );
        }

        [TestMethod]
        public void TryGetDirectoryFromCodeBase_WithNull_ReturnsNull()
        {
            // Arrange, Act
            string? directory = SvgAssemblyProbe.TryGetDirectoryFromCodeBase(null);

            // Assert — the helper runs inside an AssemblyResolve handler, so an unusable input must
            // produce a skipped candidate rather than an exception.
            directory.Should().BeNull("a null code base yields no candidate directory");
        }

        [TestMethod]
        public void TryGetDirectoryFromCodeBase_WithEmptyString_ReturnsNull()
        {
            // Arrange, Act
            string? directory = SvgAssemblyProbe.TryGetDirectoryFromCodeBase(string.Empty);

            // Assert
            directory.Should().BeNull("an empty code base yields no candidate directory");
        }

        [TestMethod]
        public void TryGetDirectoryFromCodeBase_WithWhitespaceOnly_ReturnsNull()
        {
            // Arrange, Act
            string? directory = SvgAssemblyProbe.TryGetDirectoryFromCodeBase("   ");

            // Assert
            directory.Should().BeNull("a whitespace-only code base yields no candidate directory");
        }

        [TestMethod]
        public void TryGetDirectoryFromCodeBase_WithANonUriString_ReturnsNullWithoutThrowing()
        {
            // Arrange
            string notAUri = "not a uri";

            // Act
            Action act = () => SvgAssemblyProbe.TryGetDirectoryFromCodeBase(notAUri);

            // Assert
            act.Should()
                .NotThrow(
                    "an unparsable code base must be skipped, not raised, inside a resolve handler"
                );
            SvgAssemblyProbe
                .TryGetDirectoryFromCodeBase(notAUri)
                .Should()
                .BeNull("an unparsable code base yields no candidate directory");
        }

        [TestMethod]
        public void GetProbeDirectories_WithAllThreeInputsPopulated_PreservesTheStatedOrder()
        {
            // Arrange — the documented precedence is the assembly's own directory, then its
            // code-base directory, then the AppDomain base directory.
            string location = @"C:\probe\one\SVGControl.dll";
            string codeBase = "file:///C:/probe/two/SVGControl.dll";
            string baseDirectory = @"C:\probe\three";

            // Act
            IReadOnlyList<string> directories = SvgAssemblyProbe.GetProbeDirectories(
                location,
                codeBase,
                baseDirectory
            );

            // Assert
            directories.Should().HaveCount(3, "all three inputs produced a distinct candidate");
            directories[0].Should().EndWith("one", "the assembly's own directory is probed first");
            directories[1].Should().EndWith("two", "the code-base directory is probed second");
            directories[2]
                .Should()
                .Be(baseDirectory, "the AppDomain base directory is probed last");
        }

        [TestMethod]
        public void GetProbeDirectories_WithAnEmptyAssemblyLocation_SkipsThatCandidate()
        {
            // Arrange — an assembly loaded from a byte array reports an empty Location. That
            // candidate must be skipped rather than resolved against the current directory.
            string codeBase = "file:///C:/probe/two/SVGControl.dll";
            string baseDirectory = @"C:\probe\three";

            // Act
            Action act = () =>
                SvgAssemblyProbe.GetProbeDirectories(string.Empty, codeBase, baseDirectory);
            IReadOnlyList<string> directories = SvgAssemblyProbe.GetProbeDirectories(
                string.Empty,
                codeBase,
                baseDirectory
            );

            // Assert
            act.Should().NotThrow("an empty Location is an expected input, not an error");
            directories.Should().HaveCount(2, "the empty location contributed no candidate");
            directories[0].Should().EndWith("two", "the code-base directory moves to first place");
            directories[1].Should().Be(baseDirectory, "the base directory remains last");
        }

        [TestMethod]
        public void GetProbeDirectories_WithDirectoriesDifferingOnlyByCase_DeduplicatesThem()
        {
            // Arrange — Windows paths are case-insensitive, so two spellings of one directory must
            // be probed once. First occurrence wins.
            string location = @"C:\Probe\Shared\SVGControl.dll";
            string codeBase = "file:///C:/probe/shared/SVGControl.dll";

            // Act
            IReadOnlyList<string> directories = SvgAssemblyProbe.GetProbeDirectories(
                location,
                codeBase,
                @"c:\PROBE\shared"
            );

            // Assert
            directories
                .Should()
                .HaveCount(
                    1,
                    "three case-variant spellings of the same directory collapse to one candidate"
                );
            directories[0]
                .Should()
                .Be(
                    @"C:\Probe\Shared",
                    "de-duplication preserves the first occurrence exactly as supplied"
                );
        }

        [TestMethod]
        public void GetProbeDirectories_WithAnInvalidCharacterInTheBaseDirectory_DropsThatCandidateWithoutThrowing()
        {
            // Arrange — Path.Combine raises ArgumentException for a path containing an invalid
            // character, and that call sits inside the AssemblyResolve handler, so the candidate must
            // be dropped here rather than raised there. The character is taken from the platform's own
            // list so the test does not depend on which character occupies a given position.
            string badBaseDirectory = @"C:\probe\three" + Path.GetInvalidPathChars()[0] + "bad";

            // Act
            Action act = () =>
                SvgAssemblyProbe.GetProbeDirectories(
                    @"C:\probe\one\SVGControl.dll",
                    null,
                    badBaseDirectory
                );
            IReadOnlyList<string> directories = SvgAssemblyProbe.GetProbeDirectories(
                @"C:\probe\one\SVGControl.dll",
                null,
                badBaseDirectory
            );

            // Assert
            act.Should()
                .NotThrow(
                    "an invalid path character must be filtered, not raised, inside a resolve handler"
                );
            directories
                .Should()
                .HaveCount(
                    1,
                    "the invalid base directory contributed no candidate, leaving only the assembly's own directory"
                );
            directories[0]
                .Should()
                .EndWith("one", "the surviving candidate is the assembly's own directory");
        }

        [TestMethod]
        public void GetProbeDirectories_WithAllInputsNull_ReturnsAnEmptyListWithoutThrowing()
        {
            // Arrange, Act
            Action act = () => SvgAssemblyProbe.GetProbeDirectories(null, null, null);
            IReadOnlyList<string> directories = SvgAssemblyProbe.GetProbeDirectories(
                null,
                null,
                null
            );

            // Assert — the helper is called from inside an AssemblyResolve handler, where throwing
            // would replace a recoverable bind failure with an unrecoverable one.
            act.Should().NotThrow("no usable candidate is an expected outcome, not an error");
            directories.Should().NotBeNull("the helper always returns a list");
            directories.Should().BeEmpty("no input produced a usable candidate directory");
        }

        // CR-6: PublicKeyTokensEqual gates every assembly the resolver returns across all three
        // strategies, and AC-8 requires that public-key-token match be preserved. Before these tests
        // the member measured 0/15 lines, so the requirement was verified by inspection only. The
        // eight cases below drive all fifteen lines and all ten condition outcomes of the member,
        // including both orderings of every null pairing.

        [TestMethod]
        public void PublicKeyTokensEqual_WithBothArgumentsNull_ReturnsTrue()
        {
            // Arrange, Act
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(null, null);

            // Assert
            equal
                .Should()
                .BeTrue("two absent tokens describe the same unsigned identity, so they match");
        }

        [TestMethod]
        public void PublicKeyTokensEqual_WithFirstNullAndSecondZeroLength_ReturnsTrue()
        {
            // Arrange, Act — an unsigned assembly reports its token as a zero-length array in some
            // reflection paths and as null in others, so the two spellings must be equivalent.
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(null, new byte[0]);

            // Assert
            equal.Should().BeTrue("a null token and a zero-length token both mean unsigned");
        }

        [TestMethod]
        public void PublicKeyTokensEqual_WithFirstZeroLengthAndSecondNull_ReturnsTrue()
        {
            // Arrange, Act — the reversed argument order of the preceding case.
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(new byte[0], null);

            // Assert
            equal.Should().BeTrue("the unsigned equivalence must not depend on argument order");
        }

        [TestMethod]
        public void PublicKeyTokensEqual_WithFirstNullAndSecondNonEmpty_ReturnsFalse()
        {
            // Arrange, Act
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(
                null,
                new byte[] { 0xBD, 0xBE, 0x16, 0xBE }
            );

            // Assert
            equal
                .Should()
                .BeFalse("an unsigned identity must never match a strong-named one, per AC-8");
        }

        [TestMethod]
        public void PublicKeyTokensEqual_WithFirstNonEmptyAndSecondNull_ReturnsFalse()
        {
            // Arrange, Act — the reversed argument order of the preceding case. This is the only case
            // that drives the false outcome of the zero-length check on the first argument and the
            // false outcome of the not-null check on the second argument inside the early-return
            // expression, so it is required rather than symmetric decoration.
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(
                new byte[] { 0xBD, 0xBE, 0x16, 0xBE },
                null
            );

            // Assert
            equal
                .Should()
                .BeFalse("a strong-named identity must never match an unsigned one, per AC-8");
        }

        [TestMethod]
        public void PublicKeyTokensEqual_WithEqualNonEmptyTokens_ReturnsTrue()
        {
            // Arrange — the resolver's success case: a same-key assembly found on disk.
            byte[] first = new byte[] { 0xBD, 0xBE, 0x16, 0xBE, 0x9B, 0x93, 0x6B, 0x9A };
            byte[] second = new byte[] { 0xBD, 0xBE, 0x16, 0xBE, 0x9B, 0x93, 0x6B, 0x9A };

            // Act
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(first, second);

            // Assert
            equal
                .Should()
                .BeTrue(
                    "byte-for-byte identical tokens are the same key, so the assembly is accepted"
                );
        }

        [TestMethod]
        public void PublicKeyTokensEqual_WithUnequalTokensOfEqualLength_ReturnsFalse()
        {
            // Arrange — same length, differing in the final byte, so the loop must run to completion
            // before rejecting.
            byte[] first = new byte[] { 0xBD, 0xBE, 0x16, 0xBE, 0x9B, 0x93, 0x6B, 0x9A };
            byte[] second = new byte[] { 0xBD, 0xBE, 0x16, 0xBE, 0x9B, 0x93, 0x6B, 0x9B };

            // Act
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(first, second);

            // Assert
            equal
                .Should()
                .BeFalse("a single differing byte is a different key, so the assembly is rejected");
        }

        [TestMethod]
        public void PublicKeyTokensEqual_WithTokensOfUnequalLength_ReturnsFalse()
        {
            // Arrange, Act — the length guard rejects before the loop is entered.
            bool equal = SvgAssemblyProbe.PublicKeyTokensEqual(
                new byte[] { 0xBD, 0xBE, 0x16, 0xBE },
                new byte[] { 0xBD, 0xBE, 0x16, 0xBE, 0x9B, 0x93, 0x6B, 0x9A }
            );

            // Assert
            equal.Should().BeFalse("tokens of different lengths cannot describe the same key");
        }
    }
}
