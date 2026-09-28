using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #871 regression suite for the <see cref="QfcQueue"/> enqueue path. Every test method of
    /// the suite lives here; the shared arrangement lives in the harness part of the same partial
    /// class. The suite runs entirely headless: the six injectable seams the item added are
    /// substituted so no live Outlook process, no process-wide dispatcher and no WinForms layout
    /// pass is required.
    /// </summary>
    [TestClass]
    public partial class QfcQueueEnqueueTests
    {
        /// <summary>Seam S1 exposes a non-null default move monitor and rejects null.</summary>
        [TestMethod]
        public void MoveMonitor_SeamContract_HasNonNullDefaultAndRejectsNull()
        {
            QfcQueue queue = NewQueue();

            AssertSeamContract(() => queue.MoveMonitor, value => queue.MoveMonitor = value);
        }

        /// <summary>Seam S2 exposes a non-null default dispatcher and rejects null.</summary>
        [TestMethod]
        public void UiIdleDispatcher_SeamContract_HasNonNullDefaultAndRejectsNull()
        {
            QfcQueue queue = NewQueue();

            AssertSeamContract(
                () => queue.UiIdleDispatcher,
                value => queue.UiIdleDispatcher = value
            );
        }

        /// <summary>Seam S3 exposes a non-null default viewer factory and rejects null.</summary>
        [TestMethod]
        public void ItemViewerFactory_SeamContract_HasNonNullDefaultAndRejectsNull()
        {
            QfcQueue queue = NewQueue();

            AssertSeamContract(
                () => queue.ItemViewerFactory,
                value => queue.ItemViewerFactory = value
            );
        }

        /// <summary>Seam S4 exposes a non-null default row placer and rejects null.</summary>
        [TestMethod]
        public void ViewerRowPlacer_SeamContract_HasNonNullDefaultAndRejectsNull()
        {
            QfcQueue queue = NewQueue();

            AssertSeamContract(() => queue.ViewerRowPlacer, value => queue.ViewerRowPlacer = value);
        }

        /// <summary>Seam S5 exposes a non-null default item-group factory and rejects null.</summary>
        [TestMethod]
        public void ItemGroupFactory_SeamContract_HasNonNullDefaultAndRejectsNull()
        {
            QfcQueue queue = NewQueue();

            AssertSeamContract(
                () => queue.ItemGroupFactory,
                value => queue.ItemGroupFactory = value
            );
        }

        /// <summary>Seam S6 exposes a non-null default template factory and rejects null.</summary>
        [TestMethod]
        public void BackgroundTlpFactory_SeamContract_HasNonNullDefaultAndRejectsNull()
        {
            QfcQueue queue = NewQueue();

            AssertSeamContract(
                () => queue.BackgroundTlpFactory,
                value => queue.BackgroundTlpFactory = value
            );
        }

        /// <summary>
        /// Constructing a queue in the headless test host throws nothing, and the dispatcher seam
        /// defaults to the production adapter declared alongside it.
        /// </summary>
        [TestMethod]
        public void Construction_InHeadlessHost_SucceedsAndDefaultsToProductionAdapter()
        {
            QfcQueue queue = null;

            this.Invoking(_ => queue = NewQueue()).Should().NotThrow();

            queue.UiIdleDispatcher.Should().BeOfType<UiThreadIdleDispatcher>();
        }

        /// <summary>
        /// The default viewer factory is the static dequeue method group of the viewer queue helper.
        /// The delegate is inspected, never invoked: invoking it would read the process-wide
        /// dispatcher.
        /// </summary>
        [TestMethod]
        public void ItemViewerFactory_Default_IsTheViewerQueueDequeueMethodGroup()
        {
            QfcQueue queue = NewQueue();

            MethodInfo target = queue.ItemViewerFactory.Method;

            target.Name.Should().Be("Dequeue");
            target.DeclaringType.Should().Be(typeof(ItemViewerQueue));
        }

        /// <summary>A null item list is rejected before any seam is touched.</summary>
        [TestMethod]
        public async Task EnqueueAsync_WithNullItemList_ThrowsArgumentNullException()
        {
            QfcQueue queue = NewHeadlessQueue();

            await queue
                .Awaiting(q => EnqueuePageAsync(q, null))
                .Should()
                .ThrowAsync<ArgumentNullException>();
        }

        /// <summary>An empty item list is rejected before any seam is touched.</summary>
        [TestMethod]
        public async Task EnqueueAsync_WithEmptyItemList_ThrowsArgumentException()
        {
            QfcQueue queue = NewHeadlessQueue();

            await queue
                .Awaiting(q => EnqueuePageAsync(q, new List<MailItem>()))
                .Should()
                .ThrowAsync<ArgumentException>();
        }

        /// <summary>
        /// A page of items enqueues one entry carrying the substituted template panel and its item
        /// groups in input order.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WithOnePage_QueuesTheTemplatePanelAndGroupsInInputOrder()
        {
            QfcQueue queue = NewHeadlessQueue();
            IList<MailItem> items = NewMailItems(3);

            await EnqueuePageAsync(queue, items);

            queue.Count.Should().Be(1);
            (TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups) entry = queue.Dequeue();
            entry.Tlp.Should().BeSameAs(_sentinelTlp);
            entry.ItemGroups.Select(group => group.MailItem).Should().Equal(items);
        }

        /// <summary>
        /// The running-jobs counter reads 1 while the enqueue call is in flight, proving the
        /// increment took effect, and 0 once it returns, proving the finally block decremented it.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WhenItRuns_IncrementsRunningJobsAndDecrementsOnCompletion()
        {
            QfcQueue queue = NewHeadlessQueue();
            int midFlightJobs = -1;
            _itemGroupObserver = () => midFlightJobs = queue.JobsRunning;

            await EnqueuePageAsync(queue, NewMailItems(1));

            midFlightJobs.Should().Be(1);
            queue.JobsRunning.Should().Be(0);
        }

        /// <summary>
        /// A cancellation raised inside the try block is swallowed, nothing is queued and the
        /// counter returns to 0.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WhenLoaderIsCancelled_SwallowsAndLeavesNothingQueued()
        {
            await AssertLoaderFailureIsContainedAsync(new OperationCanceledException());
        }

        /// <summary>
        /// A non-cancellation failure raised inside the try block is logged and swallowed, nothing
        /// is queued and the counter returns to 0.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WhenLoaderFails_SwallowsAndLeavesNothingQueued()
        {
            await AssertLoaderFailureIsContainedAsync(new InvalidOperationException("loader"));
        }

        /// <summary>A subscriber receives exactly one add notification per enqueue call.</summary>
        [TestMethod]
        public async Task EnqueueAsync_WithSubscriber_RaisesExactlyOneAddNotification()
        {
            QfcQueue queue = NewHeadlessQueue();
            List<NotifyCollectionChangedEventArgs> observed =
                new List<NotifyCollectionChangedEventArgs>();
            queue.CollectionChanged += (sender, args) => observed.Add(args);

            await EnqueuePageAsync(queue, NewMailItems(1));

            observed.Should().ContainSingle();
            observed[0].Action.Should().Be(NotifyCollectionChangedAction.Add);
        }

        /// <summary>The same flow with no subscriber attached raises nothing and throws nothing.</summary>
        [TestMethod]
        public async Task EnqueueAsync_WithNoSubscriber_CompletesWithoutThrowing()
        {
            QfcQueue queue = NewHeadlessQueue();

            await queue
                .Awaiting(q => EnqueuePageAsync(q, NewMailItems(1)))
                .Should()
                .NotThrowAsync();

            queue.Count.Should().Be(1);
        }

        /// <summary>
        /// Every enqueued item is hooked into the move monitor exactly once, with that item and a
        /// non-null action. The captured delegate is never invoked, because it is an async-void
        /// lambda whose failures cannot be observed by the caller.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WithStrictMoveMonitor_HooksEachItemExactlyOnce()
        {
            QfcQueue queue = NewHeadlessQueue();
            Mock<IEmailMoveMonitor> strict = new Mock<IEmailMoveMonitor>(MockBehavior.Strict);
            List<Action<MailItem>> captured = new List<Action<MailItem>>();
            strict
                .Setup(x => x.HookItem(It.IsAny<MailItem>(), It.IsAny<Action<MailItem>>()))
                .Callback<MailItem, Action<MailItem>>((mail, action) => captured.Add(action));
            queue.MoveMonitor = strict.Object;
            IList<MailItem> items = NewMailItems(2);

            await EnqueuePageAsync(queue, items);

            foreach (MailItem item in items)
            {
                strict.Verify(x => x.HookItem(item, It.IsAny<Action<MailItem>>()), Times.Once);
            }

            captured.Should().HaveCount(2).And.NotContainNulls();
        }

        /// <summary>
        /// The item-number digit width is 1 below ten items and 2 from ten items upward, which is
        /// the boundary the loader computes before it builds any row.
        /// </summary>
        [DataTestMethod]
        [DataRow(9, 1)]
        [DataRow(10, 2)]
        [DataRow(11, 2)]
        public async Task EnqueueAsync_WithItemTotal_PassesExpectedDigitsToEachController(
            int itemTotal,
            int expectedDigits
        )
        {
            QfcQueue queue = NewHeadlessQueue();

            await EnqueuePageAsync(queue, NewMailItems(itemTotal));

            _controllerCalls.Should().HaveCount(itemTotal);
            _controllerCalls.Select(call => call.Digits).Should().AllBeEquivalentTo(expectedDigits);
        }

        /// <summary>
        /// When a carrier was pre-scored for the enqueued item, its handler reaches the item
        /// controller.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WithMatchingCarrier_PassesTheCarriedHandler()
        {
            QfcQueue queue = NewHeadlessQueue();
            IList<MailItem> items = NewMailItems(1);
            IFolderSearchHandler handler = new Mock<IFolderSearchHandler>().Object;
            IList<QfcPreScoredItem> preScored = new List<QfcPreScoredItem>
            {
                new QfcPreScoredItem(items[0], "Archive", handler),
            };

            await EnqueuePageAsync(queue, items, preScored);

            _controllerCalls.Should().ContainSingle();
            _controllerCalls[0].CarriedHandler.Should().BeSameAs(handler);
        }

        /// <summary>Without a carrier list the carried handler reaching the controller is null.</summary>
        [TestMethod]
        public async Task EnqueueAsync_WithNoCarrierList_PassesANullCarriedHandler()
        {
            QfcQueue queue = NewHeadlessQueue();

            await EnqueuePageAsync(queue, NewMailItems(1), null);

            _controllerCalls.Should().ContainSingle();
            _controllerCalls[0].CarriedHandler.Should().BeNull();
        }

        /// <summary>
        /// Every one of the nine arguments the loader passes to the item-controller factory carries
        /// the value the queue holds for it.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WithOneItem_PassesEveryControllerArgumentThrough()
        {
            QfcQueue queue = NewHeadlessQueue();
            IList<MailItem> items = NewMailItems(1);

            await EnqueuePageAsync(queue, items);

            _controllerCalls.Should().ContainSingle();
            ItemControllerCall call = _controllerCalls[0];
            call.Globals.Should().BeSameAs(_globals.Object);
            call.HomeController.Should().BeNull();
            call.CollectionController.Should().BeSameAs(_collectionController.Object);
            call.Viewer.Should().BeNull();
            call.Position.Should().Be(1);
            call.Digits.Should().Be(1);
            call.MailItem.Should().BeSameAs(items[0]);
            call.TlpStates.Should().BeSameAs(_tlpStates);
            call.CarriedHandler.Should().BeNull();
        }

        /// <summary>The item controller built for each row is initialized exactly once.</summary>
        [TestMethod]
        public async Task EnqueueAsync_WithThreeItems_AwaitsInitializeOncePerRow()
        {
            QfcQueue queue = NewHeadlessQueue();

            await EnqueuePageAsync(queue, NewMailItems(3));

            _controllerMocks.Should().HaveCount(3);
            foreach (Mock<IQfcItemController> controller in _controllerMocks)
            {
                controller.Verify(x => x.InitializeAsync(), Times.Once);
            }
        }

        /// <summary>
        /// With a non-zero start the loader maps the single supplied item onto that index and
        /// widens the digit count, because the start offset counts toward the ten-item boundary.
        /// </summary>
        [TestMethod]
        public async Task LoadControllersViewersAsync_WithNonZeroStart_MapsIndexAndWidensDigits()
        {
            QfcQueue queue = NewHeadlessQueue();
            IList<MailItem> items = NewMailItems(1);

            await InvokeLoadControllersViewersAsync(queue, items, start: 9);

            _itemGroupCalls.Should().ContainSingle();
            _itemGroupCalls[0].Mail.Should().BeSameAs(items[0]);
            _itemGroupCalls[0].Index.Should().Be(9);
            _controllerCalls.Should().ContainSingle();
            _controllerCalls[0].Digits.Should().Be(2);
        }

        /// <summary>
        /// The production row builder is exercised rather than displaced: it asks the viewer
        /// factory for a viewer using the queue's own token and hands that viewer, the panel and
        /// the index to the row placer.
        /// </summary>
        [TestMethod]
        public async Task AddAsync_WithSubstitutedViewerSeams_BuildsTheGroupAndPlacesTheViewer()
        {
            QfcQueue queue = NewProductionItemGroupQueue();
            MailItem mailItem = NewMailItem("row");

            QfcItemGroup group = await queue.AddAsync(_sentinelTlp, mailItem, indexNumber: 4);

            group.MailItem.Should().BeSameAs(mailItem);
            _viewerFactoryTokens.Should().ContainSingle();
            _viewerFactoryTokens[0].Should().Be(_tokenSource.Token);
            _rowPlacements.Should().ContainSingle();
            _rowPlacements[0].Tlp.Should().BeSameAs(_sentinelTlp);
            _rowPlacements[0].Viewer.Should().BeSameAs(_stubViewer);
            _rowPlacements[0].Index.Should().Be(4);
        }

        /// <summary>
        /// With the item-group seam left at its default, one enqueue call routes through all three
        /// marshalling shapes exactly once each and still queues its page.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WithDefaultItemGroupFactory_UsesEachDispatcherShapeOnce()
        {
            QfcQueue queue = NewProductionItemGroupQueue();

            await EnqueuePageAsync(queue, NewMailItems(1));

            _dispatcher.ActionInvocations.Should().Be(1);
            _dispatcher.FuncInvocations.Should().Be(1);
            _dispatcher.AsyncFuncInvocations.Should().Be(1);
            queue.Count.Should().Be(1);
        }

        /// <summary>
        /// The panel the background-template seam returns reaches the dequeued entry unmodified, so
        /// the value flows through rather than being rebuilt.
        /// </summary>
        [TestMethod]
        public async Task EnqueueAsync_WithSubstitutedTemplate_FlowsThatPanelToTheDequeuedEntry()
        {
            QfcQueue queue = NewHeadlessQueue();
            queue.BackgroundTlpFactory = _ => _alternateTlp;

            await EnqueuePageAsync(queue, NewMailItems(1));

            queue.Dequeue().Tlp.Should().BeSameAs(_alternateTlp);
        }
    }
}
