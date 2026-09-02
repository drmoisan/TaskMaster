using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;
using QuickFiler.Test.TestSupport;
using QuickFiler.Viewers;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// #230 pump-hosted de-exemption tests. Partial continuation of
    /// <see cref="QfcItemController_InitializationTests"/> (no second [TestClass] attribute); split
    /// out because the combined file exceeds the 500-line repository limit. The shared fixture
    /// lives in QfcItemController.InitializationTests.Part2.cs.
    /// </summary>
    public partial class QfcItemController_InitializationTests
    {
        /// <summary>
        /// #230 (de-exempted): <c>InitializeSequentialAsync</c> orchestrates the whole sequential
        /// initialization against the concrete <c>ItemViewer</c> — control-group resolution, theme
        /// setup and application, control population, tip/navigation toggles, and event wiring —
        /// then fires web-view init and forgets it. Every step marshals through the viewer's
        /// WinForms context, so the member is only awaitable with a live message pump. Its
        /// fire-and-forget <c>_ = InitializeWebViewAsync()</c> tail faults at the mocked seam and is
        /// discarded, so the member itself returns normally.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);

                // Act — awaited from the MSTest thread; continuations drain through the live pump.
                await harness.Controller.InitializeSequentialAsync().ConfigureAwait(false);

                // Assert — observable controller state produced by each orchestration step.
                harness.Controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                harness.Controller.Buttons.Should().NotBeNullOrEmpty();
                QfcItemControllerTestSupport
                    .GetField(harness.Controller, "_themes")
                    .Should()
                    .NotBeNull(because: "SetupThemes ran headlessly against the real ItemViewer");
                harness
                    .Controller.ItemHelper.Should()
                    .NotBeNull(because: "PopulateControlsAsync materializes the MailItemHelper");
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #230 (de-exempted): <c>InitializeGraphicsAsync</c> is the graphics-only sibling of
        /// <c>InitializeSequentialAsync</c> — it resolves control groups and themes, applies the
        /// dark theme branch, toggles tips/navigation, and wires events, but skips control
        /// population. The dark-mode branch is selected here so the theme code path differs from the
        /// sequential test's light branch.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: true).ConfigureAwait(false);

                // Act
                await harness.Controller.InitializeGraphicsAsync().ConfigureAwait(false);

                // Assert
                harness.Controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                harness.Controller.Buttons.Should().NotBeNullOrEmpty();
                QfcItemControllerTestSupport
                    .GetField(harness.Controller, "_themes")
                    .Should()
                    .NotBeNull();
                QfcItemControllerTestSupport
                    .GetField(harness.Controller, "_activeTheme")
                    .Should()
                    .NotBeNull(because: "the dark-mode branch selects and records an active theme");
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #230 (de-exempted): the synchronous <c>Initialize(bool async)</c> orchestration runs
        /// entirely on the pump thread — it resolves control groups, sets up and applies themes,
        /// populates controls, toggles tips and navigation, and wires events against the concrete
        /// <c>ItemViewer</c>. Its tail dispatches <c>InitializeWebViewAsync</c> fire-and-forget
        /// through the viewer's WPF dispatcher, which is serviced by the WinForms loop (interop
        /// proven by <c>WinFormsPumpHostTests.BothMarshalRoutes_*</c>); the mocked
        /// <c>IWebViewCoreInitializer</c> faults that discarded operation immediately, so no real
        /// WebView2 initialization occurs. The discarded task's fault path is deliberately not
        /// asserted (research section 9).
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeBool_ThroughThePumpHost_CompletesAndInitializesState()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);

                // Act — the whole synchronous orchestration runs on the pump thread.
                await host.InvokeAsync(() => harness.Controller.Initialize(async: false))
                    .ConfigureAwait(false);

                // Assert
                harness.Controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                harness.Controller.Buttons.Should().NotBeNullOrEmpty();
                QfcItemControllerTestSupport
                    .GetField(harness.Controller, "_themes")
                    .Should()
                    .NotBeNull(because: "SetupThemes ran against the real ItemViewer");
                harness
                    .Controller.ItemHelper.Should()
                    .NotBeNull(because: "PopulateControls materializes the MailItemHelper");
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #230 (de-exempted): the private nine-argument <c>Initialize</c> overload saves its
        /// parameters and funnels straight into <c>Initialize(bool)</c>. It is invoked here through
        /// the existing <c>InvokeNonPublic</c> reflection helper, running on the pump thread, and
        /// the state it funnels through <c>SaveParameters</c> plus the state produced by the
        /// delegated <c>Initialize(bool)</c> body are both asserted.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates()
        {
            // Arrange — the nine-arg overload calls SaveParameters itself, so the harness supplies
            // the collaborators it resolves (home controller, globals, viewer) as arguments.
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);
                Mock<IQfcCollectionController> parent = new Mock<IQfcCollectionController>();
                object globals = QfcItemControllerTestSupport.GetField(
                    harness.Controller,
                    "_globals"
                );
                object home = QfcItemControllerTestSupport.GetField(
                    harness.Controller,
                    "_homeController"
                );

                // Act
                await host.InvokeAsync(() =>
                        QfcItemControllerTestSupport.InvokeNonPublic(
                            harness.Controller,
                            "Initialize",
                            globals,
                            home,
                            parent.Object,
                            harness.Viewer,
                            9,
                            2,
                            harness.Controller.Mail,
                            null,
                            false
                        )
                    )
                    .ConfigureAwait(false);

                // Assert — SaveParameters state plus the delegated Initialize(bool) state.
                harness.Controller.Parent.Should().BeSameAs(parent.Object);
                harness.Controller.ItemNumber.Should().Be(9);
                harness.Controller.ItemNumberDigits.Should().Be(2);
                harness.Controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                QfcItemControllerTestSupport
                    .GetField(harness.Controller, "_themes")
                    .Should()
                    .NotBeNull(because: "the overload funnels into Initialize(bool)");
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #230 (de-exempted): <c>InitializeAsync</c> is the fully asynchronous orchestration —
        /// control-group resolution, theme setup and application, control population, tip and
        /// navigation toggles, conversation population, folder-combo-box population, and event
        /// wiring — every step of which marshals through the concrete viewer's WinForms context.
        /// Its final statement is <c>await InitializeWebViewAsync()</c>, which under the mocked
        /// <c>IWebViewCoreInitializer</c> faults deterministically at the seam. The test asserts
        /// that exact injected exception identity — a controlled fault, not a timeout or a hang —
        /// and asserts the observable state set by every preceding line.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);

                // Act
                Func<Task> act = () => harness.Controller.InitializeAsync();

                // Assert — the controlled fault from the mocked web-view seam.
                await act.Should()
                    .ThrowAsync<WebViewSentinelException>(
                        because: "execution must stop at the mocked web-view seam, not hang"
                    )
                    .ConfigureAwait(false);

                // Assert — everything before the faulting tail ran against the real ItemViewer.
                harness.Controller.TableLayoutPanels.Should().NotBeNullOrEmpty();
                harness.Controller.Buttons.Should().NotBeNullOrEmpty();
                QfcItemControllerTestSupport
                    .GetField(harness.Controller, "_themes")
                    .Should()
                    .NotBeNull(because: "SetupThemes precedes the faulting await");
                harness
                    .Controller.ItemHelper.Should()
                    .NotBeNull(because: "PopulateControlsAsync precedes the faulting await");
                QfcItemControllerTestSupport
                    .GetField(harness.Controller, "_folderHandler")
                    .Should()
                    .NotBeNull(because: "PopulateFolderComboBoxAsync precedes the faulting await");
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #670: awaited directly. The guard must not fault and the sink must get the seam fault.
        /// </summary>
        [TestMethod]
        public async Task InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault()
        {
            // Arrange — the supplied context must also be current, so the await at
            // ViewerSetup.cs:64 continues inline and execution reaches the mocked seam.
            SynchronizationContext previous = SynchronizationContext.Current;
            try
            {
                SynchronizationContext context = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(context);
                HarnessController controller = BuildGuardedWebViewTarget(context);
                System.Exception captured = null;
                controller.WebViewInitializationErrorSink = (message, exception) =>
                    captured = exception;

                // Act
                Func<Task> act = () => controller.InitializeWebViewGuardedAsync();

                // Assert
                await act.Should()
                    .NotThrowAsync(because: "the guard contains the fault instead of returning it")
                    .ConfigureAwait(false);
                captured
                    .Should()
                    .BeOfType<WebViewSentinelException>(
                        because: "the sink must receive the exact fault raised at the mocked seam"
                    );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        /// #670: exercises the sink's default lambda body rather than a test double.
        /// </summary>
        [TestMethod]
        public void WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing()
        {
            // Arrange — no sink assignment, so the default log4net-backed delegate is under test.
            HarnessController controller = new HarnessController();

            // Act
            System.Action act = () =>
                controller.WebViewInitializationErrorSink("smoke", new InvalidOperationException());

            // Assert
            act.Should()
                .NotThrow(
                    because: "the default sink must write through the static logger without faulting"
                );
        }

        /// <summary>
        /// #670: the site-192 dispatcher path. The sink is installed during Arrange because the
        /// dispatched operation may complete before <c>host.InvokeAsync</c> returns.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);
                var observed = new TaskCompletionSource<System.Exception>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                harness.Controller.WebViewInitializationErrorSink = (m, e) =>
                    observed.TrySetResult(e);

                // Act
                await host.InvokeAsync(() => harness.Controller.Initialize(async: false))
                    .ConfigureAwait(false);
                System.Exception fault = await observed.Task.ConfigureAwait(false);

                // Assert
                fault
                    .Should()
                    .BeOfType<WebViewSentinelException>(
                        because: "the dispatched guard must route the seam fault to the sink"
                    );
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #511/#571 regression probe: the shared pump harness must hand back an
        /// <c>ItemViewer</c> whose window handle already exists, created on the pump thread.
        /// Every pump-hosted test in this class marshals work through the viewer, and
        /// <c>Control.Invoke</c> throws on a handle-less control, so a harness that returns a
        /// viewer with no handle makes those tests fail. This probe reports the harness viewer's
        /// handle state directly, so a run in which the end-to-end tests happen to pass still
        /// records whether the handle was present.
        /// </summary>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);

                // Act — read the marshalling predicate on the pump thread that owns the viewer.
                bool invokeRequiredOnPumpThread = await host.InvokeAsync(() =>
                        harness.Viewer.InvokeRequired
                    )
                    .ConfigureAwait(false);

                // Assert — the handle exists, so Control.Invoke cannot throw for want of one.
                harness
                    .Viewer.IsHandleCreated.Should()
                    .BeTrue(
                        because: "the harness must create the viewer's window handle on the pump thread"
                    );
                invokeRequiredOnPumpThread
                    .Should()
                    .BeFalse(
                        because: "the pump thread owns the viewer's handle, so no marshalling is required there"
                    );
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// #571 minimality pin: the harness must not itself create either
        /// <c>Microsoft.Web.WebView2.WinForms.WebView2</c> child's window handle. It forces only the
        /// viewer's own handle, by reading <c>.Handle</c>, which is non-recursive.
        /// </summary>
        /// <remarks>
        /// Measured, not predicted. A bare <c>new ItemViewer()</c> constructed on the pump with no
        /// harness, no <c>SaveParameters</c>, and no <c>.Handle</c> read already reports both
        /// children as handle-created, so the handles originate in <c>InitializeComponent</c>'s
        /// third-party <c>ISupportInitialize.EndInit()</c> call, not in the harness. That is also
        /// why the viewer's own handle was already present on every pre-fix run: WinForms creates a
        /// parent's handle when a child's handle is created. This test therefore pins the state the
        /// harness inherits rather than a state it produces, and it fails if a future change makes
        /// the children handle-less at construction and so invalidates that assumption.
        /// </remarks>
        [TestMethod]
        [Timeout(PumpTimeoutMs)]
        public async Task BuildPumpHarness_DoesNotCreateTheWebViewChildHandles()
        {
            // Arrange
            WinFormsPumpHost host = new WinFormsPumpHost();
            PumpHarness harness = null;
            try
            {
                harness = await BuildPumpHarnessAsync(host, darkMode: false).ConfigureAwait(false);

                // Act - read both child handle states on the pump thread that owns the viewer.
                bool bodyWebViewHandleCreated = await host.InvokeAsync(() =>
                        harness.Viewer.L0v2h2_WebView2.IsHandleCreated
                    )
                    .ConfigureAwait(false);
                bool breadcrumbWebViewHandleCreated = await host.InvokeAsync(() =>
                        harness.Viewer.L0vhBreadcrumb_WebView2.IsHandleCreated
                    )
                    .ConfigureAwait(false);

                // Assert - both children carry the handle state ItemViewer construction gave them.
                bodyWebViewHandleCreated
                    .Should()
                    .BeTrue(
                        because: "ItemViewer construction creates the body WebView2 child's handle through ISupportInitialize.EndInit, so the harness inherits it rather than creating it"
                    );
                breadcrumbWebViewHandleCreated
                    .Should()
                    .BeTrue(
                        because: "ItemViewer construction creates the breadcrumb WebView2 child's handle through ISupportInitialize.EndInit, so the harness inherits it rather than creating it"
                    );
            }
            finally
            {
                if (harness != null)
                {
                    harness.Restore();
                }

                await host.StopAsync().ConfigureAwait(false);
            }
        }
    }
}
