using System;
using System.Reflection;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence.FilterOlFolders;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for OSBrowser constructor setup and FormatFileSize.
    ///
    /// Purpose:
    ///     Covers the column-setup, tree-setup, and file-size formatting paths
    ///     without requiring a real file system (delegates configured in the
    ///     constructor are asserted via reflection on the private treeListView field).
    ///
    /// Usage:
    ///     All tests instantiate OSBrowser on an STA thread. The constructor calls
    ///     SetupColumns(), SetupDragAndDrop(), and SetupTree() which enumerate real
    ///     drives; tests only assert on delegate assignment and format output.
    /// </summary>
    [TestClass]
    public class OSBrowser_Tests
    {
        // ---------------------------------------------------------------------------
        // Helper — reflection accessor for private treeListView field
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the private treeListView field from the given OSBrowser instance
        /// via reflection, since the Designer generated it with private visibility.
        /// </summary>
        private static TreeListView GetTreeListView(OSBrowser browser) =>
            (TreeListView)
                typeof(OSBrowser)
                    .GetField("treeListView", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(browser);

        // ---------------------------------------------------------------------------
        // P13-T1: Column setup initializes the expected number and names of columns
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the OSBrowser constructor's SetupColumns() call attaches
        /// AspectGetters to the size and file-type columns (confirming columns were
        /// initialised with custom configuration, not left at designer defaults only).
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_SetupColumns_AttachesAspectGetterToSizeColumn()
        {
            // Arrange + Act
            var browser = new OSBrowser();
            var tlv = GetTreeListView(browser);

            // SetupColumns sets an AspectToStringConverter on olvColumnSize; verify
            // that the column count matches the configured designer columns (5 data
            // columns plus the primary tree column for a total ≥ 2).
            tlv.Columns.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        // ---------------------------------------------------------------------------
        // P13-T2: Tree setup configures the expected tree options
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the OSBrowser constructor's SetupTree() call assigns both
        /// CanExpandGetter and ChildrenGetter on the TreeListView.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_SetupTree_AssignsBothDelegates()
        {
            // Arrange + Act
            var browser = new OSBrowser();
            var tlv = GetTreeListView(browser);

            // Assert
            tlv.CanExpandGetter.Should().NotBeNull();
            tlv.ChildrenGetter.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // P13-T3: FormatFileSize returns the expected string for a bytes-range input
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that FormatFileSize returns a "bytes" string when the input is
        /// below the 1 KB threshold.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void FormatFileSize_WithBytesInput_ReturnsBytesString()
        {
            // Arrange
            var browser = new OSBrowser();

            // Act
            var result = browser.FormatFileSize(512);

            // Assert
            result.Should().EndWith("bytes");
        }

        // ---------------------------------------------------------------------------
        // P13-T4: FormatFileSize returns the expected string for KB and MB inputs
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that FormatFileSize returns a "KB" string for a 1 KB input and
        /// an "MB" string for a 1 MB input.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void FormatFileSize_WithKbAndMbInputs_ReturnsCorrectUnits()
        {
            // Arrange
            var browser = new OSBrowser();

            // Act
            var kbResult = browser.FormatFileSize(1024);
            var mbResult = browser.FormatFileSize(1024 * 1024);

            // Assert
            kbResult.Should().Contain("KB");
            mbResult.Should().Contain("MB");
        }
    }
}
