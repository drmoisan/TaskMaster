using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Tests for the <see cref="QfcFormController.Cleanup"/> teardown path described by issue #731
    /// finding 2. Before the fix, <c>Cleanup()</c> disposed the undo queue outright while the undo
    /// consumer was still parked on it, so the consumer's next loop iteration faulted with an
    /// <see cref="ObjectDisposedException"/>. The fix signals the consumer with
    /// <c>CompleteAdding()</c> and defers <c>Dispose()</c> onto a continuation on the consumer.
    /// The arrangement is local to this class; <c>QfcFormControllerSeamTests.cs</c> is frozen. No
    /// live Outlook COM object, no shown WinForms form, no temporary file and no wall-clock wait is
    /// used: the clock is a <see cref="FakeTimeProvider"/>, the consumer is started inline, and the
    /// deferred disposal is observed through the handle the fix assigns to <c>_undoQueueDisposal</c>.
    /// </summary>
    [TestClass]
    public class QfcFormControllerCleanupTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        private const string UndoQueueField = "_undoQueue";
        private const string UndoConsumerTaskField = "_undoConsumerTask";
        private const string UndoQueueDisposalField = "_undoQueueDisposal";

        private Mock<IApplicationGlobals> _mockGlobals;
        private Mock<IAppAutoFileObjects> _mockAutoFileObjects;
        private Mock<IQfcFormViewer> _mockFormViewer;
        private Mock<IQfcQueue> _mockQfcQueue;
        private Mock<IQfcHomeController> _mockParent;
        private CancellationTokenSource _tokenSource;

        [TestInitialize]
        public void Setup()
        {
            _mockGlobals = new Mock<IApplicationGlobals>();
            _mockAutoFileObjects = new Mock<IAppAutoFileObjects>();
            _mockGlobals.Setup(g => g.AF).Returns(_mockAutoFileObjects.Object);
            _mockFormViewer = new Mock<IQfcFormViewer>();
            _mockQfcQueue = new Mock<IQfcQueue>();
            _mockParent = new Mock<IQfcHomeController>();
            _tokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Builds a controller through the eight-argument public constructor, with the three issue
        /// #448 seams replaced so the undo consumer runs inline against a fake clock.
        /// </summary>
        private QfcFormController CreateController(FakeTimeProvider clock)
        {
            var controller = new QfcFormController(
                _mockGlobals.Object,
                _mockFormViewer.Object,
                _mockQfcQueue.Object,
                QfEnums.InitTypeEnum.Sort,
                () => { },
                _mockParent.Object,
                _tokenSource,
                _tokenSource.Token
            );

            controller.TimeProvider = clock;
            controller.UndoConsumerStarter = body => body();
            controller.UndoItemProcessor = _ => Task.CompletedTask;
            return controller;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            field
                .Should()
                .NotBeNull(
                    because: "issue #731 finding 2 requires the private field {0} to exist on {1}",
                    fieldName,
                    target.GetType().Name
                );
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, PrivateInstance);
            field
                .Should()
                .NotBeNull(
                    because: "issue #731 finding 2 requires the private field {0} to exist on {1}",
                    fieldName,
                    target.GetType().Name
                );
            field.SetValue(target, value);
        }

        private static string ReadDisposalPartialSource()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (
                directory != null
                && !Directory.Exists(Path.Combine(directory.FullName, "QuickFiler"))
            )
            {
                directory = directory.Parent;
            }

            directory
                .Should()
                .NotBeNull(
                    because: "issue #731 finding 2 source-inspection tests must run under the "
                        + "repository working tree"
                );

            string path = Path.Combine(
                directory.FullName,
                "QuickFiler",
                "Controllers",
                "QfcFormController.SetupDisposal.cs"
            );
            return File.ReadAllText(path);
        }

        private static string NormalizeWhitespace(string text)
        {
            var builder = new System.Text.StringBuilder(text.Length);
            bool inWhitespaceRun = false;
            foreach (char character in text)
            {
                if (char.IsWhiteSpace(character))
                {
                    inWhitespaceRun = true;
                    continue;
                }

                if (inWhitespaceRun && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                inWhitespaceRun = false;
                builder.Append(character);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Scenario: a consumer is parked on the undo queue when Cleanup() runs, and the idle
        /// threshold then elapses. Expected outcome: the consumer exits its loop normally rather
        /// than faulting on a disposed queue.
        /// </summary>
        [TestMethod]
        public async Task Cleanup_WithRunningConsumer_ConsumerReachesRanToCompletion()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = CreateController(clock);
            Task consumer = controller.UndoConsumerStarter(controller.UndoConsumer);
            SetPrivateField(controller, UndoConsumerTaskField, consumer);

            // Act
            controller.Cleanup();
            clock.Advance(TimeSpan.FromSeconds(11));
            await Task.WhenAny(consumer).ConfigureAwait(false);
            AggregateException observed = consumer.Exception;

            // Assert
            consumer
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    because: "issue #731 finding 2 requires the consumer to reach RanToCompletion; "
                        + "observed fault was {0}",
                    observed
                );
        }

        /// <summary>
        /// Scenario: Cleanup() runs while a consumer is parked. Expected outcome: adding is
        /// completed but the queue is not yet disposed, which is the ordering the fix introduces.
        /// </summary>
        [TestMethod]
        public async Task Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = CreateController(clock);
            var queue = GetPrivateField<BlockingCollection<IMovedMailInfo>>(
                controller,
                UndoQueueField
            );
            Task consumer = controller.UndoConsumerStarter(controller.UndoConsumer);
            SetPrivateField(controller, UndoConsumerTaskField, consumer);

            // Act
            controller.Cleanup();

            // Assert
            queue
                .IsAddingCompleted.Should()
                .BeTrue(
                    because: "issue #731 finding 2 requires CompleteAdding() to signal the consumer "
                        + "before the queue is disposed"
                );
            Action take = () => queue.TryTake(out _);
            take.Should()
                .NotThrow(
                    because: "issue #731 finding 2 requires disposal to be deferred until the "
                        + "consumer has exited, so the queue is still usable here"
                );

            // Drain the deferred disposal so no parked consumer outlives the test.
            clock.Advance(TimeSpan.FromSeconds(11));
            await Task.WhenAny(consumer).ConfigureAwait(false);
            _ = consumer.Exception;
        }

        /// <summary>
        /// Scenario: Cleanup() runs with no consumer task in flight. Expected outcome: the queue is
        /// disposed immediately and no exception escapes.
        /// </summary>
        [TestMethod]
        public void Cleanup_WithNullConsumerTask_DisposesQueueAndDoesNotThrow()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = CreateController(clock);
            var queue = GetPrivateField<BlockingCollection<IMovedMailInfo>>(
                controller,
                UndoQueueField
            );
            SetPrivateField(controller, UndoConsumerTaskField, null);

            // Act
            Action cleanup = () => controller.Cleanup();

            // Assert
            cleanup
                .Should()
                .NotThrow(
                    because: "issue #731 finding 2 requires Cleanup() to succeed when no consumer "
                        + "is in flight"
                );
            Action take = () => queue.TryTake(out _);
            take.Should()
                .Throw<ObjectDisposedException>(
                    because: "issue #731 finding 2 requires the queue to be disposed immediately "
                        + "when there is no consumer to wait for"
                );
        }

        /// <summary>
        /// Scenario: Cleanup() runs while a consumer is parked. Expected outcome: Cleanup() returns
        /// without the consumer having completed, proving it did not block on the consumer.
        /// </summary>
        [TestMethod]
        public async Task Cleanup_WithParkedConsumer_ReturnsWithoutWaiting()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = CreateController(clock);
            Task consumer = controller.UndoConsumerStarter(controller.UndoConsumer);
            SetPrivateField(controller, UndoConsumerTaskField, consumer);

            // Act
            controller.Cleanup();

            // Assert
            consumer
                .IsCompleted.Should()
                .BeFalse(
                    because: "issue #731 finding 2 requires the teardown path never to block on the "
                        + "undo consumer"
                );

            // Drain the deferred disposal so no parked consumer outlives the test.
            clock.Advance(TimeSpan.FromSeconds(11));
            await Task.WhenAny(consumer).ConfigureAwait(false);
            _ = consumer.Exception;
        }

        /// <summary>
        /// Scenario: Cleanup() is invoked twice on the same controller. Expected outcome: the second
        /// call does not throw, because CompleteAdding() on the already-disposed queue is caught.
        /// </summary>
        [TestMethod]
        public void Cleanup_CalledTwice_DoesNotThrow()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = CreateController(clock);
            SetPrivateField(controller, UndoConsumerTaskField, null);
            controller.Cleanup();

            // Act
            Action secondCleanup = () => controller.Cleanup();

            // Assert
            secondCleanup
                .Should()
                .NotThrow(
                    because: "issue #731 finding 2 requires a repeated Cleanup() to be safe; the "
                        + "first call disposes the queue without nulling the field, so the second "
                        + "re-enters CompleteAdding() on a disposed BlockingCollection"
                );
        }

        /// <summary>
        /// Scenario: the consumer has already faulted when Cleanup() runs. Expected outcome: the
        /// deferred continuation reads and logs the fault, completes successfully itself, and still
        /// disposes the queue.
        /// </summary>
        [TestMethod]
        public async Task Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault()
        {
            // Arrange
            var clock = new FakeTimeProvider();
            QfcFormController controller = CreateController(clock);
            var queue = GetPrivateField<BlockingCollection<IMovedMailInfo>>(
                controller,
                UndoQueueField
            );
            Task planted = Task.FromException(
                new InvalidOperationException("planted consumer fault for issue #731 finding 2")
            );
            SetPrivateField(controller, UndoConsumerTaskField, planted);

            // Act
            controller.Cleanup();
            var disposal = GetPrivateField<Task>(controller, UndoQueueDisposalField);
            await Task.WhenAny(disposal).ConfigureAwait(false);

            // Assert
            disposal
                .Status.Should()
                .Be(
                    TaskStatus.RanToCompletion,
                    because: "issue #731 finding 2 requires the deferred disposal continuation to "
                        + "read and log the antecedent fault without faulting itself; observed "
                        + "fault was {0}",
                    disposal.Exception
                );
            Action take = () => queue.TryTake(out _);
            take.Should()
                .Throw<ObjectDisposedException>(
                    because: "issue #731 finding 2 requires the continuation to dispose the queue "
                        + "even when the consumer faulted"
                );
            planted
                .IsFaulted.Should()
                .BeTrue(
                    because: "issue #731 finding 2 requires the fault the continuation read to be "
                        + "the planted one"
                );
            planted
                .Exception.Should()
                .NotBeNull(
                    because: "issue #731 finding 2 requires the planted fault to remain observable"
                );
        }

        /// <summary>
        /// Scenario: inspect the disposal partial's source. Expected outcome: the teardown path
        /// contains no synchronous wait of any form. This is a forward guard, not a reproduction:
        /// all four literals are already absent from that file before the fix.
        /// </summary>
        [TestMethod]
        public void Cleanup_SourceContainsNoSynchronousWait()
        {
            // Arrange
            string[] bannedLiterals = new[] { ".Wait(", ".Result", "Thread.Sleep", "Task.Delay" };

            // Act
            string normalized = NormalizeWhitespace(ReadDisposalPartialSource());

            // Assert
            foreach (string banned in bannedLiterals)
            {
                normalized
                    .Should()
                    .NotContain(
                        banned,
                        because: "issue #731 finding 2 requires the teardown path to contain no "
                            + "synchronous wait, and {0} is a synchronous wait",
                        banned
                    );
            }
        }
    }
}
