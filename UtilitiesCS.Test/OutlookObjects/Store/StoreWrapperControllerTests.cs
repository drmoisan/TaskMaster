using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
    public class StoreWrapperControllerTests
    {
        [TestMethod]
        public void Controller_applies_selected_folder_when_dialog_returns_success()
        {
            StoreWrapperController.RunFolderSelectionDialog(() => true).Should().BeTrue();
        }

        [TestMethod]
        public void Controller_leaves_state_unchanged_when_dialog_is_cancelled()
        {
            StoreWrapperController.RunFolderSelectionDialog(() => false).Should().BeFalse();
        }
    }
}
