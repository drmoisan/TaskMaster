#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

        private readonly WebView2 _control;
        private readonly IWebViewCoreInitializer _initializer;

        /// <summary>Creates the adapter over the Designer-owned control.</summary>
        /// <param name="control">The WebView2 control hosting the breadcrumb document.</param>
        /// <param name="initializer">The existing core-initializer seam.</param>
        /// <exception cref="ArgumentNullException">Any argument is null.</exception>
        public WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
            _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));

            // Idempotent hookup: pooled viewers re-run initialization, so unhook before hooking.
            _control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;
            _control.CoreWebView2InitializationCompleted += OnCoreInitializationCompleted;
        }

        /// <inheritdoc />
        public bool IsCoreInitialized { get; private set; }

        /// <inheritdoc />
        public event EventHandler<string>? MessageReceived;

        /// <summary>
        /// Raised after CoreWebView2 initialization completes successfully; the controller wires
        /// this to <c>BreadcrumbBridgeRouter.NotifyCoreInitialized</c>.
        /// </summary>
        public event EventHandler? CoreInitialized;

        /// <inheritdoc />
        public void NavigateToString(string html)
        {
            _control.NavigateToString(html);
        }

        /// <inheritdoc />
        public void PostMessageJson(string json)
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

            // WebView2 controls must be touched on the WinForms UI (STA) thread.
            await uiSyncContext;

            CoreWebView2Environment environment = await _initializer.CreateEnvironmentAsync(
                cacheFolder,
                options
            );
            await _initializer.EnsureCoreWebView2Async(_control, environment);
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

            IsCoreInitialized = true;
            CoreInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e.WebMessageAsJson);
        }
    }
}
