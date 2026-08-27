using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Moq;
using QuickFiler.Test.TestSupport;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Behavioural regression tests for the WebView2 breadcrumb host defects #476 and #458. Every
    /// test constructs its own <see cref="WebView2"/> control on <see cref="WinFormsPumpHost"/>, so
    /// the process-wide per-control owner registry cannot couple one test to another. No test drives
    /// <c>EnsureCoreWebView2Async</c> or <c>CoreWebView2Environment.CreateAsync</c> to completion, so
    /// no Evergreen WebView2 runtime is required.
    /// </summary>
    [TestClass]
    public sealed class WebView2BreadcrumbHostTests
    {
        private const int PumpTimeoutMs = 60000;

        /// <summary>
        /// #476 defect 1: <c>PostMessageJson</c> must marshal its SDK touch through exactly one
        /// dispatcher callback instead of reading and posting on the caller's thread.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task PostMessageJson_PostsExactlyOnceToTheUiContext()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    var recording = new RecordingSynchronizationContext();
                    var errors = new List<Exception>();
                    var dispatcher = new BreadcrumbUiDispatcher(recording, errors.Add);
                    WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(
                                control,
                                Mock.Of<IWebViewCoreInitializer>(),
                                dispatcher
                            )
                        )
                        .ConfigureAwait(false);

                    // Act - called from the MSTest thread, which is not the dispatcher's boundary.
                    subject.PostMessageJson("{\"kind\":\"probe\"}");

                    // Assert
                    recording
                        .PostCount.Should()
                        .Be(
                            1,
                            because: "the CoreWebView2 read, the null guard and the post must all run inside one marshalled callback rather than inline on the caller's thread"
                        );
                    errors
                        .Should()
                        .BeEmpty(
                            because: "scheduling onto the recording context must not report a dispatch failure"
                        );
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// #476 defect 1: <c>NavigateToString</c> must marshal its SDK touch through exactly one
        /// dispatcher callback instead of calling the control on the caller's thread.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task NavigateToString_PostsExactlyOnceToTheUiContext()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    var recording = new RecordingSynchronizationContext();
                    var errors = new List<Exception>();
                    var dispatcher = new BreadcrumbUiDispatcher(recording, errors.Add);
                    WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(
                                control,
                                Mock.Of<IWebViewCoreInitializer>(),
                                dispatcher
                            )
                        )
                        .ConfigureAwait(false);

                    // Act - called from the MSTest thread, which is not the dispatcher's boundary.
                    subject.NavigateToString("<html></html>");

                    // Assert
                    recording
                        .PostCount.Should()
                        .Be(
                            1,
                            because: "the NavigateToString forward must run inside one marshalled callback rather than inline on the caller's thread"
                        );
                    errors
                        .Should()
                        .BeEmpty(
                            because: "scheduling onto the recording context must not report a dispatch failure"
                        );
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// #458: constructing a second host over the same control must detach the predecessor and
        /// take ownership, so exactly one host handles initialization completion. The assertion is
        /// about the host's own attachment state, not about a reflected SDK handler count, because
        /// whether the SDK implements the event as a field-like backing delegate or through a
        /// WinForms <c>EventHandlerList</c> is unverified.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task SecondHost_DetachesThePredecessorAndTakesOwnership()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    WebView2BreadcrumbHost first = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(control, Mock.Of<IWebViewCoreInitializer>())
                        )
                        .ConfigureAwait(false);

                    // Act
                    WebView2BreadcrumbHost second = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(control, Mock.Of<IWebViewCoreInitializer>())
                        )
                        .ConfigureAwait(false);

                    // Assert
                    first
                        .IsAttached.Should()
                        .BeFalse(
                            because: "the predecessor must be detached by the successor's construction, otherwise it stays subscribed for the control's lifetime and handles initialization completion a second time"
                        );
                    second
                        .IsAttached.Should()
                        .BeTrue(because: "the most recently constructed host is the registered owner");
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// #458 edge case: a predecessor that never completed core initialization never subscribed to
        /// <c>core.WebMessageReceived</c> and has a null <c>CoreWebView2</c>, so the detach path must
        /// tolerate that without throwing.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task PredecessorDetach_ToleratesNullCoreWebView2()
        {
            // Arrange - host A is never initialized, so control.CoreWebView2 stays null.
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    WebView2BreadcrumbHost first = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(control, Mock.Of<IWebViewCoreInitializer>())
                        )
                        .ConfigureAwait(false);

                    // Act
                    Func<Task> act = () =>
                        pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(control, Mock.Of<IWebViewCoreInitializer>())
                        );

                    // Assert
                    await act.Should()
                        .NotThrowAsync(
                            because: "the predecessor never subscribed to core.WebMessageReceived, so the detach must null-check CoreWebView2 rather than dereference it"
                        )
                        .ConfigureAwait(false);
                    first
                        .IsAttached.Should()
                        .BeFalse(
                            because: "the predecessor must still be detached even when it never completed initialization"
                        );
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// #458 secondary hygiene: disposing the control must detach the host and leave no registry
        /// entry, so a disposed control no longer retains its host.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task ControlDisposed_DetachesTheHost()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                        new WebView2BreadcrumbHost(control, Mock.Of<IWebViewCoreInitializer>())
                    )
                    .ConfigureAwait(false);

                // Act - dispose on the pump thread that owns the control.
                await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);

                // Assert
                subject
                    .IsAttached.Should()
                    .BeFalse(
                        because: "a disposed control must leave no attached host and no registry entry behind"
                    );
            }
        }

        /// <summary>
        /// #476 defect 1, variant V1: the dispatcher is installed inside <c>InitializeAsync</c> from
        /// the <c>uiSyncContext</c> argument the host already receives, so the constructor gains no
        /// new throwing precondition.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeAsync_InstallsUiDispatcherFromUiSyncContext()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    Mock<IWebViewCoreInitializer> initializer = BuildCompletingInitializer();
                    WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(control, initializer.Object, null)
                        )
                        .ConfigureAwait(false);
                    subject
                        .HasUiDispatcher.Should()
                        .BeFalse(
                            because: "under variant V1 no dispatcher exists before InitializeAsync has run"
                        );

                    // Act
                    await subject.InitializeAsync(pump.SyncContext).ConfigureAwait(false);

                    // Assert
                    subject
                        .HasUiDispatcher.Should()
                        .BeTrue(
                            because: "InitializeAsync must build the dispatcher from its uiSyncContext argument"
                        );
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// #476 defect 1: a dispatcher supplied through the internal three-argument constructor must
        /// survive <c>InitializeAsync</c> rather than being replaced by one built from
        /// <c>uiSyncContext</c>. Without this, the seam the whole marshalling regression test rests on
        /// would be discarded the moment initialization ran.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeAsync_PreservesAnInjectedDispatcher()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    var recording = new RecordingSynchronizationContext();
                    var errors = new List<Exception>();
                    var injected = new BreadcrumbUiDispatcher(recording, errors.Add);
                    Mock<IWebViewCoreInitializer> initializer = BuildCompletingInitializer();
                    WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(control, initializer.Object, injected)
                        )
                        .ConfigureAwait(false);
                    await subject.InitializeAsync(pump.SyncContext).ConfigureAwait(false);

                    // Act - called from the MSTest thread, which is not the injected boundary.
                    subject.PostMessageJson("{\"kind\":\"probe\"}");

                    // Assert
                    recording
                        .PostCount.Should()
                        .Be(
                            1,
                            because: "the injected dispatcher must still be the one routing SDK touches after InitializeAsync, otherwise it was replaced by one built from uiSyncContext"
                        );
                    errors
                        .Should()
                        .BeEmpty(because: "no dispatch failure should be reported");
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Pins the documented pre-initialization behaviour: before a dispatcher exists there is
        /// nothing to marshal through, so the callback executes inline on the calling thread and the
        /// payload is still dropped with the existing log message, exactly as before this change.
        /// The fix is deliberately not total, and this test records that boundary.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload()
        {
            // Arrange
            using (var pump = new WinFormsPumpHost())
            {
                WebView2 control = await pump.InvokeAsync(() => new WebView2())
                    .ConfigureAwait(false);
                try
                {
                    WebView2BreadcrumbHost subject = await pump.InvokeAsync(() =>
                            new WebView2BreadcrumbHost(
                                control,
                                Mock.Of<IWebViewCoreInitializer>(),
                                null
                            )
                        )
                        .ConfigureAwait(false);
                    subject
                        .HasUiDispatcher.Should()
                        .BeFalse(
                            because: "no dispatcher was supplied and InitializeAsync has not run"
                        );

                    // Act
                    Action act = () => subject.PostMessageJson("{\"kind\":\"probe\"}");

                    // Assert
                    act.Should()
                        .NotThrow(
                            because: "the pre-dispatcher window must still execute inline and drop the payload rather than fail"
                        );
                    subject
                        .IsCoreInitialized.Should()
                        .BeFalse(
                            because: "dropping the payload must not be mistaken for a completed initialization"
                        );
                }
                finally
                {
                    await pump.InvokeAsync(() => control.Dispose()).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// A loose mock whose two seam members return already-completed tasks, so
        /// <c>InitializeAsync</c> runs end-to-end without reaching the WebView2 SDK. The environment
        /// result is null because <see cref="CoreWebView2Environment"/> cannot be constructed without
        /// the Evergreen runtime, and the host forwards it to the mock rather than dereferencing it.
        /// </summary>
        private static Mock<IWebViewCoreInitializer> BuildCompletingInitializer()
        {
            var initializer = new Mock<IWebViewCoreInitializer>();
            initializer
                .Setup(seam =>
                    seam.CreateEnvironmentAsync(
                        It.IsAny<string>(),
                        It.IsAny<CoreWebView2EnvironmentOptions>()
                    )
                )
                .Returns(Task.FromResult<CoreWebView2Environment>(null));
            initializer
                .Setup(seam =>
                    seam.EnsureCoreWebView2Async(
                        It.IsAny<WebView2>(),
                        It.IsAny<CoreWebView2Environment>()
                    )
                )
                .Returns(Task.CompletedTask);
            return initializer;
        }

        /// <summary>
        /// Counts <see cref="SynchronizationContext.Post"/> calls and deliberately never invokes the
        /// queued callback. Because the callback is never drained, the <see cref="WebView2"/> control
        /// is never touched and no WebView2 runtime is involved. This context is never installed as
        /// the ambient context on the test thread: <c>BreadcrumbUiDispatcher.Dispatch</c> executes
        /// inline when it is already on its boundary, which would record zero posts.
        /// </summary>
        private sealed class RecordingSynchronizationContext : SynchronizationContext
        {
            private int _postCount;

            internal int PostCount => Volatile.Read(ref _postCount);

            public override void Post(SendOrPostCallback d, object state)
            {
                Interlocked.Increment(ref _postCount);
            }
        }
    }
}
