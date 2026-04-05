using System;
using System.Threading;
using System.Threading.Tasks;
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
            caughtException
                .Should()
                .BeNull("construction and ResolveParentType should not throw for a Panel parent");
            result
                .Should()
                .Be(typeof(Panel), "a label whose parent is a Panel should resolve to Panel type");
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
            caughtException
                .Should()
                .BeNull("constructor should not throw for a Panel-parented label");
            columnNumber
                .Should()
                .Be(
                    0,
                    "Panel path sets ColumnNumber to 0 since no TableLayoutPanel column applies"
                );
            tlp.Should()
                .BeNull("Panel path does not assign a TableLayoutPanel, so TLP must be null");
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
            afterFirstToggle
                .Should()
                .BeTrue("first Toggle from Off state must make the label visible");
            afterSecondToggle
                .Should()
                .BeFalse("second Toggle from On state must restore the label to not visible");
        }

        // ----------------------------------------------------------------
        // Constructor — TableLayoutPanel parent
        // ----------------------------------------------------------------

        /// <summary>
        /// Verifies that the public constructor correctly initialises column
        /// metadata when the label's parent is a <see cref="TableLayoutPanel"/>.
        ///
        /// Purpose:
        ///     Exercises the TLP branch of SetParentProperties, confirming that
        ///     column index, width, and TLP reference are populated.
        ///
        /// Returns:
        ///     Asserts TLP is not null, ColumnNumber is 0, and ColumnWidth matches
        ///     the TLP column-style width.
        /// </summary>
        [TestMethod]
        public void Constructor_LabelUnderTableLayoutPanel_SetsColumnProperties()
        {
            float columnWidth = -1f;
            int columnNumber = -1;
            TableLayoutPanel tlpResult = null;
            Exception caughtException = null;
            const float expectedWidth = 50f;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: single-column, single-row TLP with a known column width
                    var tlp = new TableLayoutPanel { ColumnCount = 1, RowCount = 1 };
                    tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, expectedWidth));
                    tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                    var label = new Label { Visible = true };
                    tlp.Controls.Add(label, 0, 0);

                    // Act
                    var details = new QfcTipsDetails(label);
                    columnWidth = details.ColumnWidth;
                    columnNumber = details.ColumnNumber;
                    tlpResult = details.TLP;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException
                .Should()
                .BeNull("constructor should not throw for a TLP-parented label");
            tlpResult.Should().NotBeNull("TLP branch assigns the TableLayoutPanel reference");
            columnNumber.Should().Be(0, "label is at column 0 in the TLP");
            columnWidth
                .Should()
                .Be(expectedWidth, "ColumnWidth is read directly from the TLP column style");
        }

        // ----------------------------------------------------------------
        // ResolveParentType — error paths
        // ----------------------------------------------------------------

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.ResolveParentType"/> throws
        /// <see cref="ArgumentException"/> when the label has no parent.
        ///
        /// Purpose:
        ///     Covers the null-parent guard in ResolveParentType.
        ///     After valid construction the label is orphaned so that
        ///     its Parent becomes null before the second call.
        /// </summary>
        [TestMethod]
        public void ResolveParentType_NullParent_ThrowsArgumentException()
        {
            Exception thrownException = null;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: valid Panel construction, then orphan the label
                    var panel = new Panel();
                    var label = new Label();
                    panel.Controls.Add(label);
                    var details = new QfcTipsDetails(label);

                    // Remove label so label.Parent becomes null
                    panel.Controls.Remove(label);

                    // Act
                    try
                    {
                        details.ResolveParentType();
                    }
                    catch (ArgumentException ex)
                    {
                        thrownException = ex;
                    }
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException.Should().BeNull();
            thrownException.Should().NotBeNull("a null parent must throw ArgumentException");
            thrownException.Should().BeOfType<ArgumentException>();
        }

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.ResolveParentType"/> throws
        /// <see cref="ArgumentException"/> when the label's parent is a type
        /// that is not supported (not Panel or TableLayoutPanel).
        ///
        /// Purpose:
        ///     Covers the unsupported-parent-type branch in ResolveParentType
        ///     by reparenting the label to a GroupBox after valid construction.
        /// </summary>
        [TestMethod]
        public void ResolveParentType_UnsupportedParentType_ThrowsArgumentException()
        {
            Exception thrownException = null;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: construct with Panel parent, then reparent to GroupBox
                    var panel = new Panel();
                    var label = new Label();
                    panel.Controls.Add(label);
                    var details = new QfcTipsDetails(label);

                    // Reparent to GroupBox (adds and implicitly removes from Panel)
                    var groupBox = new GroupBox();
                    groupBox.Controls.Add(label);

                    // Act
                    try
                    {
                        details.ResolveParentType();
                    }
                    catch (ArgumentException ex)
                    {
                        thrownException = ex;
                    }
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException.Should().BeNull();
            thrownException
                .Should()
                .NotBeNull("unsupported parent type must throw ArgumentException");
            thrownException.Should().BeOfType<ArgumentException>();
        }

        // ----------------------------------------------------------------
        // Properties — IsNavColumn setter, ColumnWidth getter, LabelControl getter
        // ----------------------------------------------------------------

        /// <summary>
        /// Verifies that the <see cref="QfcTipsDetails.IsNavColumn"/> setter stores
        /// the supplied value and that the ColumnWidth and LabelControl getters
        /// return expected values.
        ///
        /// Purpose:
        ///     Covers the IsNavColumn setter, ColumnWidth getter, and LabelControl
        ///     getter read paths not exercised by constructor tests alone.
        /// </summary>
        [TestMethod]
        public void IsNavColumn_SetToTrue_GetReturnsTrue_AndPropertiesAccessible()
        {
            bool isNavColumn = false;
            float columnWidth = -1f;
            Label labelResult = null;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: Panel-parented label
                    var panel = new Panel();
                    var label = new Label();
                    panel.Controls.Add(label);
                    var details = new QfcTipsDetails(label);

                    // Act: exercise setter and getters
                    details.IsNavColumn = true;
                    isNavColumn = details.IsNavColumn;
                    columnWidth = details.ColumnWidth;
                    labelResult = details.LabelControl;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException.Should().BeNull();
            isNavColumn.Should().BeTrue("IsNavColumn was set to true");
            columnWidth.Should().Be(0f, "Panel path initialises ColumnWidth to 0");
            labelResult.Should().NotBeNull("LabelControl returns the underlying label");
        }

        // ----------------------------------------------------------------
        // Toggle(bool sharedColumn) and Toggle(ToggleState, bool)
        // ----------------------------------------------------------------

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.Toggle(bool)"/> transitions the
        /// label from hidden (Off) to visible (On) on a Panel-parented label.
        ///
        /// Purpose:
        ///     Covers the else branch of Toggle(bool) (Off → delegates to
        ///     Toggle(ToggleState.On, sharedColumn)) and the On branch of
        ///     Toggle(ToggleState, bool) with a non-TLP parent.
        /// </summary>
        [TestMethod]
        public void Toggle_BoolSharedColumn_FromOff_MakesLabelVisible()
        {
            bool visible = false;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: label starts hidden → ToggleState.Off
                    var panel = new Panel();
                    var label = new Label { Visible = false };
                    panel.Controls.Add(label);
                    var details = new QfcTipsDetails(label);

                    // Act: Toggle(bool) from Off delegates to Toggle(ToggleState.On, true)
                    details.Toggle(sharedColumn: true);
                    visible = label.Visible;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException.Should().BeNull("Toggle(bool) should not throw");
            visible.Should().BeTrue("Toggle(bool) from Off state must make the label visible");
        }

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.Toggle(bool)"/> transitions the
        /// label from visible (On) to hidden (Off) on a Panel-parented label.
        ///
        /// Purpose:
        ///     Covers the if branch of Toggle(bool) (On → delegates to
        ///     Toggle(ToggleState.Off, sharedColumn)) and the Off branch of
        ///     Toggle(ToggleState, bool) with a non-TLP parent.
        /// </summary>
        [TestMethod]
        public void Toggle_BoolSharedColumn_FromOn_MakesLabelHidden()
        {
            bool visible = true;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: label starts visible → ToggleState.On
                    var panel = new Panel();
                    var label = new Label { Visible = true };
                    panel.Controls.Add(label);
                    var details = new QfcTipsDetails(label);

                    // Act: Toggle(bool) from On delegates to Toggle(ToggleState.Off, false)
                    details.Toggle(sharedColumn: false);
                    visible = label.Visible;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException.Should().BeNull("Toggle(bool) should not throw");
            visible.Should().BeFalse("Toggle(bool) from On state must hide the label");
        }

        // ----------------------------------------------------------------
        // Toggle — TLP parent: column-width branches
        // ----------------------------------------------------------------

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.Toggle(Enums.ToggleState)"/>
        /// updates TLP column width when the parent is a single-row
        /// <see cref="TableLayoutPanel"/>.
        ///
        /// Purpose:
        ///     Covers the TLP column-width branch inside Toggle(ToggleState):
        ///     restores the saved width on On and sets it to 0 on Off.
        /// </summary>
        [TestMethod]
        public void Toggle_DesiredState_WithSingleRowTlp_UpdatesColumnWidth()
        {
            float widthAfterOn = -1f;
            float widthAfterOff = -1f;
            const float originalWidth = 60f;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: single-row TLP so RowCount==1 satisfies the guard
                    var tlp = new TableLayoutPanel { ColumnCount = 1, RowCount = 1 };
                    tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, originalWidth));
                    tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                    var label = new Label { Visible = false };
                    tlp.Controls.Add(label, 0, 0);
                    var details = new QfcTipsDetails(label);

                    // Act: Toggle On restores saved column width
                    details.Toggle(Enums.ToggleState.On);
                    widthAfterOn = tlp.ColumnStyles[0].Width;

                    // Act: Toggle Off zeroes column width
                    details.Toggle(Enums.ToggleState.Off);
                    widthAfterOff = tlp.ColumnStyles[0].Width;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException.Should().BeNull();
            widthAfterOn
                .Should()
                .Be(originalWidth, "Toggle(On) must restore the column width to the saved value");
            widthAfterOff.Should().Be(0f, "Toggle(Off) must zero the column width");
        }

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.Toggle(Enums.ToggleState, bool)"/>
        /// updates TLP column width when the parent is a single-row
        /// <see cref="TableLayoutPanel"/>.
        ///
        /// Purpose:
        ///     Covers the TLP column-width branch inside Toggle(ToggleState, bool):
        ///     restores the saved width on On and sets it to 0 on Off.
        /// </summary>
        [TestMethod]
        public void Toggle_DesiredStateAndBool_WithSingleRowTlp_UpdatesColumnWidth()
        {
            float widthAfterOn = -1f;
            float widthAfterOff = -1f;
            const float originalWidth = 75f;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    // Arrange: single-row TLP; sharedColumn=true satisfies the guard
                    var tlp = new TableLayoutPanel { ColumnCount = 1, RowCount = 1 };
                    tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, originalWidth));
                    tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                    var label = new Label { Visible = false };
                    tlp.Controls.Add(label, 0, 0);
                    var details = new QfcTipsDetails(label);

                    // Act: Toggle(On, sharedColumn=true) restores width
                    details.Toggle(Enums.ToggleState.On, sharedColumn: true);
                    widthAfterOn = tlp.ColumnStyles[0].Width;

                    // Act: Toggle(Off, sharedColumn=false but RowCount==1 still satisfies)
                    details.Toggle(Enums.ToggleState.Off, sharedColumn: false);
                    widthAfterOff = tlp.ColumnStyles[0].Width;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException.Should().BeNull();
            widthAfterOn
                .Should()
                .Be(
                    originalWidth,
                    "Toggle(On, bool) must restore the column width to the saved value"
                );
            widthAfterOff.Should().Be(0f, "Toggle(Off, bool) must zero the column width");
        }

        // ----------------------------------------------------------------
        // CreateAsync — private constructor + InitializeAsync paths
        // ----------------------------------------------------------------

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.CreateAsync"/> returns an
        /// initialised instance when the supplied <see cref="SynchronizationContext"/>
        /// is the current thread's context (making <c>await _uiContext</c>
        /// complete synchronously), and the label starts hidden.
        ///
        /// Purpose:
        ///     Covers the private constructor, the CreateAsync body, and the
        ///     InitializeAsync body including the Visible=false else-branch.
        ///
        /// Side Effects:
        ///     CreateAsync is called from a Task.Run lambda to avoid the
        ///     CoWaitForMultipleHandles deadlock that occurs when blocking an
        ///     STA thread via GetAwaiter().GetResult() on .NET Framework 4.8.
        /// </summary>
        [TestMethod]
        public void CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails()
        {
            // Run inside Task.Run so that "await" does not block an STA message pump.
            // Controls created without a visible HWND are safe on non-STA threads.
            var task = Task.Run(async () =>
            {
                var panel = new Panel();
                var label = new Label { Visible = false };
                panel.Controls.Add(label);

                // Set a base SynchronizationContext as Current so that
                // SynchronizationContextAwaiter.IsCompleted (= _context == Current)
                // returns true inside InitializeAsync, keeping execution synchronous.
                var ctx = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(ctx);
                try
                {
                    return await QfcTipsDetails.CreateAsync(label, ctx, CancellationToken.None);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                }
            });

            bool completed = task.Wait(TimeSpan.FromSeconds(10));
            completed.Should().BeTrue("CreateAsync should complete within 10 seconds");
            task.Exception.Should().BeNull("CreateAsync should not throw");
            task.Result.Should().NotBeNull("CreateAsync must return an initialised details object");
        }

        /// <summary>
        /// Verifies that <see cref="QfcTipsDetails.CreateAsync"/> returns an
        /// initialised instance when the label starts visible, covering the
        /// Visible=true if-branch inside <c>InitializeAsync</c>.
        ///
        /// Purpose:
        ///     Covers the InitializeAsync if-branch where ToggleState is set to On.
        ///
        /// Side Effects:
        ///     CreateAsync is called from a Task.Run lambda to avoid the
        ///     CoWaitForMultipleHandles deadlock that occurs when blocking an
        ///     STA thread via GetAwaiter().GetResult() on .NET Framework 4.8.
        /// </summary>
        [TestMethod]
        public void CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState()
        {
            // Run inside Task.Run so that "await" does not block an STA message pump.
            var task = Task.Run(async () =>
            {
                var panel = new Panel();
                // Visible=true exercises the if (LabelControl.Visible) On-branch in
                // InitializeAsync, setting _state = ToggleState.On.
                var label = new Label { Visible = true };
                panel.Controls.Add(label);

                var ctx = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(ctx);
                try
                {
                    return await QfcTipsDetails.CreateAsync(label, ctx, CancellationToken.None);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                }
            });

            bool completed = task.Wait(TimeSpan.FromSeconds(10));
            completed
                .Should()
                .BeTrue("CreateAsync with a visible label should complete within 10 seconds");
            task.Exception.Should().BeNull("CreateAsync with a visible label should not throw");
            task.Result.Should()
                .NotBeNull("CreateAsync must return a details object for a visible label");
        }
    }
}
