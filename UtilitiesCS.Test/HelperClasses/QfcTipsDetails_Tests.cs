using System;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses
{
    /// <summary>
    /// Unit tests for <see cref="QfcTipsDetails"/>.
    ///
    /// Purpose:
    ///     Covers the public initialization path, parent-type resolution, and
    ///     visibility toggle behavior of <see cref="QfcTipsDetails"/>.
    ///
    /// Constraints:
    ///     WinForms controls must be created on an STA thread.
    ///     All tests run control creation and assertions on a dedicated STA thread
    ///     and surface any exception to the main thread for MSTest to record.
    /// </summary>
    [TestClass]
    public class QfcTipsDetails_Tests
    {
        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.ResolveParentType"/> returns
        /// <see cref="Panel"/> when the label's parent is a <see cref="Panel"/>.
        ///
        /// Purpose:
        ///     Exercises the parent-type resolution branch that accepts a Panel
        ///     control as a valid parent, confirming the method returns the exact
        ///     runtime type of the parent.
        ///
        /// Returns:
        ///     Asserts result equals <c>typeof(Panel)</c>.
        /// </summary>
        [TestMethod]
        public void ResolveParentType_LabelUnderPanel_ReturnsPanelType()
        {
            Type result = null;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: label parented to a Panel (accepted by ResolveParentType)
                    var panel = new Panel();
                    var label = new Label();
                    panel.Controls.Add(label);

                    // Act: construct details, then call ResolveParentType a second time
                    var details = new QfcTipsDetails(label);
                    result = details.ResolveParentType();
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
            caughtException.Should().BeNull("construction and ResolveParentType should not throw for a Panel parent");
            result.Should().Be(typeof(Panel), "a label whose parent is a Panel should resolve to Panel type");
        }

        /// <summary>
        /// Verifies that the public constructor initialises the details object
        /// with the correct property values when the label's parent is a <see cref="Panel"/>.
        ///
        /// Purpose:
        ///     The public constructor runs the same initialization path as
        ///     <c>InitializeAsync</c>: it resolves the parent type, calls
        ///     <c>SetParentProperties</c>, and sets the toggle state.  This test
        ///     asserts that <see cref="QfcTipsDetails.ColumnNumber"/> is 0 (Panel
        ///     path does not use a TableLayoutPanel column), and that
        ///     <see cref="QfcTipsDetails.TLP"/> is null, confirming the expected
        ///     post-initialization state for a Panel-parented label.
        ///
        /// Returns:
        ///     Asserts ColumnNumber equals 0 and TLP is null.
        /// </summary>
        [TestMethod]
        public void Constructor_LabelUnderPanel_SetsColumnNumberZeroAndNullTlp()
        {
            int columnNumber = -1;
            System.Windows.Forms.TableLayoutPanel tlp = null;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: visible label in a Panel
                    var panel = new Panel();
                    var label = new Label { Visible = true };
                    panel.Controls.Add(label);

                    // Act: construct initialises parentType and column metadata
                    var details = new QfcTipsDetails(label);

                    // Capture properties for assertion outside the STA thread
                    columnNumber = details.ColumnNumber;
                    tlp = details.TLP;
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
            caughtException.Should().BeNull("constructor should not throw for a Panel-parented label");
            columnNumber.Should().Be(0, "Panel path sets ColumnNumber to 0 since no TableLayoutPanel column applies");
            tlp.Should().BeNull("Panel path does not assign a TableLayoutPanel, so TLP must be null");
        }

        /// <summary>
        /// Verifies that calling <see cref="QfcTipsDetails.Toggle()"/> twice returns
        /// the label's <see cref="Control.Visible"/> property to its original state.
        ///
        /// Purpose:
        ///     Exercises the stateful toggle logic: Off → Toggle() → On → Toggle() → Off.
        ///     Confirms that the Toggle method reliably inverts state and that two
        ///     consecutive calls restore the original visibility.
        ///
        /// Side Effects:
        ///     Modifies and then restores <see cref="Label.Visible"/> on a transient
        ///     WinForms label; no persistent state is left.
        /// </summary>
        [TestMethod]
        public void Toggle_CalledTwice_RestoresOriginalLabelVisibility()
        {
            bool initialVisible = false;
            bool afterFirstToggle = false;
            bool afterSecondToggle = false;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: label starts hidden (ToggleState.Off) under a Panel
                    var panel = new Panel();
                    var label = new Label { Visible = false };
                    panel.Controls.Add(label);
                    var details = new QfcTipsDetails(label);

                    initialVisible = label.Visible; // false

                    // Act: first toggle (Off → On)
                    details.Toggle();
                    afterFirstToggle = label.Visible; // true

                    // Act: second toggle (On → Off)
                    details.Toggle();
                    afterSecondToggle = label.Visible; // false (restored)
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
            initialVisible.Should().BeFalse("label is initialised with Visible = false");
            afterFirstToggle.Should().BeTrue("first Toggle from Off state must make the label visible");
            afterSecondToggle.Should().BeFalse("second Toggle from On state must restore the label to not visible");
        }
    }
}
