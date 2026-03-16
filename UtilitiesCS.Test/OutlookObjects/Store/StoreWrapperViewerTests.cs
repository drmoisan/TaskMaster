using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;

using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    [TestClass]
    public class StoreWrapperViewerTests
    {
        [TestMethod]
        public void Constructor_WithController_AssignsController()
        {
            var controller = new StoreWrapperController(new Mock<IApplicationGlobals>().Object);

            using var viewer = new StoreWrapperViewer(controller);

            viewer.Controller.Should().BeSameAs(controller);
        }

        [TestMethod]
        public void ButtonOk_Click_ForwardsToControllerAndClosesViewer()
        {
            var controller = CreateController();
            var dispatchedViewer = new Mock<IStoreWrapperViewer>();
            SetInternalProperty(controller, "Viewer", dispatchedViewer.Object);
            using var viewer = new StoreWrapperViewer(controller);

            viewer.ButtonOk_Click(viewer.ButtonOk, EventArgs.Empty);

            dispatchedViewer.Verify(x => x.Close(), Times.Once);
        }

        [TestMethod]
        public void ButtonCancel_Click_ForwardsToControllerAndClosesViewer()
        {
            var controller = CreateController();
            var dispatchedViewer = new Mock<IStoreWrapperViewer>();
            SetInternalProperty(controller, "Viewer", dispatchedViewer.Object);
            using var viewer = new StoreWrapperViewer(controller);

            viewer.ButtonCancel_Click(viewer.ButtonCancel, EventArgs.Empty);

            dispatchedViewer.Verify(x => x.Close(), Times.Once);
        }

        [TestMethod]
        public void DisplayName_SelectedValueChanged_ForwardsToControllerAndPopulatesViewer()
        {
            var controller = CreateController();
            using var viewer = CreateViewer(controller);
            var store = CreateProjectedStore("Mailbox", "Inbox Path", "Root Path", "owner@example.com");
            var model = new StoresWrapper { Stores = new List<StoreWrapper> { store } };

            SetInternalProperty(controller, "Model", model);
            viewer.DisplayName.DataSource = new List<string> { "Mailbox" };
            viewer.DisplayName.SelectedIndex = 0;

            viewer.DisplayName_SelectedValueChanged(viewer.DisplayName, EventArgs.Empty);

            GetInternalProperty<StoreWrapper>(controller, "Current").Should().BeSameAs(store);
            viewer.Inbox.Text.Should().Be("Inbox Path");
            viewer.RootFolder.Text.Should().Be("Root Path");
            viewer.UserEmail.Text.Should().Be("owner@example.com");
            viewer.ArchiveFS.Text.Should().Be("Please select an archive");
            viewer.ArchiveOutlook.Text.Should().Be("Please select an archive");
            viewer.JunkEmail.Text.Should().Be("Please select a folder");
            viewer.JunkPotential.Text.Should().Be("Please select a folder");
        }

        [TestMethod]
        public void ForwardingHandlers_WhenControllerIsNull_DoNotThrow()
        {
            using var viewer = new StoreWrapperViewer();
            viewer.DisplayName.DataSource = new List<string> { "Mailbox" };
            viewer.DisplayName.SelectedIndex = 0;

            Action act = () =>
            {
                viewer.ButtonOk_Click(viewer.ButtonOk, EventArgs.Empty);
                viewer.ButtonCancel_Click(viewer.ButtonCancel, EventArgs.Empty);
                viewer.DisplayName_SelectedValueChanged(viewer.DisplayName, EventArgs.Empty);
                viewer.ArchiveFS_Click(viewer.ArchiveFS, EventArgs.Empty);
                viewer.ArchiveOutlook_Click(viewer.ArchiveOutlook, EventArgs.Empty);
                viewer.JunkEmail_Click(viewer.JunkEmail, EventArgs.Empty);
                viewer.JunkPotential_Click(viewer.JunkPotential, EventArgs.Empty);
            };

            act.Should().NotThrow();
        }

        private static StoreWrapperController CreateController()
        {
            return new StoreWrapperController(new Mock<IApplicationGlobals>().Object);
        }

        private static StoreWrapperViewer CreateViewer(StoreWrapperController controller)
        {
            var viewer = new StoreWrapperViewer(controller);
            SetInternalProperty(controller, "Viewer", viewer);
            viewer.CreateControl();
            return viewer;
        }

        private static StoreWrapper CreateProjectedStore(
            string displayName,
            string inboxPath,
            string rootPath,
            string userEmailAddress)
        {
            var inbox = new Mock<OutlookFolder>();
            var rootFolder = new Mock<OutlookFolder>();

            inbox.SetupGet(x => x.FolderPath).Returns(inboxPath);
            rootFolder.SetupGet(x => x.FolderPath).Returns(rootPath);

            return new StoreWrapper(null)
            {
                DisplayName = displayName,
                Inbox = inbox.Object,
                RootFolder = rootFolder.Object,
                UserEmailAddress = userEmailAddress,
                ArchiveRoot = null,
                ArchiveFsRoot = null,
                JunkCertain = null,
                JunkPotential = null,
            };
        }

        private static T GetInternalProperty<T>(object instance, string propertyName)
        {
            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            property.Should().NotBeNull($"property {propertyName} should exist");
            return (T)property!.GetValue(instance);
        }

        private static void SetInternalProperty(object instance, string propertyName, object value)
        {
            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            property.Should().NotBeNull($"property {propertyName} should exist");
            property!.SetValue(instance, value);
        }
    }
}
