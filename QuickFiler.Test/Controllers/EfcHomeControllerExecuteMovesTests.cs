using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Helper_Classes;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    [TestClass]
    public class EfcHomeControllerExecuteMovesTests
    {
        [TestMethod]
        public void SelectMoveMetricsItems_WhenMovingConversation_ReturnsAllSameFolderItems()
        {
            var sameFolder = new List<MailItemHelper>
            {
                CreateMailItemHelper("first"),
                CreateMailItemHelper("second"),
            };

            var result = EfcHomeController.SelectMoveMetricsItems(
                sameFolder,
                moveConversation: true,
                mailEntryId: "first"
            );

            result.Should().Equal(sameFolder);
        }

        [TestMethod]
        public void SelectMoveMetricsItems_WhenMovingSingleItem_FiltersByCurrentMailEntryId()
        {
            var current = CreateMailItemHelper("current");
            var sameFolder = new List<MailItemHelper> { CreateMailItemHelper("other"), current };

            var result = EfcHomeController.SelectMoveMetricsItems(
                sameFolder,
                moveConversation: false,
                mailEntryId: "current"
            );

            result.Should().ContainSingle().Which.Should().BeSameAs(current);
        }

        [TestMethod]
        public void TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset()
        {
            var controller = CreateController();

            controller.TryBeginExecuteMoves().Should().BeTrue();
            controller.TryBeginExecuteMoves().Should().BeFalse();
            controller.ResetExecuteMovesState();

            controller.TryBeginExecuteMoves().Should().BeTrue();
        }

        [TestMethod]
        public async Task MoveToFolderAsync_WithInjectedAction_UsesCapturedMoveOptions()
        {
            var controller = CreateController();
            var captured = default(MoveRequest);
            controller.MoveToFolderAsyncAction = (
                selectedFolder,
                saveAttachments,
                saveEmail,
                savePictures,
                moveConversation
            ) =>
            {
                captured = new MoveRequest(
                    selectedFolder,
                    saveAttachments,
                    saveEmail,
                    savePictures,
                    moveConversation
                );
                return Task.FromResult(true);
            };

            var result = await controller.MoveToFolderAsync(
                "Archive/Target",
                saveAttachments: true,
                saveEmail: false,
                savePictures: true,
                moveConversation: false
            );

            result.Should().BeTrue();
            captured.SelectedFolder.Should().Be("Archive/Target");
            captured.SaveAttachments.Should().BeTrue();
            captured.SaveEmail.Should().BeFalse();
            captured.SavePictures.Should().BeTrue();
            captured.MoveConversation.Should().BeFalse();
        }

        [TestMethod]
        public async Task ExecuteMovesCoreAsync_UsesFormOptionsAndRoutesSuccessfulMetrics()
        {
            var controller = CreateController();
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict).Object;
            var mail = new Mock<MailItem>(MockBehavior.Strict);
            mail.SetupGet(item => item.EntryID).Returns("current");
            var current = CreateMailItemHelper("current");
            var other = CreateMailItemHelper("other");
            var dataModel = CreateControllerDataModel(
                mail.Object,
                new List<MailItemHelper> { current, other }
            );
            var formController = CreateFormController(
                selectedFolder: "Archive/Target",
                saveAttachments: true,
                saveEmail: false,
                savePictures: true,
                moveConversation: false
            );
            var capturedMove = default(MoveRequest);
            var metricsCall = default(MetricsCall);
            controller.MoveToFolderAsyncAction = (
                selectedFolder,
                saveAttachments,
                saveEmail,
                savePictures,
                moveConversation
            ) =>
            {
                capturedMove = new MoveRequest(
                    selectedFolder,
                    saveAttachments,
                    saveEmail,
                    savePictures,
                    moveConversation
                );
                return Task.FromResult(true);
            };
            controller.MoveMetricsAction = (callGlobals, selectedFolder, callMovedItems) =>
                metricsCall = new MetricsCall(callGlobals, selectedFolder, callMovedItems);
            SetPrivateField(controller, "_globals", globals);
            SetPrivateField(controller, "_dataModel", dataModel);
            SetPrivateField(controller, "_formController", formController);

            await controller.ExecuteMovesCoreAsync();

            capturedMove.SelectedFolder.Should().Be("Archive/Target");
            capturedMove.SaveAttachments.Should().BeTrue();
            capturedMove.SaveEmail.Should().BeFalse();
            capturedMove.SavePictures.Should().BeTrue();
            capturedMove.MoveConversation.Should().BeFalse();
            metricsCall.Globals.Should().BeSameAs(globals);
            metricsCall.SelectedFolder.Should().Be("Archive/Target");
            metricsCall.MovedItems.Should().ContainSingle().Which.Should().BeSameAs(current);
        }

        [TestMethod]
        public void HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction()
        {
            var controller = CreateController();
            var message = string.Empty;
            controller.MoveFailureMessageAction = text => message = text;

            controller.HandleMoveResult(
                result: false,
                globals: new Mock<IApplicationGlobals>(MockBehavior.Strict).Object,
                selectedFolder: "Archive/Target",
                movedItems: new List<MailItemHelper>()
            );

            message.Should().Be("Cannot move to folderpath Archive/Target");
        }

        [TestMethod]
        public void HandleMoveResult_WhenMoveSucceeds_RoutesMetricsThroughInjectedAction()
        {
            var controller = CreateController();
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict).Object;
            var movedItems = new List<MailItemHelper> { CreateMailItemHelper("current") };
            var metricsCall = default(MetricsCall);
            controller.MoveMetricsAction = (callGlobals, selectedFolder, callMovedItems) =>
                metricsCall = new MetricsCall(callGlobals, selectedFolder, callMovedItems);

            controller.HandleMoveResult(
                result: true,
                globals: globals,
                selectedFolder: "Archive/Target",
                movedItems: movedItems
            );

            metricsCall.Globals.Should().BeSameAs(globals);
            metricsCall.SelectedFolder.Should().Be("Archive/Target");
            metricsCall.MovedItems.Should().BeSameAs(movedItems);
        }

        private static EfcHomeController CreateController()
        {
            return (EfcHomeController)
                FormatterServices.GetUninitializedObject(typeof(EfcHomeController));
        }

        private static EfcDataModel CreateControllerDataModel(
            MailItem mail,
            List<MailItemHelper> sameFolder
        )
        {
            var dataModel = (EfcDataModel)
                FormatterServices.GetUninitializedObject(typeof(EfcDataModel));
            var resolver = new ConversationResolver(
                new Mock<IApplicationGlobals>(MockBehavior.Strict).Object,
                mail
            )
            {
                ConversationInfo = new Pair<List<MailItemHelper>>(
                    sameFolder: sameFolder,
                    expanded: sameFolder
                ),
            };
            dataModel.Mail = mail;
            SetPrivateField(dataModel, "_conversationResolver", resolver);
            return dataModel;
        }

        private static EfcFormController CreateFormController(
            string selectedFolder,
            bool saveAttachments,
            bool saveEmail,
            bool savePictures,
            bool moveConversation
        )
        {
            var viewer = (EfcViewer)FormatterServices.GetUninitializedObject(typeof(EfcViewer));
            viewer.FolderListBox = new BrightIdeasSoftware.TreeListView();
            var formController = (EfcFormController)
                FormatterServices.GetUninitializedObject(typeof(EfcFormController));
            SetPrivateField(formController, "_formViewer", viewer);
            // SelectedFolder now derives from the cached highlighted FolderSuggestionNode; inject it
            // directly because the TreeListView cannot select an item without a native window handle.
            SetPrivateField(
                formController,
                "_selectedNode",
                new FolderSuggestionNode(
                    selectedFolder,
                    selectedFolder,
                    FolderSuggestionNodeKind.Folder
                )
            );
            formController.SaveAttachments = saveAttachments;
            formController.SaveEmail = saveEmail;
            formController.SavePictures = savePictures;
            formController.MoveConversation = moveConversation;
            return formController;
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target
                .GetType()
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            field.Should().NotBeNull($"field '{name}' must exist");
            field.SetValue(target, value);
        }

        private static MailItemHelper CreateMailItemHelper(string entryId)
        {
            return new MailItemHelper { EntryId = entryId };
        }

        private readonly struct MoveRequest
        {
            public MoveRequest(
                string selectedFolder,
                bool saveAttachments,
                bool saveEmail,
                bool savePictures,
                bool moveConversation
            )
            {
                SelectedFolder = selectedFolder;
                SaveAttachments = saveAttachments;
                SaveEmail = saveEmail;
                SavePictures = savePictures;
                MoveConversation = moveConversation;
            }

            public string SelectedFolder { get; }

            public bool SaveAttachments { get; }

            public bool SaveEmail { get; }

            public bool SavePictures { get; }

            public bool MoveConversation { get; }
        }

        private readonly struct MetricsCall
        {
            public MetricsCall(
                IApplicationGlobals globals,
                string selectedFolder,
                List<MailItemHelper> movedItems
            )
            {
                Globals = globals;
                SelectedFolder = selectedFolder;
                MovedItems = movedItems;
            }

            public IApplicationGlobals Globals { get; }

            public string SelectedFolder { get; }

            public List<MailItemHelper> MovedItems { get; }
        }
    }
}
