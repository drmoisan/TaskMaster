using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #796 (AC6): the host-side close-ordering diagnostic must carry every field that
    /// discriminates between the candidate close paths, so the Phase 2 transcript can be read
    /// without inference.
    /// <para>
    /// Issue #796 (AC3): a native-reason close arriving while a selection commit is in flight must
    /// not cancel that selection, while a native-reason close with no commit in flight still must.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class BreadcrumbDropDownCloseOrderingTests
    {
        /// <summary>
        /// Scenario: the pure formatter is called with a fixed argument tuple. Expected outcome:
        /// the returned line carries all six discriminating field labels and the supplied close
        /// reason. Asserting on the formatter rather than on source text makes the AC6 evidence a
        /// deterministic managed-seam assertion; no popup, window, or WebView2 surface is created.
        /// </summary>
        [TestMethod]
        public void FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField()
        {
            // Arrange
            const ToolStripDropDownCloseReason CloseReason =
                ToolStripDropDownCloseReason.AppFocusChange;

            // Act
            string line = BreadcrumbDropDownHost.FormatDropDownClosedDiagnostics(
                CloseReason,
                programmaticClose: false,
                openState: true,
                autoClose: true,
                disposed: false,
                pendingClose: true
            );

            // Assert
            line.Should().Contain("CloseReason=");
            line.Should().Contain("ProgrammaticClose=");
            line.Should().Contain("OpenState=");
            line.Should().Contain("AutoClose=");
            line.Should().Contain("Disposed=");
            line.Should().Contain("PendingClose=");
            line.Should().Contain(CloseReason.ToString());
        }

        /// <summary>
        /// Issue #796 (AC3). Scenario: the popup is open, a selection commit has been requested for
        /// that popup lifetime, and the framework then reports a native close. Expected outcome: the
        /// cancel delegate is not invoked, so the commit is not undone by the close that accompanies
        /// it.
        /// </summary>
        /// <remarks>
        /// The framework cannot be made to choose a close reason in a headless test — no window is
        /// shown, so <c>ToolStripDropDown</c> never raises <c>Closed</c> of its own accord. The test
        /// therefore hands the handler a constructed reason and proves the branch, not the
        /// framework's choice of reason. That limit is recorded rather than papered over.
        /// </remarks>
        [TestMethod]
        public void NativeCloseWhileCommitPending_DoesNotCancelSelection()
        {
            // Arrange
            using (var harness = new CloseOrderingHostHarness())
            {
                harness.OpenAndSettle();
                harness.Host.IsCommitPending = true;

                // Act
                harness.RaiseNativeClose(ToolStripDropDownCloseReason.AppFocusChange);

                // Assert
                harness
                    .CancelCount.Should()
                    .Be(0, "a close racing an in-flight commit must not cancel the selection");
            }
        }

        /// <summary>
        /// Issue #796 (AC3). Scenario: the popup is open, no selection commit has been requested,
        /// and the framework then reports a native close. Expected outcome: the cancel delegate is
        /// invoked exactly once. This is the scoping half of the pair — it is what proves the
        /// suppression above is conditional on a pending commit rather than global.
        /// </summary>
        [TestMethod]
        public void NativeCloseWithNoCommitPending_StillCancelsSelection()
        {
            // Arrange
            using (var harness = new CloseOrderingHostHarness())
            {
                harness.OpenAndSettle();
                harness.Host.IsCommitPending.Should().BeFalse("a fresh show clears the latch");

                // Act
                harness.RaiseNativeClose(ToolStripDropDownCloseReason.AppFocusChange);

                // Assert
                harness
                    .CancelCount.Should()
                    .Be(1, "a close with no commit in flight still cancels the selection");
            }
        }

        /// <summary>
        /// Drives a real <see cref="BreadcrumbDropDownHost"/> headlessly under an inline
        /// synchronization context, in the style of the <c>PendingHostHarness</c> already used by
        /// <c>BreadcrumbPendingOpenCloseTests</c>. No window is shown and no WebView2 is
        /// initialised: the surface factory seam returns a plain panel and a stub messenger.
        /// </summary>
        private sealed class CloseOrderingHostHarness : IDisposable
        {
            private readonly SynchronizationContext _previousContext;
            private readonly Panel _anchor;
            private readonly Panel _surface = new Panel();
            private readonly StubMessenger _messenger = new StubMessenger();

            internal CloseOrderingHostHarness()
            {
                _previousContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(
                    new InlineSynchronizationContext()
                );
                try
                {
                    _anchor = new Panel();
                    var environment = (CoreWebView2Environment)
                        FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                    Host = new BreadcrumbDropDownHost(
                        _anchor,
                        environment,
                        CreateSurfaceAsync,
                        () => FocusPendingCount++,
                        () => FocusAnchorCount++,
                        () => CancelCount++,
                        (dropDown, owner, point) => ShowCount++
                    );
                }
                catch
                {
                    SynchronizationContext.SetSynchronizationContext(_previousContext);
                    throw;
                }
            }

            internal BreadcrumbDropDownHost Host { get; }
            internal int ShowCount { get; private set; }
            internal int FocusPendingCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }

            /// <summary>Opens the popup and asserts it reached the open state before the act step.</summary>
            internal void OpenAndSettle()
            {
                Task<bool> opening = Host.OpenAsync(
                    new Rectangle(120, 240, 390, 25),
                    new Rectangle(0, 0, 1920, 1040),
                    new Size(390, 180)
                );
                opening
                    .IsCompleted.Should()
                    .BeTrue("the inline context completes the open synchronously");
                opening.Result.Should().BeTrue();
                Host.IsOpen.Should().BeTrue();
                ShowCount.Should().Be(1);
                CancelCount.Should().Be(0, "opening must not cancel anything");
            }

            /// <summary>
            /// Hands the host's own native-close handler a constructed close reason. The handler is
            /// private and no window exists to raise <c>ToolStripDropDown.Closed</c>, so reflection
            /// is the only way to exercise the branch this pair is about.
            /// </summary>
            internal void RaiseNativeClose(ToolStripDropDownCloseReason reason)
            {
                MethodInfo handler = typeof(BreadcrumbDropDownHost).GetMethod(
                    "OnDropDownClosed",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                handler
                    .Should()
                    .NotBeNull(
                        because: "OnDropDownClosed must exist on BreadcrumbDropDownHost.Diagnostics.cs"
                    );
                handler.Invoke(
                    Host,
                    new object[] { Host.DropDown, new ToolStripDropDownClosedEventArgs(reason) }
                );
            }

            public void Dispose()
            {
                try
                {
                    Host.Dispose();
                    if (!_surface.IsDisposed)
                    {
                        _surface.Dispose();
                    }
                    _anchor.Dispose();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(_previousContext);
                }
            }

            private Task<Tuple<Control, IWebViewMessenger, Task>> CreateSurfaceAsync(
                CoreWebView2Environment environment
            ) =>
                Task.FromResult(
                    Tuple.Create<Control, IWebViewMessenger, Task>(
                        _surface,
                        _messenger,
                        Task.CompletedTask
                    )
                );
        }

        /// <summary>A messenger that records nothing; the close-ordering branch posts no JSON.</summary>
        private sealed class StubMessenger : IWebViewMessenger, IDisposable
        {
            private EventHandler<string> _messageReceived;

            public event EventHandler<string> MessageReceived
            {
                add => _messageReceived += value;
                remove => _messageReceived -= value;
            }

            public void PostJson(string json) { }

            public void Dispose() { }
        }

        /// <summary>Runs posted callbacks inline so the popup lifecycle settles deterministically.</summary>
        private sealed class InlineSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state) => callback(state);
        }
    }
}
