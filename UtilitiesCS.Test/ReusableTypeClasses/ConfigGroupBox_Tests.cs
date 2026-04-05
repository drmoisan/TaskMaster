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

        /// <summary>
        /// Verifies that the <see cref="ConfigGroupBox.FileName"/> and
        /// <see cref="ConfigGroupBox.RelativePath"/> setter properties propagate
        /// the assigned string value to the underlying child TextBox controls.
        ///
        /// Purpose:
        ///     The <c>FileName</c> setter writes to <c>FileNameTextBox.Text</c> and
        ///     the <c>RelativePath</c> setter writes to <c>RelativePathTextBox.Text</c>.
        ///     This test assigns through the wrapper setter and confirms the child
        ///     control Text property reflects the new value.
        ///
        /// Returns:
        ///     Asserts <c>FileNameTextBox.Text</c> equals the string set via the
        ///     <c>FileName</c> setter, and <c>RelativePathTextBox.Text</c> equals the
        ///     string set via the <c>RelativePath</c> setter.
        /// </summary>
        [TestMethod]
        public void WrapperSetters_UpdateChildControlText()
        {
            string capturedFileNameText = null;
            string capturedRelativePathText = null;
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

                    // Act: assign via wrapper setters
                    box.FileName = "settings.json";
                    box.RelativePath = @"SubDir\Config";

                    // Capture the underlying control Text for assertion
                    capturedFileNameText = fileNameBox.Text;
                    capturedRelativePathText = relativePathBox.Text;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    box?.Dispose();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException
                .Should()
                .BeNull("wrapper setters must not throw when child controls are assigned");
            capturedFileNameText
                .Should()
                .Be(
                    "settings.json",
                    "FileName setter must write the value to FileNameTextBox.Text"
                );
            capturedRelativePathText
                .Should()
                .Be(
                    @"SubDir\Config",
                    "RelativePath setter must write the value to RelativePathTextBox.Text"
                );
        }

        /// <summary>
        /// Verifies that the <see cref="ConfigGroupBox.SpecialFolderName"/> setter
        /// selects the item in the ComboBox when it exists in the items collection,
        /// and that the getter returns the selected item as a string.
        ///
        /// Purpose:
        ///     Covers the true-branch of the conditional setter:
        ///     <c>SpecialFolderComboBox.Items.Contains(value) ? value : null</c>.
        ///     Also exercises the getter which casts <c>SelectedItem as string</c>.
        ///
        /// Returns:
        ///     Asserts the getter returns the string that was set, confirming the
        ///     setter selected the existing item.
        /// </summary>
        [TestMethod]
        public void SpecialFolderName_WhenItemExists_SelectsItemAndGetterReturnsIt()
        {
            string capturedName = null;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ConfigGroupBox box = null;
                try
                {
                    // Arrange: create box with ComboBox that contains the target item
                    box = new ConfigGroupBox();
                    var combo = new ComboBox();
                    combo.Items.Add("Desktop");
                    combo.Items.Add("Documents");
                    box.SpecialFolderComboBox = combo;

                    // Act: set to an item that IS in the list; getter returns the value
                    box.SpecialFolderName = "Desktop";
                    capturedName = box.SpecialFolderName;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    box?.Dispose();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException
                .Should()
                .BeNull("SpecialFolderName setter should not throw when item exists");
            capturedName
                .Should()
                .Be(
                    "Desktop",
                    "setter must select the item when it exists, and getter must return it"
                );
        }

        /// <summary>
        /// Verifies that the <see cref="ConfigGroupBox.SpecialFolderName"/> setter
        /// assigns <c>null</c> to <c>SpecialFolderComboBox.SelectedItem</c> when the
        /// supplied value is not present in the items collection.
        ///
        /// Purpose:
        ///     Covers the false-branch of the conditional setter:
        ///     <c>SpecialFolderComboBox.Items.Contains(value) ? value : null</c>.
        ///
        /// Returns:
        ///     Asserts the getter returns <c>null</c> because no item was selected.
        /// </summary>
        [TestMethod]
        public void SpecialFolderName_WhenItemNotInList_SetsSelectedItemToNull()
        {
            string capturedName = "unexpected";
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ConfigGroupBox box = null;
                try
                {
                    // Arrange: create box with ComboBox that does NOT contain the target item
                    box = new ConfigGroupBox();
                    var combo = new ComboBox();
                    combo.Items.Add("Desktop");
                    box.SpecialFolderComboBox = combo;

                    // Act: set to a value absent from the list; null path selected
                    box.SpecialFolderName = "Downloads";
                    capturedName = box.SpecialFolderName;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    box?.Dispose();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            caughtException
                .Should()
                .BeNull("SpecialFolderName setter should not throw when item is absent");
            capturedName
                .Should()
                .BeNull(
                    "setter must assign null to SelectedItem when value is not in the items list"
                );
        }
    }
}
