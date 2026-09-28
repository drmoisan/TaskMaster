using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #792 on <see cref="EfcFormController"/>. Every test builds a
    /// minimal controller through the private no-arg constructor, so no Outlook COM context and no
    /// WinForms window is required. The user-facing surface is observed through
    /// <see cref="EfcFormController.UserFaultNotifier"/>, which is per-async-flow storage, so a
    /// capture installed here is invisible to tests running in parallel.
    /// </summary>
    [TestClass]
    public sealed class EfcFormControllerIssue792Tests
    {
        /// <summary>
        /// Creates an EfcFormController via the private no-arg constructor, which allocates the
        /// object without initializing any sub-components, leaving all fields null.
        /// </summary>
        private static EfcFormController CreateMinimalController()
        {
            var ctor = typeof(EfcFormController).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );
            ctor.Should().NotBeNull("private no-arg constructor must exist on EfcFormController");
            return (EfcFormController)ctor.Invoke(Array.Empty<object>());
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }

        /// <summary>
        /// Routes <see cref="EfcFormController.UserFaultNotifier"/> for the current async flow into
        /// <paramref name="captured"/> and returns a scope that restores the previous value.
        /// </summary>
        private static IDisposable CaptureUserFaults(List<string> captured)
        {
            var previous = EfcFormController.UserFaultNotifier;
            EfcFormController.UserFaultNotifier = captured.Add;
            return new NotifierScope(previous);
        }

        private sealed class NotifierScope : IDisposable
        {
            private readonly Action<string> _previous;

            internal NotifierScope(Action<string> previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                EfcFormController.UserFaultNotifier = _previous;
            }
        }

        /// <summary>
        /// AC-U4, PopulateFolderCombobox half (strengthened user-surface test). The sink is left
        /// at its default so the fault must reach the user through the notifier; the existing
        /// call-count test cannot see whether the default sink notifies anyone. This test passes
        /// before the fix and is mutation-proven in Phase 5.
        /// </summary>
        [TestMethod]
        public async Task PopulateFolderCombobox_WhenDataModelFaults_NotifiesTheUserThroughTheDefaultSink()
        {
            // Arrange
            var controller = CreateMinimalController();
            var viewer = (EfcViewer)FormatterServices.GetUninitializedObject(typeof(EfcViewer));
            SetPrivateField(controller, "_formViewer", viewer);
            var captured = new List<string>();
            using (CaptureUserFaults(captured))
            {
                // Act
                Func<Task> act = () => controller.PopulateFolderCombobox();

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        "a fire-and-forget call site cannot observe a faulted Task, so the method"
                            + " must contain its own fault"
                    );
                captured
                    .Should()
                    .ContainSingle(
                        "the default boundary sink must surface the contained fault to the user"
                            + " exactly once"
                    );
            }
        }

        /// <summary>
        /// AC-U4, InitializeBreadcrumbHostAsync half. With no host, no router and no viewer, every
        /// attempt faults; after the attempt limit the failure must be reported through the
        /// boundary sink to the user, naming the attempt count. Before the fix the single attempt
        /// raises NullReferenceException, which the catch only logs, so nothing reaches the user.
        /// </summary>
        [TestMethod]
        public async Task InitializeBreadcrumbHostAsync_WhenHostIsNull_ReportsThroughTheBoundarySinkToTheUser()
        {
            // Arrange
            var controller = CreateMinimalController();
            var method = typeof(EfcFormController).GetMethod(
                "InitializeBreadcrumbHostAsync",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            method.Should().NotBeNull("InitializeBreadcrumbHostAsync must remain available");
            var captured = new List<string>();
            using (CaptureUserFaults(captured))
            {
                // Act
                Func<Task> act = () => (Task)method.Invoke(controller, Array.Empty<object>());

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        "the host initializer is fire-and-forget, so it must contain its own fault"
                    );
                captured
                    .Should()
                    .ContainSingle(
                        "the final initialization failure must be reported to the user exactly"
                            + " once"
                    )
                    .Which.Should()
                    .Contain(
                        "after 3 attempts",
                        "the report must name the exhausted attempt limit"
                    );
            }
        }

        /// <summary>
        /// Recording seam for <see cref="EfcFormController.BreadcrumbHostInitializer"/>. Every
        /// invocation is counted and resolves per the scripted outcomes: a scripted exception is
        /// returned as a faulted task, a null entry completes, and the last entry repeats once the
        /// script is exhausted. No invocation yields, so no continuation needs a pumped thread.
        /// </summary>
        private sealed class ScriptedInitializer
        {
            private readonly Exception[] _script;

            internal ScriptedInitializer(params Exception[] script)
            {
                _script = script;
            }

            internal int Invocations { get; private set; }

            internal Task InvokeAsync()
            {
                int index = Math.Min(Invocations, _script.Length - 1);
                Invocations++;
                Exception outcome = _script[index];
                return outcome == null ? Task.CompletedTask : Task.FromException(outcome);
            }
        }

        private static ScriptedInitializer InstallAlwaysFailingInitializer(
            EfcFormController controller
        )
        {
            var initializer = new ScriptedInitializer(new InvalidOperationException("boom"));
            controller.BreadcrumbHostInitializer = initializer.InvokeAsync;
            return initializer;
        }

        /// <summary>
        /// AC-U1: the host initializer is retried up to the attempt limit and the exhausted limit
        /// is reported to the user exactly once. Before the fix the seam is never consulted (the
        /// single attempt goes straight to the null host), so the invocation count is zero.
        /// </summary>
        [TestMethod]
        public async Task InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce()
        {
            // Arrange
            var controller = CreateMinimalController();
            var initializer = InstallAlwaysFailingInitializer(controller);
            var captured = new List<string>();
            using (CaptureUserFaults(captured))
            {
                // Act
                Func<Task> act = () => controller.InitializeBreadcrumbHostAsync();

                // Assert
                await act.Should().NotThrowAsync("the initializer must contain its own fault");
                initializer
                    .Invocations.Should()
                    .Be(
                        3,
                        "the host initializer must be attempted exactly the limit of three times"
                    );
                captured
                    .Should()
                    .ContainSingle("the exhausted limit must be reported to the user exactly once")
                    .Which.Should()
                    .Contain(
                        "after 3 attempts",
                        "the report must name the exhausted attempt limit"
                    );
            }
        }

        /// <summary>
        /// AC-U1: a failure followed by a success stops the loop after the second attempt and
        /// reports nothing. Before the fix the seam is never consulted, so the count is zero.
        /// </summary>
        [TestMethod]
        public async Task InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing()
        {
            // Arrange
            var controller = CreateMinimalController();
            var initializer = new ScriptedInitializer(new InvalidOperationException("boom"), null);
            controller.BreadcrumbHostInitializer = initializer.InvokeAsync;
            var captured = new List<string>();
            using (CaptureUserFaults(captured))
            {
                // Act
                Func<Task> act = () => controller.InitializeBreadcrumbHostAsync();

                // Assert
                await act.Should().NotThrowAsync("a successful retry must not surface anything");
                initializer
                    .Invocations.Should()
                    .Be(2, "the loop must stop on the first successful attempt");
                captured.Should().BeEmpty("a recovered initialization must not be reported");
            }
        }

        /// <summary>
        /// AC-U1: cancellation is not a fault. It stops the loop after the first attempt and is
        /// neither retried nor reported. Before the fix the seam is never consulted.
        /// </summary>
        [TestMethod]
        public async Task InitializeBreadcrumbHostAsync_WhenCanceled_DoesNotRetryOrReport()
        {
            // Arrange
            var controller = CreateMinimalController();
            var initializer = new ScriptedInitializer(new OperationCanceledException());
            controller.BreadcrumbHostInitializer = initializer.InvokeAsync;
            var captured = new List<string>();
            using (CaptureUserFaults(captured))
            {
                // Act
                Func<Task> act = () => controller.InitializeBreadcrumbHostAsync();

                // Assert
                await act.Should().NotThrowAsync("cancellation must be absorbed at the boundary");
                initializer.Invocations.Should().Be(1, "a canceled attempt must not be retried");
                captured.Should().BeEmpty("cancellation is not a fault and must not be reported");
            }
        }

        /// <summary>
        /// AC-U1 visible error state (D4): on final failure the folder-area label carries the
        /// failure text. Before the fix nothing writes the label, so its text stays empty.
        /// </summary>
        [TestMethod]
        public async Task InitializeBreadcrumbHostAsync_OnFinalFailure_ShowsTheErrorTextInTheFolderAreaLabel()
        {
            // Arrange
            var controller = CreateMinimalController();
            var viewer = (EfcViewer)FormatterServices.GetUninitializedObject(typeof(EfcViewer));
            var label = new System.Windows.Forms.Label();

            // Constructing a WinForms control installs WindowsFormsSynchronizationContext on this
            // thread; clear it so a genuine await in the code under test cannot post its
            // continuation to a thread that no test host pumps.
            SynchronizationContext.SetSynchronizationContext(null);
            SetPrivateField(viewer, "label2", label);
            SetPrivateField(controller, "_formViewer", viewer);
            InstallAlwaysFailingInitializer(controller);
            using (CaptureUserFaults(new List<string>()))
            {
                // Act
                Func<Task> act = () => controller.InitializeBreadcrumbHostAsync();

                // Assert
                await act.Should().NotThrowAsync("the initializer must contain its own fault");
                label
                    .Text.Should()
                    .Be(
                        EfcFormController.FolderAreaInitializationFailedText,
                        "the folder-area label is the visible carrier of the final failure"
                    );
            }
        }

        /// <summary>
        /// AC-U1/AC-U7: on final failure the router is notified, which navigates the error
        /// banner and discards the outbound queue. Before the fix the router is never notified.
        /// </summary>
        [TestMethod]
        public async Task InitializeBreadcrumbHostAsync_OnFinalFailure_NotifiesTheRouter()
        {
            // Arrange
            var controller = CreateMinimalController();
            var host = new Mock<IBreadcrumbWebHost>();
            host.SetupGet(h => h.IsCoreInitialized).Returns(false);
            var navigated = new List<string>();
            host.Setup(h => h.NavigateToString(It.IsAny<string>()))
                .Callback<string>(html => navigated.Add(html));
            var queue = new BreadcrumbOutboundQueue(host.Object);
            queue.PostOrQueue("{\"type\":\"render\"}");
            var router = new BreadcrumbBridgeRouter(
                new Mock<IFolderHierarchyProvider>().Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                queue
            );
            SetPrivateField(controller, "_router", router);
            InstallAlwaysFailingInitializer(controller);
            using (CaptureUserFaults(new List<string>()))
            {
                // Act
                Func<Task> act = () => controller.InitializeBreadcrumbHostAsync();

                // Assert
                await act.Should().NotThrowAsync("the initializer must contain its own fault");
                navigated
                    .Should()
                    .ContainSingle("the router must navigate exactly one document on failure")
                    .Which.Should()
                    .Contain("Folder list unavailable", "the navigated document is the banner");
                queue.PendingCount.Should().Be(0, "the failure must discard the outbound queue");
            }
        }
    }
}
