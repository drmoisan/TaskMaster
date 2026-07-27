#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFiler.Viewers
{
    using InstalledSurface = Tuple<ToolStripControlHost, Control, IWebViewMessenger>;
    using LegacySurface = Tuple<Control, IWebViewMessenger>;
    using Messenger = IWebViewMessenger;
    using NavigationSurface = Tuple<IWebViewMessenger, Task>;
    using PopupDropDown = ToolStripDropDown;
    using PopupHost = ToolStripControlHost;
    using Readiness = BreadcrumbNavigationReadiness;
    using ReadySurface = Tuple<Control, IWebViewMessenger, Task>;
    using WebCore = CoreWebView2;
    using WebEnvironment = CoreWebView2Environment;
    using WebInitializer = IWebViewCoreInitializer;

    /// <summary>
    /// Owns every popup WebView and WinForms operation that must execute on the captured UI
    /// boundary.
    /// </summary>
    internal sealed class BreadcrumbPopupUiOperations
    {
        internal delegate Task BeginInitialization(
            WebInitializer initializer,
            Control control,
            WebEnvironment environment
        );

        private readonly BreadcrumbUiDispatcher _dispatcher;
        private readonly Func<Control> _createControl;
        private readonly BeginInitialization _beginInitialization;
        private readonly Func<Control, WebCore> _readCore;
        private readonly Func<WebCore, Control, string, NavigationSurface> _beginNavigation;
        private readonly Action<Control?, Messenger?> _disposeSurface;

        internal BreadcrumbPopupUiOperations(BreadcrumbUiDispatcher dispatcher)
            : this(
                dispatcher,
                CreateProductionControl,
                BeginProductionInitialization,
                ReadProductionCore,
                (core, control, html) => BeginProductionNavigation(dispatcher, core, control, html),
                DisposeProductionSurface
            ) { }

        internal BreadcrumbPopupUiOperations(
            BreadcrumbUiDispatcher dispatcher,
            Func<Control> create,
            BeginInitialization initialize,
            Func<Control, WebCore> readCore,
            Func<WebCore, Control, string, NavigationSurface> navigate,
            Action<Control?, Messenger?> dispose
        )
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _createControl = create ?? throw new ArgumentNullException(nameof(create));
            _beginInitialization =
                initialize ?? throw new ArgumentNullException(nameof(initialize));
            _readCore = readCore ?? throw new ArgumentNullException(nameof(readCore));
            _beginNavigation = navigate ?? throw new ArgumentNullException(nameof(navigate));
            _disposeSurface = dispose ?? throw new ArgumentNullException(nameof(dispose));
        }

        internal static BreadcrumbPopupUiOperations CaptureCurrent() =>
            new BreadcrumbPopupUiOperations(BreadcrumbUiDispatcher.CaptureCurrent());

        internal static BreadcrumbPopupUiOperations CreateForCurrentThreadTests() =>
            new BreadcrumbPopupUiOperations(BreadcrumbUiDispatcher.CreateForCurrentThreadTests());

        internal static BreadcrumbPopupUiOperations CaptureCurrentOrTests() =>
            SynchronizationContext.Current == null
                ? CreateForCurrentThreadTests()
                : CaptureCurrent();

        internal static Func<WebEnvironment, Task<ReadySurface>> NormalizeFactory(
            Func<WebEnvironment, Task<LegacySurface>> factory
        )
        {
            _ = factory ?? throw new ArgumentNullException(nameof(factory));
            return async environment =>
            {
                LegacySurface created =
                    await factory(environment).ConfigureAwait(false)
                    ?? throw Invalid("Popup initialization returned no surface.");
                return Tuple.Create(created.Item1, created.Item2, Task.CompletedTask);
            };
        }

        [ExcludeFromCodeCoverage]
        internal static void ShowOwnedPopup(
            PopupDropDown dropDown,
            Control anchor,
            Point screenLocation
        ) => dropDown.Show(anchor, anchor.PointToClient(screenLocation));

        internal Task RunAsync(Action action, bool reportFailure = true)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            return _dispatcher.DispatchValue(
                () =>
                {
                    action();
                    return true;
                },
                reportFailure
            );
        }

        internal Task<T> RunAsync<T>(Func<T> action, bool reportFailure = true) =>
            _dispatcher.DispatchValue(action, reportFailure);

        internal Task PostAsync(Action action) => _dispatcher.Dispatch(action);

        internal void Report(Exception exception) => _dispatcher.Report(exception);

        internal Task<Control> CreateControlAsync() => RunAsync(_createControl);

        internal Task<Task> BeginInitializationAsync(
            WebInitializer initializer,
            Control control,
            WebEnvironment environment
        ) =>
            BeginInitializationAsync(() => _beginInitialization(initializer, control, environment));

        internal Task<Task> BeginInitializationAsync(Func<Task> initialize) =>
            RunAsync(() =>
                initialize()
                ?? throw Invalid("Popup WebView initialization returned no completion task.")
            );

        internal Task<WebCore> ReadCoreAsync(Control control) =>
            ReadCoreAsync(() => _readCore(control));

        internal Task<WebCore> ReadCoreAsync(Func<WebCore> readCore) =>
            ReadRequiredAsync(
                readCore,
                "Popup CoreWebView2 initialization completed without a core instance."
            );

        internal Task<T> ReadRequiredAsync<T>(Func<T> read, string missingMessage)
            where T : class => RunAsync(() => read() ?? throw Invalid(missingMessage));

        internal Task<NavigationSurface> BeginNavigationAsync(
            WebCore core,
            Control control,
            string html
        ) =>
            RunAsync(() =>
            {
                NavigationSurface navigation = _beginNavigation(core, control, html);
                if (navigation?.Item1 != null && navigation.Item2 != null)
                    return navigation;
                (navigation?.Item1 as IDisposable)?.Dispose();
                throw Invalid("Popup navigation did not provide a messenger and readiness task.");
            });

        internal Task ObserveInitializationAsync(Task initialization) =>
            ObserveExternalAsync(initialization, reportCancellation: true);

        internal Task ObserveReadinessAsync(Task readiness) =>
            ObserveExternalAsync(readiness, reportCancellation: false);

        internal Task DisposeSurfaceAsync(
            Control? control,
            Messenger? messenger,
            bool reportFailure = true
        ) =>
            control == null && messenger == null
                ? Task.CompletedTask
                : RunAsync(() => _disposeSurface(control, messenger), reportFailure);

        internal Task DisposeSurfaceAfterFailureAsync(
            Control? control,
            IWebViewMessenger? messenger
        ) => IgnoreFailureAsync(DisposeSurfaceAsync(control, messenger, reportFailure: false));

        internal Task<BreadcrumbPopupPlacementResult?> PlaceSurfaceAsync(
            PopupDropDown dropDown,
            PopupHost host,
            Control control,
            Rectangle anchorBounds,
            Rectangle workingArea,
            Size desiredSize,
            Func<bool> isCurrent
        ) =>
            RunAsync(() =>
            {
                if (!isCurrent())
                    return null;
                BreadcrumbPopupPlacementResult placement = BreadcrumbPopupPlacement.Calculate(
                    anchorBounds,
                    workingArea,
                    desiredSize
                );
                if (!isCurrent())
                    return null;
                host.Size = placement.Bounds.Size;
                if (!isCurrent())
                    return null;
                control.Size = placement.Bounds.Size;
                if (!isCurrent())
                    return null;
                dropDown.Size = placement.Bounds.Size;
                return (BreadcrumbPopupPlacementResult?)placement;
            });

        internal Task DisposeHostedSurfaceAsync(
            PopupDropDown dropDown,
            PopupHost? host,
            Control? control,
            Messenger? messenger,
            bool reportFailure = true
        ) =>
            RetryAsync(
                reportFailure,
                retry: false,
                () => dropDown.Items.Remove(host!),
                () => host?.Dispose(),
                () => (host == null && control?.IsDisposed == false ? control : null)?.Dispose(),
                () => (messenger as IDisposable)?.Dispose()
            );

        internal Task DisposeHostedSurfaceAfterFailureAsync(
            PopupDropDown dropDown,
            PopupHost? host,
            Control? control,
            Messenger? messenger
        ) => IgnoreFailureAsync(DisposeHostedSurfaceAsync(dropDown, host, control, messenger));

        internal async Task<InstalledSurface?> CreateAndInstallSurfaceAsync(
            Func<WebEnvironment, Task<ReadySurface>> factory,
            WebEnvironment environment,
            PopupDropDown dropDown,
            Func<bool> isCurrent,
            Task cancellation
        )
        {
            ReadySurface? created = null;
            PopupHost? host = null;
            try
            {
                created = await factory(environment).ConfigureAwait(false);
                if (created?.Item1 == null || created.Item2 == null || created.Item3 == null)
                {
                    throw Invalid(
                        "Popup initialization did not provide a control, messenger, and readiness task."
                    );
                }
                Task completed = await Task.WhenAny(created.Item3, cancellation)
                    .ConfigureAwait(false);
                if (!ReferenceEquals(completed, created.Item3))
                {
                    ReadySurface surfaceToDispose = created;
                    created = null;
                    await RetryAsync(
                            true,
                            retry: true,
                            () => (surfaceToDispose.Item2 as IDisposable)?.Dispose(),
                            () => surfaceToDispose.Item1.Dispose()
                        )
                        .ConfigureAwait(false);
                    return null;
                }
                await created.Item3.ConfigureAwait(false);
                bool installed = await RunAsync(() =>
                    {
                        if (!isCurrent())
                            return false;
                        var installedHost = new PopupHost(created.Item1)
                        {
                            AutoSize = false,
                            Margin = Padding.Empty,
                            Padding = Padding.Empty,
                        };
                        host = installedHost;
                        if (!isCurrent())
                            return false;
                        dropDown.Items.Add(installedHost);
                        return true;
                    })
                    .ConfigureAwait(false);
                if (!installed)
                {
                    ReadySurface surfaceToDispose = created;
                    created = null;
                    PopupHost? hostToDispose = host;
                    host = null;
                    await DisposeHostedSurfaceAsync(
                            dropDown,
                            hostToDispose,
                            surfaceToDispose.Item1,
                            surfaceToDispose.Item2
                        )
                        .ConfigureAwait(false);
                    return null;
                }
                return Tuple.Create(host!, created.Item1, created.Item2);
            }
            catch
            {
                await DisposeHostedSurfaceAfterFailureAsync(
                        dropDown,
                        host,
                        created?.Item1,
                        created?.Item2
                    )
                    .ConfigureAwait(false);
                throw;
            }
        }

        private async Task ObserveExternalAsync(Task operation, bool reportCancellation)
        {
            _ = operation ?? throw new ArgumentNullException(nameof(operation));
            try
            {
                await operation.ConfigureAwait(false);
            }
            catch (Exception exception)
                when (reportCancellation || !(exception is OperationCanceledException))
            {
                _dispatcher.Report(exception);
                throw;
            }
        }

        private static async Task IgnoreFailureAsync(Task cleanup)
        {
            try
            {
                await cleanup.ConfigureAwait(false);
            }
            catch { }
        }

        private async Task RetryAsync(bool report, bool retry, params Action[] cleanups)
        {
            var completed = new bool[cleanups.Length];
            Exception? failure = null;
            for (int attempt = 0; attempt < (retry ? 2 : 1); attempt++)
            {
                for (int index = 0; index < cleanups.Length; index++)
                {
                    if (completed[index])
                        continue;
                    try
                    {
                        await RunAsync(cleanups[index], report && attempt == 0)
                            .ConfigureAwait(false);
                        completed[index] = true;
                    }
                    catch (Exception exception)
                    {
                        failure ??= exception;
                    }
                }
            }
            if (failure != null)
                throw failure;
        }

        private static Exception Invalid(string message) => new InvalidOperationException(message);

        [ExcludeFromCodeCoverage]
        private static Control CreateProductionControl() => new WebView2 { Dock = DockStyle.Fill };

        [ExcludeFromCodeCoverage]
        private static Task BeginProductionInitialization(
            WebInitializer initializer,
            Control control,
            WebEnvironment environment
        ) => initializer.EnsureCoreWebView2Async((WebView2)control, environment);

        [ExcludeFromCodeCoverage]
        private static WebCore ReadProductionCore(Control control) =>
            ((WebView2)control).CoreWebView2;

        [ExcludeFromCodeCoverage]
        private static NavigationSurface BeginProductionNavigation(
            BreadcrumbUiDispatcher dispatcher,
            WebCore core,
            Control control,
            string html
        ) =>
            BreadcrumbPopupLifecycleOperations.CreateNavigationSurface(
                NavigateToDocument(
                    dispatcher,
                    core,
                    control,
                    () => ((WebView2)control).NavigateToString(html),
                    "Popup"
                ),
                () => new WebView2Messenger(core, dispatcher)
            );

        [ExcludeFromCodeCoverage]
        private static void DisposeProductionSurface(Control? control, Messenger? messenger) =>
            BreadcrumbPopupLifecycleOperations.DisposeTwoResources(
                () => (messenger as IDisposable)?.Dispose(),
                () => control?.Dispose()
            );

        internal static Readiness CreateDispatchedReadiness(
            BreadcrumbUiDispatcher dispatcher,
            string surfaceName,
            Action detachHandlers
        ) => new Readiness(surfaceName, () => _ = dispatcher.Dispatch(detachHandlers));

        internal static Readiness NavigateToDocument(
            BreadcrumbUiDispatcher dispatcher,
            WebCore core,
            Control owner,
            Action navigate,
            string surfaceName
        ) => NavigateToDocumentCore(dispatcher, core, owner, navigate, surfaceName);

        private static Readiness NavigateToDocumentCore(
            BreadcrumbUiDispatcher dispatcher,
            WebCore core,
            Control owner,
            Action navigate,
            string surfaceName
        )
        {
            _ = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _ = core ?? throw new ArgumentNullException(nameof(core));
            _ = owner ?? throw new ArgumentNullException(nameof(owner));
            return BindProductionNavigation(dispatcher, core, owner, navigate, surfaceName);
        }

        [ExcludeFromCodeCoverage]
        private static Readiness BindProductionNavigation(
            BreadcrumbUiDispatcher dispatcher,
            WebCore core,
            Control owner,
            Action navigate,
            string surfaceName
        )
        {
            return BreadcrumbPopupLifecycleOperations.NavigateWithSubscription(
                dispatcher,
                surfaceName,
                navigate,
                (navigationStarted, navigationCompleted, ownerDisposed) =>
                {
                    EventHandler<CoreWebView2NavigationStartingEventArgs> starting = (_, args) =>
                        navigationStarted(args.NavigationId);
                    EventHandler<CoreWebView2NavigationCompletedEventArgs> completed = (_, args) =>
                        navigationCompleted(
                            args.NavigationId,
                            args.IsSuccess,
                            args.WebErrorStatus.ToString()
                        );
                    EventHandler disposed = (_, __) => ownerDisposed();
                    core.NavigationStarting += starting;
                    core.NavigationCompleted += completed;
                    owner.Disposed += disposed;
                    return new BreadcrumbNavigationSubscription(() =>
                    {
                        core.NavigationStarting -= starting;
                        core.NavigationCompleted -= completed;
                        owner.Disposed -= disposed;
                    });
                }
            );
        }
    }
}
