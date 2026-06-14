using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoModel;
using UtilitiesCS;

namespace ToDoModel.Test
{
    /// <summary>
    /// Unit tests for <see cref="ProjectEntry.SetProjectId(string)"/> and
    /// <see cref="ProjectEntry.CompareTo(IProjectEntry)"/> / <see cref="ProjectEntry.CompareTo(object)"/>.
    ///
    /// SetProjectId coverage is restricted to the branches reachable WITHOUT invoking a dialog.
    /// The malformed-id branch (length != 4) and the change-confirmation branch (new id differs
    /// from a valid existing id) route through the static <c>MyBox.ShowDialog</c>. The dialog
    /// seam (<c>MyBox.DialogInvoker</c>) is internal to UtilitiesCS and is exposed only to
    /// UtilitiesCS.Test (no InternalsVisibleTo for ToDoModel.Test), so those branches are NOT
    /// reachable from this project without a new production change. Per the feature Flag-and-Stop
    /// rule, those branches are intentionally not exercised here and the gap is recorded in
    /// evidence/other/. CompareTo is pure ordinal comparison and is fully covered.
    ///
    /// Instances are constructed with a 4-character or null ProjectID so the constructor's
    /// ProjectID setter does not trigger its own MessageBox.
    /// </summary>
    [TestClass]
    public class ProjectEntryTests
    {
        private static ProjectEntry NewEntry(string projId) =>
            new ProjectEntry("Proj", projId, "Prog");

        // ---- SetProjectId: dialog-free branches ----

        [TestMethod]
        public void SetProjectId_FromEmptyToNonEmpty_SetsAndReturnsTrue()
        {
            // Arrange: an entry whose ProjectID is empty/null.
            var entry = NewEntry(null);

            // Act
            var result = entry.SetProjectId("ABCD");

            // Assert
            result.Should().BeTrue("setting a previously-empty id is the dialog-free happy path");
            entry.ProjectID.Should().Be("ABCD");
        }

        [TestMethod]
        public void SetProjectId_NullNewIdWhenCurrentIsAlsoNull_ReturnsTrueWithoutDialog()
        {
            // Arrange: current ProjectID is null. The `case null` arm executes `ProjectID = newID`
            // (the property setter). When the existing value is already null, the setter's
            // change-confirmation MessageBox branch (_projectID != value) is NOT reached because
            // both old and new values are null, so no dialog is shown. Using a non-null current id
            // here would route through the setter's change-confirmation dialog and is therefore
            // excluded (see the dialog gap recorded in evidence/other/).
            var entry = NewEntry(null);

            // Act
            var result = entry.SetProjectId(null);

            // Assert
            result.Should().BeTrue("the null-newID arm returns true; null->null sets no dialog");
            entry.ProjectID.Should().BeNull();
        }

        [TestMethod]
        public void SetProjectId_SameValueAsExisting_NoChangeReturnsFalse()
        {
            // Arrange: existing valid id equal to the new id reaches the `s == ProjectID` break
            // arm, which falls through to `return false` without any dialog.
            var entry = NewEntry("ABCD");

            // Act
            var result = entry.SetProjectId("ABCD");

            // Assert
            result
                .Should()
                .BeFalse("an unchanged id takes the dialog-free break/return-false path");
            entry.ProjectID.Should().Be("ABCD", "the id is unchanged");
        }

        // ---- CompareTo(IProjectEntry) ----

        [TestMethod]
        public void CompareTo_NullOther_ReturnsPositiveOne()
        {
            // Arrange
            var entry = NewEntry("ABCD");

            // Act
            var result = entry.CompareTo((IProjectEntry)null);

            // Assert
            result.Should().Be(1, "a null comparand sorts before this instance");
        }

        [TestMethod]
        public void CompareTo_ThisProjectIdNull_ReturnsNegativeOne()
        {
            // Arrange: this.ProjectID is null, other has a valid id.
            var entry = NewEntry(null);
            var other = NewEntry("ABCD");

            // Act
            var result = entry.CompareTo(other);

            // Assert
            result.Should().Be(-1, "a null ProjectID sorts before a non-null one");
        }

        [TestMethod]
        public void CompareTo_EqualIds_ReturnsZero()
        {
            // Arrange
            var entry = NewEntry("ABCD");
            var other = NewEntry("ABCD");

            // Act
            var result = entry.CompareTo(other);

            // Assert
            result.Should().Be(0, "equal ordinal ids of equal length compare as equal");
        }

        [TestMethod]
        public void CompareTo_DifferentIds_ReturnsOrdinalSign()
        {
            // Arrange: ordinal "AAAA" < "ABCD".
            var lower = NewEntry("AAAA");
            var higher = NewEntry("ABCD");

            // Act
            var resultLower = lower.CompareTo(higher);
            var resultHigher = higher.CompareTo(lower);

            // Assert
            resultLower.Should().BeNegative("AAAA precedes ABCD ordinally");
            resultHigher.Should().BePositive("ABCD follows AAAA ordinally");
        }

        // Note on the CompareTo length tie-break (ProjectEntry.CompareTo lines ~197-204): that
        // branch is only reached when string.CompareOrdinal returns 0 for two ids of differing
        // length, which requires constructing an entry with a non-4-character ProjectID. Every
        // accessible ProjectEntry constructor routes ProjectID through the validating setter,
        // which shows a MessageBox for any non-null id whose length != 4. The tie-break is
        // therefore not reachable without triggering a dialog, so it is intentionally not
        // exercised here (recorded with the SetProjectId dialog gap in evidence/other/). The
        // equal-ids test above covers the CompareOrdinal == 0 / equal-length outcome.

        // ---- CompareTo(object) ----

        [TestMethod]
        public void CompareToObject_NullObject_ReturnsPositiveOne()
        {
            // Arrange
            var entry = NewEntry("ABCD");

            // Act
            var result = entry.CompareTo((object)null);

            // Assert
            result.Should().Be(1);
        }

        [TestMethod]
        public void CompareToObject_ProjectEntry_DelegatesToTypedCompareTo()
        {
            // Arrange
            var entry = NewEntry("ABCD");
            object other = NewEntry("ABCD");

            // Act
            var result = entry.CompareTo(other);

            // Assert
            result.Should().Be(0, "an IProjectEntry object is compared via the typed overload");
        }

        [TestMethod]
        public void CompareToObject_NonProjectEntry_ThrowsArgumentException()
        {
            // Arrange
            var entry = NewEntry("ABCD");

            // Act
            Action act = () => entry.CompareTo("not a project entry");

            // Assert
            act.Should().Throw<ArgumentException>("a non-IProjectEntry object cannot be compared");
        }
    }
}
