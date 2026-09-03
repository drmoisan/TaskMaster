using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Partial continuation of <see cref="QfcCollectionControllerDefects468Tests"/> holding the
    /// issue #731 finding 4 structural proxy. It is a separate file because the primary file sits
    /// two lines below the 500-line ceiling; the type is already attributed there.
    /// </summary>
    public partial class QfcCollectionControllerDefects468Tests
    {
        private static string ReadCollectionControllerSource()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (
                directory != null
                && !Directory.Exists(Path.Combine(directory.FullName, "QuickFiler"))
            )
            {
                directory = directory.Parent;
            }

            directory.Should().NotBeNull(because: "the test must run under the repository tree");
            return File.ReadAllText(
                Path.Combine(
                    directory.FullName,
                    "QuickFiler",
                    "Controllers",
                    "QfcCollectionController.cs"
                )
            );
        }

        private static string NormalizeSourceWhitespace(string text)
        {
            var builder = new System.Text.StringBuilder(text.Length);
            bool inWhitespaceRun = false;
            foreach (char character in text)
            {
                if (char.IsWhiteSpace(character))
                {
                    inWhitespaceRun = true;
                    continue;
                }

                if (inWhitespaceRun && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                inWhitespaceRun = false;
                builder.Append(character);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Scenario: inspect the source of <c>QfcCollectionController</c>. Expected outcome: the
        /// sole read of the reentrancy counter goes through <c>Volatile.Read</c>, the two
        /// <c>Interlocked</c> writes are unchanged, and the field is not marked <c>volatile</c>
        /// (issue #731 finding 4).
        /// </summary>
        /// <remarks>
        /// This assertion is a STRUCTURAL PROXY for the memory-ordering fix and is explicitly NOT a
        /// proof that the race is eliminated. A memory-ordering defect cannot be made to fail
        /// deterministically by a unit test: on x86/x64 the missing acquire barrier is very unlikely
        /// to produce an observable reordering, and a test that spun threads hoping to catch one
        /// would violate the determinism requirement in <c>.claude/rules/general-unit-test.md</c>.
        /// What it does establish is that the last unsynchronised read now goes through an explicit
        /// acquire, and that the fix did not reach for <c>volatile</c> instead, which would produce
        /// CS0420 at both <c>Interlocked</c> call sites under the TreatWarningsAsErrors gate.
        /// </remarks>
        [TestMethod]
        public void ReentrancyCounterSoleReadGoesThroughVolatileRead()
        {
            // Arrange
            string normalized = NormalizeSourceWhitespace(ReadCollectionControllerSource());

            // Act / Assert
            normalized
                .Should()
                .Contain(
                    "Volatile.Read(ref removespecificcontrolgroupcounter)",
                    because: "issue #731 finding 4 requires the sole read of the reentrancy counter "
                        + "to go through an explicit acquire"
                );
            normalized
                .Should()
                .NotContain(
                    "if (removespecificcontrolgroupcounter >",
                    because: "issue #731 finding 4 removes the bare, unsynchronised read of the "
                        + "reentrancy counter"
                );
            normalized
                .Should()
                .Contain(
                    "Interlocked.Increment(ref removespecificcontrolgroupcounter)",
                    because: "issue #731 finding 4 leaves both counter writes unchanged"
                );
            normalized
                .Should()
                .Contain(
                    "Interlocked.Decrement(ref removespecificcontrolgroupcounter)",
                    because: "issue #731 finding 4 leaves both counter writes unchanged"
                );
            normalized
                .Should()
                .NotContain(
                    "volatile int removespecificcontrolgroupcounter",
                    because: "issue #731 finding 4 must not add the volatile modifier: passing a "
                        + "volatile field by ref to Interlocked produces CS0420 at both call sites, "
                        + "and the type-check gate runs with TreatWarningsAsErrors"
                );
        }
    }
}
