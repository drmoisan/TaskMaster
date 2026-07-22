#nullable enable
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace QuickFiler.Viewers
{
    using LegacySurfaceFactory = Func<
        CoreWebView2Environment,
        Task<Tuple<Control, IWebViewMessenger>>
    >;
    using ReadySurfaceFactory = Func<
        CoreWebView2Environment,
        Task<Tuple<Control, IWebViewMessenger, Task>>
    >;

    /// <summary>Owns the native popup and its lazily created breadcrumb surface.</summary>
    public sealed class BreadcrumbDropDownHost : IBreadcrumbDropDownHost
    {
        private readonly ReadySurfaceFactory _surfaceFactory;
        private readonly BreadcrumbPopupUiOperations _uiOperations;
        private readonly BreadcrumbDropDownOpenLifetime _openLifetime;
        private readonly Action _focusPending;
        private readonly Action _focusAnchor;
        private readonly Action _cancelSelection;
        private readonly Action<ToolStripDropDown, Control, Point> _showPopup;
        private readonly Action<ToolStripDropDown, ToolStripDropDownCloseReason> _closePopup;
        private ToolStripControlHost? _controlHost;
        private Control? _popupControl;
        private IWebViewMessenger? _popupMessenger;
        private bool _isOpen;
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
            _surfaceFactory =
                surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory));
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
        public ToolStripControlHost? ControlHost => _controlHost;

        /// <inheritdoc />
        public IWebViewMessenger? PopupMessenger => _popupMessenger;

        /// <inheritdoc />
        public bool IsOpen => _isOpen;

        /// <summary>The retained popup theme.</summary>
        public string Theme { get; private set; } = "light";

        /// <summary>The latest initialization or open failure.</summary>
        public Exception? LastInitializationException { get; internal set; }

        /// <inheritdoc />
        public event EventHandler? PopupMessengerReady;

        internal ReadySurfaceFactory SurfaceFactory => _surfaceFactory;

        internal ToolStripControlHost? InstalledControlHost
        {
            get => _controlHost;
            set => _controlHost = value;
        }

        internal Control? InstalledPopupControl
        {
            get => _popupControl;
            set => _popupControl = value;
        }

        internal IWebViewMessenger? InstalledPopupMessenger
        {
            get => _popupMessenger;
            set => _popupMessenger = value;
        }

        internal bool HasInstalledSurface =>
            _controlHost != null && _popupControl != null && _popupMessenger != null;

        internal bool OpenState
        {
            get => _isOpen;
            set => _isOpen = value;
        }

        /// <inheritdoc />
        public Task<bool> OpenAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize
        )
        {
            ThrowIfDisposed();
            if (_isOpen)
            {
                _openLifetime.Schedule(_focusPending);
                return Task.FromResult(true);
            }
            LastInitializationException = null;
            return _openLifetime.OpenAsync(anchorScreenBounds, workingArea, desiredSize);
        }

        /// <inheritdoc />
        public bool Close(BreadcrumbDropDownCloseReason reason)
        {
            if (_disposed || !_isOpen)
                return false;
            _openLifetime.InvalidateAndSchedule(() => CompleteClose(reason, closeNative: true));
            return true;
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
                        if (_isOpen)
                            CompleteClose(
                                BreadcrumbDropDownCloseReason.Uncommitted,
                                closeNative: true
                            );
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
                }
            }
        }

        private Task DisposeCoreAsync() =>
            _uiOperations.RunAsync(() =>
            {
                Tuple<ToolStripControlHost?, Control?, IWebViewMessenger?> owned =
                    TakeOwnedSurface();
                _isOpen = false;
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
            Tuple<ToolStripControlHost?, Control?, IWebViewMessenger?> owned = await _uiOperations
                .RunAsync(TakeOwnedSurface)
                .ConfigureAwait(false);
            await _uiOperations
                .DisposeHostedSurfaceAsync(DropDown, owned.Item1, owned.Item2, owned.Item3)
                .ConfigureAwait(false);
        }

        internal async Task DisposeSurfaceAfterFailureAsync()
        {
            Tuple<ToolStripControlHost?, Control?, IWebViewMessenger?> owned = await _uiOperations
                .RunAsync(TakeOwnedSurface, reportFailure: false)
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

        private Tuple<ToolStripControlHost?, Control?, IWebViewMessenger?> TakeOwnedSurface()
        {
            var owned = Tuple.Create(_controlHost, _popupControl, _popupMessenger);
            _controlHost = null;
            _popupControl = null;
            _popupMessenger = null;
            return owned;
        }

        private void CompleteClose(BreadcrumbDropDownCloseReason reason, bool closeNative)
        {
            if (!_isOpen)
                return;
            _isOpen = false;
            CompleteAll(
                () =>
                {
                    if (closeNative)
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
            if (_disposed || _programmaticClose || !_isOpen)
                return;
            _openLifetime.InvalidateAndSchedule(() =>
            {
                if (_disposed || _programmaticClose || !_isOpen)
                    return;
                _isOpen = false;
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
            bool closeNative = _isOpen || DropDown.Visible;
            _isOpen = false;
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
