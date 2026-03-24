using System;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    /// <summary>
    /// Unit tests for <see cref="ConfigGroupBox"/>.
    ///
    /// Purpose:
    ///     Covers the wrapper getter/setter properties that delegate to child
    ///     controls (FileNameTextBox, RelativePathTextBox) and the DiskType
    ///     active-disk selection property.
    ///
    /// Constraints:
    ///     ConfigGroupBox extends GroupBox (WinForms); all tests run on a dedicated
    ///     STA thread and surface any exception to the main thread for MSTest.
    /// </summary>
    [TestClass]
    public class ConfigGroupBox_Tests
    {
        /// <summary>
        /// Verifies that the <see cref="ConfigGroupBox.FileName"/> and
        /// <see cref="ConfigGroupBox.RelativePath"/> wrapper getter properties stay
        /// synchronized with the values set directly on the underlying child controls.
        ///
        /// Purpose:
        ///     The <c>FileName</c> getter delegates to <c>FileNameTextBox.Text</c>,
        ///     and the <c>RelativePath</c> getter delegates to
        ///     <c>RelativePathTextBox.Text</c>.  This test sets Text on the child
        ///     control directly and confirms the wrapper getter reflects the new value.
        ///
        /// Returns:
        ///     Asserts <c>FileName</c> equals the text set on <c>FileNameTextBox</c>,
        ///     and <c>RelativePath</c> equals the text set on <c>RelativePathTextBox</c>.
        /// </summary>
        [TestMethod]
        public void WrapperGetters_ReflectChildControlValues()
        {
            string capturedFileName = null;
            string capturedRelativePath = null;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ConfigGroupBox box = null;
                try
                {
                    // Arrange: create box and wire up child controls
                    box = new ConfigGroupBox();
                    var fileNameBox = new TextBox();
                    var relativePathBox = new TextBox();
                    box.FileNameTextBox = fileNameBox;
                    box.RelativePathTextBox = relativePathBox;

                    // Act: set child control values directly
                    fileNameBox.Text = "config.json";
                    relativePathBox.Text = @"AppData\Local\App";

                    // Capture wrapper-getter values for assertion on main thread
                    capturedFileName = box.FileName;
                    capturedRelativePath = box.RelativePath;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    if (box != null)
                    {
                        box.Dispose();
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException
                .Should()
                .BeNull("wrapper getters must not throw when child controls are assigned");
            capturedFileName
                .Should()
                .Be("config.json", "FileName getter must return the TextBox's current text");
            capturedRelativePath
                .Should()
                .Be(
                    @"AppData\Local\App",
                    "RelativePath getter must return the TextBox's current text"
                );
        }

        /// <summary>
        /// Verifies that the <see cref="ConfigGroupBox.DiskType"/> property correctly
        /// stores and returns the assigned <see cref="ISmartSerializableConfig.ActiveDiskEnum"/>
        /// value, covering the Local and Net disk-type mappings.
        ///
        /// Purpose:
        ///     <c>DiskType</c> is a stored property used by the config layer to
        ///     distinguish which disk (local vs. network) a config group box controls.
        ///     This test confirms the property round-trips each meaningful enum value
        ///     without error.
        ///
        /// Returns:
        ///     Asserts each assigned <c>ActiveDiskEnum</c> value is returned unchanged
        ///     from the getter.
        /// </summary>
        [TestMethod]
        public void DiskType_SetToLocalAndNet_RoundTripsCorrectly()
        {
            ISmartSerializableConfig.ActiveDiskEnum capturedLocal = ISmartSerializableConfig
                .ActiveDiskEnum
                .Neither;
            ISmartSerializableConfig.ActiveDiskEnum capturedNet = ISmartSerializableConfig
                .ActiveDiskEnum
                .Neither;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ConfigGroupBox box = null;
                try
                {
                    // Arrange
                    box = new ConfigGroupBox();

                    // Act: set to Local and read back
                    box.DiskType = ISmartSerializableConfig.ActiveDiskEnum.Local;
                    capturedLocal = box.DiskType;

                    // Act: set to Net and read back
                    box.DiskType = ISmartSerializableConfig.ActiveDiskEnum.Net;
                    capturedNet = box.DiskType;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    if (box != null)
                    {
                        box.Dispose();
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException.Should().BeNull("DiskType assignment and retrieval should not throw");
            capturedLocal
                .Should()
                .Be(
                    ISmartSerializableConfig.ActiveDiskEnum.Local,
                    "DiskType must round-trip the Local value"
                );
            capturedNet
                .Should()
                .Be(
                    ISmartSerializableConfig.ActiveDiskEnum.Net,
                    "DiskType must round-trip the Net value"
                );
        }
    }
}
