using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
