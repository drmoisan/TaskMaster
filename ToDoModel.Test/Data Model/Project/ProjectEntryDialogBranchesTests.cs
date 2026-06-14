using System;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace ToDoModel.Test
{
    /// <summary>
    /// Unit tests for the dialog-dependent branches of
    /// <see cref="ProjectEntry.SetProjectId(string)"/> / <see cref="ProjectEntry.ChangeId(string)"/>
    /// and the length tie-break branch of <see cref="ProjectEntry.CompareTo(IProjectEntry)"/>.
    ///
    /// Purpose:
    ///     These branches were previously deferred (Flag-and-Stop) because the validation and
    ///     change-confirmation paths route through the static <c>MyBox.ShowDialog</c>. Phase 5
    ///     exposes the internal <see cref="MyBox.DialogInvoker"/> seam to this project via
    ///     <c>InternalsVisibleTo("ToDoModel.Test")</c> on UtilitiesCS, so the branches are now
    ///     exercised by injecting a deterministic, non-modal dialog stub.
    ///
    /// Usage:
    ///     Each dialog-dependent test sets <see cref="MyBox.DialogInvoker"/> to a stub that
    ///     returns a fixed <see cref="DialogResult"/> without showing a modal dialog.
    ///     <see cref="TestInitialize_SeedSeam"/> seeds an OK default before each test and
    ///     <see cref="TestCleanup_ResetSeam"/> restores the real invoker afterward so no seam
    ///     mutation leaks across tests.
    ///
    /// Invariants / Constraints:
    ///     The class runs under MSTest STA execution because <c>MyBox.ShowDialog</c> constructs a
    ///     WinForms <c>MyBoxViewer</c> control (the stubbed seam means it is never shown). No real
    ///     dialog is displayed and no message loop runs. MSTest + Moq + FluentAssertions; AAA.
    /// </summary>
    [STATestClass]
    public class ProjectEntryDialogBranchesTests
    {
        private static ProjectEntry NewEntry(string projId) =>
            new ProjectEntry("Proj", projId, "Prog");

        /// <summary>Seeds a non-modal OK default so no test accidentally shows a real dialog.</summary>
        [TestInitialize]
        public void TestInitialize_SeedSeam()
        {
            MyBox.DialogInvoker = _ => DialogResult.OK;
        }

        /// <summary>Restores the real (modal) invoker after each test to avoid cross-test leakage.</summary>
        [TestCleanup]
        public void TestCleanup_ResetSeam()
        {
            MyBox.DialogInvoker = viewer => viewer.ShowDialog();
        }

        // ---- SetProjectId: malformed-ID validation branch (length != 4) ----

        [TestMethod]
        public void SetProjectId_MalformedId_ShowsErrorDialogAndReturnsFalse()
        {
            // Arrange: a valid 4-char id, then attempt to set a malformed (length != 4) id.
            // The malformed arm routes through MyBox.ShowDialog(OK/Error); the stub returns OK
            // without showing a dialog.
            var entry = NewEntry("ABCD");
            var invocationCount = 0;
            MyBox.DialogInvoker = _ =>
            {
                invocationCount++;
                return DialogResult.OK;
            };

            // Act
            var result = entry.SetProjectId("AB");

            // Assert
            result
                .Should()
                .BeFalse("a malformed id (length != 4) is rejected after the error dialog");
            entry.ProjectID.Should().Be("ABCD", "the id is not changed when validation fails");
            invocationCount.Should().Be(1, "the error dialog seam is invoked exactly once");
        }

        // ---- SetProjectId / ChangeId: change-confirmation branch (FLAG-AND-STOP, not covered) ----
        //
        // The change-confirmation branch (a valid existing id replaced by a different valid id)
        // routes SetProjectId -> ChangeId, and ChangeId completes by assigning `ProjectID = newID`
        // (ProjectEntry.cs line ~166). That assignment runs the ProjectID *property setter*
        // (ProjectEntry.cs lines 49-76), whose `_projectID != value` arm calls a RAW, un-seamed
        // System.Windows.Forms.MessageBox.Show — NOT the MyBox.DialogInvoker seam. Injecting the
        // MyBox stub therefore cannot prevent a real modal dialog from being shown when the id is
        // committed, which blocks the STA test thread (verified: the change-confirmation tests hang
        // under vstest while the seam-only malformed and CompareTo tests pass).
        //
        // Covering this branch would require adding the MyBox seam to the ProjectID property setter
        // (a THIRD production change beyond the two seams authorized for Phase 5). Per the plan
        // Flag-and-Stop Rule and the Phase 5 hard constraint limiting production changes to the two
        // authorized seams, this branch is intentionally NOT exercised. The gap is recorded in
        // evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md. The malformed-ID
        // dialog branch (below) and the CompareTo length tie-break ARE reachable via the authorized
        // MyBox.DialogInvoker seam and are covered here.

        // ---- CompareTo(IProjectEntry): length tie-break branch ----
        //
        // The tie-break at ProjectEntry.CompareTo (string.CompareOrdinal == 0 then Length
        // comparison) is only reachable when the comparand reports an equal ordinal value on the
        // first read and a different Length on the subsequent reads. A plain ProjectEntry cannot
        // produce that because its ProjectID is stable and equal-content strings have equal
        // length. CompareTo reads other.ProjectID up to three times (CompareOrdinal, then each
        // Length comparison), so the comparand mock returns an ordinal-equal value on the FIRST
        // read and the length-differing value on every subsequent read. This reproduces the exact
        // state the branch was written to handle, without altering production behavior.

        /// <summary>
        /// Builds an IProjectEntry comparand whose ProjectID returns <paramref name="firstRead"/>
        /// on the first access and <paramref name="laterReads"/> on every subsequent access.
        /// </summary>
        private static IProjectEntry ComparandWithShiftingProjectId(
            string firstRead,
            string laterReads
        )
        {
            var other = new Mock<IProjectEntry>();
            var callCount = 0;
            other.Setup(o => o.ProjectID).Returns(() => callCount++ == 0 ? firstRead : laterReads);
            return other.Object;
        }

        [TestMethod]
        public void CompareTo_EqualOrdinalThenShorterOtherLength_ReturnsNegativeOne()
        {
            // Arrange: this id "ABCD" (length 4). The comparand reports "ABCD" on the ordinal read
            // (tie) then "ABCDE" (length 5 > 4) on the Length reads, driving the
            // `this.Length < other.Length` arm.
            var entry = NewEntry("ABCD");
            var other = ComparandWithShiftingProjectId("ABCD", "ABCDE");

            // Act
            var result = entry.CompareTo(other);

            // Assert
            result
                .Should()
                .Be(-1, "an ordinal tie with a longer comparand sorts this instance first");
        }

        [TestMethod]
        public void CompareTo_EqualOrdinalThenLongerOtherLength_ReturnsPositiveOne()
        {
            // Arrange: this id "ABCD" (length 4). The comparand reports "ABCD" on the ordinal read
            // (tie) then "ABC" (length 3 < 4) on the Length reads, driving the
            // `this.Length > other.Length` arm.
            var entry = NewEntry("ABCD");
            var other = ComparandWithShiftingProjectId("ABCD", "ABC");

            // Act
            var result = entry.CompareTo(other);

            // Assert
            result
                .Should()
                .Be(1, "an ordinal tie with a shorter comparand sorts this instance last");
        }
    }
}
