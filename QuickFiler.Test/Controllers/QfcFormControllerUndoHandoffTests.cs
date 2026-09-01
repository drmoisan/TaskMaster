using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Interfaces;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.EmailParsingSorting;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Ordering tests for issue 633: the batch-move path must not reach the metrics dispatch or the
    /// cleanup dispatch while the filer queue still holds unprocessed work, because the undo pushes
    /// for that batch happen on the queue worker.
    /// </summary>
    /// <remarks>
    /// All concurrency here is driven by <see cref="TaskCompletionSource{TResult}"/> gates through the
    /// queue's <c>ItemProcessor</c> seam and by dispatcher queue order. There is no sleep, no delay, no
    /// polling loop, and no timeout-based assertion, as
    /// <c>.claude/rules/general-unit-test.md</c> requires.
    /// </remarks>
    [TestClass]
    public class QfcFormControllerUndoHandoffTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private const string MetricsToken = "metrics";
        private const string CleanupToken = "cleanup";

        private readonly List<string> _recorder = new List<string>();
        private readonly object _recorderLock = new object();

        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IAppAutoFileObjects> _mockAF;
        private Mock<IFileSystemFolderPaths> _mockFS;
        private Mock<IAppStagingFilenames> _mockFilenames;
        private Mock<IQfcFormViewer> _mockFormViewer;
        private Mock<IQfcQueue> _mockQfcQueue;
        private Mock<IQfcHomeController> _mockParent;
        private Mock<IQfcCollectionController> _mockGroups;
        private CancellationTokenSource _tokenSource;
        private FilerQueue _filerQueue;
        private TaskCompletionSource<bool> _gate;
        private TaskCompletionSource<bool> _processorEntered;

        [TestInitialize]
        public void Setup()
        {
            _recorder.Clear();

            _mockFilenames = new Mock<IAppStagingFilenames>();
            _mockFilenames.SetupGet(f => f.EmailSession).Returns("email-session");

            _mockFS = new Mock<IFileSystemFolderPaths>();
            _mockFS.SetupGet(f => f.Filenames).Returns(_mockFilenames.Object);

            _mockAF = new Mock<IAppAutoFileObjects>();
            _mockAF.SetupGet(a => a.MovedMails).Returns(new SloStack<IMovedMailInfo>());

            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockGlobals.SetupGet(g => g.FS).Returns(_mockFS.Object);
            _mockGlobals.SetupGet(g => g.AF).Returns(_mockAF.Object);

            _mockFormViewer = new Mock<IQfcFormViewer>();
            _mockQfcQueue = new Mock<IQfcQueue>();

            _mockGroups = new Mock<IQfcCollectionController>();
            _mockGroups
                .Setup(g => g.MoveEmailsAsync(It.IsAny<SloStack<IMovedMailInfo>>()))
                .Returns(Task.CompletedTask);
            _mockGroups.Setup(g => g.CleanupBackground()).Callback(() => Record(CleanupToken));

            _gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _processorEntered = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );

            _filerQueue = new FilerQueue();

            _mockParent = new Mock<IQfcHomeController>();
            _mockParent.SetupGet(p => p.FilerQueue).Returns(_filerQueue);

            _tokenSource = new CancellationTokenSource();
        }

        [TestCleanup]
        public void Teardown()
        {
            // Never leave a worker thread parked on the gate, whatever path the test exited by.
            _gate?.TrySetResult(true);
            _tokenSource?.Dispose();
        }

        /// <summary>
        /// Appends an ordered token. Metrics and cleanup run on the installed dispatcher's own STA
        /// thread while the assertions run on the test thread, so the list is guarded.
        /// </summary>
        private void Record(string token)
        {
            lock (_recorderLock)
            {
                _recorder.Add(token);
            }
        }

        private int CountOf(string token)
        {
            lock (_recorderLock)
            {
                return _recorder.FindAll(t => t == token).Count;
            }
        }

        private List<string> RecordedOrder()
        {
            lock (_recorderLock)
            {
                return new List<string>(_recorder);
            }
        }

        /// <summary>
        /// The instance method installed into the controller's private <c>WriteMetrics</c> field.
        /// It records as its very first statement and then returns an already-completed task; it must
        /// not await anything beforehand. The production call site wraps this in
        /// <c>async () =&gt; await WriteMetrics(...)</c>, so the dispatcher operation completes at that
        /// lambda's first suspension point. A delegate that suspended before recording would let the
        /// ordering probe complete with the count still at zero and destroy the discriminator.
        /// </summary>
        private Task RecordWriteMetrics(string filename)
        {
            Record(MetricsToken);
            return Task.CompletedTask;
        }

        private static T GetPrivateField<T>(object target, string fieldName) =>
            (T)target.GetType().GetField(fieldName, PrivateInstance).GetValue(target);

        private static void SetPrivateField<T>(object target, string fieldName, T value) =>
            target.GetType().GetField(fieldName, PrivateInstance).SetValue(target, value);

        private QfcFormController CreateQfcFormController()
        {
            return new QfcFormController(
                _mockGlobals.Object,
                _mockFormViewer.Object,
                _mockQfcQueue.Object,
                QfEnums.InitTypeEnum.Sort,
                () => { },
                _mockParent.Object,
                _tokenSource,
                _tokenSource.Token
            );
        }

        /// <summary>
        /// Installs the recording metrics delegate. The delegate type is declared <c>private</c> on
        /// <c>QfcFormController</c> and cannot be named from this assembly, so the field's
        /// <see cref="FieldInfo.FieldType"/> is read at run time and bound with
        /// <see cref="Delegate.CreateDelegate(Type, object, MethodInfo)"/>.
        /// </summary>
        private void InstallRecordingMetrics(QfcFormController controller)
        {
            FieldInfo field = typeof(QfcFormController).GetField("WriteMetrics", PrivateInstance);
            field.Should().NotBeNull(because: "QfcFormController declares a private WriteMetrics field");

            MethodInfo method = typeof(QfcFormControllerUndoHandoffTests).GetMethod(
                nameof(RecordWriteMetrics),
                PrivateInstance
            );
            method.Should().NotBeNull(because: "the recording metrics method must be resolvable");

            field.SetValue(controller, Delegate.CreateDelegate(field.FieldType, this, method));
        }

        /// <summary>
        /// Builds a controller with the collection controller injected and a recording metrics
        /// delegate installed, so that <c>BackGroundMoveAsync</c> passes its early-return guard.
        /// </summary>
        private QfcFormController CreateWiredController()
        {
            QfcFormController controller = CreateQfcFormController();
            SetPrivateField(controller, "_groups", _mockGroups.Object);
            InstallRecordingMetrics(controller);
            return controller;
        }

        /// <summary>
        /// Assigns a gated processor and enqueues exactly one item, then waits for the worker to have
        /// entered the processor. The wait is on a <see cref="TaskCompletionSource{TResult}"/> the
        /// processor itself signals, so it carries no timing assumption.
        /// </summary>
        private async Task EnqueueOneGatedItemAsync()
        {
            _filerQueue.ItemProcessor = async item =>
            {
                _processorEntered.TrySetResult(true);
                await _gate.Task;
            };

            _filerQueue.Enqueue(new EmailFiler(), new List<MailItemHelper> { new MailItemHelper() });
            await _processorEntered.Task;
        }

        /// <summary>
        /// The metrics dispatch must not have run while the queue still holds an unprocessed item.
        /// </summary>
        /// <remarks>
        /// Determinism comes from dispatcher queue order, not from elapsed time. The mocked
        /// <c>MoveEmailsAsync</c> returns an already-completed task, so before the fix the metrics
        /// operation is enqueued at <c>ContextIdle</c> synchronously, before the method returns to this
        /// caller. A WPF dispatcher runs equal-priority operations in enqueue order, so the probe posted
        /// below at the same priority cannot complete until that metrics operation has run. After the
        /// fix the method yields on the drain barrier and posts nothing.
        /// </remarks>
        [TestMethod]
        public async Task BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain()
        {
            using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())
            {
                Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
                try
                {
                    // Arrange
                    transaction.Install(dispatcher);
                    QfcFormController controller = CreateWiredController();
                    await EnqueueOneGatedItemAsync();

                    // Act
                    Task moveTask = controller.BackGroundMoveAsync();
                    await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);

                    // Assert: the gate is still closed, so the batch has not been filed.
                    CountOf(MetricsToken)
                        .Should()
                        .Be(
                            0,
                            "the barrier withholds the metrics dispatch until the queue has drained"
                        );

                    // Act: release the item and let the batch finish.
                    _gate.TrySetResult(true);
                    await moveTask;

                    // Assert
                    CountOf(MetricsToken)
                        .Should()
                        .Be(1, "metrics are written exactly once once the drain completes");
                }
                finally
                {
                    _gate.TrySetResult(true);
                    QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
                }
            }
        }

        /// <summary>
        /// The cleanup dispatch must not have run while the queue still holds an unprocessed item.
        /// </summary>
        /// <remarks>
        /// The metrics clause in the pre-release assertion is what makes this test fail deterministically
        /// before the fix, and it is a sound part of this test's own claim: production reaches the
        /// cleanup dispatch only through the metrics dispatch, so a metrics dispatch already made while
        /// the queue is undrained proves that no barrier is withholding the cleanup dispatch either.
        /// </remarks>
        [TestMethod]
        public async Task BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain()
        {
            using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())
            {
                Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
                try
                {
                    // Arrange
                    transaction.Install(dispatcher);
                    QfcFormController controller = CreateWiredController();
                    await EnqueueOneGatedItemAsync();

                    // Act
                    Task moveTask = controller.BackGroundMoveAsync();
                    await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);

                    // Assert: the gate is still closed, so neither downstream step may have run.
                    CountOf(CleanupToken)
                        .Should()
                        .Be(0, "cleanup is withheld until the queue has drained");
                    CountOf(MetricsToken)
                        .Should()
                        .Be(
                            0,
                            "cleanup is reached only through metrics, so a metrics dispatch made "
                                + "while the queue is undrained proves cleanup is unguarded too"
                        );

                    // Act: release the item and let the batch finish.
                    _gate.TrySetResult(true);
                    await moveTask;

                    // Assert
                    CountOf(CleanupToken)
                        .Should()
                        .Be(1, "cleanup runs exactly once once the drain completes");
                    RecordedOrder()
                        .Should()
                        .Equal(
                            new List<string> { MetricsToken, CleanupToken },
                            "metrics must still be written before cleanup resets the state they read"
                        );
                }
                finally
                {
                    _gate.TrySetResult(true);
                    QfcItemControllerTestSupport.ShutdownDispatcher(dispatcher);
                }
            }
        }
    }
}
