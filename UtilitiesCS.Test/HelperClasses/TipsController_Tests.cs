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
    /// Unit tests for <see cref="TipsController"/>.
    ///
    /// Purpose:
    ///     Covers the initialization path (label assignment, parent-type resolution,
    ///     and column/panel metadata setup), the toggle state transitions, and the
    ///     idempotency of double-toggle operations on <see cref="TipsController"/>.
    ///
    /// Constraints:
    ///     WinForms controls must be created on an STA thread.
    ///     All tests run control setup and assertions on a dedicated STA thread and
    ///     surface any exception to the main thread for MSTest to record.
    /// </summary>
    [TestClass]
    public class TipsController_Tests
    {
        /// <summary>
        /// Verifies that constructing a <see cref="TipsController"/> with a label
        /// whose parent is a <see cref="Panel"/> stores the label reference and
        /// sets column metadata to the Panel-path defaults.
        ///
        /// Purpose:
        ///     Exercises <c>InitializeLabel</c>'s Panel branch: since no
        ///     TableLayoutPanel is involved, ColumnNumber should be 0 and TLP null.
        ///
        /// Returns:
        ///     Asserts <c>LabelControl</c> is the same reference, <c>ColumnNumber</c>
        ///     equals 0, and <c>TLP</c> is null.
        /// </summary>
        [TestMethod]
        public void Constructor_LabelUnderPanel_StoresLabelAndSetsColumnDefaults()
        {
            Label capturedLabelControl = null;
            int columnNumber = -1;
            System.Windows.Forms.TableLayoutPanel tlp = null;
            Exception caughtException = null;
            Label originalLabel = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: label under a Panel so the Panel branch is exercised
                    var panel = new Panel();
                    originalLabel = new Label();
                    panel.Controls.Add(originalLabel);

                    // Act: construct TipsController via the label-only overload
                    var controller = new TipsController(originalLabel);

                    // Capture results for assertion on main thread
                    capturedLabelControl = controller.LabelControl;
                    columnNumber = controller.ColumnNumber;
                    tlp = controller.TLP;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException
                .Should()
                .BeNull("construction with a Panel-parented label must not throw");
            capturedLabelControl
                .Should()
                .BeSameAs(
                    originalLabel,
                    "LabelControl must be the exact label passed to the constructor"
                );
            columnNumber
                .Should()
                .Be(
                    0,
                    "Panel path does not use a TableLayoutPanel column, so ColumnNumber defaults to 0"
                );
            tlp.Should()
                .BeNull("Panel path does not assign a TableLayoutPanel, so TLP must be null");
        }

        /// <summary>
        /// Verifies that <see cref="TipsController.Toggle(Enums.ToggleState)"/> with
        /// <see cref="Enums.ToggleState.Off"/> sets <see cref="Label.Visible"/> and
        /// <see cref="Label.Enabled"/> to false, and with
        /// <see cref="Enums.ToggleState.On"/> restores them to true.
        ///
        /// Purpose:
        ///     Exercises the targeted toggle path for Panel-parented labels where no
        ///     TableLayoutPanel column-width side effect applies.  Confirms the toggle
        ///     affects only the intended label properties.
        ///
        /// Side Effects:
        ///     Modifies Visible and Enabled on a transient WinForms label;
        ///     no persistent state is left.
        /// </summary>
        [TestMethod]
        public void Toggle_DesiredStateOffThenOn_SetsLabelVisibilityAndEnabledCorrectly()
        {
            bool visibleAfterOff = true;
            bool enabledAfterOff = true;
            bool visibleAfterOn = false;
            bool enabledAfterOn = false;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: label under Panel; initial state is On after construction
                    var panel = new Panel();
                    var label = new Label();
                    panel.Controls.Add(label);
                    var controller = new TipsController(label);

                    // Act: toggle to Off
                    controller.Toggle(Enums.ToggleState.Off);
                    visibleAfterOff = label.Visible;
                    enabledAfterOff = label.Enabled;

                    // Act: toggle back to On
                    controller.Toggle(Enums.ToggleState.On);
                    visibleAfterOn = label.Visible;
                    enabledAfterOn = label.Enabled;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException.Should().BeNull("Toggle with explicit ToggleState should not throw");
            visibleAfterOff.Should().BeFalse("Toggle(Off) must set label Visible to false");
            enabledAfterOff.Should().BeFalse("Toggle(Off) must set label Enabled to false");
            visibleAfterOn.Should().BeTrue("Toggle(On) must set label Visible to true");
            enabledAfterOn.Should().BeTrue("Toggle(On) must set label Enabled to true");
        }

        /// <summary>
        /// Verifies that calling <see cref="TipsController.Toggle()"/> twice in
        /// succession restores the label's <see cref="Label.Visible"/> to its
        /// pre-toggle value.
        ///
        /// Purpose:
        ///     Exercises the stateful toggle logic: construction sets state to On,
        ///     first Toggle() transitions to Off, second Toggle() transitions back
        ///     to On.  Confirms the toggle is fully reversible.
        ///
        /// Side Effects:
        ///     Modifies and then restores Visible on a transient WinForms label.
        /// </summary>
        [TestMethod]
        public void Toggle_CalledTwice_RestoresLabelVisibilityToOriginal()
        {
            bool visibleAfterConstruction = false;
            bool visibleAfterFirstToggle = false;
            bool visibleAfterSecondToggle = false;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: TipsController starts with state = On after construction
                    var panel = new Panel();
                    var label = new Label();
                    panel.Controls.Add(label);
                    var controller = new TipsController(label);

                    // Capture the baseline visibility (label default is true in WinForms)
                    visibleAfterConstruction = label.Visible;

                    // Act: first toggle (On → Off)
                    controller.Toggle();
                    visibleAfterFirstToggle = label.Visible;

                    // Act: second toggle (Off → On)
                    controller.Toggle();
                    visibleAfterSecondToggle = label.Visible;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException.Should().BeNull("Toggle should not throw");
            visibleAfterFirstToggle
                .Should()
                .BeFalse("first Toggle from On state must set label to not visible");
            visibleAfterSecondToggle
                .Should()
                .Be(
                    visibleAfterConstruction,
                    "second Toggle from Off state must restore label visibility to its post-construction value"
                );
        }
    }
}
