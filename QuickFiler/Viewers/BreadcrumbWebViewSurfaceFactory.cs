#nullable enable
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

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
            return Create(initializer, html, BreadcrumbPopupUiOperations.CaptureCurrent());
        }

        internal static ReadySurfaceFactory Create(
            IWebViewCoreInitializer initializer,
            string html,
            BreadcrumbPopupUiOperations operations
        )
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));
            if (html == null)
                throw new ArgumentNullException(nameof(html));
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));
            return environment => CreateSurfaceAsync(initializer, environment, html, operations);
        }

        private static async Task<ReadySurface> CreateSurfaceAsync(
            IWebViewCoreInitializer initializer,
            CoreWebView2Environment environment,
            string html,
            BreadcrumbPopupUiOperations operations
        )
        {
            Control? control = null;
            IWebViewMessenger? messenger = null;
            try
            {
                control = await operations.CreateControlAsync().ConfigureAwait(false);
                Task initialization = await operations
                    .BeginInitializationAsync(initializer, control, environment)
                    .ConfigureAwait(false);
                await operations.ObserveInitializationAsync(initialization).ConfigureAwait(false);
                CoreWebView2 core = await operations.ReadCoreAsync(control).ConfigureAwait(false);
                Tuple<IWebViewMessenger, Task> navigation = await operations
                    .BeginNavigationAsync(core, control, html)
                    .ConfigureAwait(false);
                messenger = navigation.Item1;
                Task readiness = operations.ObserveReadinessAsync(navigation.Item2);
                return Tuple.Create<Control, IWebViewMessenger, Task>(
                    control,
                    messenger,
                    readiness
                );
            }
            catch
            {
                await operations
                    .DisposeSurfaceAfterFailureAsync(control, messenger)
                    .ConfigureAwait(false);
                throw;
            }
        }

    }
}
