using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Controllers;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// The single STA-apartment regression test for issue #471: the item panel must shrink, not
    /// grow, when a conversation collapses and its rows are removed.
    /// <para>
    /// <strong>Why an STA test is taken here, and why no seam can replace it.</strong>
    /// The rounding contract itself is already covered without a control by the pure static
    /// <c>QfcCollectionController.ShrinkByRows</c>, exercised from the MTA file
    /// <c>QfcCollectionControllerDefects468Tests.cs</c>. What that helper cannot prove is the sign
    /// of the argument the <em>call site</em> passes to it. <c>ShrinkByRows</c> is deliberately
    /// sign-agnostic — a negative row count grows, which is how the insertion path expresses
    /// "make room for N rows" — so both the correct call and the defective call are valid uses of
    /// the helper and neither can be distinguished by any test of the helper alone. Extracting a
    /// further seam only moves the same question one level up: whatever member computes the row
    /// count, the defect is the sign of the value handed to it at the point of use, and only
    /// executing the real <c>EliminateSpaceForItems</c> against a real
    /// <see cref="TableLayoutPanel"/> observes that. That method assigns
    /// <see cref="Control.MinimumSize"/> and <see cref="Control.Size"/> on a WinForms control,
    /// which must be constructed on a single-threaded apartment.
    /// </para>
    /// <para>
    /// This is the ratified last-resort pattern and is kept as narrow as the defect allows. The
    /// class constructs one bare in-memory <see cref="TableLayoutPanel"/>, never calls
    /// <c>Show()</c> or <c>ShowDialog()</c>, never parents the panel to a form, never creates a
    /// window handle, relies on no message pump, and disposes the panel after every test. No new
    /// STA test may be added here for anything a seam can cover.
    /// </para>
    /// </summary>
    [STATestClass]
    public class QfcCollectionControllerLayoutStaTests
    {
        /// <summary>Height in pixels of a single template row used by every test in this class.</summary>
        private const float TemplateRowHeight = 25f;

        /// <summary>Starting minimum height of the panel under test, in pixels.</summary>
        private const int StartingMinimumHeight = 200;

        /// <summary>Starting minimum width of the panel under test, in pixels.</summary>
        private const int StartingMinimumWidth = 300;

        private TableLayoutPanel _panel;

        /// <summary>
        /// Builds the bare panel and template row style shared by the tests in this class. The
        /// panel is never shown and never given a parent, so no window handle is created and no
        /// message pump is required.
        /// </summary>
        [TestInitialize]
        public void CreatePanel()
        {
            _panel = new TableLayoutPanel { ColumnCount = 1, RowCount = 1 };
            _panel.RowStyles.Add(new RowStyle(SizeType.Absolute, TemplateRowHeight));
            _panel.MinimumSize = new Size(StartingMinimumWidth, StartingMinimumHeight);
        }

        /// <summary>
        /// Disposes the panel after every test. A <see cref="TableLayoutPanel"/> is an unmanaged
        /// resource holder even without a handle, and the General Unit Test Policy requires tests
        /// to leave no state behind for the next test in the run.
        /// </summary>
        [TestCleanup]
        public void DisposePanel()
        {
            _panel?.Dispose();
            _panel = null;
        }

        /// <summary>
        /// Builds a controller with its WinForms constructor bypassed and the two fields the
        /// layout arithmetic reads injected: the panel under test and the template row style.
        /// </summary>
        private QfcCollectionController CreateControllerOverPanel()
        {
            QfcCollectionController controller =
                QfcCollectionControllerTestSupport.CreateUninitializedController();
            QfcCollectionControllerTestSupport.SetField(controller, "_itemTlp", _panel);
            QfcCollectionControllerTestSupport.SetField(
                controller,
                "_template",
                new RowStyle(SizeType.Absolute, TemplateRowHeight)
            );
            return controller;
        }

        /// <summary>
        /// Issue #471. Removing rows must reduce the panel's minimum height by the template row
        /// height times the number of rows removed. Before the fix the two negations cancelled and
        /// the panel grew by that amount instead.
        /// </summary>
        /// <remarks>
        /// The removal index is deliberately past the end of the panel's rows, so
        /// <c>TableLayoutHelper.RemoveSpecificRow</c> takes its <c>rowIndex &gt;= RowCount</c>
        /// early return and the test observes the size arithmetic in isolation, with no row
        /// mutation and no control reparenting.
        /// </remarks>
        [TestMethod]
        public void EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount()
        {
            // Arrange
            QfcCollectionController controller = CreateControllerOverPanel();
            const int removalCount = 2;
            int indexPastTheLastRow = _panel.RowCount + 1;
            int expectedHeight = StartingMinimumHeight - (int)(TemplateRowHeight * removalCount);

            // Act
            controller.EliminateSpaceForItems(indexPastTheLastRow, removalCount);

            // Assert
            _panel
                .MinimumSize.Height.Should()
                .Be(
                    expectedHeight,
                    because: $"removing {removalCount} template rows of {TemplateRowHeight} px must "
                        + $"shrink the panel's minimum height from {StartingMinimumHeight} px to "
                        + $"{expectedHeight} px; a larger value means the panel grew when it should "
                        + "have shrunk, which is issue #471"
                );
            _panel
                .MinimumSize.Width.Should()
                .Be(
                    StartingMinimumWidth,
                    because: "the row arithmetic must not disturb the panel's width"
                );
        }

        /// <summary>
        /// Issue #471, AC-11. Making space for N rows and then eliminating space for the same N
        /// rows at the same index must leave the panel's <see cref="Control.MinimumSize"/> height
        /// exactly where it started. Before the fix the second call added a second increase instead
        /// of cancelling the first, so the pairing drifted upward by two row blocks.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The assertion is deliberately scoped to <see cref="Control.MinimumSize"/> and says
        /// nothing about <see cref="Control.Size"/>. <c>MakeSpaceForItems</c> has exactly two
        /// statements — one assignment to <c>MinimumSize</c> and one call to
        /// <c>TableLayoutHelper.InsertSpecificRow</c> — and neither references the panel's
        /// <c>Size</c> property at all. <c>EliminateSpaceForItems</c>, by contrast, assigns both
        /// <c>MinimumSize</c> and <c>Size</c>. The pairing is therefore neutral for
        /// <c>MinimumSize</c> and cannot be neutral for <c>Size</c> by construction: there is no
        /// code path that grows <c>Size</c> back. That asymmetry is pre-existing, predates this
        /// feature, and is not changed by it; asserting size-height neutrality here would assert a
        /// property the production code has never had.
        /// </para>
        /// <para>
        /// Any incidental rise in <c>Size</c> observed during this pairing comes from the WinForms
        /// minimum-size clamp inside <c>Control.SetBoundsCore</c>, which lifts a control whose size
        /// has fallen below a newly raised minimum. That is a framework side effect, not a
        /// restoration performed by <c>MakeSpaceForItems</c>.
        /// </para>
        /// </remarks>
        [TestMethod]
        public void MakeSpaceThenEliminateSpace_IsMinimumHeightNeutral()
        {
            // Arrange
            QfcCollectionController controller = CreateControllerOverPanel();
            const int rowCount = 2;
            const int index = 0;
            int startingHeight = _panel.MinimumSize.Height;

            // Act
            controller.MakeSpaceForItems(index, rowCount);
            controller.EliminateSpaceForItems(index, rowCount);

            // Assert
            _panel
                .MinimumSize.Height.Should()
                .Be(
                    startingHeight,
                    because: $"making space for {rowCount} rows and then eliminating space for the "
                        + $"same {rowCount} rows at index {index} must cancel exactly, returning the "
                        + $"minimum height to its recorded starting value of {startingHeight} px"
                );
        }
    }
}
