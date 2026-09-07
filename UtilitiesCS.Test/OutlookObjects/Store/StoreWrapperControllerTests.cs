using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
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
            // Retargeted to the typed seam (issue #797, AC5). What is "missing" is no longer a
            // method discoverable by name but an implementation of IJunkFolderSelectionSink: the
            // double below declares a matching public method yet does not implement the interface.
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(new NonSinkOlObjects());
            var controller = new StoreWrapperController(globals.Object)
            {
                JunkEmail = new FolderMinimalWrapper("Junk", "Inbox\\Junk Email"),
                JunkPotential = new FolderMinimalWrapper("Potential", "Inbox\\Junk Potential"),
            };

            var act = () => controller.PersistJunkFolderSelections();

            act.Should().NotThrow();
        }

        [TestMethod]
        public void PersistJunkFolderSelections_PassesJunkCertainPathFirst()
        {
            // Arrange (issue #797, AC5): the argument order is enforced by nothing except
            // positional agreement between the call site and the signature, which is exactly the
            // fragility the typed seam removes. Pin it with distinguishable values.
            var olObjects = new RecordingOlObjects();
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects);
            var controller = new StoreWrapperController(globals.Object)
            {
                JunkEmail = new FolderMinimalWrapper("Certain", "Inbox\\Certain Folder"),
                JunkPotential = new FolderMinimalWrapper("Potential", "Inbox\\Potential Folder"),
            };

            // Act
            controller.PersistJunkFolderSelections();

            // Assert
            olObjects.ApplyCallCount.Should().Be(1);
            olObjects
                .AppliedJunkCertainPath.Should()
                .Be("Inbox\\Certain Folder", "the junk-certain path is supplied first.");
            olObjects
                .AppliedJunkPotentialPath.Should()
                .Be("Inbox\\Potential Folder", "the junk-potential path is supplied second.");
        }

        [TestMethod]
        public void PersistJunkFolderSelections_WhenGlobalsAreNotTheTypedSink_LogsErrorAndDoesNotInvoke()
        {
            // Arrange (issue #797, AC5): the failure must be loud. The double declares a public
            // method with the historic name and signature but does not implement the sink
            // interface, so the reflection lookup would have succeeded while the typed cast fails.
            var olObjects = new NonSinkOlObjects();
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(x => x.Ol).Returns(olObjects);
            var controller = new StoreWrapperController(globals.Object)
            {
                JunkEmail = new FolderMinimalWrapper("Junk", "Inbox\\Junk Email"),
                JunkPotential = new FolderMinimalWrapper("Potential", "Inbox\\Junk Potential"),
            };

            var appender = AttachControllerMemoryAppender(out var restore);
            try
            {
                // Act
                controller.PersistJunkFolderSelections();

                // Assert: existence, not an exact count. The controller's logger is a static field
                // shared with every other controller test class in this assembly and the run
                // settings impose a class-level parallel scope, so a sibling class can only add
                // events. The paired assertion that the double recorded no invocation is what
                // attributes the event to this test.
                SinkErrorEvents(appender)
                    .Should()
                    .NotBeEmpty(
                        "a failed cast to the typed sink must be reported at error level (AC5)."
                    );
                olObjects
                    .ApplyCallCount.Should()
                    .Be(0, "a double that is not the typed sink must never be invoked.");
            }
            finally
            {
                restore();
            }
        }

        /// <summary>
        /// Attaches an in-memory appender to the logger the controller writes to. The controller is
        /// not generic, so its logger name is the full name of the controller type and the appender
        /// can be attached to that named logger in the ordinary way.
        /// </summary>
        /// <param name="restore">
        /// Receives the action that detaches the appender and restores the logger's previous level
        /// and the repository's previous configured flag.
        /// </param>
        private static MemoryAppender AttachControllerMemoryAppender(out System.Action restore)
        {
            var appender = new MemoryAppender();
            appender.ActivateOptions();

            var controllerType = typeof(StoreWrapperController);
            var hierarchy = (Hierarchy)LogManager.GetRepository(controllerType.Assembly);
            var logger = (Logger)hierarchy.GetLogger(controllerType.FullName);
            var previousLevel = logger.Level;
            var previousConfigured = hierarchy.Configured;

            logger.Level = Level.Debug;
            hierarchy.Configured = true;
            logger.AddAppender(appender);

            restore = () =>
            {
                logger.RemoveAppender(appender);
                logger.Level = previousLevel;
                hierarchy.Configured = previousConfigured;
            };

            return appender;
        }

        private static LoggingEvent[] SinkErrorEvents(MemoryAppender appender)
        {
            return appender
                .GetEvents()
                .Where(loggingEvent =>
                    loggingEvent.Level >= Level.Error
                    && loggingEvent.RenderedMessage != null
                    && loggingEvent.RenderedMessage.Contains(nameof(IJunkFolderSelectionSink))
                )
                .ToArray();
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

            // Required by IOlObjects : INotifyPropertyChanged; this stub never raises it (no
            // test needs the notification), so CS0067 fires. Deleting it is not possible (the
            // interface requires it); suppressing narrowly preserves the exact pre-existing
            // behavior (no behavior change per AC7).
#pragma warning disable CS0067
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

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

        /// <summary>
        /// Globals double that implements the typed junk-folder sink (issue #797, AC5) in addition
        /// to the globals stub base, and records both arguments and the invocation count.
        /// </summary>
        private sealed class RecordingOlObjects : OlObjectsStubBase, IJunkFolderSelectionSink
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

        /// <summary>
        /// Globals double that implements only the globals interface while still declaring a public
        /// method named <c>ApplyJunkFolderSelections</c> with the same two string parameters (issue
        /// #797, AC5). The historic reflection lookup would bind to that method; the typed cast does
        /// not, so this double drives the loud-failure branch.
        /// </summary>
        private sealed class NonSinkOlObjects : OlObjectsStubBase
        {
            public int ApplyCallCount { get; private set; }

            public void ApplyJunkFolderSelections(
                string junkCertainRelativePath,
                string junkPotentialRelativePath
            )
            {
                ApplyCallCount++;
                SetJunkFolders(junkCertainRelativePath, junkPotentialRelativePath);
            }
        }
    }
}
