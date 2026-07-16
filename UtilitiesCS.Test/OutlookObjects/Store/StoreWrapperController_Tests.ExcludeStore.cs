using System.Collections.Generic;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// ExcludeStore checkbox behavior for <see cref="StoreWrapperController"/> (issue #328): binding
    /// to <c>StoresWrapper.ExcludedStoreIds</c> membership on selection, add/remove mutation on save,
    /// idempotency, and the fail-safe when the current store's StoreID is unreadable. Uses a mocked
    /// <see cref="IStoreWrapperViewer"/> with a real <see cref="CheckBox"/> and a real
    /// <see cref="StoresWrapper"/> model (no live COM, no temporary files). Partial of
    /// <see cref="StoreWrapperController_Tests"/> so it reuses the base mock harness.
    /// </summary>
    public partial class StoreWrapperController_Tests
    {
        private static (
            StoreWrapperController controller,
            Mock<IStoreWrapperViewer> viewer,
            CheckBox checkbox
        ) CreateControllerWithExcludeCheckbox()
        {
            var (controller, viewer) = CreateControllerWithViewer();
            var checkbox = new CheckBox();
            viewer.Setup(v => v.ExcludeStore).Returns(checkbox);
            controller.FsConverter = _ => (string.Empty, string.Empty);
            return (controller, viewer, checkbox);
        }

        [TestMethod]
        public void PopulateWithCurrent_WhenStoreIdInExcludedSet_ChecksAndEnablesCheckbox()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            controller.Model = new StoresWrapper
            {
                // Case-insensitive membership: stored lower-case, current store upper-case.
                ExcludedStoreIds = new List<string> { "sid-1" },
            };
            controller.Current = new StoreWrapper(null) { StoreId = "SID-1" };

            controller.PopulateWithCurrent();

            checkbox.Enabled.Should().BeTrue();
            checkbox.Checked.Should().BeTrue();
        }

        [TestMethod]
        public void PopulateWithCurrent_WhenStoreIdNotExcluded_UnchecksAndEnablesCheckbox()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            controller.Model = new StoresWrapper { ExcludedStoreIds = new List<string>() };
            controller.Current = new StoreWrapper(null) { StoreId = "SID-1" };

            controller.PopulateWithCurrent();

            checkbox.Enabled.Should().BeTrue();
            checkbox.Checked.Should().BeFalse();
        }

        [TestMethod]
        public void PopulateWithCurrent_WhenStoreIdUnreadable_DisablesAndUnchecksCheckbox()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            checkbox.Checked = true;
            controller.Model = new StoresWrapper { ExcludedStoreIds = new List<string>() };
            controller.Current = new StoreWrapper(null) { StoreId = null };

            controller.PopulateWithCurrent();

            checkbox.Enabled.Should().BeFalse();
            checkbox.Checked.Should().BeFalse();
        }

        [TestMethod]
        public void SaveChanges_WhenCheckboxCheckedAndStoreIdAbsent_AddsStoreId()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            checkbox.Checked = true;
            var model = new StoresWrapper { ExcludedStoreIds = new List<string>() };
            controller.Model = model;
            controller.Current = new StoreWrapper(null) { StoreId = "SID-1" };

            controller.SaveChanges();

            model.ExcludedStoreIds.Should().ContainSingle().Which.Should().Be("SID-1");
        }

        [TestMethod]
        public void SaveChanges_WhenCheckboxUncheckedAndStoreIdPresent_RemovesStoreId()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            checkbox.Checked = false;
            var model = new StoresWrapper { ExcludedStoreIds = new List<string> { "SID-1" } };
            controller.Model = model;
            controller.Current = new StoreWrapper(null) { StoreId = "SID-1" };

            controller.SaveChanges();

            model.ExcludedStoreIds.Should().BeEmpty();
        }

        [TestMethod]
        public void SaveChanges_WhenCheckboxCheckedAndStoreIdAlreadyPresent_DoesNotDuplicate()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            checkbox.Checked = true;
            var model = new StoresWrapper { ExcludedStoreIds = new List<string> { "SID-1" } };
            controller.Model = model;
            controller.Current = new StoreWrapper(null) { StoreId = "SID-1" };

            controller.SaveChanges();

            model.ExcludedStoreIds.Should().ContainSingle().Which.Should().Be("SID-1");
        }

        [TestMethod]
        public void SaveChanges_WhenStoreIdUnreadable_DoesNotMutateExcludedStoreIds()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            checkbox.Checked = true;
            var model = new StoresWrapper { ExcludedStoreIds = new List<string>() };
            controller.Model = model;
            controller.Current = new StoreWrapper(null) { StoreId = null };

            controller.SaveChanges();

            model.ExcludedStoreIds.Should().BeEmpty();
        }

        [TestMethod]
        public void AnyChanges_WhenCheckboxMatchesMembershipAfterPopulate_ReturnsFalse()
        {
            var (controller, _, _) = CreateControllerWithExcludeCheckbox();
            controller.Model = new StoresWrapper
            {
                ExcludedStoreIds = new List<string> { "SID-1" },
            };
            controller.Current = new StoreWrapper(null) { StoreId = "SID-1" };

            // PopulateWithCurrent mirrors the folder fields and binds the checkbox to the
            // membership, so no dimension differs afterward.
            controller.PopulateWithCurrent();

            controller.AnyChanges().Should().BeFalse();
        }

        [TestMethod]
        public void AnyChanges_WhenCheckboxDiffersFromMembership_ReturnsTrue()
        {
            var (controller, _, checkbox) = CreateControllerWithExcludeCheckbox();
            controller.Model = new StoresWrapper { ExcludedStoreIds = new List<string>() };
            controller.Current = new StoreWrapper(null) { StoreId = "SID-1" };
            controller.PopulateWithCurrent();

            // User toggles the checkbox on; membership still empty, so a change is pending.
            checkbox.Checked = true;

            controller.AnyChanges().Should().BeTrue();
        }
    }
}
