#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using UtilitiesCS;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// 1:1 SDK-forwarding adapter implementing <see cref="IBreadcrumbWebHost"/> over the
    /// Designer-owned <see cref="WebView2"/> control (#349). Initialization awaits the form's
    /// UI SynchronizationContext BEFORE EnsureCoreWebView2Async (pattern
    /// QfcItemController.ViewerSetup), uses the shared %LocalAppData%\WindowsFormsWebView2 cache
    /// folder through the existing <see cref="IWebViewCoreInitializer"/> seam, and hooks
    /// CoreWebView2 events idempotently for pooled-viewer re-initialization (EfcViewerQueue).
    /// Waiting is event-driven (CoreWebView2InitializationCompleted) — no polling, no delays.
    /// </summary>
    /// <remarks>
    /// Coverage exemption justification (precedent <see cref="WebView2CoreInitializer"/>): every
    /// member forwards 1:1 to the WebView2 SDK or reacts to its events on a live control that
    /// cannot exist in a unit-test host; all routing/decision logic lives in the non-exempt
    /// <c>BreadcrumbBridgeRouter</c>/<c>BreadcrumbOutboundQueue</c>, tested via
    /// Mock&lt;IBreadcrumbWebHost&gt;.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public sealed class WebView2BreadcrumbHost : IBreadcrumbWebHost
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        // Per-control owner registry (#458). The dead constructor-side "-=" it replaces could never
        // remove a predecessor's subscription, because the delegate formed in a constructor is bound
        // to the instance under construction and delegate equality is pairwise over (target, method).
        // Entries are keyed on control identity through a dependent handle, so an entry is
        // collectible once the control is and the table adds no edge outliving the control.
        private static readonly ConditionalWeakTable<
            WebView2,
            WebView2BreadcrumbHost
        > _owners = new ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>();

        // Individual ConditionalWeakTable operations are thread-safe but a read-then-write sequence
        // is not atomic, so the compound lookup-detach-replace is taken under an explicit gate.
        private static readonly object _ownersGate = new object();

        private readonly WebView2 _control;
        private readonly IWebViewCoreInitializer _initializer;
        // Not readonly: InitializeAsync installs the dispatcher under capture variant V1, and only
        // when none was supplied through the internal constructor.
        private BreadcrumbUiDispatcher? _dispatcher;

        private bool _isAttached;

        // Explicit backing field for IsCoreInitialized (#476 defect 2). An auto-property's
        // compiler-generated field is non-volatile and therefore carries no barrier.
        private bool _isCoreInitialized;

        /// <summary>Creates the adapter over the Designer-owned control.</summary>
        /// <param name="control">The WebView2 control hosting the breadcrumb document.</param>
        /// <param name="initializer">The existing core-initializer seam.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer)
            : this(control, initializer, null) { }

        /// <summary>
        /// Creates the adapter with a caller-supplied UI dispatcher. Assembly-visible so tests can
        /// substitute a recording synchronization boundary; production uses the public two-argument
        /// overload, which supplies no dispatcher because none exists at construction time under
        /// capture variant V1.
        /// </summary>
        /// <param name="control">The WebView2 control hosting the breadcrumb document.</param>
        /// <param name="initializer">The existing core-initializer seam.</param>
        /// <param name="dispatcher">
        /// The UI marshalling boundary, or null to have one installed by
        /// <see cref="InitializeAsync"/> from its <c>uiSyncContext</c> argument.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="control"/> or <paramref name="initializer"/> is null.
        /// </exception>
        internal WebView2BreadcrumbHost(
            WebView2 control,
            IWebViewCoreInitializer initializer,
            BreadcrumbUiDispatcher? dispatcher
        )
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            _dispatcher = dispatcher;

            // Take ownership of this control: detach whichever host owned it before, then replace
            // the registry entry. Only TryGetValue / Remove / Add are used; AddOrUpdate's presence
            // on net481 is unverified.
            lock (_ownersGate)
            {
                if (_owners.TryGetValue(_control, out WebView2BreadcrumbHost? previous))
                {
                    previous?.DetachCore();
                    _owners.Remove(_control);
                }

                _owners.Add(_control, this);
            }

            _control.CoreWebView2InitializationCompleted += OnCoreInitializationCompleted;
            _control.Disposed += OnControlDisposed;
            _isAttached = true;
        }

        /// <summary>
        /// True while this instance is the attached owner of its control's initialization event.
        /// Assembly-visible observation point for the per-control ownership contract (#458).
        /// </summary>
        internal bool IsAttached => _isAttached;

        /// <summary>
        /// True once a UI marshalling boundary is available to this instance. Assembly-visible
        /// observation point for the dispatcher-installation contract (#476 defect 1).
        /// </summary>
        internal bool HasUiDispatcher => _dispatcher != null;

        /// <inheritdoc />
        /// <remarks>
        /// Read through <see cref="Volatile.Read(ref bool)"/> — an acquire load — so a reader that
        /// observes the flag is guaranteed to observe the <c>core.WebMessageReceived</c> subscription
        /// that precedes the release store in <c>OnCoreInitializationCompleted</c>. The two
        /// production readers call this property directly and synchronously from arbitrary threads,
        /// so a dispatcher cannot substitute for the barrier.
        /// </remarks>
        public bool IsCoreInitialized => Volatile.Read(ref _isCoreInitialized);

        /// <inheritdoc />
        public event EventHandler<string>? MessageReceived;

        /// <summary>
        /// Raised after CoreWebView2 initialization completes successfully; the controller wires
        /// this to <c>BreadcrumbBridgeRouter.NotifyCoreInitialized</c>.
        /// </summary>
        public event EventHandler? CoreInitialized;

        /// <inheritdoc />
        /// <remarks>
        /// The SDK forward runs inside a single <c>BreadcrumbUiDispatcher.Dispatch</c> callback, so a
        /// non-UI-thread caller no longer touches the control on its own thread. Dispatch is
        /// fire-and-forget, so this member returns before the forward executes; order between
        /// successive calls is preserved by the single post queue. Before <c>InitializeAsync</c> has
        /// installed a dispatcher there is none to marshal through, and the callback executes inline
        /// on the calling thread exactly as it did before this change.
        /// </remarks>
        public void NavigateToString(string html)
        {
            BreadcrumbUiDispatcher? dispatcher = _dispatcher;
            if (dispatcher == null)
            {
                _control.NavigateToString(html);
                return;
            }

            _ = dispatcher.Dispatch(() => _control.NavigateToString(html));
        }

        /// <inheritdoc />
        /// <remarks>
        /// The <c>CoreWebView2</c> read, the null guard, the log-and-drop message, and the post all
        /// run inside a single <c>BreadcrumbUiDispatcher.Dispatch</c> callback. The read is
        /// deliberately not performed as a separate value-returning dispatch step: that overload
        /// runs inline only from inside an already-executing <c>Dispatch</c> callback and faults on
        /// an owner-thread-only test dispatcher. Dispatch is fire-and-forget, so this member returns
        /// before the post executes; order is preserved by the single post queue. Before
        /// <c>InitializeAsync</c> has installed a dispatcher the same callback executes inline on the
        /// calling thread, preserving the existing log-and-drop behaviour.
        /// </remarks>
        public void PostMessageJson(string json)
        {
            // One unit of work, so the read and the post cannot be split across two dispatch hops.
            void PostCore()
            {
                CoreWebView2? core = _control.CoreWebView2;
                if (core == null)
                {
                    log.Error(
                        "PostMessageJson called before CoreWebView2 initialization; payload dropped."
                    );
                    return;
                }

                core.PostWebMessageAsJson(json);
            }

            BreadcrumbUiDispatcher? dispatcher = _dispatcher;
            if (dispatcher == null)
            {
                PostCore();
                return;
            }

            _ = dispatcher.Dispatch(PostCore);
        }

        /// <summary>
        /// Initializes the CoreWebView2 through the <see cref="IWebViewCoreInitializer"/> seam:
        /// awaits the UI SynchronizationContext first, then creates the shared-cache environment
        /// and calls EnsureCoreWebView2Async. Safe to re-run for pooled viewers.
        /// </summary>
        /// <param name="uiSyncContext">The form's UI synchronization context.</param>
        public async Task InitializeAsync(SynchronizationContext uiSyncContext)
        {
            if (uiSyncContext == null)
            {
                throw new ArgumentNullException(nameof(uiSyncContext));
            }

            string cacheFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WindowsFormsWebView2"
            );
            var options = new CoreWebView2EnvironmentOptions();

            // Capture variant V1: build the UI marshalling boundary from the context the caller
            // already supplies, so the constructor gains no new throwing precondition. Assign only
            // when no dispatcher was supplied through the internal constructor, otherwise an
            // injected boundary would be discarded here. BreadcrumbUiDispatcher.CaptureCurrent() is
            // deliberately not used: it throws when SynchronizationContext.Current is null.
            if (_dispatcher == null)
            {
                _dispatcher = new BreadcrumbUiDispatcher(uiSyncContext, LogDispatchFailure);
            }

            // WebView2 controls must be touched on the WinForms UI (STA) thread.
            await uiSyncContext;

            CoreWebView2Environment environment = await _initializer.CreateEnvironmentAsync(
                cacheFolder,
                options
            );
            await _initializer.EnsureCoreWebView2Async(_control, environment);
        }

        /// <summary>
        /// Error sink for the UI dispatcher installed by <see cref="InitializeAsync"/>. Dispatch
        /// failures are logged rather than propagated, because the dispatch is fire-and-forget and
        /// has no caller left to observe an exception.
        /// </summary>
        private static void LogDispatchFailure(Exception exception)
        {
            log.Error("Breadcrumb host UI dispatch failed.", exception);
        }

        /// <summary>
        /// Secondary hygiene (#458): a disposed control leaves no attached host and no registry
        /// entry behind. This does not address the two-live-hosts-over-one-undisposed-control case,
        /// which the owner registry handles.
        /// </summary>
        private void OnControlDisposed(object? sender, EventArgs e)
        {
            DetachCore();

            lock (_ownersGate)
            {
                if (
                    _owners.TryGetValue(_control, out WebView2BreadcrumbHost? owner)
                    && ReferenceEquals(owner, this)
                )
                {
                    _owners.Remove(_control);
                }
            }
        }

        /// <summary>
        /// Removes this instance's own subscriptions from the control and marks it detached. The
        /// removal is performed on the instance that made the subscription, which is the only way
        /// delegate equality — pairwise over (target, method) — can match.
        /// </summary>
        private void DetachCore()
        {
            _control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;

            // A predecessor that never completed initialization never subscribed to
            // core.WebMessageReceived and has a null CoreWebView2, so this must be null-checked.
            CoreWebView2? core = _control.CoreWebView2;
            if (core != null)
            {
                core.WebMessageReceived -= OnWebMessageReceived;
            }

            _isAttached = false;
        }

        private void OnCoreInitializationCompleted(
            object? sender,
            CoreWebView2InitializationCompletedEventArgs e
        )
        {
            if (!e.IsSuccess)
            {
                log.Error(
                    $"Breadcrumb CoreWebView2 initialization failed: {e.InitializationException?.Message}",
                    e.InitializationException
                );
                return;
            }

            CoreWebView2 core = _control.CoreWebView2;
            // Idempotent event hookup across pooled-viewer re-initialization.
            core.WebMessageReceived -= OnWebMessageReceived;
            core.WebMessageReceived += OnWebMessageReceived;

            // Release store. Must stay strictly after the subscription above and strictly before the
            // event below: that pairing is what makes the subscription visible to a reader that
            // observes the flag through Volatile.Read. Do not reorder these three statements.
            Volatile.Write(ref _isCoreInitialized, true);
            CoreInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e.WebMessageAsJson);
        }
    }
}
