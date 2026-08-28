using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.Threading;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Cycle-2 Phase 6 (AC9, P6-T3) coverage for the members whose only barrier was the static
    /// <c>UiThread.Dispatcher</c>. Each is now routed through the injectable <c>IUiDispatcher</c> seam
    /// and exercised with a synchronous dispatcher mock (<see cref="QfcItemControllerTestSupport.BuildSyncDispatcher"/>)
    /// that executes the marshaled delegate on the calling thread, so the dispatched behavior is
    /// verified deterministically without a live WPF message pump.
    /// </summary>
    [TestClass]
    public class QfcItemController_SeamDispatcherTests
    {
        private static (HarnessController controller, Mock<IItemViewer> viewer) BuildWithDispatcher(
            out Mock<UtilitiesCS.Threading.IUiDispatcher> dispatcher
        )
        {
            dispatcher = QfcItemControllerTestSupport.BuildSyncDispatcher();
            var viewer = new Mock<IItemViewer>();
            var controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_uiDispatcher", dispatcher.Object);
            QfcItemControllerTestSupport.SetField(controller, "_itemViewer", viewer.Object);
            return (controller, viewer);
        }

        [TestMethod]
        public void PopulateConversationInt_NonZero_BeginInvokesAndSetsCountText()
        {
            var (controller, viewer) = BuildWithDispatcher(out var dispatcher);

            controller.PopulateConversation(3);

            dispatcher.Verify(d => d.BeginInvoke(It.IsAny<Action>()), Times.Once());
            viewer.VerifySet(v => v.ConversationCountText = "3", Times.Once());
            viewer.VerifySet(v => v.ConversationCountBackColor = It.IsAny<Color>(), Times.Never());
        }

        [TestMethod]
        public void PopulateConversationInt_Zero_SetsRedBackColor()
        {
            var (controller, viewer) = BuildWithDispatcher(out _);

            controller.PopulateConversation(0);

            viewer.VerifySet(v => v.ConversationCountText = "0", Times.Once());
            viewer.VerifySet(v => v.ConversationCountBackColor = Color.Red, Times.Once());
        }

        [TestMethod]
        public async Task RenderConversationCountAsync_BackgroundLoad_UsesBackgroundPriorityAndSetsText()
        {
            var (controller, viewer) = BuildWithDispatcher(out var dispatcher);

            await controller.RenderConversationCountAsync(4, CancellationToken.None, true);

            viewer.VerifySet(v => v.ConversationCountText = "4", Times.Once());
            dispatcher.Verify(
                d =>
                    d.InvokeAsync(
                        It.IsAny<Action>(),
                        DispatcherPriority.Background,
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once()
            );
        }

        [TestMethod]
        public async Task JumpToFolderDropDownAsync_TogglesKeyboardAndFocusesFolder()
        {
            var (controller, viewer) = BuildWithDispatcher(out _);
            var kbd = new Mock<IQfcKeyboardHandler>();
            kbd.Setup(k => k.ToggleKeyboardDialogAsync()).Returns(Task.CompletedTask);
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", kbd.Object);

            await controller.JumpToFolderDropDownAsync();

            kbd.Verify(k => k.ToggleKeyboardDialogAsync(), Times.Once());
            viewer.Verify(v => v.FocusFolderDropDown(), Times.Once());
            viewer.Verify(v => v.SetFolderDroppedDown(true), Times.Once());
        }

        [TestMethod]
        public async Task MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher()
        {
            var (controller, viewer) = BuildWithDispatcher(out var dispatcher);

            await controller.MenuDropDown();

            dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
            viewer.Verify(v => v.ShowMoveOptionsMenu(), Times.Once());
        }

        [TestMethod]
        public void ToggleConversationCheckbox_TogglesCurrentState()
        {
            var (controller, viewer) = BuildWithDispatcher(out _);
            viewer.SetupGet(v => v.ConversationModeChecked).Returns(false);

            controller.ToggleConversationCheckbox();

            viewer.VerifySet(v => v.ConversationModeChecked = true, Times.Once());
        }

        [TestMethod]
        public void ToggleConversationCheckboxState_On_WhenUnchecked_SetsChecked()
        {
            var (controller, viewer) = BuildWithDispatcher(out _);
            viewer.SetupGet(v => v.ConversationModeChecked).Returns(false);

            controller.ToggleConversationCheckbox(Enums.ToggleState.On);

            viewer.VerifySet(v => v.ConversationModeChecked = true, Times.Once());
        }

        [TestMethod]
        public void ToggleConversationCheckboxState_Off_WhenChecked_ClearsChecked()
        {
            var (controller, viewer) = BuildWithDispatcher(out _);
            viewer.SetupGet(v => v.ConversationModeChecked).Returns(true);

            controller.ToggleConversationCheckbox(Enums.ToggleState.Off);

            viewer.VerifySet(v => v.ConversationModeChecked = false, Times.Once());
        }

        [TestMethod]
        public void ToggleSaveCopyOfMail_TogglesEmailCopyThroughDispatcher()
        {
            var (controller, viewer) = BuildWithDispatcher(out var dispatcher);
            viewer.SetupGet(v => v.EmailCopyChecked).Returns(false);

            controller.ToggleSaveCopyOfMail();

            dispatcher.Verify(d => d.Invoke(It.IsAny<Action>()), Times.Once());
            viewer.VerifySet(v => v.EmailCopyChecked = true, Times.Once());
        }

        [TestMethod]
        public async Task EnumerateConversationAsync_RunsEnumerateThroughDispatcher()
        {
            var (controller, viewer) = BuildWithDispatcher(out var dispatcher);
            var folderItems = new[] { @"\\Archive\A" };
            viewer.Setup(v => v.GetFolderItems()).Returns(folderItems);
            var parent = new Mock<IQfcCollectionController>();
            var mailActions = new Mock<IMailItemActions>();
            mailActions.SetupGet(m => m.EntryID).Returns("entry-1");
            var mockGlobals = new Mock<IApplicationGlobals>();
            var mockMail = new Mock<Microsoft.Office.Interop.Outlook.MailItem>();
            var resolver = new ConversationResolver(mockGlobals.Object, mockMail.Object)
            {
                Count = new Pair<int>(2, 2),
            };
            QfcItemControllerTestSupport.SetField(controller, "_parent", parent.Object);
            QfcItemControllerTestSupport.SetField(controller, "_mailActions", mailActions.Object);
            QfcItemControllerTestSupport.SetField(controller, "_conversationResolver", resolver);

            await controller.EnumerateConversationAsync();

            dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
            parent.Verify(
                p => p.ToggleUnGroupConv(resolver, "entry-1", 2, folderItems),
                Times.Once()
            );
        }

        [TestMethod]
        public async Task MarkItemForDeletionAsync_AddsAndSelectsTrashThroughDispatcher()
        {
            var (controller, viewer) = BuildWithDispatcher(out var dispatcher);
            viewer.Setup(v => v.FolderContains("Trash to Delete")).Returns(false);

            await controller.MarkItemForDeletionAsync();

            dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
            viewer.Verify(
                v =>
                    v.AddFolderItems(
                        It.Is<string[]>(a => a.Length == 1 && a[0] == "Trash to Delete")
                    ),
                Times.Once()
            );
            viewer.Verify(v => v.SetFolderSelectedItem("Trash to Delete"), Times.Once());
        }

        // ------------------------- Theme + IUiDispatcher retrofit (P10-T32/T33/T34) -------------------------

        private static (
            Mock<IQfcKeyboardHandler> mock,
            MailItemHelper helper
        ) BuildKbdAndHelperStub(string entryId)
        {
            var mockKbd = new Mock<IQfcKeyboardHandler>();
            var keyActionsAsync = new KbdActions<Keys, KaKeyAsync, Func<Keys, Task>>();
            var charActionsAsync = new KbdActions<char, KaCharAsync, Func<char, Task>>();
            mockKbd.Setup(k => k.KeyActionsAsync).Returns(keyActionsAsync);
            mockKbd.Setup(k => k.CharActionsAsync).Returns(charActionsAsync);
            var helper = new MailItemHelper { EntryId = entryId };
            return (mockKbd, helper);
        }

        /// <summary>
        /// Cycle-3 P10-T32 (member #34, de-exempted): <c>ToggleFocusAsync(Enums.ToggleState)</c>
        /// unconditionally awaits <c>Theme.SetQfcThemeAsync()</c>, which now routes through the
        /// injected <see cref="IUiDispatcher"/> instead of the static <c>UiThread.Dispatcher</c>.
        /// </summary>
        [TestMethod]
        public async Task ToggleFocusAsync_StateOverload_WhenTurningOn_RegistersAndRoutesThemeThroughInjectedDispatcher()
        {
            // Arrange
            Mock<IUiDispatcher> themeDispatcher = new Mock<IUiDispatcher>();
            themeDispatcher
                .Setup(d => d.InvokeAsync(It.IsAny<Action>()))
                .Returns(Task.CompletedTask);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(
                controller,
                "_themes",
                QfcItemControllerTestSupport.BuildThemeDictionary(
                    "LightActive",
                    QfcItemControllerTestSupport.BuildDispatchableTheme(themeDispatcher.Object)
                )
            );
            QfcItemControllerTestSupport.SetField(controller, "_activeTheme", "LightNormal");
            QfcItemControllerTestSupport.SetField(
                controller,
                "_listTipsDetails",
                new List<IQfcTipsDetails>()
            );
            QfcItemControllerTestSupport.SetField(
                controller,
                "_listTipsExpanded",
                new List<IQfcTipsDetails>()
            );
            var (mockKbd, helper) = BuildKbdAndHelperStub("toggle-on-entry");
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", mockKbd.Object);
            controller.ItemHelper = helper;
            QfcItemControllerTestSupport.SetField(controller, "_activeUI", false);

            // Act
            await controller.ToggleFocusAsync(Enums.ToggleState.On);

            // Assert
            QfcItemControllerTestSupport.GetField(controller, "_activeUI").Should().Be(true);
            QfcItemControllerTestSupport
                .GetField(controller, "_activeTheme")
                .Should()
                .Be("LightActive");
            themeDispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
        }

        /// <summary>Cycle-3 P10-T33 (member #36, de-exempted): the parameterless overload.</summary>
        [TestMethod]
        public async Task ToggleFocusAsync_ParameterlessOverload_WhenActive_RoutesToOffAndThemeThroughInjectedDispatcher()
        {
            // Arrange
            Mock<IUiDispatcher> themeDispatcher = new Mock<IUiDispatcher>();
            themeDispatcher
                .Setup(d => d.InvokeAsync(It.IsAny<Action>()))
                .Returns(Task.CompletedTask);
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(
                controller,
                "_themes",
                QfcItemControllerTestSupport.BuildThemeDictionary(
                    "LightNormal",
                    QfcItemControllerTestSupport.BuildDispatchableTheme(themeDispatcher.Object)
                )
            );
            QfcItemControllerTestSupport.SetField(controller, "_activeTheme", "LightNormal");
            QfcItemControllerTestSupport.SetField(
                controller,
                "_listTipsDetails",
                new List<IQfcTipsDetails>()
            );
            QfcItemControllerTestSupport.SetField(
                controller,
                "_listTipsExpanded",
                new List<IQfcTipsDetails>()
            );
            var (mockKbd, helper) = BuildKbdAndHelperStub("toggle-off-entry");
            QfcItemControllerTestSupport.SetField(controller, "_kbdHandler", mockKbd.Object);
            controller.ItemHelper = helper;
            QfcItemControllerTestSupport.SetField(controller, "_activeUI", true);

            // Act
            await controller.ToggleFocusAsync();

            // Assert
            QfcItemControllerTestSupport.GetField(controller, "_activeUI").Should().Be(false);
            themeDispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once());
        }

        /// <summary>
        /// Cycle-3 P10-T34 (member #37, de-exempted): <c>ApplyReadEmailFormat</c>'s residual
        /// <c>Theme.SetMailRead(async: true)</c> call now routes through the injected
        /// <see cref="IUiDispatcher"/>'s <c>BeginInvoke</c> instead of the live-handle-requiring
        /// <c>_lblSender.BeginInvoke</c>.
        /// </summary>
        [TestMethod]
        public void ApplyReadEmailFormat_MarksMailReadFalseAndRoutesThemeThroughInjectedDispatcherBeginInvoke()
        {
            // Arrange
            Mock<IUiDispatcher> themeDispatcher = new Mock<IUiDispatcher>();
            themeDispatcher
                .Setup(d => d.BeginInvoke(It.IsAny<Action>()))
                .Returns(Mock.Of<IAsyncResult>());
            HarnessController controller = new HarnessController();
            QfcItemControllerTestSupport.SetField(controller, "_activeTheme", "LightNormal");
            QfcItemControllerTestSupport.SetField(
                controller,
                "_themes",
                QfcItemControllerTestSupport.BuildThemeDictionary(
                    "LightNormal",
                    QfcItemControllerTestSupport.BuildDispatchableTheme(themeDispatcher.Object)
                )
            );
            var mailActions = new Mock<IMailItemActions>();
            QfcItemControllerTestSupport.SetField(controller, "_mailActions", mailActions.Object);
            // MailItemHelper.UnRead's setter forwards to Item.UnRead/Item.Save(), so a real (mocked)
            // MailItem-backed helper is used instead of the parameterless constructor (whose Item is
            // null).
            var mailItemMock = new Mock<Microsoft.Office.Interop.Outlook.MailItem>();
            var globalsMock = new Mock<IApplicationGlobals>();
            controller.ItemHelper = new MailItemHelper(mailItemMock.Object, globalsMock.Object);

            // Act
            controller.ApplyReadEmailFormat(null);

            // Assert
            controller.ItemHelper.UnRead.Should().BeFalse();
            mailActions.VerifySet(m => m.UnRead = false, Times.Once());
            mailActions.Verify(m => m.Save(), Times.Once());
            themeDispatcher.Verify(d => d.BeginInvoke(It.IsAny<Action>()), Times.Once());
        }
    }
}
