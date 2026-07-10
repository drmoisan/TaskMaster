using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;
using UtilitiesCS.OutlookObjects.Store;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
    [DoNotParallelize]
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

        [TestMethod]
        public void PopulateWithCurrent_ShowsCurrentJunkSelectionsInViewer()
        {
            using var viewer = new StoreWrapperViewer();
            var controller = new StoreWrapperController(null!) { Viewer = viewer };
            controller.Current = new StoreWrapper(null)
            {
                JunkCertain = new FolderMinimalWrapper("Junk", "Inbox\\Junk Email"),
                JunkPotential = new FolderMinimalWrapper("Potential", "Inbox\\Junk Potential"),
            };

            controller.PopulateWithCurrent();

            viewer.JunkEmail.Text.Should().Be("Inbox\\Junk Email");
            viewer.JunkPotential.Text.Should().Be("Inbox\\Junk Potential");
        }

        [TestMethod]
        public void SaveChanges_PersistsBothSettingsAndRefreshesActiveJunkFolders()
        {
            var olObjects = new RecordingOlObjects();
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects);
            var controller = new StoreWrapperController(globals.Object)
            {
                Model = new StoresWrapper(),
                Current = new StoreWrapper(null),
                JunkEmail = new FolderMinimalWrapper("Junk", "Inbox\\Junk Email"),
                JunkPotential = new FolderMinimalWrapper("Potential", "Inbox\\Junk Potential"),
            };

            controller.SaveChanges();

            controller.Current.JunkCertain.RelativePath.Should().Be("Inbox\\Junk Email");
            controller.Current.JunkPotential.RelativePath.Should().Be("Inbox\\Junk Potential");
            olObjects.ApplyCallCount.Should().Be(1);
            olObjects.AppliedJunkCertainPath.Should().Be("Inbox\\Junk Email");
            olObjects.AppliedJunkPotentialPath.Should().Be("Inbox\\Junk Potential");
            olObjects.JunkCertain.FolderPath.Should().Be("Inbox\\Junk Email");
            olObjects.JunkPotential.FolderPath.Should().Be("Inbox\\Junk Potential");
        }

        [TestMethod]
        public void ButtonCancel_Click_LeavesStoredSettingsAndActiveFoldersUnchanged()
        {
            var olObjects = new RecordingOlObjects();
            olObjects.ApplyJunkFolderSelections("Inbox\\Current Junk", "Inbox\\Current Potential");
            var currentCertain = olObjects.JunkCertain;
            var currentPotential = olObjects.JunkPotential;
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects);
            var viewer = new Mock<IStoreWrapperViewer>();
            var controller = new StoreWrapperController(globals.Object)
            {
                Viewer = viewer.Object,
                Current = new StoreWrapper(null)
                {
                    JunkCertain = new FolderMinimalWrapper("Current", "Inbox\\Current Junk"),
                    JunkPotential = new FolderMinimalWrapper(
                        "CurrentPotential",
                        "Inbox\\Current Potential"
                    ),
                },
                JunkEmail = new FolderMinimalWrapper("New", "Inbox\\New Junk"),
                JunkPotential = new FolderMinimalWrapper("NewPotential", "Inbox\\New Potential"),
            };

            controller.ButtonCancel_Click();

            controller.Current.JunkCertain.RelativePath.Should().Be("Inbox\\Current Junk");
            controller.Current.JunkPotential.RelativePath.Should().Be("Inbox\\Current Potential");
            olObjects.ApplyCallCount.Should().Be(1);
            olObjects.JunkCertain.Should().BeSameAs(currentCertain);
            olObjects.JunkPotential.Should().BeSameAs(currentPotential);
            viewer.Verify(x => x.Close(), Times.Once);
        }

        [TestMethod]
        public void PopulateWithCurrent_WhenInvokeRequired_DelegatesToViewerInvoke()
        {
            var viewer = new Mock<IStoreWrapperViewer>();
            viewer.SetupGet(x => x.InvokeRequired).Returns(true);
            viewer.Setup(x => x.Invoke(It.IsAny<Delegate>())).Returns((object)null);
            var controller = new StoreWrapperController(null!) { Viewer = viewer.Object };

            controller.PopulateWithCurrent();

            viewer.Verify(x => x.Invoke(It.IsAny<Delegate>()), Times.Once);
        }

        [TestMethod]
        public void PersistJunkFolderSelections_WhenApplyMethodIsMissing_DoesNotThrow()
        {
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(new NoApplyOlObjects());
            var controller = new StoreWrapperController(globals.Object)
            {
                JunkEmail = new FolderMinimalWrapper("Junk", "Inbox\\Junk Email"),
                JunkPotential = new FolderMinimalWrapper("Potential", "Inbox\\Junk Potential"),
            };

            var act = () => controller.PersistJunkFolderSelections();

            act.Should().NotThrow();
        }

        private abstract class OlObjectsStubBase : IOlObjects
        {
            public Application App => null!;
            public string InboxPath => string.Empty;
            public string ArchiveRootPath => string.Empty;
            public OutlookFolder ArchiveRoot => null!;
            public IOutlookFolderTreeService FolderTreeService => null!;
            public string UserEmailAddress => string.Empty;
            public string EmailPrefixToStrip => string.Empty;
            public OutlookFolder Inbox => null!;
            public IEnumerable<OutlookFolder> Inboxes => [];
            public NameSpace NamespaceMAPI => null!;
            public OutlookFolder Root => null!;
            public StoresWrapper StoresWrapper { get; set; } = new();
            public Reminders OlReminders => null!;
            public OutlookFolder ToDoFolder => null!;
            public StackObjectCS<object> MovedMailsStack { get; set; } = null!;
            public string ViewWide => string.Empty;
            public string ViewCompact => string.Empty;
            public bool DarkMode { get; set; }
            public TimedDiskWriter<string> EmailMoveWriter => null!;
            public OutlookFolder JunkCertain { get; private set; } = null!;
            public OutlookFolder JunkPotential { get; private set; } = null!;

            public int GetExplorerScreenNumber() => 0;

            public System.Windows.Forms.Screen GetExplorerScreen() => null!;

            public Size GetExplorerScreenSize() => default;

            public Task LoadAsync() => Task.CompletedTask;

            public event PropertyChangedEventHandler PropertyChanged;

            protected static OutlookFolder CreateFolder(string folderPath)
            {
                var folder = new Mock<OutlookFolder>();
                folder.SetupGet(x => x.FolderPath).Returns(folderPath);
                return folder.Object;
            }

            protected void SetJunkFolders(
                string junkCertainRelativePath,
                string junkPotentialRelativePath
            )
            {
                JunkCertain = CreateFolder(junkCertainRelativePath);
                JunkPotential = CreateFolder(junkPotentialRelativePath);
            }
        }

        private sealed class RecordingOlObjects : OlObjectsStubBase
        {
            public string AppliedJunkCertainPath { get; private set; } = string.Empty;
            public string AppliedJunkPotentialPath { get; private set; } = string.Empty;
            public int ApplyCallCount { get; private set; }

            public void ApplyJunkFolderSelections(
                string junkCertainRelativePath,
                string junkPotentialRelativePath
            )
            {
                AppliedJunkCertainPath = junkCertainRelativePath;
                AppliedJunkPotentialPath = junkPotentialRelativePath;
                ApplyCallCount++;
                SetJunkFolders(junkCertainRelativePath, junkPotentialRelativePath);
            }
        }

        private sealed class NoApplyOlObjects : OlObjectsStubBase { }
    }
}
