#nullable enable
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace QuickFiler.Viewers
{
    using InstalledSurface = Tuple<ToolStripControlHost, Control, IWebViewMessenger>;
    using LegacySurfaceFactory = Func<
        CoreWebView2Environment,
        Task<Tuple<Control, IWebViewMessenger>>
    >;
    using OwnedSurface = Tuple<ToolStripControlHost?, Control?, IWebViewMessenger?>;
    using ReadySurfaceFactory = Func<
        CoreWebView2Environment,
        Task<Tuple<Control, IWebViewMessenger, Task>>
    >;

    /// <summary>Owns the native popup and its lazily created breadcrumb surface.</summary>
    public sealed partial class BreadcrumbDropDownHost : IBreadcrumbDropDownHost
    {
        private readonly ReadySurfaceFactory _factory;
        private readonly BreadcrumbPopupUiOperations _uiOperations;
        private readonly BreadcrumbDropDownOpenLifetime _openLifetime;
        private readonly Action _focusPending;
        private readonly Action _focusAnchor;
        private readonly Action _cancelSelection;
        private readonly Action<ToolStripDropDown, Control, Point> _showPopup;
        private readonly Action<ToolStripDropDown, ToolStripDropDownCloseReason> _closePopup;
        private bool _resetPending;
        private bool _programmaticClose;
        private bool _disposed;

        /// <summary>Creates a production popup host on the current UI boundary.</summary>
        public BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            IWebViewCoreInitializer initializer,
            string html,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection
        )
            : this(
                anchor,
                environment,
                initializer ?? throw new ArgumentNullException(nameof(initializer)),
                html ?? throw new ArgumentNullException(nameof(html)),
                focusPending,
                focusAnchor,
                cancelSelection,
                BreadcrumbPopupUiOperations.CaptureCurrent()
            ) { }

        internal BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            IWebViewCoreInitializer initializer,
            string html,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection,
            BreadcrumbPopupUiOperations operations
        )
            : this(
                anchor,
                environment,
                BreadcrumbWebViewSurfaceFactory.Create(initializer, html, operations),
                focusPending,
                focusAnchor,
                cancelSelection,
                BreadcrumbPopupUiOperations.ShowOwnedPopup,
                operations
            ) { }

        /// <summary>Creates a popup host using a host-neutral surface factory seam.</summary>
        public BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            LegacySurfaceFactory surfaceFactory,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection,
            Action<ToolStripDropDown, Control, Point> showPopup
        )
            : this(
                anchor,
                environment,
                BreadcrumbPopupUiOperations.NormalizeFactory(
                    surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory))
                ),
                focusPending,
                focusAnchor,
                cancelSelection,
                showPopup,
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests()
            ) { }

        internal BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            ReadySurfaceFactory surfaceFactory,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection,
            Action<ToolStripDropDown, Control, Point> showPopup
        )
            : this(
                anchor,
                environment,
                surfaceFactory,
                focusPending,
                focusAnchor,
                cancelSelection,
                showPopup,
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests()
            ) { }

        internal BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            ReadySurfaceFactory surfaceFactory,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection,
            Action<ToolStripDropDown, Control, Point> showPopup,
            BreadcrumbPopupUiOperations operations
        )
            : this(
                anchor,
                environment,
                surfaceFactory,
                focusPending,
                focusAnchor,
                cancelSelection,
                showPopup,
                operations,
                (popup, reason) => popup.Close(reason)
            ) { }

        internal BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            ReadySurfaceFactory surfaceFactory,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection,
            Action<ToolStripDropDown, Control, Point> showPopup,
            BreadcrumbPopupUiOperations operations,
            Action<ToolStripDropDown, ToolStripDropDownCloseReason> closePopup
        )
        {
            Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _factory = surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory));
            _focusPending = focusPending ?? throw new ArgumentNullException(nameof(focusPending));
            _focusAnchor = focusAnchor ?? throw new ArgumentNullException(nameof(focusAnchor));
            _cancelSelection =
                cancelSelection ?? throw new ArgumentNullException(nameof(cancelSelection));
            _showPopup = showPopup ?? throw new ArgumentNullException(nameof(showPopup));
            _closePopup = closePopup ?? throw new ArgumentNullException(nameof(closePopup));
            _uiOperations = operations ?? throw new ArgumentNullException(nameof(operations));
            DropDown = new ToolStripDropDown
            {
                AutoClose = true,
                AutoSize = false,
                Padding = Padding.Empty,
            };
            DropDown.Closed += OnDropDownClosed;
            _openLifetime = new BreadcrumbDropDownOpenLifetime(this, _uiOperations);
        }

        /// <summary>The collapsed control that owns popup placement.</summary>
        public Control Anchor { get; }

        /// <summary>The WebView environment used for lazy surface creation.</summary>
        public CoreWebView2Environment Environment { get; }

        /// <summary>The native popup surface.</summary>
        public ToolStripDropDown DropDown { get; }

        /// <summary>The installed hosted control, when initialized.</summary>
        public ToolStripControlHost? ControlHost => InstalledControlHost;

        /// <inheritdoc />
        public IWebViewMessenger? PopupMessenger => _popupMessenger;

        /// <inheritdoc />
        public bool IsOpen => OpenState;

        /// <summary>The retained popup theme.</summary>
        public string Theme { get; private set; } = "light";

        /// <summary>The latest initialization or open failure.</summary>
        public Exception? LastInitializationException { get; internal set; }

        /// <inheritdoc />
        public event EventHandler? PopupMessengerReady;

        internal ReadySurfaceFactory SurfaceFactory => _factory;

        internal ToolStripControlHost? InstalledControlHost { get; set; }

        internal Control? _popupControl;

        internal Control? InstalledPopupControl
        {
            get => _popupControl;
            set => _popupControl = value;
        }

        internal IWebViewMessenger? _popupMessenger;

        internal IWebViewMessenger? InstalledPopupMessenger
        {
            get => _popupMessenger;
            set => _popupMessenger = value;
        }

        internal bool HasInstalledSurface =>
            InstalledControlHost != null && _popupControl != null && _popupMessenger != null;

        internal bool OpenState { get; set; }

        /// <inheritdoc />
        public bool Close(BreadcrumbDropDownCloseReason reason)
        {
            if (_disposed)
                return false;
            if (OpenState)
            {
                _openLifetime.InvalidateAndSchedule(() => CompleteClose(reason, true));
                return true;
            }
            return _openLifetime.TryCancelPendingOpen(() => CompleteClose(reason, OpenState));
        }

        /// <inheritdoc />
        public void SetTheme(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
                throw new ArgumentException("A non-empty theme is required.", nameof(theme));
            ThrowIfDisposed();
            Theme = theme;
        }

        /// <inheritdoc />
        public void Reset()
        {
            ThrowIfDisposed();
            _resetPending = true;
            _openLifetime.InvalidateAndSchedule(ResetCoreAsync);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _openLifetime.DisposeAndSchedule(DisposeCoreAsync);
            GC.SuppressFinalize(this);
        }

        internal void FocusPending() => _focusPending();

        internal void ShowPopup(Point location) => _showPopup(DropDown, Anchor, location);

        internal void PublishPopupMessengerReady() =>
            PopupMessengerReady?.Invoke(this, EventArgs.Empty);

        private async Task ResetCoreAsync()
        {
            try
            {
                await _uiOperations
                    .RunAsync(() =>
                    {
                        if (OpenState)
                            CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, true);
                    })
                    .ConfigureAwait(false);
            }
            finally
            {
                try
                {
                    await DisposeSurfaceAsync().ConfigureAwait(false);
                }
                finally
                {
                    LastInitializationException = null;
                    _resetPending = false;
                }
            }
        }

        private Task DisposeCoreAsync() =>
            _uiOperations.RunAsync(() =>
            {
                if (OpenState && !_resetPending)
                    CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, true);
                OpenState = false;
                OwnedSurface owned = TakeOwnedSurface();
                CompleteAll(
                    () => DropDown.Closed -= OnDropDownClosed,
                    () =>
                    {
                        if (owned.Item1 != null)
                            DropDown.Items.Remove(owned.Item1);
                    },
                    () => owned.Item1?.Dispose(),
                    () =>
                    {
                        if (owned.Item2 != null && !owned.Item2.IsDisposed)
                            owned.Item2.Dispose();
                    },
                    () => (owned.Item3 as IDisposable)?.Dispose(),
                    DropDown.Dispose
                );
            });

        private async Task DisposeSurfaceAsync()
        {
            OwnedSurface owned = await _uiOperations
                .RunAsync(() => TakeOwnedSurface())
                .ConfigureAwait(false);
            await _uiOperations
                .DisposeHostedSurfaceAsync(DropDown, owned.Item1, owned.Item2, owned.Item3)
                .ConfigureAwait(false);
        }

        internal async Task DisposeSurfaceAfterFailureAsync(InstalledSurface? expected)
        {
            OwnedSurface owned = await _uiOperations
                .RunAsync(() => TakeOwnedSurface(expected), reportFailure: false)
                .ConfigureAwait(false);
            await _uiOperations
                .DisposeHostedSurfaceAfterFailureAsync(
                    DropDown,
                    owned.Item1,
                    owned.Item2,
                    owned.Item3
                )
                .ConfigureAwait(false);
        }

        private OwnedSurface TakeOwnedSurface(InstalledSurface? expected = null)
        {
            if (
                expected != null
                && (
                    !ReferenceEquals(InstalledControlHost, expected.Item1)
                    || !ReferenceEquals(_popupControl, expected.Item2)
                    || !ReferenceEquals(_popupMessenger, expected.Item3)
                )
            )
                return new OwnedSurface(null, null, null);
            var owned = Tuple.Create(InstalledControlHost, _popupControl, _popupMessenger);
            InstalledControlHost = null;
            _popupControl = null;
            _popupMessenger = null;
            return owned;
        }

        private void CompleteClose(BreadcrumbDropDownCloseReason reason, bool closeNative)
        {
            if (!OpenState && !_openLifetime.IsPendingClose)
                return;
            bool wasOpen = OpenState;
            OpenState = false;
            CompleteAll(
                () =>
                {
                    if (closeNative && wasOpen)
                        CloseNative();
                },
                () => FinishClose(reason)
            );
        }

        private void CloseNative()
        {
            _programmaticClose = true;
            try
            {
                _closePopup(DropDown, ToolStripDropDownCloseReason.CloseCalled);
            }
            finally
            {
                _programmaticClose = false;
            }
        }

        private void OnDropDownClosed(object? sender, ToolStripDropDownClosedEventArgs e)
        {
            if (_disposed || _programmaticClose || !OpenState)
                return;
            _openLifetime.InvalidateAndSchedule(() =>
            {
                if (_disposed || _programmaticClose || !OpenState)
                    return;
                OpenState = false;
                FinishClose(BreadcrumbDropDownCloseReason.Uncommitted);
            });
        }

        private void FinishClose(BreadcrumbDropDownCloseReason reason)
        {
            CompleteAll(
                () =>
                {
                    if (reason == BreadcrumbDropDownCloseReason.Uncommitted)
                        _cancelSelection();
                },
                _focusAnchor
            );
        }

        internal void RestoreAfterOpenFailure()
        {
            bool closeNative = OpenState || DropDown.Visible;
            OpenState = false;
            CompleteAll(
                () =>
                {
                    if (closeNative)
                        CloseNative();
                },
                () => FinishClose(BreadcrumbDropDownCloseReason.Uncommitted)
            );
        }

        private void CompleteAll(params Action[] operations)
        {
            Exception? failure = null;
            foreach (Action operation in operations)
            {
                try
                {
                    operation();
                }
                catch (Exception exception)
                {
                    if (failure == null)
                        failure = exception;
                    else
                        _uiOperations.Report(exception);
                }
            }
            if (failure != null)
                throw failure;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BreadcrumbDropDownHost));
        }
    }
}
