using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    /// <summary>
    /// Unit tests for <see cref="ConfigController"/>, targeting the configuration-management
    /// methods that can be exercised without the full Outlook/file-system stack.
    ///
    /// <para>
    /// Purpose:
    ///     Covers the local-disk activation branch, the Cancel guard that prevents
    ///     unsaved changes from propagating, and the not-yet-implemented file-chooser
    ///     path that must throw a <see cref="NotImplementedException"/>.
    /// </para>
    ///
    /// <para>
    /// Constraints:
    ///     <see cref="ConfigViewer"/> extends <see cref="System.Windows.Forms.Form"/>;
    ///     tests that exercise code paths touching the Viewer run on a dedicated STA
    ///     thread and surface any exception to the main thread for MSTest.
    ///     The constructor-only tests and the async-throw test run on the default
    ///     MTA thread because they do not create WinForms controls.
    /// </para>
    /// </summary>
    [TestClass]
    public class ConfigController_Tests
    {
        /// <summary>
        /// Verifies that <c>ActivateDiskGroup</c> for the local disk option delegates
        /// to <c>ConfigCopy.ActivateLocalDisk()</c> exactly once and does not throw.
        ///
        /// <para>
        /// Purpose:
        ///     Confirms the Local branch of the switch statement calls the correct
        ///     config activation method on the working copy.  The UI side of the call
        ///     (<c>Viewer.ActivateUiBox</c>) is satisfied by a real <see cref="ConfigViewer"/>
        ///     on an STA thread; because the viewer starts with Local already active,
        ///     no label-visibility mutation is triggered.
        /// </para>
        ///
        /// <para>
        /// Side Effects:
        ///     Creates and disposes a <see cref="ConfigViewer"/> on an STA thread.
        /// </para>
        /// </summary>
        [TestMethod]
        public void ActivateDiskGroup_ForLocalDisk_CallsActivateLocalDiskOnConfigCopy()
        {
            // Arrange
            var mockConfig = new Mock<ISmartSerializableConfig>();
            var mockConfigCopy = new Mock<ISmartSerializableConfig>();

            // DeepCopy is called in the ConfigController constructor to create the working copy
            mockConfig.Setup(c => c.DeepCopy()).Returns(mockConfigCopy.Object);
            var mockGlobals = new Mock<IApplicationGlobals>();
            var controller = new ConfigController(mockGlobals.Object, mockConfig.Object);

            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ConfigViewer viewer = null;
                try
                {
                    // ConfigViewer.InitializeComponent sets up labels so ActivateUiBox
                    // can set LabelActive.Visible without a NullReferenceException
                    viewer = new ConfigViewer();
                    controller.Viewer = viewer;

                    // Act
                    controller.ActivateDiskGroup(ISmartSerializableConfig.ActiveDiskEnum.Local);
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    viewer?.Dispose();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException.Should().BeNull("ActivateDiskGroup(Local) must not throw");
            mockConfigCopy.Verify(
                c => c.ActivateLocalDisk(),
                Times.Once(),
                "the Local branch must delegate to ConfigCopy.ActivateLocalDisk"
            );
        }

        /// <summary>
        /// Verifies that <c>Cancel</c> does not apply the working copy back to the
        /// original config — confirming that unsaved edits are discarded.
        ///
        /// <para>
        /// Purpose:
        ///     <c>Cancel</c> calls <c>Viewer.Close()</c> but never calls
        ///     <c>Config.CopyChanged</c>.  This test confirms that the original
        ///     <c>Config</c> mock receives no <c>CopyChanged</c> invocation, meaning
        ///     the prior config state is preserved.
        /// </para>
        ///
        /// <para>
        /// Side Effects:
        ///     Creates and disposes a <see cref="ConfigViewer"/> on an STA thread.
        /// </para>
        /// </summary>
        [TestMethod]
        public void Cancel_DoesNotApplyWorkingCopyToOriginalConfig()
        {
            // Arrange
            var mockConfig = new Mock<ISmartSerializableConfig>();
            var mockConfigCopy = new Mock<ISmartSerializableConfig>();
            mockConfig.Setup(c => c.DeepCopy()).Returns(mockConfigCopy.Object);
            var mockGlobals = new Mock<IApplicationGlobals>();
            var controller = new ConfigController(mockGlobals.Object, mockConfig.Object);

            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ConfigViewer viewer = null;
                try
                {
                    viewer = new ConfigViewer();
                    controller.Viewer = viewer;

                    // Act
                    controller.Cancel();
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    viewer?.Dispose();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert — Cancel must not throw
            caughtException.Should().BeNull("Cancel must not throw");

            // The original config is unchanged: CopyChanged is only called in SaveAsync,
            // never in Cancel
            mockConfig.Verify(
                c =>
                    c.CopyChanged(
                        It.IsAny<ISmartSerializableConfig>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>()
                    ),
                Times.Never(),
                "Cancel must not propagate the working copy back to the original config"
            );
        }

        /// <summary>
        /// Verifies that <c>OpenFileChooserAsync</c> throws <see cref="NotImplementedException"/>
        /// because the file-chooser feature has not been implemented yet.
        ///
        /// <para>
        /// Purpose:
        ///     Guards against accidental removal of the not-implemented guard; any
        ///     future implementation must update this test accordingly.
        /// </para>
        ///
        /// <para>
        /// Returns:
        ///     Asserts a <see cref="NotImplementedException"/> is thrown when the
        ///     method is awaited.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task OpenFileChooserAsync_WhenCalled_ThrowsNotImplementedException()
        {
            // Arrange — no Viewer required; the method throws before any UI access
            var mockConfig = new Mock<ISmartSerializableConfig>();
            var mockConfigCopy = new Mock<ISmartSerializableConfig>();
            mockConfig.Setup(c => c.DeepCopy()).Returns(mockConfigCopy.Object);
            var mockGlobals = new Mock<IApplicationGlobals>();
            var controller = new ConfigController(mockGlobals.Object, mockConfig.Object);

            // Act & Assert
            Func<Task> act = async () => await controller.OpenFileChooserAsync();
            await act.Should()
                .ThrowAsync<NotImplementedException>(
                    "OpenFileChooserAsync is explicitly marked NotImplementedException"
                );
        }
    }
}
