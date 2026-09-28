using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
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
    /// Shared harness part of the issue #871 enqueue-path regression suite. It carries every piece
    /// of arrangement the test-class part uses: a factory building a real <see cref="QfcQueue"/>
    /// through its real primary constructor, a hand-written synchronous fake for the UI-idle
    /// dispatcher seam, recording substitutes for the item-group, item-controller, viewer,
    /// row-placer and background-template seams, and the generic seam-contract helper. The
    /// dispatcher fake is hand-written rather than built with Moq because two of the three
    /// interface members are generic methods whose return type depends on the type parameter. No
    /// temporary file, no filesystem, no network, no Outlook process, no sleep and no real
    /// wall-clock wait appears in either part.
    /// </summary>
    public partial class QfcQueueEnqueueTests
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly Mock<IApplicationGlobals> _globals = new Mock<IApplicationGlobals>();
        private readonly Mock<IEmailMoveMonitor> _moveMonitor = new Mock<IEmailMoveMonitor>();
        private readonly Mock<IQfcCollectionController> _collectionController =
            new Mock<IQfcCollectionController>();
        private readonly SynchronousIdleDispatcher _dispatcher = new SynchronousIdleDispatcher();
        private readonly TableLayoutPanel _sentinelTlp = new TableLayoutPanel();

        /// <summary>
        /// A second panel, distinguishable from the sentinel by reference, for the test that proves
        /// the background-template seam's return value is what reaches the queue entry. It is built
        /// here rather than in a test body because constructing a control installs a WinForms
        /// synchronization context on the constructing thread, and the context this class's
        /// <c>Initialize</c> method clears is the one installed by these field initializers.
        /// </summary>
        private readonly TableLayoutPanel _alternateTlp = new TableLayoutPanel();
        private readonly TlpCellStates _tlpStates = new TlpCellStates();
        private readonly ItemViewer _stubViewer = (ItemViewer)
            FormatterServices.GetUninitializedObject(typeof(ItemViewer));

        private readonly List<(TableLayoutPanel Tlp, MailItem Mail, int Index)> _itemGroupCalls =
            new List<(TableLayoutPanel, MailItem, int)>();
        private readonly List<ItemControllerCall> _controllerCalls = new List<ItemControllerCall>();
        private readonly List<Mock<IQfcItemController>> _controllerMocks =
            new List<Mock<IQfcItemController>>();
        private readonly List<CancellationToken> _viewerFactoryTokens =
            new List<CancellationToken>();
        private readonly List<(TableLayoutPanel Tlp, ItemViewer Viewer, int Index)> _rowPlacements =
            new List<(TableLayoutPanel, ItemViewer, int)>();

        /// <summary>
        /// Exception the recording item-group factory raises after recording its call, or null when
        /// it should succeed. The throw is raised from inside the enqueue member's try block.
        /// </summary>
        private System.Exception _itemGroupFailure;

        /// <summary>
        /// Observer the recording item-group factory invokes mid-flight, used to sample queue state
        /// while the enqueue member is still running.
        /// </summary>
        private System.Action _itemGroupObserver;

        /// <summary>
        /// Detaches any synchronization context from the test thread before the test body runs.
        /// Constructing the sentinel <see cref="TableLayoutPanel"/> in this class's field
        /// initializers installs a <c>WindowsFormsSynchronizationContext</c> on the current thread,
        /// and the enqueue path's first genuinely asynchronous await then posts its continuation
        /// back to that thread, which no unit-test host pumps; the await would never resume. Field
        /// initializers run before this method, so clearing the context here removes the captured
        /// context for every await the test performs and continuations complete on the thread pool.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            SynchronizationContext.SetSynchronizationContext(null);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _tokenSource.Dispose();
            _sentinelTlp.Dispose();
            _alternateTlp.Dispose();
        }

        /// <summary>
        /// Builds a real queue through the real primary constructor with every seam left at its
        /// production default: the token is drawn from the harness's own cancellation token source,
        /// the concrete home controller is a literal null and the globals are a loose mock,
        /// mirroring the construction pattern the existing coverage-expansion tests use.
        /// </summary>
        private QfcQueue NewQueue() =>
            new QfcQueue(_tokenSource.Token, (QfcHomeController)null, _globals.Object);

        /// <summary>
        /// A queue whose seams are substituted so the enqueue path runs headless: the move monitor
        /// and dispatcher are replaced, the background template returns the sentinel panel, and the
        /// item-group and item-controller factories record their arguments.
        /// </summary>
        private QfcQueue NewHeadlessQueue()
        {
            QfcQueue queue = NewQueue();
            queue.MoveMonitor = _moveMonitor.Object;
            queue.UiIdleDispatcher = _dispatcher;
            queue.BackgroundTlpFactory = _ => _sentinelTlp;
            queue.ItemGroupFactory = RecordItemGroup;
            queue.ItemControllerFactory = RecordItemController;
            queue.TlpStates = _tlpStates;
            return queue;
        }

        /// <summary>
        /// A queue substituted only far enough to stay headless while leaving
        /// <c>ItemGroupFactory</c> at its production default, so the production <c>AddAsync</c> body
        /// runs: the viewer factory returns an uninitialised stand-in and the row placer records
        /// instead of touching a live layout pass.
        /// </summary>
        private QfcQueue NewProductionItemGroupQueue()
        {
            QfcQueue queue = NewQueue();
            queue.MoveMonitor = _moveMonitor.Object;
            queue.UiIdleDispatcher = _dispatcher;
            queue.BackgroundTlpFactory = _ => _sentinelTlp;
            queue.ItemControllerFactory = RecordItemController;
            queue.ItemViewerFactory = RecordViewerRequest;
            queue.ViewerRowPlacer = RecordRowPlacement;
            queue.TlpStates = _tlpStates;
            return queue;
        }

        /// <summary>Enqueues one page through the member under test.</summary>
        private Task EnqueuePageAsync(
            QfcQueue queue,
            IList<MailItem> items,
            IList<QfcPreScoredItem> preScored = null
        ) => queue.EnqueueAsync(items, _collectionController.Object, preScored);

        /// <summary>Records the call, then either fails, observes, or returns a new group.</summary>
        private Task<QfcItemGroup> RecordItemGroup(
            TableLayoutPanel tlp,
            MailItem mailItem,
            int index
        )
        {
            _itemGroupCalls.Add((tlp, mailItem, index));
            _itemGroupObserver?.Invoke();
            if (_itemGroupFailure != null)
            {
                throw _itemGroupFailure;
            }

            return Task.FromResult(new QfcItemGroup(mailItem));
        }

        /// <summary>Records all nine arguments and returns a mock whose initialize completes.</summary>
        private IQfcItemController RecordItemController(
            IApplicationGlobals globals,
            IFilerHomeController homeController,
            IQfcCollectionController collectionController,
            IItemViewer viewer,
            int position,
            int digits,
            MailItem mailItem,
            TlpCellStates tlpStates,
            IFolderSearchHandler carriedHandler
        )
        {
            _controllerCalls.Add(
                new ItemControllerCall
                {
                    Globals = globals,
                    HomeController = homeController,
                    CollectionController = collectionController,
                    Viewer = viewer,
                    Position = position,
                    Digits = digits,
                    MailItem = mailItem,
                    TlpStates = tlpStates,
                    CarriedHandler = carriedHandler,
                }
            );

            Mock<IQfcItemController> controller = new Mock<IQfcItemController>();
            controller.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            _controllerMocks.Add(controller);
            return controller.Object;
        }

        private ItemViewer RecordViewerRequest(CancellationToken token)
        {
            _viewerFactoryTokens.Add(token);
            return _stubViewer;
        }

        private void RecordRowPlacement(TableLayoutPanel tlp, ItemViewer viewer, int indexNumber)
        {
            _rowPlacements.Add((tlp, viewer, indexNumber));
        }

        private static IList<MailItem> NewMailItems(int count)
        {
            List<MailItem> items = new List<MailItem>();
            for (int i = 0; i < count; i++)
            {
                items.Add(NewMailItem("mail-" + i));
            }

            return items;
        }

        private static MailItem NewMailItem(string entryId)
        {
            Mock<MailItem> mailItem = new Mock<MailItem>();
            mailItem.Setup(x => x.EntryID).Returns(entryId);
            return mailItem.Object;
        }

        /// <summary>
        /// Invokes the private per-page loader by reflection and awaits the value-task it returns.
        /// The member is private and has no public caller that accepts a start offset, so
        /// reflection is the only way to pin the index mapping for a non-zero start.
        /// </summary>
        private async Task<List<QfcItemGroup>> InvokeLoadControllersViewersAsync(
            QfcQueue queue,
            IList<MailItem> items,
            int start
        )
        {
            MethodInfo loader = typeof(QfcQueue).GetMethod(
                "LoadControllersViewersAsync",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            loader.Should().NotBeNull("the loader member must exist on the queue type");

            object result = loader.Invoke(
                queue,
                new object[]
                {
                    items,
                    _globals.Object,
                    null,
                    _collectionController.Object,
                    _sentinelTlp,
                    start,
                    null,
                }
            );

            return await ((ValueTask<List<QfcItemGroup>>)result).AsTask();
        }

        /// <summary>
        /// Drives one enqueue call whose substituted item-group factory raises
        /// <paramref name="failure"/> from inside the enqueue member's try block, and asserts the
        /// failure does not propagate, that nothing was queued, and that the running-jobs counter
        /// returned to 0 through the finally block. The throw is deliberately confined to the
        /// item-group factory: a throw from the background-template factory or the hook loop lies
        /// outside that try block and would require the separately promoted counter-leak behaviour
        /// to be treated as expected.
        /// </summary>
        private async Task AssertLoaderFailureIsContainedAsync(System.Exception failure)
        {
            QfcQueue queue = NewHeadlessQueue();
            _itemGroupFailure = failure;

            await queue
                .Awaiting(q => EnqueuePageAsync(q, NewMailItems(1)))
                .Should()
                .NotThrowAsync();

            _itemGroupCalls.Should().ContainSingle();
            queue.Count.Should().Be(0);
            queue.JobsRunning.Should().Be(0);
        }

        /// <summary>
        /// Asserts the contract every issue #871 seam carries: the getter yields a non-null
        /// production default and the setter rejects null with
        /// <see cref="ArgumentNullException"/>.
        /// </summary>
        private static void AssertSeamContract<T>(Func<T> getter, Action<T> setter)
            where T : class
        {
            getter().Should().NotBeNull("the seam must expose a non-null production default");
            setter.Invoking(assign => assign(null)).Should().Throw<ArgumentNullException>();
        }

        /// <summary>One recorded call of the substituted item-controller factory.</summary>
        private sealed class ItemControllerCall
        {
            internal IApplicationGlobals Globals { get; set; }
            internal IFilerHomeController HomeController { get; set; }
            internal IQfcCollectionController CollectionController { get; set; }
            internal IItemViewer Viewer { get; set; }
            internal int Position { get; set; }
            internal int Digits { get; set; }
            internal MailItem MailItem { get; set; }
            internal TlpCellStates TlpStates { get; set; }
            internal IFolderSearchHandler CarriedHandler { get; set; }
        }

        /// <summary>
        /// Hand-written synchronous fake for the UI-idle dispatcher seam: each of the three shapes
        /// invokes its argument inline on the calling thread and returns an already-completed task,
        /// so no process-wide dispatcher and no wall-clock wait is involved. Each shape counts its
        /// invocations so a test can assert which shapes the enqueue path used.
        /// </summary>
        private sealed class SynchronousIdleDispatcher : IUiIdleDispatcher
        {
            internal int ActionInvocations { get; private set; }
            internal int FuncInvocations { get; private set; }
            internal int AsyncFuncInvocations { get; private set; }

            public Task InvokeIdleAsync(System.Action action)
            {
                ActionInvocations++;
                action();
                return Task.CompletedTask;
            }

            public Task<T> InvokeIdleAsync<T>(Func<T> func)
            {
                FuncInvocations++;
                return Task.FromResult(func());
            }

            public Task<T> InvokeIdleAsync<T>(Func<Task<T>> func)
            {
                AsyncFuncInvocations++;
                return func();
            }
        }
    }
}
