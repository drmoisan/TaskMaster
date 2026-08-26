using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Mail-actions cluster tests (research §5.2). Covers PackageItems single-item packaging and
    /// MarkItemForDeletion add-when-absent / select-when-present routing through the narrowed
    /// IItemViewer folder intent members.
    /// </summary>
    [TestClass]
    public class QfcItemController_MailActionsTests
    {
        private sealed class MailController : QfcItemController
        {
            internal MailController()
                : base() { }
        }

        private static void SetField(QfcItemController controller, string name, object value) =>
            typeof(QfcItemController)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(controller, value);

        [TestMethod]
        public void PackageItems_WhenConversationUnchecked_ReturnsSingleItem()
        {
            // Arrange — conversation mode off: PackageItems returns only the controller's own item.
            var controller = new MailController();
            var helper = new MailItemHelper();
            controller.ItemHelper = helper;
            SetField(controller, "_optionConversationChecked", false);

            // Act
            IList<MailItemHelper> packaged = controller.PackageItems();

            // Assert
            packaged.Should().ContainSingle().Which.Should().BeSameAs(helper);
        }

        [TestMethod]
        public void MarkItemForDeletion_WhenTrashFolderAbsent_AddsAndSelectsIt()
        {
            // Arrange — the "Trash to Delete" pseudo-folder is not present; it must be added, then
            // selected.
            var mock = new Mock<IItemViewer>();
            mock.Setup(v => v.FolderContains("Trash to Delete")).Returns(false);
            var controller = new MailController();
            SetField(controller, "_itemViewer", mock.Object);

            // Act
            controller.MarkItemForDeletion();

            // Assert
            mock.Verify(
                v =>
                    v.SetFolderItems(
                        It.Is<string[]>(a => a.Length == 1 && a[0] == "Trash to Delete")
                    ),
                Times.Once()
            );
            mock.Verify(v => v.SetFolderSelectedItem("Trash to Delete"), Times.Once());
        }

        [TestMethod]
        public void MarkItemForDeletion_WhenTrashFolderPresent_SelectsWithoutAdding()
        {
            // Arrange — the pseudo-folder already exists; it must be selected without re-adding.
            var mock = new Mock<IItemViewer>();
            mock.Setup(v => v.FolderContains("Trash to Delete")).Returns(true);
            var controller = new MailController();
            SetField(controller, "_itemViewer", mock.Object);

            // Act
            controller.MarkItemForDeletion();

            // Assert
            mock.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Never());
            mock.Verify(v => v.SetFolderSelectedItem("Trash to Delete"), Times.Once());
        }

        // ---------------------------------------------------------------------------
        // Cycle-2 Phase 5 (AC8) de-exemption coverage: RightKeyActions / RightKeyActionsAsync getters
        // (dictionary-membership; the lambda bodies are not invoked so no COM is touched), and the
        // CollapseConversation / EnumerateConversation collaborator routing.
        // ---------------------------------------------------------------------------

        private static ConversationResolver BuildResolverWithCount(int sameFolder)
        {
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockMail = new Mock<MailItem>();
            var resolver = new ConversationResolver(mockGlobals.Object, mockMail.Object);
            resolver.Count = new Pair<int>(sameFolder: sameFolder, expanded: sameFolder);
            return resolver;
        }

        [TestMethod]
        public void RightKeyActions_Getter_ContainsExpectedMenuKeys()
        {
            // Arrange
            var controller = new MailController();

            // Act
            Dictionary<string, System.Action> actions = controller.RightKeyActions;

            // Assert
            actions.Should().ContainKey("&Pop Out");
            actions.Should().ContainKey("&Expand");
            actions.Should().ContainKey("&Cancel");
        }

        [TestMethod]
        public void RightKeyActionsAsync_Getter_ContainsExpectedMenuKeys()
        {
            // Arrange
            var controller = new MailController();

            // Act
            Dictionary<string, System.Func<Task>> actions = controller.RightKeyActionsAsync;

            // Assert
            actions.Should().ContainKey("&Pop Out");
            actions.Should().ContainKey("&Expand");
            actions.Should().ContainKey("&Cancel");
        }

        [TestMethod]
        public void CollapseConversation_WhenConvOriginIdSet_TogglesGroupWithThatId()
        {
            // Arrange — a non-empty _convOriginID selects the origin id branch, avoiding the COM
            // Mail.EntryID fallback (deferred to the Phase 6 IMailItemActions seam).
            var viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.GetFolderItems()).Returns(new[] { @"\\Archive\A" });
            var parent = new Mock<IQfcCollectionController>();
            var controller = new MailController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_parent", parent.Object);
            controller.ConvOriginID = "origin-123";

            // Act
            controller.CollapseConversation();

            // Assert
            parent.Verify(p => p.ToggleGroupConv("origin-123"), Times.Once());
        }

        [TestMethod]
        public void EnumerateConversation_TogglesUnGroupWithResolverEntryIdAndCount()
        {
            // Arrange — the resolver and entry id are read from mockable collaborators; the EntryID now
            // comes from the Phase 6 IMailItemActions seam (P6-T7) instead of a live Mail.EntryID.
            var folderItems = new[] { @"\\Archive\A" };
            var viewer = new Mock<IItemViewer>();
            viewer.Setup(v => v.GetFolderItems()).Returns(folderItems);
            var parent = new Mock<IQfcCollectionController>();
            var mailActions = new Mock<IMailItemActions>();
            mailActions.SetupGet(m => m.EntryID).Returns("entry-xyz");
            var resolver = BuildResolverWithCount(4);
            var controller = new MailController();
            SetField(controller, "_itemViewer", viewer.Object);
            SetField(controller, "_parent", parent.Object);
            SetField(controller, "_conversationResolver", resolver);
            SetField(controller, "_mailActions", mailActions.Object);

            // Act
            controller.EnumerateConversation();

            // Assert
            parent.Verify(
                p => p.ToggleUnGroupConv(resolver, "entry-xyz", 4, folderItems),
                Times.Once()
            );
        }

        // ---------------------------------------------------------------------------------------
        // Issue #480 — ToggleNavigation(bool) must dispatch IQfcTipsDetails.Toggle(false) exactly
        // once per branch. Routed here from QfcItemController.FocusAndThemeTests.cs by the plan's
        // constraint C2 capacity table: that file is at its 497-line baseline with 3 spare lines.
        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// Issue #480: the previously-untested <c>async: true</c> branch of
        /// <c>ToggleNavigation(bool)</c> must produce exactly one <c>Toggle(false)</c> dispatch.
        /// <c>QfcTipsDetails.Toggle(bool)</c> is a flip, not an idempotent set, so two dispatches
        /// restore the starting state and the affordance does nothing.
        /// </summary>
        [TestMethod]
        public void ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce()
        {
            // Arrange — an executing viewer so the BeginInvoke delegate runs synchronously and the
            // Toggle(false) call reaches the tips mock where it can be counted.
            var tips = new Mock<IQfcTipsDetails>();
            Mock<IItemViewer> viewer = QfcItemControllerTestSupport.BuildExecutingViewer();
            var controller = new MailController();
            SetField(controller, "_itemPositionTips", tips.Object);
            SetField(controller, "_itemViewer", viewer.Object);

            // Act
            controller.ToggleNavigation(async: true);

            // Assert
            tips.Verify(t => t.Toggle(false), Times.Once());
        }

        // ---------------------------------------------------------------------------------------
        // Issue #485 — TryResolveCidResource must guard every externally-supplied input. No test in
        // this group constructs a controller, an ItemViewer, a MailItemHelper, or any CoreWebView2
        // type: the extracted member is internal static and is called with plain values and
        // Mock<IAttachment> objects.
        // ---------------------------------------------------------------------------------------

        /// <summary>
        /// Shared assert helper for the issue #485 "ignore this request" cases: the call must return
        /// false and must leave both <c>out</c> values null.
        /// </summary>
        private static void AssertRequestIgnored(
            string uri,
            IReadOnlyDictionary<string, IAttachment> map
        )
        {
            bool resolved = QfcItemController.TryResolveCidResource(
                uri,
                map,
                out var pay,
                out var mime
            );
            resolved.Should().BeFalse();
            pay.Should().BeNull();
            mime.Should().BeNull();
        }

        /// <summary>
        /// Issue #485: an unusable requested URI is ignored rather than throwing. A malformed URI
        /// previously threw <c>UriFormatException</c> from the unguarded <c>new Uri(...)</c>; a
        /// relative URI would throw <c>InvalidOperationException</c> from <c>Uri.Segments</c>; an
        /// absolute URI whose final segment is empty carries no content id.
        /// </summary>
        [DataTestMethod]
        [DataRow("::not a uri::", DisplayName = "malformed URI")]
        [DataRow("/x/y", DisplayName = "relative URI")]
        [DataRow("https://cid.quickfiler.local/", DisplayName = "empty final segment")]
        public void TryResolveCidResource_RejectsUnusableUri_ReturnsFalseWithNullOutputs(string uri)
        {
            // Arrange a populated map, so a false result can only come from the URI itself; Act+Assert
            AssertRequestIgnored(uri, MapWith("logo", new byte[] { 1 }, ".png"));
        }

        /// <summary>
        /// Issue #485: a null map is ignored rather than throwing from the unguarded lookup.
        /// </summary>
        [TestMethod]
        public void TryResolveCidResource_WithNullMap_ReturnsFalse() =>
            AssertRequestIgnored("https://cid.quickfiler.local/logo", null);

        /// <summary>
        /// Issue #485: a content id absent from the map yields no response and null outputs.
        /// </summary>
        [TestMethod]
        public void TryResolveCidResource_WithMapMiss_ReturnsFalse() =>
            AssertRequestIgnored(
                "https://cid.quickfiler.local/absent",
                MapWith("logo", new byte[] { 1 }, ".png")
            );

        /// <summary>
        /// Issue #485: a map hit does not imply a payload. <c>BuildContentIdMap</c> does not filter on
        /// <c>AttachmentData</c>, so a matched attachment can carry a null byte array; serving it
        /// previously threw <c>ArgumentNullException</c> from <c>new MemoryStream</c>.
        /// </summary>
        [TestMethod]
        public void TryResolveCidResource_WithNullAttachmentData_ReturnsFalse() =>
            AssertRequestIgnored(
                "https://cid.quickfiler.local/logo",
                MapWith("logo", null, ".png")
            );

        /// <summary>
        /// Issue #485 happy path: a map hit with real bytes and a known extension returns the exact
        /// payload reference and the matching MIME type.
        /// </summary>
        [TestMethod]
        public void TryResolveCidResource_WithKnownExtension_ReturnsPayloadAndMimeType() =>
            AssertRequestServed(new byte[] { 7, 8, 9 }, ".png", "image/png");

        /// <summary>
        /// Issue #485: an unrecognised extension still serves the payload, falling back to the
        /// generic octet stream rather than failing the intercepted request.
        /// </summary>
        [TestMethod]
        public void TryResolveCidResource_WithUnrecognisedExtension_ReturnsOctetStream() =>
            AssertRequestServed(new byte[] { 4, 2 }, ".zzz", "application/octet-stream");

        /// <summary>
        /// Shared assert helper for the issue #485 "serve this request" cases: the call must return
        /// true, hand back the exact payload reference, and report the expected MIME type.
        /// </summary>
        private static void AssertRequestServed(byte[] bytes, string extension, string expectedMime)
        {
            bool resolved = QfcItemController.TryResolveCidResource(
                "https://cid.quickfiler.local/logo",
                MapWith("logo", bytes, extension),
                out var payload,
                out var mimeType
            );
            resolved.Should().BeTrue();
            payload.Should().BeSameAs(bytes);
            mimeType.Should().Be(expectedMime);
        }

        private static IReadOnlyDictionary<string, IAttachment> MapWith(
            string contentId,
            byte[] data,
            string extension
        ) => QfcItemControllerTestSupport.BuildContentIdMap(contentId, data, extension);

        // ---------------------------------------------------------------------------------------
        // Issue #483 — MoveMailAsync must propagate instead of swallowing, must route its user
        // message through the MoveFailureNotifier seam on the UI dispatcher, and the three async
        // members must honour a cancelled Token. Every collaborator is a Moq mock or an injected
        // delegate, and every test replaces the notifier seam, so no modal dialog is reachable.
        // ---------------------------------------------------------------------------------------

        private static MailController Filing(System.Func<EmailFilerConfig, EmailFiler> filerFactory)
        {
            var controller = new MailController();
            QfcItemControllerTestSupport.InjectFilingCollaborators(controller, filerFactory);
            controller.MoveFailureNotifier = _ => { };
            return controller;
        }

        /// <summary>Issue #483: the catch must propagate with added context, not return normally.</summary>
        [TestMethod]
        public async Task MoveMailAsync_WhenFilerFactoryThrows_WrapsAndRethrowsWithInnerException()
        {
            // Arrange
            var fault = new System.InvalidTimeZoneException("filer factory refused the config");
            MailController controller = Filing(_ => throw fault);
            int notifications = 0;
            controller.MoveFailureNotifier = _ => notifications++;

            // Act
            System.Func<Task> act = () => controller.MoveMailAsync();

            // Assert
            (
                await act.Should().ThrowAsync<System.InvalidOperationException>()
            ).WithInnerException<System.InvalidTimeZoneException>();
            notifications.Should().Be(1);
        }

        /// <summary>
        /// Issue #483: a fault raised inside FilerQueue.Enqueue is wrapped too. A conversation-mode
        /// package holding a null helper makes the FilerQueueItem constructor throw
        /// ArgumentNullException (FilerQueue.cs:70-78).
        /// </summary>
        [TestMethod]
        public async Task MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException()
        {
            // Arrange
            MailController controller = Filing(c => new EmailFiler(c));
            ConversationResolver resolver = BuildResolverWithCount(1);
            resolver.ConversationInfo = new Pair<List<MailItemHelper>>(
                sameFolder: new List<MailItemHelper> { null },
                expanded: new List<MailItemHelper>()
            );
            SetField(controller, "_conversationResolver", resolver);
            SetField(controller, "_optionConversationChecked", true);

            // Act
            System.Func<Task> act = () => controller.MoveMailAsync();

            // Assert
            (
                await act.Should().ThrowAsync<System.InvalidOperationException>()
            ).WithInnerException<System.ArgumentNullException>();
        }

        /// <summary>Issue #483: the notification is marshalled through the injected UI dispatcher.</summary>
        [TestMethod]
        public async Task MoveMailAsync_WithUiDispatcher_MarshalsNotificationThroughDispatcher()
        {
            // Arrange
            MailController controller = Filing(_ => throw new System.InvalidTimeZoneException("x"));
            bool notified = false;
            controller.MoveFailureNotifier = _ => notified = true;
            var dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
            SetField(controller, "_uiDispatcher", dispatcher.Object);

            // Act
            System.Func<Task> act = () => controller.MoveMailAsync();

            // Assert
            await act.Should().ThrowAsync<System.InvalidOperationException>();
            dispatcher.Verify(d => d.Invoke(It.IsAny<System.Action>()), Times.Once());
            notified.Should().BeTrue();
        }

        /// <summary>Issue #483: a cancelled Token aborts MoveMailAsync before any filing work.</summary>
        [TestMethod]
        public async Task MoveMailAsync_WhenTokenAlreadyCancelled_ThrowsAndNeverInvokesFilerFactory()
        {
            // Arrange
            bool factoryCalled = false;
            MailController controller = Filing(c =>
            {
                factoryCalled = true;
                return new EmailFiler(c);
            });
            controller.Token = QfcItemControllerTestSupport.CancelledToken();

            // Act
            System.Func<Task> act = () => controller.MoveMailAsync();

            // Assert
            await act.Should().ThrowAsync<System.OperationCanceledException>();
            factoryCalled.Should().BeFalse();
        }

        /// <summary>Issue #483: FlagAsTaskAsync checks cancellation before its COM Mail read.</summary>
        [TestMethod]
        public async Task FlagAsTaskAsync_WhenTokenAlreadyCancelled_Throws()
        {
            // Arrange
            var controller = new MailController();
            controller.Token = QfcItemControllerTestSupport.CancelledToken();

            // Act
            System.Func<Task> act = () => controller.FlagAsTaskAsync();

            // Assert
            await act.Should().ThrowAsync<System.OperationCanceledException>();
        }

        /// <summary>Issue #483: EnumerateConversationAsync checks cancellation before dispatching.</summary>
        [TestMethod]
        public async Task EnumerateConversationAsync_WhenTokenAlreadyCancelled_Throws()
        {
            // Arrange
            var controller = new MailController();
            controller.Token = QfcItemControllerTestSupport.CancelledToken();

            // Act
            System.Func<Task> act = () => controller.EnumerateConversationAsync();

            // Assert
            await act.Should().ThrowAsync<System.OperationCanceledException>();
        }
    }
}
