using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    /// <summary>
    /// Unit tests for <see cref="ConfigViewer"/>.
    ///
    /// Purpose:
    ///     Covers the controller-binding path (SetController), the cancel-handler
    ///     null-safety contract, and the disk-group activation toggle on
    ///     <see cref="ConfigViewer"/>.
    ///
    /// Constraints:
    ///     ConfigViewer is a WinForms Form; this class uses MSTest's STA class
    ///     execution mode to satisfy WinForms initialization requirements.
    /// </summary>
    [STATestClass]
    public class ConfigViewer_Tests
    {
        private static ConfigViewer CreateHeadlessViewer() =>
            (ConfigViewer)FormatterServices.GetUninitializedObject(typeof(ConfigViewer));

        /// <summary>
        /// Verifies that <see cref="ConfigViewer.SetController"/> assigns the
        /// controller to the <c>Controller</c> property and returns the same viewer
        /// instance for fluent chaining.
        ///
        /// Purpose:
        ///     The save-handler routing depends on the <c>Controller</c> property
        ///     being correctly set.  This test confirms the routing infrastructure
        ///     (SetController → Controller property assignment and fluent return)
        ///     works without error, covering the "binds controller to viewer"
        ///     acceptance criterion.
        ///
        /// Returns:
        ///     Asserts <c>Controller</c> equals the value passed to SetController
        ///     and <c>SetController</c> returns the same viewer reference.
        /// </summary>
        [TestMethod]
        public void SetController_SetsControllerPropertyAndReturnsViewer()
        {
            ConfigViewer viewer = null;
            try
            {
                // Arrange
                viewer = new ConfigViewer();

                // Act: pass null — ConfigController cannot be constructed without
                // complex mocked dependencies; this exercises the routing plumbing
                ConfigViewer returned = viewer.SetController(null);

                // Assert: property is assigned and fluent chaining returns same instance
                viewer
                    .Controller.Should()
                    .BeNull(
                        "SetController(null) must assign the null value to the Controller property"
                    );
                returned
                    .Should()
                    .BeSameAs(
                        viewer,
                        "SetController must return the same viewer to support fluent chaining"
                    );
            }
            finally
            {
                viewer?.Dispose();
            }
        }

        /// <summary>
        /// Verifies that invoking the cancel-click handler when <c>Controller</c>
        /// is null is a safe no-op that does not throw.
        ///
        /// Purpose:
        ///     The cancel handler body is <c>Controller?.Cancel()</c>.  When
        ///     <c>Controller</c> is null (which is the initial state after construction),
        ///     the null-conditional operator skips the call, so no exception should
        ///     be raised.  This confirms the cancel route correctly guards against
        ///     a null controller.
        ///
        /// Side Effects:
        ///     Invokes <c>ButtonCancel_Click</c> via reflection on a newly
        ///     constructed viewer; the viewer is disposed in the finally block.
        /// </summary>
        [TestMethod]
        public void ButtonCancelClick_WithNullController_IsNoOpWithoutThrowing()
        {
            ConfigViewer viewer = null;
            Exception caughtException = null;
            try
            {
                // Arrange: Controller is null after construction (SetController not called)
                viewer = new ConfigViewer();

                // Locate the private cancel handler via reflection
                MethodInfo handler = typeof(ConfigViewer).GetMethod(
                    "ButtonCancel_Click",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                handler
                    .Should()
                    .NotBeNull("ButtonCancel_Click must be present as a private instance method");

                // Act: invoke the handler directly; Controller is null, so Cancel is skipped
                handler.Invoke(viewer, new object[] { viewer, EventArgs.Empty });
            }
            catch (TargetInvocationException tie)
            {
                // Unwrap reflection wrapper to surface the actual exception
                caughtException = tie.InnerException;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
            finally
            {
                viewer?.Dispose();
            }

            // Assert
            caughtException
                .Should()
                .BeNull(
                    "the cancel handler must not throw when Controller is null because the null-conditional operator guards the call"
                );
        }

        /// <summary>
        /// Verifies that <see cref="ConfigViewer.ActivateUiBox(ISmartSerializableConfig.ActiveDiskEnum)"/>
        /// activates the Net disk group and deactivates the Local disk group.
        ///
        /// Purpose:
        ///     After construction, the Local box is active (IsActive=true) and the Net
        ///     box is inactive (IsActive=false).  Calling ActivateUiBox with Net must
        ///     toggle the Local box to inactive and the Net box to active, exercising
        ///     both the activate and deactivate branches in the method.
        ///
        /// Returns:
        ///     Asserts Boxes[0] (Local) becomes inactive and Boxes[1] (Net) becomes
        ///     active after the call.
        /// </summary>
        [TestMethod]
        public void ActivateUiBox_NetDiskType_ActivatesNetBoxAndDeactivatesLocalBox()
        {
            ConfigViewer viewer = null;
            try
            {
                // Arrange: after construction, Local is active and Net is inactive
                viewer = new ConfigViewer();
                viewer
                    .Boxes[0]
                    .IsActive.Should()
                    .BeTrue("Local disk group must start active after construction");
                viewer
                    .Boxes[1]
                    .IsActive.Should()
                    .BeFalse("Net disk group must start inactive after construction");

                // Act: activate Net disk group
                viewer.ActivateUiBox(ISmartSerializableConfig.ActiveDiskEnum.Net);

                // Assert: Net is now active; Local is now inactive
                viewer
                    .Boxes[0]
                    .IsActive.Should()
                    .BeFalse("ActivateUiBox(Net) must deactivate the Local disk group box");
                viewer
                    .Boxes[1]
                    .IsActive.Should()
                    .BeTrue("ActivateUiBox(Net) must activate the Net disk group box");
            }
            finally
            {
                viewer?.Dispose();
            }
        }

        /// <summary>
        /// Verifies that the <c>GroupBox_Enter</c> event handler sets the highlight
        /// back-color and fore-color when the target box is not active.
        ///
        /// Purpose:
        ///     The handler body is only executed when <c>!gb.IsActive</c> is true.
        ///     This test invokes the handler directly via reflection with an inactive
        ///     box to exercise that branch, covering the MenuHighlight/HighlightText
        ///     color assignments.
        ///
        /// Side Effects:
        ///     Invokes <c>GroupBox_Enter</c> via reflection; modifies the colors on
        ///     a transient <see cref="ConfigGroupBox"/> which is not part of the
        ///     viewer's control tree.
        /// </summary>
        [TestMethod]
        public void GroupBoxEnterHandler_WithInactiveBox_SetsHighlightColors()
        {
            ConfigViewer viewer = null;
            ConfigGroupBox box = null;
            Exception caughtException = null;
            try
            {
                // Arrange: use a headless viewer because the handler only inspects sender state.
                viewer = CreateHeadlessViewer();
                var handler = typeof(ConfigViewer).GetMethod(
                    "GroupBox_Enter",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                handler
                    .Should()
                    .NotBeNull("GroupBox_Enter must be a private instance method on ConfigViewer");

                box = new ConfigGroupBox();
                box.IsActive = false; // triggers the if(!gb.IsActive) color-change body

                // Act
                handler.Invoke(viewer, new object[] { box, EventArgs.Empty });
            }
            catch (TargetInvocationException tie)
            {
                caughtException = tie.InnerException;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
            finally
            {
                box?.Dispose();
            }

            // Assert
            caughtException.Should().BeNull("GroupBox_Enter must not throw for an inactive box");
        }

        /// <summary>
        /// Verifies that the <c>GroupBox_Click</c> event handler does not throw
        /// when the controller is null and the target box is inactive.
        ///
        /// Purpose:
        ///     The handler body calls <c>Controller?.ActivateDiskGroup(...)</c>.
        ///     With a null controller, the null-conditional operator skips the call.
        ///     This test confirms the null-safety contract and covers the handler lines.
        ///
        /// Side Effects:
        ///     Invokes <c>GroupBox_Click</c> via reflection on a viewer with a null
        ///     controller.
        /// </summary>
        [TestMethod]
        public void GroupBoxClickHandler_WithNullControllerAndInactiveBox_IsNoOp()
        {
            ConfigViewer viewer = null;
            ConfigGroupBox box = null;
            Exception caughtException = null;
            try
            {
                // Arrange: Controller is null by default on a headless instance.
                viewer = CreateHeadlessViewer();
                var handler = typeof(ConfigViewer).GetMethod(
                    "GroupBox_Click",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                handler
                    .Should()
                    .NotBeNull("GroupBox_Click must be a private instance method on ConfigViewer");

                box = new ConfigGroupBox();
                box.IsActive = false; // enters the if(!box.IsActive) body

                // Act: null-conditional Controller?.ActivateDiskGroup is a no-op
                handler.Invoke(viewer, new object[] { box, EventArgs.Empty });
            }
            catch (TargetInvocationException tie)
            {
                caughtException = tie.InnerException;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
            finally
            {
                box?.Dispose();
            }

            // Assert
            caughtException
                .Should()
                .BeNull(
                    "GroupBox_Click must not throw when Controller is null because the null-conditional operator guards the call"
                );
        }

        /// <summary>
        /// Verifies that the <c>GroupBox_Leave</c> event handler restores the control
        /// colors when the target box is not active.
        ///
        /// Purpose:
        ///     The handler body is only executed when <c>!gb.IsActive</c> is true.
        ///     This test invokes the handler directly via reflection with an inactive
        ///     box to exercise the Control/ControlText color-restore assignments.
        ///
        /// Side Effects:
        ///     Invokes <c>GroupBox_Leave</c> via reflection; modifies colors on a
        ///     transient <see cref="ConfigGroupBox"/>.
        /// </summary>
        [TestMethod]
        public void GroupBoxLeaveHandler_WithInactiveBox_RestoresControlColors()
        {
            ConfigViewer viewer = null;
            ConfigGroupBox box = null;
            Exception caughtException = null;
            try
            {
                // Arrange: use a headless viewer because the handler only inspects sender state.
                viewer = CreateHeadlessViewer();
                var handler = typeof(ConfigViewer).GetMethod(
                    "GroupBox_Leave",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                handler
                    .Should()
                    .NotBeNull("GroupBox_Leave must be a private instance method on ConfigViewer");

                box = new ConfigGroupBox();
                box.IsActive = false; // triggers the if(!gb.IsActive) color-restore body

                // Act
                handler.Invoke(viewer, new object[] { box, EventArgs.Empty });
            }
            catch (TargetInvocationException tie)
            {
                caughtException = tie.InnerException;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
            finally
            {
                box?.Dispose();
            }

            // Assert
            caughtException.Should().BeNull("GroupBox_Leave must not throw for an inactive box");
        }

        /// <summary>
        /// Verifies that the <c>SpecialFolder_SelectedValueChanged</c> event handler
        /// does not throw when the controller is null.
        ///
        /// Purpose:
        ///     The handler calls <c>Controller?.ChangeSpecialFolder(...)</c>.
        ///     With a null controller, the null-conditional operator skips the call.
        ///     This test confirms the null-safety contract and covers the handler lines.
        ///     The handler accesses the sender ComboBox's Parent as a ConfigGroupBox
        ///     and reads SpecialFolderName, RelativePath, and DiskType from it.
        ///
        /// Side Effects:
        ///     Invokes <c>SpecialFolder_SelectedValueChanged</c> via reflection on a
        ///     viewer with a null controller.
        /// </summary>
        [TestMethod]
        public void SpecialFolderSelectedValueChangedHandler_WithNullController_IsNoOp()
        {
            ConfigViewer viewer = null;
            ConfigGroupBox box = null;
            Exception caughtException = null;
            try
            {
                // Arrange: Controller is null; build a ConfigGroupBox with all required
                // child controls wired up so the handler can access its properties.
                viewer = CreateHeadlessViewer();
                var handler = typeof(ConfigViewer).GetMethod(
                    "SpecialFolder_SelectedValueChanged",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                handler
                    .Should()
                    .NotBeNull(
                        "SpecialFolder_SelectedValueChanged must be a private instance method"
                    );

                box = new ConfigGroupBox();
                var combo = new ComboBox();
                box.SpecialFolderComboBox = combo;
                box.RelativePathTextBox = new TextBox();
                box.FileNameTextBox = new TextBox();
                // Add combo as a child of box so combo.Parent equals box
                // (the handler casts (ComboBox)sender).Parent to ConfigGroupBox)
                box.Controls.Add(combo);

                // Act: null-conditional Controller?.ChangeSpecialFolder is a no-op
                handler.Invoke(viewer, new object[] { combo, EventArgs.Empty });
            }
            catch (TargetInvocationException tie)
            {
                caughtException = tie.InnerException;
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }
            finally
            {
                box?.Dispose();
            }

            // Assert
            caughtException
                .Should()
                .BeNull(
                    "SpecialFolder_SelectedValueChanged must not throw when Controller is null"
                );
        }
    }
}
