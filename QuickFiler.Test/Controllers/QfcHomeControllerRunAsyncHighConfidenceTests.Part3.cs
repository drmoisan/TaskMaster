using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.ReusableTypeClasses;

namespace QuickFiler.Controllers.Tests
{
    // Issue #678, remediation cycle 1, item R1. This part carries the leg-A item-set invariant
    // test. It declares no [TestClass] attribute of its own: the attribute on the base part
    // (QfcHomeControllerRunAsyncTests.cs) covers the whole partial class, and a second one would
    // be a duplicate-attribute error.
    public partial class QfcHomeControllerRunAsyncTests
    {
        /// <summary>
        /// Builds a loose mail-item mock whose <c>EntryID</c> is the supplied value. Loose rather
        /// than strict because the production scoring and logging paths also read
        /// <c>Subject</c>, which a strict mock would reject; no live Outlook COM is touched.
        /// </summary>
        private static MailItem MailItemWithEntryId(string entryId)
        {
            var mail = new Mock<MailItem>(MockBehavior.Loose);
            mail.SetupGet(x => x.EntryID).Returns(entryId);
            return mail.Object;
        }

        /// <summary>
        /// Issue #678, R1. The set of mail items displayed on leg A must be exactly the set that
        /// survived <c>UnhookDequeuedNodes</c>. No item whose <c>UnhookItem</c> call failed may be
        /// displayed, and no item that <c>TryUnhookOrReplace</c> pulled out of the master queue may
        /// go undisplayed.
        ///
        /// The test has two stages in one method so the divergence it asserts against is produced
        /// by the real <c>TryUnhookOrReplace</c> throw branch rather than hand-built. Stage one
        /// drives <c>QfcDatamodel.DequeueNextItemGroupWithOutcomeAsync</c> down that branch and
        /// asserts the resulting batch genuinely diverges: <c>Items</c> holds only the substitute
        /// and <c>PreScored</c> holds only the failed item. Stage two feeds that same batch through
        /// <c>QfcHomeController.RunAsync</c> and asserts the carrier list reaching
        /// <c>LoadItemsAsync</c> — the boundary that
        /// <c>QfcCollectionController.LoadControlsAndHandlers_01Async</c> turns into rendered rows —
        /// names the substitute and not the failed item.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary()
        {
            // ---------------------------------------------------------------------------------
            // Stage one — arrange: produce a genuinely divergent batch from the real datamodel.
            // ---------------------------------------------------------------------------------
            var model = (QfcDatamodel)
                FormatterServices.GetUninitializedObject(typeof(QfcDatamodel));

            // FormatterServices.GetUninitializedObject runs no field initialiser, so TimeProvider
            // is null and the gate's GetTimestamp call would throw. A FakeTimeProvider is the
            // deterministic seam .claude/rules/general-unit-test.md requires; the clock is never
            // advanced here because the quantity-satisfied exit is reached on the first iteration
            // and needs no simulated time to elapse.
            model.TimeProvider = new FakeTimeProvider();

            MailItem failedItem = MailItemWithEntryId("entry-failed");
            MailItem substituteItem = MailItemWithEntryId("entry-substitute");

            var masterQueue = new LockingLinkedList<MailItem>();
            masterQueue.AddLast(failedItem);
            masterQueue.AddLast(substituteItem);

            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(x => x.HighConfidenceThreshold).Returns(0.90);
            var modelGlobals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            modelGlobals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            IFolderSearchHandler scoredHandler = new Mock<IFolderSearchHandler>().Object;
            var scoringService = new Mock<IFolderScoringService>(MockBehavior.Strict);
            scoringService
                .Setup(x =>
                    x.ScoreAsync(
                        It.IsAny<MailItem>(),
                        It.IsAny<IApplicationGlobals>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.FromResult((950L, @"\\Archive\Projects\Active", scoredHandler)));

            // The monitor throws for the first item it is handed and succeeds afterwards, which is
            // exactly the TryUnhookOrReplace throw branch: remove the failed node, pull a
            // replacement from the master queue, re-insert it at the same index.
            var unhookCalls = new List<MailItem>();
            var moveMonitor = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            moveMonitor
                .Setup(x => x.UnhookItem(It.IsAny<MailItem>()))
                .Callback<MailItem>(item =>
                {
                    unhookCalls.Add(item);
                    if (unhookCalls.Count == 1)
                    {
                        throw new InvalidOperationException(
                            "simulated EmailMoveMonitor unhook failure"
                        );
                    }
                });

            SetPrivateField(model, "_globals", modelGlobals.Object);
            SetPrivateField(model, "_masterQueue", masterQueue);
            SetPrivateField(model, "_moveMonitor", moveMonitor.Object);
            SetPrivateField(model, "_worker", new BackgroundWorker());
            SetPrivateField(model, "_remainingLoadActive", true);
            model.ScoringServiceFactory = () => scoringService.Object;

            // ---------------------------------------------------------------------------------
            // Stage one — act. The quantity of 1 is load-bearing and not a free choice: with 2 the
            // gate accepts both queued items, _masterQueue.TryTakeFirst() returns null inside
            // TryUnhookOrReplace, no substitute is inserted, and PreScored would hold two entries
            // rather than the one the stage-one assertion requires.
            // ---------------------------------------------------------------------------------
            QfcDequeueBatch batch = await model.DequeueNextItemGroupWithOutcomeAsync(
                1,
                0,
                TimeSpan.FromSeconds(3),
                null
            );

            // ---------------------------------------------------------------------------------
            // Stage one — assert the divergence is real before relying on it.
            // ---------------------------------------------------------------------------------
            batch
                .Items.Should()
                .ContainSingle(
                    "the throw branch removes the failed item and inserts exactly one substitute"
                );
            batch
                .Items[0]
                .Should()
                .BeSameAs(
                    substituteItem,
                    "TryUnhookOrReplace replaces the failed node with the next master-queue entry"
                );
            batch
                .PreScored.Should()
                .ContainSingle("the gate accepted exactly one candidate before the unhook pass");
            batch
                .PreScored[0]
                .MailItem.Should()
                .BeSameAs(
                    failedItem,
                    "PreScored is captured before UnhookDequeuedNodes, so it still names the item "
                        + "whose UnhookItem call threw"
                );

            // ---------------------------------------------------------------------------------
            // Stage two — arrange: drive the real RunAsync with that exact batch.
            // ---------------------------------------------------------------------------------
            var tokenSource = new CancellationTokenSource();
            _mockProgressTracker = SetupMockProgressTracker(tokenSource);
            ProgressTracker progress = _mockProgressTracker.Object;

            const int itemsPerIteration = 7;
            SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90);

            var mockDataModel = new Mock<IQfcDatamodel>();
            mockDataModel
                .Setup(x =>
                    x.InitEmailQueueAsync(
                        It.IsAny<int>(),
                        It.IsAny<BackgroundWorker>(),
                        It.IsAny<CancellationToken>(),
                        It.IsAny<CancellationTokenSource>()
                    )
                )
                .ReturnsAsync(new List<MailItem>());
            mockDataModel
                .Setup(x =>
                    x.DequeueNextItemGroupWithOutcomeAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<TimeSpan>(),
                        It.IsAny<System.Action<int, int, int>>()
                    )
                )
                .ReturnsAsync(batch);
            mockDataModel.Setup(x => x.Complete).Returns(true);
            _controller.DataModel = mockDataModel.Object;

            IList<QfcPreScoredItem> loaded = null;
            var mockFormController = new Mock<IQfcFormController>();
            mockFormController.SetupGet(x => x.ItemsPerIteration).Returns(itemsPerIteration);
            mockFormController
                .Setup(x => x.LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>()))
                .Callback<IList<QfcPreScoredItem>>(carriers => loaded = carriers)
                .Returns(Task.CompletedTask);
            SetPrivateField(_controller, "_formController", mockFormController.Object);

            var mockFormViewer = new Mock<IQfcFormViewer>();
            mockFormViewer.SetupGet(x => x.Worker).Returns(new BackgroundWorker());
            SetPrivateField(_controller, "_formViewer", mockFormViewer.Object);

            // ---------------------------------------------------------------------------------
            // Stage two — act.
            // ---------------------------------------------------------------------------------
            await _controller.RunAsync(progress);

            // ---------------------------------------------------------------------------------
            // Stage two — assert at the consuming boundary. QfcFormController forwards this list to
            // QfcCollectionController.LoadControlsAndHandlers_01Async, whose body derives the
            // displayed spine as preScored.Select(x => x.MailItem) and builds one QfcItemGroup per
            // carrier, so this list IS the displayed set.
            // ---------------------------------------------------------------------------------
            loaded
                .Should()
                .NotBeNull("RunAsync must invoke the carrier overload in high-confidence mode");
            loaded
                .Should()
                .ContainSingle(
                    "the displayed set must match the one item that survived the unhook"
                );
            loaded[0]
                .MailItem.Should()
                .BeSameAs(
                    substituteItem,
                    "the substitute left the master queue and is lost for the session unless it is "
                        + "displayed"
                );
            loaded
                .Should()
                .NotContain(
                    carrier => ReferenceEquals(carrier.MailItem, failedItem),
                    "an item still hooked to the EmailMoveMonitor must never reach the display"
                );
            loaded[0]
                .FolderHandler.Should()
                .BeNull(
                    "the substitute was pulled from the master queue after scoring, so no carrier "
                        + "was ever built for it and the item controller must fall back to its own "
                        + "scoring pass"
                );
        }
    }
}
