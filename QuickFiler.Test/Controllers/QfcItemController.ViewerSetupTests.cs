using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler;
using QuickFiler.Controllers;
using QuickFiler.Test.TestSupport;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// ViewerSetup-cluster tests (cycle-2 Phase 5, AC8). Covers the de-exempted
    /// PopulateControls(MailItemHelper,int), AssignControls, AssignControlsAsync, and Cleanup members,
    /// exercised through the narrowed IItemViewer intent members and a mocked settings object. No
    /// live WinForms control is required: the InvokeRequired guard is mocked and the async overload
    /// dispatches through a real (test-thread) WPF Dispatcher pumped deterministically.
    /// </summary>
    [TestClass]
    public class QfcItemController_ViewerSetupTests
    {
        /// <summary>
        /// Harness bound for the #230 pump-hosted tests (MSTest <c>[Timeout]</c> precedent
        /// <c>TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs</c>). Every wait in those tests is
        /// on a deterministic completion signal; this attribute only converts a genuine deadlock in
        /// production code into a test failure instead of a CI hang.
        /// </summary>
        private const int PumpTimeoutMs = 60000;

        private static Mock<IApplicationGlobals> BuildGlobals(
            bool moveConversation,
            bool saveEmailCopy,
            bool saveAttachments,
            bool savePictures
        )
        {
            Mock<IAppQuickFilerSettings> settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.MoveEntireConversation).Returns(moveConversation);
            settings.SetupGet(s => s.SaveEmailCopy).Returns(saveEmailCopy);
            settings.SetupGet(s => s.SaveAttachments).Returns(saveAttachments);
            settings.SetupGet(s => s.SavePictures).Returns(savePictures);
            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.QfSettings).Returns(settings.Object);
            return globals;
        }

        private static MailItemHelper BuildHelper(bool isTaskFlagSet, string body)
        {
            MailItemHelper helper = new MailItemHelper();
            helper.Body = body;
            helper.IsTaskFlagSet = isTaskFlagSet;
            return helper;
        }

        /// <summary>
        /// Cycle-3 (P9-T1/P9-T2): builds a globals mock that satisfies both
        /// <c>MailItemHelper</c>'s lazily-materialized <c>Globals.Ol.EmailPrefixToStrip</c> read and
        /// the controller's own <c>_globals.QfSettings</c> reads in <c>AssignControls</c>.
        /// </summary>
        private static Mock<IApplicationGlobals> BuildGlobalsWithOl(
            bool moveConversation,
            bool saveEmailCopy,
            bool saveAttachments,
            bool savePictures
        )
        {
            Mock<IAppQuickFilerSettings> settings = new Mock<IAppQuickFilerSettings>();
            settings.SetupGet(s => s.MoveEntireConversation).Returns(moveConversation);
            settings.SetupGet(s => s.SaveEmailCopy).Returns(saveEmailCopy);
            settings.SetupGet(s => s.SaveAttachments).Returns(saveAttachments);
            settings.SetupGet(s => s.SavePictures).Returns(savePictures);

            Mock<IOlObjects> olObjects = new Mock<IOlObjects>();
            olObjects.SetupGet(o => o.EmailPrefixToStrip).Returns(string.Empty);

            Mock<IApplicationGlobals> globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.QfSettings).Returns(settings.Object);
            globals.SetupGet(g => g.Ol).Returns(olObjects.Object);
            return globals;
        }

        /// <summary>
        /// Cycle-3 (P9-T1/P9-T2): a <see cref="Mock{MailItem}"/> with every property read by
        /// <c>MailItemHelper</c>'s lazy fields exercised by <c>AssignControls</c> (sync path) and by
        /// <c>MaterializeTokenizationDependencies</c> (async path via <c>FromMailItemAsync</c>), mirroring
        /// the <c>Mock&lt;InteropMailItem&gt;</c> setup pattern in
        /// <c>UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs</c>. <c>UserProperties</c>
        /// is additionally mocked (returning no "Triage" property) since <c>AssignControls</c> reads
        /// <c>Triage</c>, which the shared core test does not need.
        /// </summary>
        private static Mock<MailItem> BuildMailItemMock()
        {
            Mock<MailItem> mailItem = new Mock<MailItem>();

            Mock<PropertyAccessor> propertyAccessor = new Mock<PropertyAccessor>();
            Mock<AddressEntry> sender = new Mock<AddressEntry>();
            sender
                .SetupGet(x => x.AddressEntryUserType)
                .Returns(OlAddressEntryUserType.olSmtpAddressEntry);
            sender.SetupGet(x => x.Name).Returns("Ada Sender");
            sender.SetupGet(x => x.Address).Returns("ada@example.com");
            sender.SetupGet(x => x.PropertyAccessor).Returns(propertyAccessor.Object);

            Mock<Recipients> recipients = new Mock<Recipients>();
            recipients.SetupGet(x => x.Count).Returns(0);
            recipients
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)Array.Empty<Recipient>()).GetEnumerator());

            Mock<Attachments> attachments = new Mock<Attachments>();
            attachments.SetupGet(x => x.Count).Returns(0);
            attachments
                .Setup(x => x.GetEnumerator())
                .Returns(() => ((IEnumerable)Array.Empty<Attachment>()).GetEnumerator());

            Mock<UserProperties> userProperties = new Mock<UserProperties>();
            userProperties
                .Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);

            mailItem.SetupGet(x => x.Subject).Returns("Subject");
            mailItem.SetupGet(x => x.Body).Returns("Body");
            mailItem.SetupGet(x => x.HTMLBody).Returns("<html><body>Body</body></html>");
            mailItem.SetupGet(x => x.SenderName).Returns("Ada Sender");
            mailItem.SetupGet(x => x.SenderEmailAddress).Returns("ada@example.com");
            mailItem.SetupGet(x => x.EntryID).Returns("entry-1");
            mailItem.SetupGet(x => x.Sender).Returns(sender.Object);
            mailItem.SetupGet(x => x.Recipients).Returns(recipients.Object);
            mailItem.SetupGet(x => x.Attachments).Returns(attachments.Object);
            mailItem.SetupGet(x => x.FlagStatus).Returns(OlFlagStatus.olNoFlag);
            mailItem.SetupGet(x => x.SentOn).Returns(new DateTime(2026, 1, 1));
            mailItem.SetupGet(x => x.Categories).Returns(string.Empty);
            mailItem.SetupGet(x => x.UserProperties).Returns(userProperties.Object);

            return mailItem;
        }

        [TestMethod]
        public void PopulateControls_WithHelper_StoresHelperAndAssignsViewerFields()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(false, false, false, false).Object
            );
            MailItemHelper helper = BuildHelper(false, "populated-body");

            // Act
            controller.PopulateControls(helper, 4);

            // Assert — the helper is stored and its values are pushed onto the viewer intent members.
            controller.ItemHelper.Should().BeSameAs(helper);
            viewer.VerifySet(v => v.BodyText = "populated-body", Times.Once());
            viewer.VerifySet(v => v.ItemNumberText = "4", Times.Once());
        }

        /// <summary>
        /// Cycle-3 P9-T1 (member #11, de-exempted): <c>PopulateControls(MailItem,int)</c> constructs a
        /// <c>MailItemHelper</c> from a live-typed COM <c>MailItem</c>. Since <c>MailItem</c> is a
        /// mockable COM interface, no live Outlook host is required — mirrors
        /// <c>MailItemHelperCoreTests.FromMailItemAsync_...</c>.
        /// </summary>
        [TestMethod]
        public void PopulateControls_WithMailItem_ConstructsHelperAndAssignsControls()
        {
            // Arrange
            Mock<MailItem> mailItem = BuildMailItemMock();
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobalsWithOl(false, false, false, false).Object
            );

            // Act
            controller.PopulateControls(mailItem.Object, 3);

            // Assert — the helper is constructed from the mocked MailItem and pushed to the viewer.
            controller.ItemHelper.Should().NotBeNull();
            viewer.VerifySet(v => v.SubjectText = "Subject", Times.Once());
            viewer.VerifySet(v => v.ItemNumberText = "3", Times.Once());
        }

        /// <summary>
        /// Cycle-3 P9-T2 (member #12, de-exempted): <c>PopulateControlsAsync</c> loads the helper via
        /// <c>MailItemHelper.FromMailItemAsync</c> (the same call signature already exercised against a
        /// mocked <c>MailItem</c> in <c>MailItemHelperCoreTests</c>) and dispatches the assignment through
        /// the viewer's <c>UiDispatcher</c>, mirroring <c>AssignControlsAsync_DispatchesAssignThroughViewerDispatcher</c>.
        /// </summary>
        [TestMethod]
        public async Task PopulateControlsAsync_WithMailItem_LoadsHelperViaFromMailItemAsyncAndAssignsControls()
        {
            // Arrange
            Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                Mock<MailItem> mailItem = BuildMailItemMock();
                Mock<IItemViewer> viewer = new Mock<IItemViewer>();
                viewer.SetupGet(v => v.InvokeRequired).Returns(false);
                viewer.SetupGet(v => v.UiDispatcher).Returns(dispatcher);
                HarnessController controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
                QfcItemControllerTestSupport.SetField(
                    controller,
                    "_globals",
                    BuildGlobalsWithOl(false, false, false, false).Object
                );

                // Act — block deterministically on the dispatched task's completion (no polling).
                await controller.PopulateControlsAsync(mailItem.Object, 5, loadAll: false);

                // Assert
                controller.ItemHelper.Should().NotBeNull();
                viewer.VerifySet(v => v.SubjectText = "Subject", Times.Once());
                viewer.VerifySet(v => v.ItemNumberText = "5", Times.Once());
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
            }
        }

        [TestMethod]
        public void AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings()
        {
            // Arrange — distinct settings values so each checkbox intent member is verified.
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(true, true, false, true).Object
            );
            MailItemHelper helper = BuildHelper(isTaskFlagSet: true, body: "b");

            // Act
            controller.AssignControls(helper, 9);

            // Assert
            viewer.VerifySet(v => v.BodyText = "b", Times.Once());
            viewer.VerifySet(v => v.ItemNumberText = "9", Times.Once());
            viewer.VerifySet(v => v.FlagTaskDialogResult = DialogResult.OK, Times.Once());
            viewer.VerifySet(v => v.ConversationModeChecked = true, Times.Once());
            viewer.VerifySet(v => v.EmailCopyChecked = true, Times.Once());
            viewer.VerifySet(v => v.AttachmentsChecked = false, Times.Once());
            viewer.VerifySet(v => v.PicturesChecked = true, Times.Once());
        }

        [TestMethod]
        public void AssignControls_WhenTaskFlagUnset_SetsCancelDialogResult()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(false, false, false, false).Object
            );

            // Act
            controller.AssignControls(BuildHelper(isTaskFlagSet: false, body: "b"), 1);

            // Assert
            viewer.VerifySet(v => v.FlagTaskDialogResult = DialogResult.Cancel, Times.Once());
        }

        [TestMethod]
        public void AssignControls_WhenInvokeRequired_MarshalsViaInvoke()
        {
            // Arrange
            Mock<IItemViewer> viewer = new Mock<IItemViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(true);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                BuildGlobals(false, false, false, false).Object
            );

            // Act
            controller.AssignControls(BuildHelper(false, "b"), 1);

            // Assert — the write is marshaled through Invoke rather than applied directly.
            viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once());
            viewer.VerifySet(v => v.BodyText = It.IsAny<string>(), Times.Never());
        }

        [TestMethod]
        public void AssignControlsAsync_DispatchesAssignThroughViewerDispatcher()
        {
            // Arrange — supply a dedicated running WPF Dispatcher (on its own thread) for the async
            // overload; the inner AssignControls sees InvokeRequired == false and writes directly. A
            // dedicated dispatcher is used instead of the shared test-thread dispatcher so this test
            // only executes its own dispatched operation and is immune to fire-and-forget operations
            // posted to the thread dispatcher by unrelated tests.
            Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
            try
            {
                Mock<IItemViewer> viewer = new Mock<IItemViewer>();
                viewer.SetupGet(v => v.InvokeRequired).Returns(false);
                viewer.SetupGet(v => v.UiDispatcher).Returns(dispatcher);
                HarnessController controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
                QfcItemControllerTestSupport.SetField(
                    controller,
                    "_globals",
                    BuildGlobals(false, false, false, false).Object
                );

                // Act — block deterministically on the dispatched task's completion (no polling).
                controller
                    .AssignControlsAsync(BuildHelper(false, "async-body"), 2)
                    .GetAwaiter()
                    .GetResult();

                // Assert
                viewer.VerifySet(v => v.BodyText = "async-body", Times.Once());
                viewer.VerifySet(v => v.ItemNumberText = "2", Times.Once());
            }
            finally
            {
                QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
            }
        }

        [TestMethod]
        public void Cleanup_NullsTrackedPrivateFields()
        {
            // Arrange — populate the fields Cleanup is responsible for releasing.
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(
                controller,
                "_globals",
                new Mock<IApplicationGlobals>().Object
            );
            QfcItemControllerTestSupport.SetField(
                controller,
                "_itemViewer",
                new Mock<IItemViewer>().Object
            );
            QfcItemControllerTestSupport.SetField(
                controller,
                "_homeController",
                new Mock<QuickFiler.Interfaces.IFilerHomeController>().Object
            );
            controller.ItemHelper = new MailItemHelper();

            // Act
            controller.Cleanup();

            // Assert — the released references are null after cleanup.
            QfcItemControllerTestSupport.GetField(controller, "_globals").Should().BeNull();
            QfcItemControllerTestSupport.GetField(controller, "_itemViewer").Should().BeNull();
            QfcItemControllerTestSupport.GetField(controller, "_homeController").Should().BeNull();
            controller.ItemHelper.Should().BeNull();
        }

        /// <summary>
        /// Cycle-5 (R1, de-exempted): <c>ResolveControlGroups(ItemViewer)</c> walks a real, headless
        /// <see cref="QuickFiler.ItemViewer"/>'s Designer-constructed control tree and classifies its
        /// children by concrete type. Constructing <c>ItemViewer</c> requires a non-null ambient
        /// <see cref="SynchronizationContext"/> on the calling thread (for
        /// <c>TaskScheduler.FromCurrentSynchronizationContext()</c>); the context is installed and
        /// restored exactly (mirroring <c>ProgressPane_Tests.cs</c>'s try/finally pattern) so no context
        /// leaks across tests.
        /// </summary>
        [TestMethod]
        public void ResolveControlGroups_WithHeadlessItemViewer_PopulatesConcreteControlCollections()
        {
            // Arrange
            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            try
            {
                var viewer = new QuickFiler.ItemViewer();
                var controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer);

                // Act
                QfcItemControllerTestSupport.InvokeNonPublic(
                    controller,
                    "ResolveControlGroups",
                    viewer
                );

                // Assert — both concrete control collections are populated from the real control tree.
                controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                controller.Buttons.Should().NotBeNullOrEmpty();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// #230 (de-exempted): <c>ResolveControlGroupsAsync(ItemViewer)</c> is the pure pump case —
        /// it builds <c>QfcTipsDetails</c> for every tip label and awaits
        /// <c>itemViewer.UiSyncContext</c> before walking the Designer control tree. On a thread-pool
        /// MSTest thread that await never completes; running the viewer on
        /// <see cref="WinFormsPumpHost"/> supplies the message loop that drains it, so the member can
        /// be awaited from the MSTest thread and its observable state asserted.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups()
        {
            // Arrange — the viewer must be constructed on the pump so UiSyncContext binds there.
            WinFormsPumpHost host = new WinFormsPumpHost();
            try
            {
                QuickFiler.ItemViewer viewer = await host.InvokeAsync(() =>
                        new QuickFiler.ItemViewer()
                    )
                    .ConfigureAwait(false);
                HarnessController controller = new HarnessController();
                QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer);
                controller.Token = CancellationToken.None;

                // Act — awaited from the MSTest thread; continuations drain through the live pump.
                await controller.ResolveControlGroupsAsync(viewer).ConfigureAwait(false);

                // Assert — the tip-detail collections and concrete control groups are populated.
                QfcItemControllerTestSupport
                    .GetField(controller, "_itemPositionTips")
                    .Should()
                    .NotBeNull(because: "the item-number tip is built before the context await");
                ICollection tipsDetails = (ICollection)
                    QfcItemControllerTestSupport.GetField(controller, "_listTipsDetails");
                tipsDetails.Should().NotBeNull();
                tipsDetails
                    .Count.Should()
                    .BeGreaterThan(0, because: "the viewer's Designer tree carries tip labels");
                QfcItemControllerTestSupport
                    .GetField(controller, "_listTipsExpanded")
                    .Should()
                    .NotBeNull();
                controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                controller.Buttons.Should().NotBeNullOrEmpty();
            }
            finally
            {
                await host.StopAsync().ConfigureAwait(false);
            }
        }
    }
}
