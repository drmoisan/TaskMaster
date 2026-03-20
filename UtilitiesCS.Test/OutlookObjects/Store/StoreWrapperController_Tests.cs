using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
    public class StoreWrapperController_Tests
    {
        #region RunFolderSelectionDialog

        [TestMethod]
        public void RunFolderSelectionDialog_NullSelector_ReturnsFalse()
        {
            StoreWrapperController.RunFolderSelectionDialog(null).Should().BeFalse();
        }

        [TestMethod]
        public void RunFolderSelectionDialog_SelectorReturnsTrue_ReturnsTrue()
        {
            StoreWrapperController.RunFolderSelectionDialog(() => true).Should().BeTrue();
        }

        [TestMethod]
        public void RunFolderSelectionDialog_SelectorReturnsFalse_ReturnsFalse()
        {
            StoreWrapperController.RunFolderSelectionDialog(() => false).Should().BeFalse();
        }

        #endregion

        #region PairwiseEquals

        [TestMethod]
        public void PairwiseEquals_BothNull_ReturnsTrue()
        {
            var controller = CreateController();
            controller.PairwiseEquals<string>(null, null).Should().BeTrue();
        }

        [TestMethod]
        public void PairwiseEquals_FirstNull_ReturnsFalse()
        {
            var controller = CreateController();
            controller.PairwiseEquals<string>(null, "b").Should().BeFalse();
        }

        [TestMethod]
        public void PairwiseEquals_SecondNull_ReturnsFalse()
        {
            var controller = CreateController();
            controller.PairwiseEquals<string>("a", null).Should().BeFalse();
        }

        [TestMethod]
        public void PairwiseEquals_Equal_ReturnsTrue()
        {
            var controller = CreateController();
            controller.PairwiseEquals("abc", "abc").Should().BeTrue();
        }

        [TestMethod]
        public void PairwiseEquals_NotEqual_ReturnsFalse()
        {
            var controller = CreateController();
            controller.PairwiseEquals("abc", "xyz").Should().BeFalse();
        }

        #endregion

        #region Constructor

        [TestMethod]
        public void Constructor_SetsGlobals()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var controller = new StoreWrapperController(mockGlobals.Object);
            controller.Globals.Should().BeSameAs(mockGlobals.Object);
        }

        #endregion

        #region AnyChanges

        [TestMethod]
        public void AnyChanges_AllNull_ReturnsFalse()
        {
            var controller = CreateController();
            // Current is null, all folder references are null
            controller.AnyChanges().Should().BeFalse();
        }

        #endregion

        #region ButtonCancel_Click

        [TestMethod]
        public void ButtonCancel_Click_ClosesViewer()
        {
            var mockViewer = new Mock<IStoreWrapperViewer>();
            var controller = CreateController();
            controller.Viewer = mockViewer.Object;

            controller.ButtonCancel_Click();

            mockViewer.Verify(v => v.Close(), Times.Once);
        }

        #endregion

        #region GetRelativeFsPath

        [TestMethod]
        public void GetRelativeFsPath_NullArchiveFsRoot_ReturnsPlaceholder()
        {
            var controller = CreateController();
            var mockStore = new Mock<StoreWrapper>();
            controller.Current = mockStore.Object;

            var result = controller.GetRelativeFsPath();
            result.Should().Be("Please select an archive");
        }

        #endregion

        #region SaveChanges

        [TestMethod]
        public void SaveChanges_SetsCurrentProperties()
        {
            var controller = CreateController();
            var mockModel = new Mock<StoresWrapper>();
            var mockStore = new Mock<StoreWrapper>();
            controller.Model = mockModel.Object;
            controller.Current = mockStore.Object;

            controller.SaveChanges();

            mockModel.Verify(m => m.Serialize(), Times.Once);
        }

        #endregion

        #region Helpers

        private static StoreWrapperController CreateController()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            return new StoreWrapperController(mockGlobals.Object);
        }

        #endregion
    }
}
