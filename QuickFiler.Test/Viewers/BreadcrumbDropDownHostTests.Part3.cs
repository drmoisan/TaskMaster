using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;

namespace QuickFiler.Test.Viewers
{
    /// <summary>
    /// Issue #677: execution-time focus-permission contracts for the additive
    /// <c>MayTakeFocus</c> guard on <see cref="BreadcrumbDropDownHost"/>.
    /// <para>
    /// A third partial part of <see cref="BreadcrumbDropDownHostTests"/>. No <c>[TestClass]</c>
    /// attribute is repeated here: <c>TestClassAttribute</c> is <c>AllowMultiple = false</c> and
    /// declaring it on more than one part is a CS0579 duplicate-attribute error. The primary file
    /// has no line headroom, so this part carries its own <see cref="PredicateHarness"/> rather
    /// than extending the primary file's <c>Harness</c>.
    /// </para>
    /// <para>
    /// <see cref="PredicateHarness"/> supplies the popup UI boundary explicitly as a captured
    /// <c>CapturingSynchronizationContext</c> and never installs it as the ambient
    /// <see cref="System.Threading.SynchronizationContext"/>. That is load-bearing:
    /// <c>BreadcrumbUiDispatcher.IsCurrentBoundary()</c> reports true when the ambient context is
    /// reference-equal to the captured one, and <c>Dispatch</c> would then run the action inline,
    /// leaving no gap between scheduling a focus action and executing it. The whole defect lives
    /// in that gap, so every test below drains the queue explicitly on the creating thread.
    /// </para>
    /// </summary>
    public sealed partial class BreadcrumbDropDownHostTests
    {
        /// <summary>
        /// The click-out close path (WinForms auto-close raising <c>DropDown.Closed</c>) must not
        /// re-focus the anchor once the predicate is false, but must still cancel the selection.
        /// </summary>
        [TestMethod]
        public void FinishClose_DropDownClosedPath_PredicateFalse_DoesNotFocusAnchor()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange — a real open, drained to quiescence, so OnDropDownClosed is not a no-op.
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");
                harness.FocusAnchorCount.Should().Be(0, "a successful open never anchors focus");
                harness.AllowFocus = false;

                // Act
                InvokePrivate(
                    harness.Host,
                    "OnDropDownClosed",
                    harness.Host,
                    new ToolStripDropDownClosedEventArgs(ToolStripDropDownCloseReason.AppClicked)
                );
                context.DrainAll();

                // Assert
                harness
                    .FocusAnchorCount.Should()
                    .Be(0, "the guard suppresses the focus step on click-out");
                harness.CancelCount.Should().Be(1, "the cancel step is never gated by the guard");
            }
        }

        /// <summary>
        /// A programmatic uncommitted close with the predicate false cancels the selection but does
        /// not hand focus back to the anchor.
        /// </summary>
        [TestMethod]
        public void FinishClose_ProgrammaticClose_PredicateFalse_DoesNotFocusAnchor()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");
                harness.FocusAnchorCount.Should().Be(0, "a successful open never anchors focus");
                harness.AllowFocus = false;

                // Act — a true result proves the queued close route ran, not the no-op early return.
                bool closed = harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted);
                context.DrainAll();

                // Assert
                closed.Should().BeTrue("Close on an open host schedules the close work");
                harness.FocusAnchorCount.Should().Be(0, "the guard suppresses the focus step");
                harness.CancelCount.Should().Be(1, "the cancel step is never gated by the guard");
            }
        }

        /// <summary>
        /// Control case for the close path: with the predicate true, the pre-fix focus return to
        /// the anchor is preserved exactly (issue #438/#400 in-form behavior).
        /// </summary>
        [TestMethod]
        public void FinishClose_PredicateTrue_FocusAnchorInvoked()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");
                harness.FocusAnchorCount.Should().Be(0, "a successful open never anchors focus");

                // Act
                bool closed = harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted);
                context.DrainAll();

                // Assert
                closed.Should().BeTrue("Close on an open host schedules the close work");
                harness.FocusAnchorCount.Should().Be(1, "an in-form close still returns focus");
                harness.CancelCount.Should().Be(1);
            }
        }

        /// <summary>
        /// Issue #677 AC-5: the predicate is read when the scheduled focus action executes, not
        /// when it is scheduled. The close work is queued while the predicate is still true and the
        /// predicate flips to false before the queue drains.
        /// </summary>
        [TestMethod]
        public void FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange — OnDropDownClosed early-returns unless the host is genuinely open.
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");

                // Act — schedule the close while the predicate still permits focus.
                InvokePrivate(
                    harness.Host,
                    "OnDropDownClosed",
                    harness.Host,
                    new ToolStripDropDownClosedEventArgs(ToolStripDropDownCloseReason.AppClicked)
                );
                context
                    .PendingCount.Should()
                    .BeGreaterThan(0, "the close work must be queued, not run inline");
                harness.AllowFocus = false;
                context.DrainAll();

                // Assert
                harness
                    .FocusAnchorCount.Should()
                    .Be(0, "the predicate is evaluated at execution time, not at scheduling time");
            }
        }

        /// <summary>
        /// The already-open refocus branch is guarded: re-issuing an open on an open popup with the
        /// predicate false must not invoke the pending-focus delegate.
        /// </summary>
        [TestMethod]
        public void AlreadyOpenRefocus_PredicateFalse_DoesNotFocusPending()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");
                harness.FocusPendingCount.Should().Be(1, "the fresh open focuses the popup once");
                harness.Host.IsOpen.Should().BeTrue("the already-open branch requires IsOpen");
                harness.AllowFocus = false;

                // Act
                Task<bool> reopening = harness.OpenAsync();
                context.DrainUntil(reopening);

                // Assert
                reopening.GetAwaiter().GetResult().Should().BeTrue();
                harness.FocusPendingCount.Should().Be(1, "the guarded refocus did not run");
                harness.FactoryCount.Should().Be(1, "the surface is reused, not recreated");
                harness.ShowCount.Should().Be(1, "the popup is not re-shown");
            }
        }

        /// <summary>
        /// Control case for the already-open refocus branch: with the predicate true the refocus
        /// still runs, so the pending-focus delegate is invoked a second time.
        /// </summary>
        [TestMethod]
        public void AlreadyOpenRefocus_PredicateTrue_FocusPendingInvoked()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");
                harness.FocusPendingCount.Should().Be(1, "the fresh open focuses the popup once");
                harness.Host.IsOpen.Should().BeTrue("the already-open branch requires IsOpen");

                // Act
                Task<bool> reopening = harness.OpenAsync();
                context.DrainUntil(reopening);

                // Assert
                reopening.GetAwaiter().GetResult().Should().BeTrue();
                harness
                    .FocusPendingCount.Should()
                    .Be(2, "fresh-open focus plus already-open focus");
                harness.FactoryCount.Should().Be(1, "the surface is reused, not recreated");
                harness.ShowCount.Should().Be(1, "the popup is not re-shown");
            }
        }

        /// <summary>
        /// The fresh-open focus completion is guarded too: an open that completes while the
        /// predicate is false still reports success but never focuses the popup surface.
        /// </summary>
        [TestMethod]
        public void FreshOpenFocus_PredicateFalse_DoesNotFocusPending()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                harness.AllowFocus = false;

                // Act
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);

                // Assert
                opening
                    .GetAwaiter()
                    .GetResult()
                    .Should()
                    .BeTrue("a guarded open still reports the same open result");
                harness.FocusPendingCount.Should().Be(0, "the late focus completion is suppressed");
            }
        }

        /// <summary>
        /// Issue #677 AC-7: an unassigned predicate keeps the property-initializer default
        /// <c>() =&gt; true</c>, so behavior is byte-identical to the pre-fix host.
        /// </summary>
        [TestMethod]
        public void UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked()
        {
            using (var harness = new PredicateHarness(assignPredicate: false))
            {
                // Arrange
                BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext context =
                    harness.Context;
                Task<bool> opening = harness.OpenAsync();
                context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");

                // Act
                bool closed = harness.Host.Close(BreadcrumbDropDownCloseReason.Uncommitted);
                context.DrainAll();

                // Assert
                closed.Should().BeTrue("Close on an open host schedules the close work");
                harness.FocusAnchorCount.Should().Be(1, "the default predicate permits focus");
            }
        }

        /// <summary>
        /// Issue #680/#677 composition: a <c>takeFocus: true</c> reopen after a non-capturing open
        /// restores <c>AutoClose</c> unconditionally (issue #680), but the handoff focus call is
        /// still suppressed while issue #677's <c>MayTakeFocus</c> predicate is false.
        /// </summary>
        [TestMethod]
        public void OpenAsync_TakeFocusReopenAfterNonCapturingOpenWithPredicateFalse_RestoresAutoCloseButSuppressesFocus()
        {
            using (var harness = new PredicateHarness())
            {
                // Arrange — a non-capturing open never focuses the popup.
                Task<bool> opening = ((IBreadcrumbDropDownHost)harness.Host).OpenAsync(
                    Anchor,
                    Work,
                    Desired,
                    false
                );
                harness.Context.DrainUntil(opening);
                opening.GetAwaiter().GetResult().Should().BeTrue("the popup must actually open");
                harness
                    .FocusPendingCount.Should()
                    .Be(0, "a non-capturing open never focuses the popup");
                harness.AllowFocus = false;

                // Act — the takeFocus reopen (Down-arrow handoff) runs while the predicate is false.
                Task<bool> reopening = ((IBreadcrumbDropDownHost)harness.Host).OpenAsync(
                    Anchor,
                    Work,
                    Desired,
                    true
                );
                harness.Context.DrainUntil(reopening);

                // Assert
                harness
                    .Host.DropDown.AutoClose.Should()
                    .BeTrue(
                        "issue #680's restore is unconditional, independent of the focus predicate"
                    );
                harness
                    .FocusPendingCount.Should()
                    .Be(
                        0,
                        "issue #677's MayTakeFocus guard suppresses the handoff focus call while the predicate is false"
                    );
            }
        }

        /// <summary>
        /// Constructs the concrete <see cref="BreadcrumbDropDownHost"/> in typed code through its
        /// internal nine-argument constructor (reachable via
        /// <c>InternalsVisibleTo("QuickFiler.Test")</c>) and assigns the internal
        /// <c>MayTakeFocus</c> predicate in typed code, so both references are compile-time
        /// references rather than reflection. The popup UI boundary is supplied explicitly and is
        /// never installed as the ambient synchronization context.
        /// </summary>
        private sealed class PredicateHarness : IDisposable
        {
            private readonly ConcurrentQueue<Exception> _errors = new ConcurrentQueue<Exception>();
            private readonly Panel _anchor = new Panel();
            private Panel _surface;

            /// <summary>Read by the injected predicate at execution time, not at scheduling time.</summary>
            internal bool AllowFocus = true;

            internal PredicateHarness(bool assignPredicate = true)
            {
                Context =
                    new BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext();
                var environment = (CoreWebView2Environment)
                    FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment));
                Func<
                    CoreWebView2Environment,
                    Task<Tuple<Control, IWebViewMessenger>>
                > legacyFactory = CreateLegacySurfaceAsync;
                var operations = new BreadcrumbPopupUiOperations(
                    new BreadcrumbUiDispatcher(Context, _errors.Enqueue)
                );
                Host = new BreadcrumbDropDownHost(
                    _anchor,
                    environment,
                    BreadcrumbPopupUiOperations.NormalizeFactory(legacyFactory),
                    FocusPending,
                    FocusAnchor,
                    CancelSelection,
                    ShowPopup,
                    operations,
                    ClosePopup
                );
                if (assignPredicate)
                {
                    Host.MayTakeFocus = () => AllowFocus;
                }
            }

            internal BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext Context { get; }
            internal BreadcrumbDropDownHost Host { get; }
            internal int FactoryCount { get; private set; }
            internal int ShowCount { get; private set; }
            internal int FocusPendingCount { get; private set; }
            internal int FocusAnchorCount { get; private set; }
            internal int CancelCount { get; private set; }

            internal Task<bool> OpenAsync() => Host.OpenAsync(Anchor, Work, Desired);

            public void Dispose()
            {
                Host.Dispose();
                Context.DrainAll();
                if (_surface != null && !_surface.IsDisposed)
                {
                    _surface.Dispose();
                }
                if (!_anchor.IsDisposed)
                {
                    _anchor.Dispose();
                }
            }

            private Task<Tuple<Control, IWebViewMessenger>> CreateLegacySurfaceAsync(
                CoreWebView2Environment environment
            )
            {
                FactoryCount++;
                _surface = new Panel();
                return Task.FromResult(
                    Tuple.Create<Control, IWebViewMessenger>(_surface, new TrackingMessenger())
                );
            }

            private void ShowPopup(ToolStripDropDown dropDown, Control owner, Point location) =>
                ShowCount++;

            private void ClosePopup(
                ToolStripDropDown dropDown,
                ToolStripDropDownCloseReason reason
            ) { }

            private void FocusPending() => FocusPendingCount++;

            private void FocusAnchor() => FocusAnchorCount++;

            private void CancelSelection() => CancelCount++;
        }
    }
}
