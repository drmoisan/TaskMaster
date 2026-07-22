#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFiler.Viewers
{
    using ReadySurface = Tuple<Control, IWebViewMessenger, Task>;
    using ReadySurfaceFactory = Func<
        CoreWebView2Environment,
        Task<Tuple<Control, IWebViewMessenger, Task>>
    >;

    /// <summary>
    /// Correlates one requested document navigation with its exact starting and completed IDs.
    /// One terminal outcome detaches the SDK handlers supplied by the owning adapter.
    /// </summary>
    internal sealed class BreadcrumbNavigationReadiness : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            typeof(BreadcrumbNavigationReadiness)
        );

        private readonly object _sync = new object();
        private readonly string _surfaceName;
        private readonly Action _detachHandlers;
        private readonly TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private ulong? _navigationId;
        private bool _navigationRequested;
        private bool _terminal;

        internal BreadcrumbNavigationReadiness(string surfaceName, Action detachHandlers)
        {
            if (string.IsNullOrWhiteSpace(surfaceName))
            {
                throw new ArgumentException(
                    "A non-empty surface name is required.",
                    nameof(surfaceName)
                );
            }
            _surfaceName = surfaceName;
            _detachHandlers =
                detachHandlers ?? throw new ArgumentNullException(nameof(detachHandlers));
        }

        /// <summary>The readiness task for the exact requested navigation.</summary>
        internal Task Completion => _completion.Task;

        /// <summary>Marks the request immediately before invoking the navigation operation.</summary>
        internal void BeginNavigation(Action navigate)
        {
            if (navigate == null)
            {
                throw new ArgumentNullException(nameof(navigate));
            }

            lock (_sync)
            {
                if (_terminal)
                {
                    throw new ObjectDisposedException(nameof(BreadcrumbNavigationReadiness));
                }
                if (_navigationRequested)
                {
                    throw new InvalidOperationException("Navigation has already been requested.");
                }
                _navigationRequested = true;
            }

            try
            {
                navigate();
            }
            catch
            {
                Cancel();
                throw;
            }
        }

        /// <summary>Captures the first navigation that starts after the request is issued.</summary>
        internal void NavigationStarted(ulong navigationId)
        {
            lock (_sync)
            {
                if (_terminal || !_navigationRequested || _navigationId.HasValue)
                {
                    return;
                }
                _navigationId = navigationId;
            }
        }

        /// <summary>Completes only for the captured navigation ID.</summary>
        internal void NavigationCompleted(ulong navigationId, bool isSuccess, string? failureStatus)
        {
            lock (_sync)
            {
                if (_terminal || !_navigationId.HasValue || _navigationId.Value != navigationId)
                {
                    return;
                }
                _terminal = true;
            }

            DetachHandlers();
            if (isSuccess)
            {
                _completion.TrySetResult(true);
                return;
            }

            string status = failureStatus ?? "Unknown";
            status = string.IsNullOrWhiteSpace(status) ? "Unknown" : status;
            _completion.TrySetException(
                new InvalidOperationException(
                    $"{_surfaceName} navigation failed with status '{status}'."
                )
            );
        }

        /// <summary>Cancels pending readiness and detaches its handlers.</summary>
        internal void Cancel()
        {
            lock (_sync)
            {
                if (_terminal)
                {
                    return;
                }
                _terminal = true;
            }

            DetachHandlers();
            _completion.TrySetCanceled();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Cancel();
            GC.SuppressFinalize(this);
        }

        private void DetachHandlers()
        {
            try
            {
                _detachHandlers();
            }
            catch (Exception exception)
            {
                log.Error("Breadcrumb navigation handler detachment failed.", exception);
            }
        }
    }

    /// <summary>Creates the production popup surface and reports document readiness.</summary>
    internal static class BreadcrumbWebViewSurfaceFactory
    {
        internal static ReadySurfaceFactory Create(IWebViewCoreInitializer initializer, string html)
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));
            if (html == null)
                throw new ArgumentNullException(nameof(html));
            return environment => CreateSurfaceAsync(initializer, environment, html);
        }

        // Direct third-party adapter; lifecycle behavior is tested through the injected factory.
        [ExcludeFromCodeCoverage]
        private static async Task<ReadySurface> CreateSurfaceAsync(
            IWebViewCoreInitializer initializer,
            CoreWebView2Environment environment,
            string html
        )
        {
            var webView = new WebView2 { Dock = DockStyle.Fill };
            try
            {
                await initializer.EnsureCoreWebView2Async(webView, environment);
                CoreWebView2 core =
                    webView.CoreWebView2
                    ?? throw new InvalidOperationException(
                        "Popup CoreWebView2 initialization completed without a core instance."
                    );
                BreadcrumbNavigationReadiness readiness = NavigateToDocument(
                    core,
                    webView,
                    () => webView.NavigateToString(html),
                    "Popup"
                );
                return Tuple.Create<Control, IWebViewMessenger, Task>(
                    webView,
                    new WebView2Messenger(core),
                    readiness.Completion
                );
            }
            catch
            {
                webView.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Registers handlers before navigation and returns the shared exact-ID readiness lifetime.
        /// The caller may dispose the lifetime to cancel and detach a reset navigation.
        /// </summary>
        // Direct third-party event adapter; host-neutral correlation is covered through the lease.
        [ExcludeFromCodeCoverage]
        internal static BreadcrumbNavigationReadiness NavigateToDocument(
            CoreWebView2 core,
            Control owner,
            Action navigate,
            string surfaceName
        )
        {
            if (core == null)
                throw new ArgumentNullException(nameof(core));
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            if (navigate == null)
                throw new ArgumentNullException(nameof(navigate));

            BreadcrumbNavigationReadiness? readiness = null;

            void DetachHandlers()
            {
                core.NavigationStarting -= OnNavigationStarting;
                core.NavigationCompleted -= OnNavigationCompleted;
                owner.Disposed -= OnDisposed;
            }

            void OnNavigationStarting(
                object? sender,
                CoreWebView2NavigationStartingEventArgs args
            ) => readiness?.NavigationStarted(args.NavigationId);

            void OnNavigationCompleted(
                object? sender,
                CoreWebView2NavigationCompletedEventArgs args
            ) =>
                readiness?.NavigationCompleted(
                    args.NavigationId,
                    args.IsSuccess,
                    args.WebErrorStatus.ToString()
                );

            void OnDisposed(object? sender, EventArgs args) => readiness?.Cancel();

            readiness = new BreadcrumbNavigationReadiness(surfaceName, DetachHandlers);
            try
            {
                core.NavigationStarting += OnNavigationStarting;
                core.NavigationCompleted += OnNavigationCompleted;
                owner.Disposed += OnDisposed;
                readiness.BeginNavigation(navigate);
                return readiness;
            }
            catch
            {
                readiness.Dispose();
                throw;
            }
        }
    }
}
