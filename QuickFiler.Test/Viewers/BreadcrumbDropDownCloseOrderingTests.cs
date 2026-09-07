using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #796 (AC6): the host-side close-ordering diagnostic must carry every field that
    /// discriminates between the candidate close paths, so the Phase 2 transcript can be read
    /// without inference.
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbDropDownCloseOrderingTests
    {
        /// <summary>
        /// Scenario: the pure formatter is called with a fixed argument tuple. Expected outcome:
        /// the returned line carries all six discriminating field labels and the supplied close
        /// reason. Asserting on the formatter rather than on source text makes the AC6 evidence a
        /// deterministic managed-seam assertion; no popup, window, or WebView2 surface is created.
        /// </summary>
        [TestMethod]
        public void FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField()
        {
            // Arrange
            const ToolStripDropDownCloseReason CloseReason =
                ToolStripDropDownCloseReason.AppFocusChange;

            // Act
            string line = BreadcrumbDropDownHost.FormatDropDownClosedDiagnostics(
                CloseReason,
                programmaticClose: false,
                openState: true,
                autoClose: true,
                disposed: false,
                pendingClose: true
            );

            // Assert
            line.Should().Contain("CloseReason=");
            line.Should().Contain("ProgrammaticClose=");
            line.Should().Contain("OpenState=");
            line.Should().Contain("AutoClose=");
            line.Should().Contain("Disposed=");
            line.Should().Contain("PendingClose=");
            line.Should().Contain(CloseReason.ToString());
        }
    }
}
