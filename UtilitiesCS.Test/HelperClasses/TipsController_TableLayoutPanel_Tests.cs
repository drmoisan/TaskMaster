using System;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskVisualization;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses
{
    /// <summary>
    /// Unit tests for <see cref="TipsController"/> exercising code paths that require
    /// a <see cref="TableLayoutPanel"/> parent.
    ///
    /// Purpose:
    ///     Covers InitializeLabel's TLP branch (column metadata capture), the
    ///     ResolveParent generic resolver, and the column-width side-effect paths
    ///     in Toggle(ToggleState) and ToggleColumnOnly(ToggleState).
    ///
    /// Constraints:
    ///     WinForms controls must be created on an STA thread.  All tests run
    ///     control setup and assertions on a dedicated STA thread and surface any
    ///     exception to the main thread for MSTest to record.
    /// </summary>
    [TestClass]
    public class TipsController_TableLayoutPanel_Tests
    {
        /// <summary>
        /// Creates a two-column single-row TableLayoutPanel with a Label placed
        /// at column 0, row 0. Used by all TLP-path tests to avoid repeating setup.
        ///
        /// Returns:
        ///     A tuple containing the configured TLP and its child label.
        ///
        /// Side Effects:
        ///     Clears any auto-populated ColumnStyles/RowStyles before adding
        ///     explicit Percent entries so that ColumnStyles[0].Width is 50f.
        /// </summary>
        private static (TableLayoutPanel Tlp, Label Label) BuildTlpWithLabel()
        {
            var tlp = new TableLayoutPanel { RowCount = 1, ColumnCount = 2 };

            // ColumnCount auto-populates ColumnStyles; clear and re-add known values
            tlp.ColumnStyles.Clear();
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            tlp.RowStyles.Clear();
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var label = new Label();
            tlp.Controls.Add(label, 0, 0);
            return (tlp, label);
        }

        /// <summary>
        /// Verifies InitializeLabel follows the TableLayoutPanel branch when the label
        /// is parented to a TLP, setting TLP reference, ColumnNumber, and ColumnWidth.
        /// Also exercises ResolveParent&lt;T&gt; by calling it directly.
        ///
        /// Returns:
        ///     Asserts TLP is non-null, ColumnNumber is 0, and ResolveParent returns
        ///     a non-null reference.
        /// </summary>
        [TestMethod]
        public void Constructor_LabelUnderTableLayoutPanel_SetsTlpAndColumnMetadata()
        {
            TableLayoutPanel capturedTlp = null;
            int capturedColumn = -1;
            TableLayoutPanel resolvedParent = null;
            Exception caught = null;

            var t = new Thread(() =>
            {
                try
                {
                    var (tlp, label) = BuildTlpWithLabel();
                    var ctrl = new TipsController(label);
                    capturedTlp = ctrl.TLP;
                    capturedColumn = ctrl.ColumnNumber;
                    resolvedParent = ctrl.ResolveParent<TableLayoutPanel>(label);
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            caught.Should().BeNull("construction with a TLP-parented label must not throw");
            capturedTlp
                .Should()
                .NotBeNull("TLP must be set when the label's parent is a TableLayoutPanel");
            capturedColumn.Should().Be(0, "label placed at column 0 must yield ColumnNumber 0");
            resolvedParent.Should().NotBeNull("ResolveParent<T> must return the label's parent");
        }

        /// <summary>
        /// Verifies Toggle(ToggleState) adjusts the TLP column width when the parent
        /// is a single-row TableLayoutPanel — the conditional column-width side-effect path.
        ///
        /// Returns:
        ///     Asserts column width is 0 after Toggle(Off) and is restored after Toggle(On).
        /// </summary>
        [TestMethod]
        public void Toggle_DesiredStateWithSingleRowTlp_AdjustsColumnWidth()
        {
            float widthAfterOff = -1f;
            float widthAfterOn = -1f;
            Exception caught = null;

            var t = new Thread(() =>
            {
                try
                {
                    var (tlp, label) = BuildTlpWithLabel();
                    var ctrl = new TipsController(label);

                    ctrl.Toggle(Enums.ToggleState.Off);
                    widthAfterOff = tlp.ColumnStyles[0].Width;

                    ctrl.Toggle(Enums.ToggleState.On);
                    widthAfterOn = tlp.ColumnStyles[0].Width;
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            caught.Should().BeNull();
            widthAfterOff
                .Should()
                .Be(0f, "Toggle(Off) on a single-row TLP must zero the column width");
            widthAfterOn
                .Should()
                .BeApproximately(50f, 0.001f, "Toggle(On) must restore the original column width");
        }

        /// <summary>
        /// Verifies ToggleColumnOnly adjusts the TLP column width when the parent is a
        /// TableLayoutPanel — the inner TLP assignment path not reachable via a Panel.
        ///
        /// Returns:
        ///     Asserts column width is 0 after ToggleColumnOnly(Off) and is restored
        ///     after ToggleColumnOnly(On).
        /// </summary>
        [TestMethod]
        public void ToggleColumnOnly_WithTlpParent_AdjustsColumnWidth()
        {
            float widthAfterOff = -1f;
            float widthAfterOn = -1f;
            Exception caught = null;

            var t = new Thread(() =>
            {
                try
                {
                    var (tlp, label) = BuildTlpWithLabel();
                    var ctrl = new TipsController(label);

                    ctrl.ToggleColumnOnly(Enums.ToggleState.Off);
                    widthAfterOff = tlp.ColumnStyles[0].Width;

                    ctrl.ToggleColumnOnly(Enums.ToggleState.On);
                    widthAfterOn = tlp.ColumnStyles[0].Width;
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            caught.Should().BeNull();
            widthAfterOff.Should().Be(0f, "ToggleColumnOnly(Off) must zero the TLP column width");
            widthAfterOn
                .Should()
                .BeApproximately(50f, 0.001f, "ToggleColumnOnly(On) must restore the column width");
        }
    }
}
