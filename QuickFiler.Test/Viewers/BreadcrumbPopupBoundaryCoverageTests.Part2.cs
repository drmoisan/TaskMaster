using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Continuation partial of <see cref="BreadcrumbPopupBoundaryCoverageTests"/>; the shared
    /// factory/readiness harness helpers and remaining cases live in the sibling primary partial so
    /// each file stays under the 500-line limit. Deterministic; no Outlook, live WebView2, timers, or
    /// temp files.
    /// </summary>
    public sealed partial class BreadcrumbPopupBoundaryCoverageTests
    {
        [TestMethod]
        public void InjectedFactory_CreateFailure_ReportsOnceWithoutCleanup() =>
            VerifyFactoryFailure("create", 0, false, false, "create");

        [TestMethod]
        public void InjectedFactory_InitializationFailure_DisposesControlOnce() =>
            VerifyFactoryFailure("initialize", 1, true, false, "create", "initialize", "cleanup");

        [TestMethod]
        public void InjectedFactory_CoreFailure_DisposesControlOnce() =>
            VerifyFactoryFailure(
                "core",
                1,
                false,
                false,
                "create",
                "initialize",
                "core",
                "cleanup"
            );

        [TestMethod]
        public void InjectedFactory_NavigationFailure_DisposesControlOnce() =>
            VerifyFactoryFailure(
                "navigate",
                1,
                false,
                false,
                "create",
                "initialize",
                "core",
                "navigate",
                "cleanup"
            );

        [TestMethod]
        public void InjectedFactory_CleanupFailure_DoesNotReplacePrimaryFailure() =>
            VerifyFactoryFailure("core", 1, false, true, "create", "initialize", "core", "cleanup");

        [TestMethod]
        public void Readiness_ConstructorGuardsBlankNameAndNullDetach()
        {
            Action blank = () => new BreadcrumbNavigationReadiness(" ", () => { });
            Action nullDetach = () => new BreadcrumbNavigationReadiness("Popup", null);
            blank.Should().Throw<ArgumentException>().WithParameterName("surfaceName");
            nullDetach.Should().Throw<ArgumentNullException>().WithParameterName("detachHandlers");
        }

        [TestMethod]
        public void Readiness_BeginNavigationGuardsNullDuplicateAndTerminalRequests()
        {
            int detaches = 0;
            using (var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++))
            {
                ((Action)(() => readiness.BeginNavigation(null)))
                    .Should()
                    .Throw<ArgumentNullException>()
                    .WithParameterName("navigate");
                readiness.BeginNavigation(() => { });
                ((Action)(() => readiness.BeginNavigation(() => { })))
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("*already*");
                readiness.Cancel();
                ((Action)(() => readiness.BeginNavigation(() => { })))
                    .Should()
                    .Throw<ObjectDisposedException>();
                readiness.Completion.IsCanceled.Should().BeTrue();
                detaches.Should().Be(1);
            }
        }

        [TestMethod]
        public void Readiness_UnrelatedAndDuplicateNotifications_CompleteCapturedSuccessOnce()
        {
            int detaches = 0;
            using (var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++))
            {
                readiness.NavigationStarted(3);
                readiness.BeginNavigation(() => { });
                readiness.NavigationStarted(7);
                readiness.NavigationStarted(8);
                readiness.NavigationCompleted(8, true, null);
                readiness.Completion.IsCompleted.Should().BeFalse();
                readiness.NavigationCompleted(7, true, null);
                readiness.NavigationCompleted(7, false, "duplicate");
                readiness.Completion.Status.Should().Be(TaskStatus.RanToCompletion);
                detaches.Should().Be(1);
            }
        }

        [TestMethod]
        public void Readiness_Failure_NormalizesNullAndBlankStatuses()
        {
            foreach (string status in new string[] { null, " " })
            {
                int detaches = 0;
                using (var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++))
                {
                    readiness.BeginNavigation(() => { });
                    readiness.NavigationStarted(5);
                    readiness.NavigationCompleted(5, false, status);
                    Action observe = () => readiness.Completion.GetAwaiter().GetResult();
                    observe.Should().Throw<InvalidOperationException>().WithMessage("*'Unknown'*");
                    detaches.Should().Be(1);
                }
            }
        }

        [TestMethod]
        public void Readiness_CancelAndDispose_AreIdempotent()
        {
            int detaches = 0;
            var readiness = new BreadcrumbNavigationReadiness("Popup", () => detaches++);
            readiness.BeginNavigation(() => { });
            readiness.Cancel();
            readiness.Cancel();
            readiness.Dispose();
            readiness.NavigationStarted(1);
            readiness.NavigationCompleted(1, true, null);
            readiness.Completion.IsCanceled.Should().BeTrue();
            detaches.Should().Be(1);
        }

        [TestMethod]
        public void Readiness_DetachFailure_IsContainedAndCompletionSucceeds()
        {
            int detaches = 0;
            var readiness = new BreadcrumbNavigationReadiness(
                "Popup",
                () =>
                {
                    detaches++;
                    throw new InvalidOperationException("detach");
                }
            );
            readiness.BeginNavigation(() => { });
            readiness.NavigationStarted(9);
            Action complete = () => readiness.NavigationCompleted(9, true, null);
            complete.Should().NotThrow();
            readiness.Completion.Status.Should().Be(TaskStatus.RanToCompletion);
            detaches.Should().Be(1);
            readiness.Dispose();
        }

        [TestMethod]
        public void CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries()
        {
            var context = new PumpSynchronizationContext();
            int testThread = 0;
            int capturedThread = 0;
            BreadcrumbPopupUiOperations testOperations = WithContext(
                null,
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests
            );
            testOperations
                .PostAsync(() => testThread = Environment.CurrentManagedThreadId)
                .GetAwaiter()
                .GetResult();
            BreadcrumbPopupUiOperations captured = WithContext(
                context,
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests
            );
            Task post = Task.Run(() =>
                captured.PostAsync(() => capturedThread = Environment.CurrentManagedThreadId)
            );
            context.Drain(post);
            testThread.Should().Be(context.OwnerThreadId);
            capturedThread.Should().Be(context.OwnerThreadId);
            context.PostCount.Should().Be(1);
        }

        [TestMethod]
        public void NormalizeFactory_SuccessAndNullResultPaths_PreserveContract()
        {
            var control = new TrackingControl();
            var messenger = new TrackingMessenger();
            CoreWebView2Environment environment = Uninitialized<CoreWebView2Environment>();
            var normalized = BreadcrumbPopupUiOperations.NormalizeFactory(_ =>
                Task.FromResult(Tuple.Create<Control, IWebViewMessenger>(control, messenger))
            );
            Tuple<Control, IWebViewMessenger, Task> created = normalized(environment)
                .GetAwaiter()
                .GetResult();
            Func<Task> nullResult = () =>
                BreadcrumbPopupUiOperations.NormalizeFactory(_ =>
                    Task.FromResult<Tuple<Control, IWebViewMessenger>>(null)
                )(environment);
            created.Item1.Should().BeSameAs(control);
            created.Item2.Should().BeSameAs(messenger);
            created.Item3.Should().BeSameAs(Task.CompletedTask);
            nullResult
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*returned no surface*")
                .GetAwaiter()
                .GetResult();
            messenger.Dispose();
            control.Dispose();
        }
    }
}
