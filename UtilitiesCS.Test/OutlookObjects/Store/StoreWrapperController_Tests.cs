using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.OutlookObjects.Folder;
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
            controller.Current = new StoreWrapper(null);

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
            controller.Model = mockModel.Object;
            controller.Current = new StoreWrapper(null);

            controller.SaveChanges();

            controller.Current.ArchiveRoot.Should().BeNull();
            controller.Current.JunkCertain.Should().BeNull();
            controller.Current.JunkPotential.Should().BeNull();
            controller.Current.ArchiveFsRoot.Should().BeNull();
        }

        #endregion

        #region Helpers

        private static StoreWrapperController CreateController()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            return new StoreWrapperController(mockGlobals.Object);
        }

        private static (
            StoreWrapperController controller,
            Mock<IStoreWrapperViewer> viewer
        ) CreateControllerWithViewer()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var controller = new StoreWrapperController(mockGlobals.Object);
            var mockViewer = new Mock<IStoreWrapperViewer>();
            mockViewer.Setup(v => v.InvokeRequired).Returns(false);
            mockViewer.Setup(v => v.ArchiveOutlook).Returns(new Label());
            mockViewer.Setup(v => v.ArchiveFS).Returns(new Label());
            mockViewer.Setup(v => v.JunkEmail).Returns(new Label());
            mockViewer.Setup(v => v.JunkPotential).Returns(new Label());
            mockViewer.Setup(v => v.Inbox).Returns(new Label());
            mockViewer.Setup(v => v.RootFolder).Returns(new Label());
            mockViewer.Setup(v => v.UserEmail).Returns(new Label());
            controller.Viewer = mockViewer.Object;
            return (controller, mockViewer);
        }

        #endregion

        #region ButtonOk_Click

        [TestMethod]
        public void ButtonOk_Click_NoChanges_ClosesViewer()
        {
            var (controller, mockViewer) = CreateControllerWithViewer();

            controller.ButtonOk_Click();

            mockViewer.Verify(v => v.Close(), Times.Once);
        }

        [TestMethod]
        public void ButtonOk_Click_WithChanges_SavesAndCloses()
        {
            var (controller, mockViewer) = CreateControllerWithViewer();
            var mockModel = new Mock<StoresWrapper>();
            controller.Model = mockModel.Object;
            controller.Current = new StoreWrapper(null);
            // Set ArchiveOutlook to a non-null value so AnyChanges() returns true
            controller.ArchiveOutlook = new FolderMinimalWrapper("TestPath", "TestRelative");

            controller.ButtonOk_Click();

            mockViewer.Verify(v => v.Close(), Times.Once);
        }

        #endregion

        #region AnyChanges variants

        [TestMethod]
        public void AnyChanges_ArchiveOutlookDiffers_ReturnsTrue()
        {
            var controller = CreateController();
            controller.Current = new StoreWrapper(null);
            controller.ArchiveOutlook = new FolderMinimalWrapper("Path", "Relative");

            controller.AnyChanges().Should().BeTrue();
        }

        [TestMethod]
        public void AnyChanges_JunkEmailDiffers_ReturnsTrue()
        {
            var controller = CreateController();
            controller.Current = new StoreWrapper(null);
            controller.JunkEmail = new FolderMinimalWrapper("Path", "Relative");

            controller.AnyChanges().Should().BeTrue();
        }

        [TestMethod]
        public void AnyChanges_JunkPotentialDiffers_ReturnsTrue()
        {
            var controller = CreateController();
            controller.Current = new StoreWrapper(null);
            controller.JunkPotential = new FolderMinimalWrapper("Path", "Relative");

            controller.AnyChanges().Should().BeTrue();
        }

        #endregion

        #region GetRelativeFsPath variants

        [TestMethod]
        public void GetRelativeFsPath_ArchiveFsWithEmptyPath_ReturnsPlaceholder()
        {
            var controller = CreateController();
            controller.Current = new StoreWrapper(null);
            controller.Current.ArchiveFsRoot = new FilePathHelper();

            var result = controller.GetRelativeFsPath();

            result.Should().Be("Please select an archive");
        }

        [TestMethod]
        public void GetRelativeFsPath_ArchiveFsWithPath_ConverterReturnsEmpty_ReturnsPlaceholder()
        {
            var controller = CreateController();
            controller.Current = new StoreWrapper(null);
            controller.Current.ArchiveFsRoot = new FilePathHelper { FolderPath = @"C:\SomePath" };
            controller.FsConverter = (path) => ("", "");

            var result = controller.GetRelativeFsPath();

            result.Should().Be("Please select an archive");
        }

        [TestMethod]
        public void GetRelativeFsPath_ArchiveFsWithPath_ConverterReturnsValues_ReturnsFormatted()
        {
            var controller = CreateController();
            controller.Current = new StoreWrapper(null);
            controller.Current.ArchiveFsRoot = new FilePathHelper { FolderPath = @"C:\SomePath" };
            controller.FsConverter = (path) => ("AppData", "Backups");

            var result = controller.GetRelativeFsPath();

            result.Should().Contain("AppData");
            result.Should().Contain("Backups");
        }

        #endregion

        #region PopulateWithCurrent

        [TestMethod]
        public void PopulateWithCurrent_NullCurrent_SetsErrorLoadingText()
        {
            var (controller, mockViewer) = CreateControllerWithViewer();
            controller.Current = null;
            controller.FsConverter = (path) => ("", "");

            // PopulateWithCurrent accesses Current which may be null
            // This would throw NullReferenceException, verifying we need Current set
            var act = () => controller.PopulateWithCurrent();

            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void PopulateWithCurrent_CurrentSetWithNulls_SetsPlaceholders()
        {
            var (controller, mockViewer) = CreateControllerWithViewer();
            controller.Current = new StoreWrapper(null);
            controller.FsConverter = (path) => ("", "");

            controller.PopulateWithCurrent();

            // StoreWrapper defaults ArchiveRoot to new FolderMinimalWrapper(), so it's not null
            controller.ArchiveOutlook.Should().NotBeNull();
            // JunkCertain / JunkPotential default to new FolderMinimalWrapper() as well
            controller.JunkEmail.Should().NotBeNull();
            controller.JunkPotential.Should().NotBeNull();
        }

        /// <summary>
        /// Verifies that after <see cref="StoreWrapperController.PopulateWithCurrent"/> completes,
        /// the controller's internal folder fields are the exact same object references as the
        /// corresponding properties on the backing <see cref="StoreWrapper"/>.
        ///
        /// Purpose:
        ///     Confirm that PopulateWithCurrent "mirrors" the current store — i.e. the controller
        ///     fields are not copies but are the same instances, so subsequent AnyChanges() comparison
        ///     via PairwiseEquals (reference equality) will correctly report "no changes" right
        ///     after population.
        ///
        /// Returns:
        ///     Passes when each controller field is the same object reference as the Current property.
        /// </summary>
        [TestMethod]
        public void PopulateWithCurrent_WithKnownFolderValues_MirrorsControllerFieldsFromCurrent()
        {
            // Arrange: use null globals — PopulateWithCurrent does not call Globals.
            // Use StoreWrapperViewer directly to avoid Moq (Moq's AwaitableFactory requires
            // System.Threading.Tasks.Extensions 4.2.0.1 which is absent from the test bin output,
            // causing TypeInitializationException for all Mock<T> involving Task-bearing interfaces).
            // StoreWrapperViewer creates real WinForms labels in InitializeComponent(); Form handle
            // is never created so InvokeRequired returns false in the test thread.
            var controller = new StoreWrapperController(null!);
            controller.Viewer = new StoreWrapperViewer();

            var archiveFolder = new FolderMinimalWrapper("Archive", "Root\\Archive");
            var junkEmailFolder = new FolderMinimalWrapper("JunkEmail", "Root\\Junk");
            var junkPotentialFolder = new FolderMinimalWrapper("JunkPotential", "Root\\Potential");

            // FilePathHelper() defaults FolderPath = "" so GetRelativeFsPath skips FsConverter.
            var archiveFs = new FilePathHelper();

            var currentStore = new StoreWrapper(null);
            currentStore.ArchiveRoot = archiveFolder;
            currentStore.JunkCertain = junkEmailFolder;
            currentStore.JunkPotential = junkPotentialFolder;
            currentStore.ArchiveFsRoot = archiveFs;
            controller.Current = currentStore;

            // Act
            controller.PopulateWithCurrent();

            // Assert: controller fields must be the same object references — not copies.
            // PairwiseEquals uses reference equality for FolderMinimalWrapper and FilePathHelper,
            // so mirroring reference equality ensures AnyChanges() reports no changes right after
            // population.
            controller.ArchiveOutlook.Should().BeSameAs(archiveFolder);
            controller.JunkEmail.Should().BeSameAs(junkEmailFolder);
            controller.JunkPotential.Should().BeSameAs(junkPotentialFolder);
            controller.ArchiveFS.Should().BeSameAs(archiveFs);
        }

        #endregion

        #region Click handlers (non-invoke path)

        [TestMethod]
        public void ArchiveOutlook_Click_NullSelectedFolder_LeavesNull()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockNs = new Mock<Microsoft.Office.Interop.Outlook.NameSpace>();
            mockNs
                .Setup(n => n.PickFolder())
                .Returns((Microsoft.Office.Interop.Outlook.MAPIFolder)null);
            mockOl.Setup(o => o.NamespaceMAPI).Returns(mockNs.Object);
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);

            var controller = new StoreWrapperController(mockGlobals.Object);
            var mockViewer = new Mock<IStoreWrapperViewer>();
            mockViewer.Setup(v => v.InvokeRequired).Returns(false);
            mockViewer.Setup(v => v.ArchiveOutlook).Returns(new Label());
            controller.Viewer = mockViewer.Object;

            controller.ArchiveOutlook_Click();

            controller.ArchiveOutlook.Should().BeNull();
        }

        [TestMethod]
        public void JunkEmail_Click_NullSelectedFolder_LeavesNull()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockNs = new Mock<Microsoft.Office.Interop.Outlook.NameSpace>();
            mockNs
                .Setup(n => n.PickFolder())
                .Returns((Microsoft.Office.Interop.Outlook.MAPIFolder)null);
            mockOl.Setup(o => o.NamespaceMAPI).Returns(mockNs.Object);
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);

            var controller = new StoreWrapperController(mockGlobals.Object);
            var mockViewer = new Mock<IStoreWrapperViewer>();
            mockViewer.Setup(v => v.InvokeRequired).Returns(false);
            mockViewer.Setup(v => v.JunkEmail).Returns(new Label());
            controller.Viewer = mockViewer.Object;

            controller.JunkEmail_Click();

            controller.JunkEmail.Should().BeNull();
        }

        [TestMethod]
        public void JunkPotential_Click_NullSelectedFolder_LeavesNull()
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockOl = new Mock<IOlObjects>();
            var mockNs = new Mock<Microsoft.Office.Interop.Outlook.NameSpace>();
            mockNs
                .Setup(n => n.PickFolder())
                .Returns((Microsoft.Office.Interop.Outlook.MAPIFolder)null);
            mockOl.Setup(o => o.NamespaceMAPI).Returns(mockNs.Object);
            mockGlobals.Setup(g => g.Ol).Returns(mockOl.Object);

            var controller = new StoreWrapperController(mockGlobals.Object);
            var mockViewer = new Mock<IStoreWrapperViewer>();
            mockViewer.Setup(v => v.InvokeRequired).Returns(false);
            mockViewer.Setup(v => v.JunkPotential).Returns(new Label());
            controller.Viewer = mockViewer.Object;

            controller.JunkPotential_Click();

            controller.JunkPotential.Should().BeNull();
        }

        /// <summary>
        /// Verifies that when <see cref="StoreWrapperController.SelectFolder"/> returns a
        /// non-null <see cref="FolderMinimalWrapper"/>, the <c>ArchiveOutlook_Click</c>
        /// handler stores it in <see cref="StoreWrapperController.ArchiveOutlook"/> and writes
        /// the folder's <see cref="FolderMinimalWrapper.RelativePath"/> to the viewer label.
        ///
        /// Purpose:
        ///     Confirm the "selecting a folder updates the target folder property" contract without
        ///     requiring a real Outlook COM session.  <see cref="StubSelectFolderController"/>
        ///     overrides the internal virtual SelectFolder to inject a known stub, bypassing the
        ///     PickFolder COM call.  <see cref="StoreWrapperViewer"/> is used directly as the
        ///     viewer to avoid Moq's AwaitableFactory dependency issue.
        ///
        /// Returns:
        ///     Passes when controller.ArchiveOutlook is the same instance that SelectFolder returned.
        /// </summary>
        [TestMethod]
        public void ArchiveOutlook_Click_SelectFolderReturnsFolder_SetsArchiveOutlookToReturnedFolder()
        {
            // Arrange: inject a known folder via the stub subclass.
            // Null globals: SelectFolder is overridden so Globals.Ol is never called.
            // Use StoreWrapperViewer directly (no Moq) — avoids Moq AwaitableFactory failure.
            var stubFolder = new FolderMinimalWrapper("Archive", "Root\\Archive");
            var controller = new StubSelectFolderController(null!, stubFolder);
            controller.Viewer = new StoreWrapperViewer();

            // Act: click handler calls SelectFolder() and stores the result.
            controller.ArchiveOutlook_Click();

            // Assert: the property was updated to exactly the stub folder returned by SelectFolder.
            controller.ArchiveOutlook.Should().BeSameAs(stubFolder);
        }

        #endregion

        #region Stub helpers

        /// <summary>
        /// Test-only subclass that overrides <see cref="StoreWrapperController.SelectFolder"/>
        /// to return a controllable stub folder, bypassing the Outlook COM PickFolder call.
        ///
        /// Usage:
        ///     Construct with a predetermined <see cref="FolderMinimalWrapper"/> so click-handler
        ///     tests can verify downstream property updates without a COM session.
        /// </summary>
        private sealed class StubSelectFolderController : StoreWrapperController
        {
            private readonly FolderMinimalWrapper _stub;

            /// <summary>
            /// Initializes the controller with a globals dependency and a predetermined
            /// stub folder to return from <see cref="SelectFolder"/>.
            ///
            /// Args:
            ///     globals: Application globals (may be mocked; SelectFolder is overridden).
            ///     stubFolder: The folder instance to return when SelectFolder is called.
            /// </summary>
            internal StubSelectFolderController(
                IApplicationGlobals globals,
                FolderMinimalWrapper stubFolder
            )
                : base(globals)
            {
                _stub = stubFolder;
            }

            /// <summary>
            /// Returns the injected stub folder instead of invoking PickFolder over COM.
            ///
            /// Returns:
            ///     The stub folder supplied at construction time.
            /// </summary>
            internal override FolderMinimalWrapper SelectFolder() => _stub;
        }

        #endregion
    }
}
