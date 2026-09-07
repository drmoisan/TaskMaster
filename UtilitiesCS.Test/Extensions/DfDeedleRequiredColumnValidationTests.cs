using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Extensions
{
    /// <summary>
    /// Regression tests for required-column validation of the email column-index map produced by
    /// the table ETL. The email dataframe projection indexes that map unconditionally, so a missing
    /// column surfaces today as an unattributable <see cref="KeyNotFoundException"/>.
    /// </summary>
    [TestClass]
    public class DfDeedleRequiredColumnValidationTests
    {
        /// <summary>Folder name used in every diagnostic assertion.</summary>
        private const string FolderName = "T&E";

        /// <summary>
        /// The five column names the email projection reads. The casing asymmetry is intentional
        /// and is part of the contract: capital D in <c>EntryID</c>, lowercase d in
        /// <c>ConversationId</c>.
        /// </summary>
        private static readonly string[] RequiredKeys =
        {
            "EntryID",
            "MessageClass",
            "SentOn",
            "ConversationId",
            "Triage",
        };

        /// <summary>
        /// Builds a column-index map holding every required key except those named. The default
        /// comparer is ordinal, matching the dictionary the table utility produces.
        /// </summary>
        private static Dictionary<string, int> ColumnsWithout(params string[] omitted)
        {
            var map = new Dictionary<string, int>();
            var index = 0;
            foreach (
                var key in RequiredKeys.Where(k => !omitted.Contains(k, StringComparer.Ordinal))
            )
            {
                map[key] = index++;
            }

            return map;
        }

        /// <summary>Invokes the validator against the supplied map and the fixture folder name.</summary>
        private static Action Validating(Dictionary<string, int> columns) =>
            () => DfDeedle.ValidateRequiredEmailColumns(columns, FolderName);

        // ----------------------------------------------------------------
        // AC3 negative cases — one per required key removed in turn
        // ----------------------------------------------------------------

        /// <summary>AC3: a map missing <c>EntryID</c> must be rejected before it is indexed.</summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_MissingEntryID_Throws()
        {
            // Arrange
            var columns = ColumnsWithout("EntryID");

            // Act
            var act = Validating(columns);

            // Assert
            act.Should().Throw<System.Exception>("the projection indexes EntryID unconditionally");
        }

        /// <summary>AC3: a map missing <c>MessageClass</c> must be rejected before it is indexed.</summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_MissingMessageClass_Throws()
        {
            // Arrange
            var columns = ColumnsWithout("MessageClass");

            // Act
            var act = Validating(columns);

            // Assert
            act.Should()
                .Throw<System.Exception>("the projection indexes MessageClass unconditionally");
        }

        /// <summary>AC3: a map missing <c>SentOn</c> must be rejected before it is indexed.</summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_MissingSentOn_Throws()
        {
            // Arrange
            var columns = ColumnsWithout("SentOn");

            // Act
            var act = Validating(columns);

            // Assert
            act.Should().Throw<System.Exception>("the projection indexes SentOn unconditionally");
        }

        /// <summary>AC3: a map missing <c>ConversationId</c> must be rejected before it is indexed.</summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_MissingConversationId_Throws()
        {
            // Arrange
            var columns = ColumnsWithout("ConversationId");

            // Act
            var act = Validating(columns);

            // Assert
            act.Should()
                .Throw<System.Exception>("the projection indexes ConversationId unconditionally");
        }

        /// <summary>AC3: a map missing <c>Triage</c> must be rejected before it is indexed.</summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_MissingTriage_Throws()
        {
            // Arrange
            var columns = ColumnsWithout("Triage");

            // Act
            var act = Validating(columns);

            // Assert
            act.Should().Throw<System.Exception>("the projection indexes Triage unconditionally");
        }

        // ----------------------------------------------------------------
        // AC3 message content, multiple omissions, case variance, positive
        // ----------------------------------------------------------------

        /// <summary>
        /// AC3: when two required columns are absent the diagnostic must name both of them and the
        /// folder, so the user can act on it without reading a stack trace.
        /// </summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder()
        {
            // Arrange
            var columns = ColumnsWithout("SentOn", "Triage");

            // Act
            var act = Validating(columns);

            // Assert
            var thrown = act.Should().Throw<System.Exception>("two required columns are absent");
            thrown
                .Which.Message.Should()
                .Contain("SentOn", "the first missing column must be named")
                .And.Contain("Triage", "the second missing column must be named")
                .And.Contain(FolderName, "the folder must be named");
        }

        /// <summary>
        /// AC3: comparison is ordinal. A map supplying <c>Entryid</c> does not satisfy the required
        /// key <c>EntryID</c>, because the dictionary the table utility produces is ordinal and the
        /// required names carry an intentional casing asymmetry.
        /// </summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing()
        {
            // Arrange
            var columns = ColumnsWithout("EntryID");
            columns["Entryid"] = columns.Count;

            // Act
            var act = Validating(columns);

            // Assert
            var thrown = act.Should()
                .Throw<System.Exception>("a case variant does not satisfy an ordinal comparison");
            thrown
                .Which.Message.Should()
                .Contain(
                    "EntryID",
                    "the required name, not the supplied variant, must be reported"
                );
        }

        /// <summary>
        /// AC3 positive case: a complete map is accepted. This guards the validator against
        /// rejecting the healthy folder that launches correctly today.
        /// </summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow()
        {
            // Arrange
            var columns = ColumnsWithout();

            // Act
            var act = Validating(columns);

            // Assert
            act.Should().NotThrow("every required column is present");
        }

        /// <summary>AC3: a single-column diagnostic must still name the folder.</summary>
        [TestMethod]
        public void ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder()
        {
            // Arrange
            var columns = ColumnsWithout("SentOn");

            // Act
            var act = Validating(columns);

            // Assert
            var thrown = act.Should().Throw<System.Exception>("SentOn is absent");
            thrown
                .Which.Message.Should()
                .Contain(FolderName, "the diagnostic must attribute the failure to a folder");
        }
    }
}
