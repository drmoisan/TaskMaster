using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #736 findings 2, 4, and 5: sibling partial of
    /// <see cref="EfcFormControllerTests"/> carrying the keyboard-dispatch, breadcrumb-bind, and
    /// default-sink regression tests. It exists as a separate file because
    /// <c>EfcFormControllerTests.cs</c> is 485 lines against the repository's 500-line ceiling; the
    /// helpers <c>CreateMinimalController()</c> and <c>SetPrivateField</c> are reused from that file
    /// rather than duplicated. The <c>[TestClass]</c> attribute is deliberately absent here, because
    /// it is already applied to the other part of this partial class.
    /// </summary>
    public partial class EfcFormControllerTests
    {
        /// <summary>
        /// Finding 2, <c>Func&lt;Task&gt;</c> overload. The all-fields-null state makes the
        /// keyboard-dialog toggle fault, which is the same fault-injection technique the existing
        /// async-void boundary test uses. The overload must contain that fault and report it once
        /// through the controller's own boundary, not let it travel three frames up into the
        /// coverage-exempt keyboard handler.
        /// </summary>
        [TestMethod]
        public async Task KbdExecuteAsync_FuncTaskOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                var dispatchCount = 0;
                Func<Task> dispatched = () =>
                {
                    dispatchCount++;
                    return Task.CompletedTask;
                };

                // Act
                Func<Task> act = () => controller.KbdExecuteAsync(dispatched);

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        "the keyboard dispatch overload must contain the fault, not rethrow it"
                    );
                sinkCallCount.Should().Be(1, "the fault must be reported exactly once");
                dispatchCount
                    .Should()
                    .Be(0, "the toggle faults before the dispatched action is reached");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 2, <c>System.Action</c> overload. This is a two-member family and a single test
        /// covers only one of them, so the sibling overload carries its own case.
        /// </summary>
        [TestMethod]
        public async Task KbdExecuteAsync_ActionOverload_WhenToggleFaults_ReportsOnceAndDoesNotThrow()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                var dispatchCount = 0;
                System.Action dispatched = () => dispatchCount++;

                // Act
                Func<Task> act = () => controller.KbdExecuteAsync(dispatched);

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        "the synchronous dispatch overload must contain the fault, not rethrow it"
                    );
                sinkCallCount.Should().Be(1, "the fault must be reported exactly once");
                dispatchCount
                    .Should()
                    .Be(0, "the toggle faults before the dispatched action is reached");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 4 support: the null-sink branch of <c>TryReportBoundaryFault</c>. A null sink
        /// must fall back to the static logger rather than reinstating the unobserved-fault
        /// behaviour the boundary exists to prevent.
        /// </summary>
        [TestMethod]
        public async Task KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                controller.BoundaryErrorSink = null;
                Func<Task> dispatched = () => Task.CompletedTask;

                // Act
                Func<Task> act = () => controller.KbdExecuteAsync(dispatched);

                // Assert
                await act.Should()
                    .NotThrowAsync("a null sink must not turn a contained fault into a rethrow");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 4 support: the throwing-sink branch of <c>TryReportBoundaryFault</c>. A sink that
        /// itself throws must be absorbed by the reporter, leaving the dispatch boundary quiet.
        /// </summary>
        [TestMethod]
        public async Task KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                controller.BoundaryErrorSink = (message, exception) =>
                    throw new InvalidOperationException("the sink itself failed");
                Func<Task> dispatched = () => Task.CompletedTask;

                // Act
                Func<Task> act = () => controller.KbdExecuteAsync(dispatched);

                // Assert
                await act.Should()
                    .NotThrowAsync("a throwing sink must not escape the dispatch boundary");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 2, cancellation classification. Cancellation is not a fault: an
        /// <see cref="OperationCanceledException"/> raised inside the guarded body is recorded at
        /// debug level and is deliberately not reported through the sink, matching the existing
        /// distinction in <c>BindBreadcrumbRowsAsync</c>.
        /// </summary>
        [TestMethod]
        public async Task RunKbdGuardedAsync_WhenBodyThrowsOperationCanceled_DoesNotReportAsFault()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                Func<Task> body = () => throw new OperationCanceledException();

                // Act
                Func<Task> act = () => controller.RunKbdGuardedAsync(body);

                // Assert
                await act.Should()
                    .NotThrowAsync("cancellation must not propagate out of the guard");
                sinkCallCount
                    .Should()
                    .Be(0, "cancellation is not a fault and must not be reported as one");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 2, fault classification. Every exception other than cancellation is a fault and
        /// must be reported through the boundary exactly once — not zero times, and not twice.
        /// </summary>
        [TestMethod]
        public async Task RunKbdGuardedAsync_WhenBodyThrowsInvalidOperation_ReportsExactlyOnce()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                Func<Task> body = () =>
                    throw new InvalidOperationException("the dispatched action failed");

                // Act
                Func<Task> act = () => controller.RunKbdGuardedAsync(body);

                // Assert
                await act.Should()
                    .NotThrowAsync("the guard must contain the fault, not rethrow it");
                sinkCallCount.Should().Be(1, "the fault must be reported exactly once");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 5, the read at the breadcrumb bind boundary. The negative sibling of
        /// <c>Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter</c>: when the
        /// archive-root read throws the documented <see cref="InvalidOperationException"/>, the bind
        /// must still not throw, and the fault must reach the controller's boundary reporter rather
        /// than only a log line.
        /// </summary>
        [TestMethod]
        public async Task BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow()
        {
            // Arrange: strict interface seams keep this binding-boundary test independent of
            // WinForms, WebView2, Outlook COM, and a UI pump.
            var ol = new Mock<IOlObjects>(MockBehavior.Strict);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            ol.SetupGet(value => value.ArchiveRootPath)
                .Throws(new InvalidOperationException("the archive root is unresolvable"));
            globals.SetupGet(value => value.Ol).Returns(ol.Object);

            var controller = CreateMinimalController();
            SetPrivateField(controller, "_globals", globals.Object);
            var sinkCallCount = 0;
            controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;

            // Act
            Func<Task> act = () => controller.BindBreadcrumbRowsAsync(new[] { @"Clients\North" });

            // Assert
            await act.Should()
                .NotThrowAsync("the bind boundary must contain the fault, not rethrow it");
            sinkCallCount
                .Should()
                .Be(
                    1,
                    "the breadcrumb bind must report through the controller's boundary reporter"
                        + " exactly once, not merely write a log line"
                );
            ol.VerifyGet(value => value.ArchiveRootPath, Times.Once);
        }

        /// <summary>
        /// Finding 4: the default <c>BoundaryErrorSink</c> must surface a fault to the user through
        /// the injectable notifier in addition to logging it, rather than writing a log line only.
        /// The test is a synchronous method so the <c>AsyncLocal</c> value it installs is visible to
        /// the invocation under test.
        /// </summary>
        [TestMethod]
        public void BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier()
        {
            // Arrange
            System.Action<string> previousNotifier = EfcFormController.UserFaultNotifier;
            var captured = new List<string>();
            EfcFormController.UserFaultNotifier = message => captured.Add(message);
            try
            {
                var controller = CreateMinimalController();

                // Act: the default sink is left in place, so this covers its body.
                controller.BoundaryErrorSink(
                    "boundary fault",
                    new InvalidOperationException("the boundary failed")
                );

                // Assert
                captured
                    .Should()
                    .ContainSingle(
                        "the default sink must report the fault to the user exactly once, not"
                            + " merely write a log line"
                    );
            }
            finally
            {
                EfcFormController.UserFaultNotifier = previousNotifier;
            }
        }

        /// <summary>
        /// Finding 4, non-blocking constraint. A modal surface would hang the test host, so the
        /// default delegate must return to its caller. Elapsed time is measured with a
        /// <see cref="Stopwatch"/> rather than with any sleep or delay.
        /// </summary>
        [TestMethod]
        public void BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread()
        {
            // Arrange
            var controller = CreateMinimalController();
            var stopwatch = Stopwatch.StartNew();

            // Act
            System.Action act = () =>
                controller.BoundaryErrorSink(
                    "boundary fault",
                    new InvalidOperationException("the boundary failed")
                );

            // Assert
            act.Should().NotThrow("the default sink must be safe on a released controller");
            stopwatch.Stop();
            stopwatch
                .Elapsed.Should()
                .BeLessThan(
                    TimeSpan.FromSeconds(1),
                    "the default user-facing surface must not block the calling thread"
                );
        }

        /// <summary>
        /// Finding 2, positive flow. Every other test of the containment guard drives it with a
        /// faulting body, so without this case the guard has never been observed letting a normal
        /// call through: a guard that silently swallowed its body would pass all of them. Asserts
        /// the body ran and that nothing was reported.
        /// </summary>
        [TestMethod]
        public async Task RunKbdGuardedAsync_WhenBodyCompletes_InvokesBodyAndReportsNothing()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                var bodyCallCount = 0;
                Func<Task> body = () =>
                {
                    bodyCallCount++;
                    return Task.CompletedTask;
                };

                // Act
                Func<Task> act = () => controller.RunKbdGuardedAsync(body);

                // Assert
                await act.Should().NotThrowAsync("a body that completes must not raise anything");
                bodyCallCount.Should().Be(1, "the guard must invoke the body it was handed");
                sinkCallCount
                    .Should()
                    .Be(0, "a successful call is not a fault and must not be reported as one");
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 2, positive flow on the <c>Func&lt;Task&gt;</c> overload. With a succeeding
        /// keyboard-dialog toggle the overload must go on to await the dispatched action rather
        /// than stopping at the toggle, and must report nothing.
        /// </summary>
        [TestMethod]
        public async Task KbdExecuteAsync_FuncTaskOverload_WhenToggleSucceeds_AwaitsTheAction()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var handler = AttachSucceedingKeyboardHandler(controller);
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                var dispatchCount = 0;
                Func<Task> dispatched = () =>
                {
                    dispatchCount++;
                    return Task.CompletedTask;
                };

                // Act
                Func<Task> act = () => controller.KbdExecuteAsync(dispatched);

                // Assert
                await act.Should().NotThrowAsync("a succeeding toggle must not raise anything");
                dispatchCount
                    .Should()
                    .Be(1, "the dispatched action must be awaited once the toggle succeeds");
                sinkCallCount.Should().Be(0, "a successful dispatch must report nothing");
                handler.Verify(
                    keyboard => keyboard.ToggleKeyboardDialogAsync(),
                    Times.Once,
                    "the overload must toggle the keyboard dialog exactly once"
                );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Finding 2, positive flow on the <c>System.Action</c> overload. This is a two-member
        /// family, so the synchronous sibling carries its own success case.
        /// </summary>
        [TestMethod]
        public async Task KbdExecuteAsync_ActionOverload_WhenToggleSucceeds_InvokesTheAction()
        {
            // Arrange
            SynchronizationContext previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateMinimalController();
                var handler = AttachSucceedingKeyboardHandler(controller);
                var sinkCallCount = 0;
                controller.BoundaryErrorSink = (message, exception) => sinkCallCount++;
                var dispatchCount = 0;
                System.Action dispatched = () => dispatchCount++;

                // Act
                Func<Task> act = () => controller.KbdExecuteAsync(dispatched);

                // Assert
                await act.Should().NotThrowAsync("a succeeding toggle must not raise anything");
                dispatchCount
                    .Should()
                    .Be(1, "the dispatched action must run once the toggle succeeds");
                sinkCallCount.Should().Be(0, "a successful dispatch must report nothing");
                handler.Verify(
                    keyboard => keyboard.ToggleKeyboardDialogAsync(),
                    Times.Once,
                    "the overload must toggle the keyboard dialog exactly once"
                );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// Installs a keyboard handler whose toggle succeeds onto a minimally constructed
        /// controller, so the two success-path overload tests share one arrangement instead of
        /// repeating it. The home controller is allocated uninitialized — the same technique the
        /// viewer seams in the sibling file use — because its constructor requires a live Outlook
        /// context; only its public <c>KeyboardHandler</c> property is set. The strict mock is
        /// returned so each caller asserts its own call count.
        /// </summary>
        private static Mock<IQfcKeyboardHandler> AttachSucceedingKeyboardHandler(
            EfcFormController controller
        )
        {
            var handler = new Mock<IQfcKeyboardHandler>(MockBehavior.Strict);
            handler
                .Setup(keyboard => keyboard.ToggleKeyboardDialogAsync())
                .Returns(Task.CompletedTask);
            var homeController = (EfcHomeController)
                System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                    typeof(EfcHomeController)
                );
            homeController.KeyboardHandler = handler.Object;
            SetPrivateField(controller, "_homeController", homeController);
            return handler;
        }
    }
}
