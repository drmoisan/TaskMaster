using System;
using System.Collections.Concurrent;
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

        /// <summary>
        /// Verifies that the controller can initialize its viewer, remap both disk folders,
        /// switch to the net disk, and persist the working copy back to the original config.
        /// </summary>
        [TestMethod]
        public void Show_Init_ChangeFolders_ActivateNetAndSave_PersistsUpdatedConfig()
        {
            var specialFolders = new ConcurrentDictionary<string, string>();
            specialFolders["Documents"] = @"C:\Special\Docs";
            specialFolders["Archive"] = @"D:\ArchiveRoot";

            var mockFileSystem = new Mock<IFileSystemFolderPaths>();
            mockFileSystem.SetupGet(x => x.SpecialFolders).Returns(specialFolders);

            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.SetupGet(x => x.FS).Returns(mockFileSystem.Object);

            var config = new NewSmartSerializableConfig
            {
                LocalDisk = new FilePathHelper(
                    "local.json",
                    System.IO.Path.Combine(specialFolders["Documents"], "ExistingLocal")
                ),
                NetDisk = new FilePathHelper("net.json", @"Z:\Unmapped\InitialNet"),
            };
            config.ActivateLocalDisk();

            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ConfigController controller = null;
                try
                {
                    controller = new ConfigController(mockGlobals.Object, config).Init();

                    controller.SpecialFolderList[0].Should().Be("None");
                    controller.SpecialFolderList.Should().Contain("Documents");
                    controller.SpecialFolderList.Should().Contain("Archive");
                    controller.Viewer.Should().NotBeNull();
                    controller.Viewer.ComboSpecialFolderLocal.SelectedItem.Should().Be("Documents");
                    controller.Viewer.RelativePathLocal.Text.Should().Contain("ExistingLocal");
                    controller.Viewer.FileNameLocal.Text.Should().Be("local.json");
                    controller.Viewer.RelativePathNet.Text.Should().Be(@"Z:\Unmapped\InitialNet");
                    controller.Viewer.FileNameNet.Text.Should().Be("net.json");

                    controller.ChangeSpecialFolder(
                        "Documents",
                        "UpdatedLocal",
                        ISmartSerializableConfig.ActiveDiskEnum.Local
                    );
                    controller
                        .ConfigCopy.LocalDisk.FolderPath.Should()
                        .Be(System.IO.Path.Combine(specialFolders["Documents"], "UpdatedLocal"));
                    controller
                        .ConfigCopy.Disk.FolderPath.Should()
                        .Be(System.IO.Path.Combine(specialFolders["Documents"], "UpdatedLocal"));

                    controller.ActivateDiskGroup(ISmartSerializableConfig.ActiveDiskEnum.Net);
                    controller
                        .ConfigCopy.ActiveDisk.Should()
                        .Be(ISmartSerializableConfig.ActiveDiskEnum.Net);

                    controller.ChangeSpecialFolder(
                        "Archive",
                        "UpdatedNet",
                        ISmartSerializableConfig.ActiveDiskEnum.Net
                    );
                    controller
                        .ConfigCopy.NetDisk.FolderPath.Should()
                        .Be(System.IO.Path.Combine(specialFolders["Archive"], "UpdatedNet"));
                    controller
                        .ConfigCopy.Disk.FolderPath.Should()
                        .Be(System.IO.Path.Combine(specialFolders["Archive"], "UpdatedNet"));

                    var saveTask = controller.SaveAsync();
                    while (!saveTask.IsCompleted)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(10);
                    }
                    saveTask.GetAwaiter().GetResult();

                    config.ActiveDisk.Should().Be(ISmartSerializableConfig.ActiveDiskEnum.Net);
                    config
                        .LocalDisk.FolderPath.Should()
                        .Be(System.IO.Path.Combine(specialFolders["Documents"], "UpdatedLocal"));
                    config
                        .NetDisk.FolderPath.Should()
                        .Be(System.IO.Path.Combine(specialFolders["Archive"], "UpdatedNet"));
                    config
                        .Disk.FolderPath.Should()
                        .Be(System.IO.Path.Combine(specialFolders["Archive"], "UpdatedNet"));
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    controller?.Viewer?.Dispose();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException
                .Should()
                .BeNull(
                    "the config workflow should initialize, update both folders, and save cleanly"
                );
        }
    }
}
