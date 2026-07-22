#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
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
    using ReadySurface = Tuple<Control, IWebViewMessenger, Task>;
    using ReadySurfaceFactory = Func<
        CoreWebView2Environment,
        Task<Tuple<Control, IWebViewMessenger, Task>>
    >;

    /// <summary>Owns the ItemViewer-scoped popup and its lazy WebView surface.</summary>
    public sealed class BreadcrumbDropDownHost : IBreadcrumbDropDownHost
    {
        private readonly ReadySurfaceFactory _surfaceFactory;
        private readonly Action _focusPending;
        private readonly Action _focusAnchor;
        private readonly Action _cancelSelection;
        private readonly Action<ToolStripDropDown, Control, Point> _showPopup;
        private ToolStripControlHost? _controlHost;
        private Control? _popupControl;
        private IWebViewMessenger? _popupMessenger;
        private TaskCompletionSource<bool> _lifecycleCancellation = NewCompletionSource();
        private Task<bool>? _openTask;
        private long _lifecycleGeneration;
        private bool _isOpen;
        private bool _programmaticClose;
        private bool _disposed;

        /// <summary>Creates the lazy production popup using the existing WebView environment.</summary>
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
                BreadcrumbWebViewSurfaceFactory.Create(initializer, html),
                focusPending,
                focusAnchor,
                cancelSelection,
                ShowOwnedPopup
            ) { }

        /// <summary>Creates a host with legacy surface and display seams for unit tests.</summary>
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
                NormalizeFactory(surfaceFactory),
                focusPending,
                focusAnchor,
                cancelSelection,
                showPopup
            ) { }

        /// <summary>Creates a host whose surface reports document readiness separately.</summary>
        internal BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            ReadySurfaceFactory surfaceFactory,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection,
            Action<ToolStripDropDown, Control, Point> showPopup
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

            DropDown = new ToolStripDropDown
            {
                AutoClose = true,
                AutoSize = false,
                Padding = Padding.Empty,
            };
            DropDown.Closed += OnDropDownClosed;
        }

        /// <summary>The control whose top-level window owns the popup.</summary>
        public Control Anchor { get; }

        /// <summary>The existing WebView2 environment reused by the popup.</summary>
        public CoreWebView2Environment Environment { get; }

        /// <summary>The native popup owned for the full host lifetime.</summary>
        public ToolStripDropDown DropDown { get; }

        /// <summary>The hosted popup control wrapper after initialization.</summary>
        public ToolStripControlHost? ControlHost => _controlHost;

        /// <inheritdoc />
        public IWebViewMessenger? PopupMessenger => _popupMessenger;

        /// <inheritdoc />
        public bool IsOpen => _isOpen;

        /// <summary>The latest requested theme.</summary>
        public string Theme { get; private set; } = "light";

        /// <summary>The last current-lifecycle initialization or show failure.</summary>
        public Exception? LastInitializationException { get; private set; }

        /// <inheritdoc />
        public event EventHandler? PopupMessengerReady;

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
                _focusPending();
                return Task.FromResult(true);
            }
            if (_openTask != null)
                return _openTask;

            InvalidateLifecycle();
            LastInitializationException = null;
            long generation = _lifecycleGeneration;
            Task cancellation = _lifecycleCancellation.Task;
            TaskCompletionSource<bool> completion = NewCompletionSource();
            _openTask = completion.Task;
            _ = CompleteOpenAsync(
                anchorScreenBounds,
                workingArea,
                desiredSize,
                generation,
                cancellation,
                completion
            );
            return completion.Task;
        }

        private async Task CompleteOpenAsync(
            Rectangle anchorBounds,
            Rectangle workingArea,
            Size desiredSize,
            long generation,
            Task cancellation,
            TaskCompletionSource<bool> completion
        )
        {
            try
            {
                completion.TrySetResult(
                    await OpenCoreAsync(
                        anchorBounds,
                        workingArea,
                        desiredSize,
                        generation,
                        cancellation
                    )
                );
            }
            catch (Exception ex)
            {
                if (IsCurrent(generation, cancellation))
                {
                    LastInitializationException = ex;
                    RestoreAfterOpenFailure();
                }
                completion.TrySetResult(false);
            }
            finally
            {
                if (
                    generation == _lifecycleGeneration
                    && ReferenceEquals(_openTask, completion.Task)
                )
                    _openTask = null;
            }
        }

        private async Task<bool> OpenCoreAsync(
            Rectangle anchorBounds,
            Rectangle workingArea,
            Size desiredSize,
            long generation,
            Task cancellation
        )
        {
            if (!await EnsureSurfaceAsync(generation, cancellation))
                return false;
            if (!IsCurrent(generation, cancellation))
                return false;

            BreadcrumbPopupPlacementResult placement = BreadcrumbPopupPlacement.Calculate(
                anchorBounds,
                workingArea,
                desiredSize
            );
            if (placement.Bounds.Width == 0 || placement.Bounds.Height == 0)
            {
                if (!IsCurrent(generation, cancellation))
                    return false;
                LastInitializationException = new InvalidOperationException(
                    "The active working area has no space for the folder selector popup."
                );
                RestoreAfterOpenFailure();
                return false;
            }

            _controlHost!.Size = placement.Bounds.Size;
            _popupControl!.Size = placement.Bounds.Size;
            DropDown.Size = placement.Bounds.Size;
            _isOpen = true;
            try
            {
                _showPopup(DropDown, Anchor, placement.Bounds.Location);
                if (!IsCurrent(generation, cancellation) || !_isOpen)
                    return false;
                _focusPending();
                if (!IsCurrent(generation, cancellation) || !_isOpen)
                    return false;
                LastInitializationException = null;
                return true;
            }
            catch (Exception ex)
            {
                if (!IsCurrent(generation, cancellation))
                    return false;
                LastInitializationException = ex;
                CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, closeNative: false);
                return false;
            }
        }

        /// <inheritdoc />
        public bool Close(BreadcrumbDropDownCloseReason reason)
        {
            if (_disposed || !_isOpen)
                return false;
            CompleteClose(reason, closeNative: true);
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
            InvalidateLifecycle();
            if (_isOpen)
                CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, closeNative: true);
            DisposeSurface();
            LastInitializationException = null;
        }

        /// <summary>Closes, unhooks native events, and disposes owned resources.</summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            InvalidateLifecycle();
            if (_isOpen)
                CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, closeNative: true);
            DropDown.Closed -= OnDropDownClosed;
            DisposeSurface();
            DropDown.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task<bool> EnsureSurfaceAsync(long generation, Task cancellation)
        {
            if (_popupControl != null && _popupMessenger != null && _controlHost != null)
                return true;

            ReadySurface? created = null;
            bool installed = false;
            try
            {
                created = await _surfaceFactory(Environment);
                if (!IsCurrent(generation, cancellation))
                    return RejectCreatedSurface(created);
                if (created?.Item1 == null || created.Item2 == null || created.Item3 == null)
                {
                    throw new InvalidOperationException(
                        "Popup initialization did not provide a control, messenger, and readiness task."
                    );
                }
                if (!await WaitForReadinessAsync(created.Item3, cancellation))
                    return RejectCreatedSurface(created);
                if (!IsCurrent(generation, cancellation))
                    return RejectCreatedSurface(created);

                var host = new ToolStripControlHost(created.Item1)
                {
                    AutoSize = false,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                };
                _popupControl = created.Item1;
                _popupMessenger = created.Item2;
                _controlHost = host;
                installed = true;
                DropDown.Items.Add(host);
                PopupMessengerReady?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                if (!IsCurrent(generation, cancellation))
                {
                    if (!installed)
                        RejectCreatedSurface(created);
                    return false;
                }
                LastInitializationException = ex;
                if (!installed)
                    RejectCreatedSurface(created);
                DisposeSurface();
                RestoreAfterOpenFailure();
                return false;
            }
        }

        private void InvalidateLifecycle()
        {
            _lifecycleGeneration++;
            _lifecycleCancellation.TrySetResult(true);
            _lifecycleCancellation = NewCompletionSource();
            _openTask = null;
        }

        private bool IsCurrent(long generation, Task cancellation) =>
            !_disposed && generation == _lifecycleGeneration && !cancellation.IsCompleted;

        private static async Task<bool> WaitForReadinessAsync(Task readiness, Task cancellation)
        {
            Task completed = await Task.WhenAny(readiness, cancellation);
            if (!ReferenceEquals(completed, readiness))
                return false;
            await readiness;
            return !cancellation.IsCompleted;
        }

        private static bool RejectCreatedSurface(ReadySurface? created)
        {
            if (created?.Item1 != null && !created.Item1.IsDisposed)
                created.Item1.Dispose();
            (created?.Item2 as IDisposable)?.Dispose();
            return false;
        }

        private void CompleteClose(BreadcrumbDropDownCloseReason reason, bool closeNative)
        {
            if (!_isOpen)
                return;
            _isOpen = false;
            if (closeNative)
            {
                _programmaticClose = true;
                try
                {
                    DropDown.Close(ToolStripDropDownCloseReason.CloseCalled);
                }
                finally
                {
                    _programmaticClose = false;
                }
            }
            FinishClose(reason);
        }

        private void OnDropDownClosed(object? sender, ToolStripDropDownClosedEventArgs e)
        {
            if (_disposed || _programmaticClose || !_isOpen)
                return;
            _isOpen = false;
            FinishClose(BreadcrumbDropDownCloseReason.Uncommitted);
        }

        private void FinishClose(BreadcrumbDropDownCloseReason reason)
        {
            if (reason == BreadcrumbDropDownCloseReason.Uncommitted)
                _cancelSelection();
            _focusAnchor();
        }

        private void RestoreAfterOpenFailure()
        {
            _isOpen = false;
            FinishClose(BreadcrumbDropDownCloseReason.Uncommitted);
        }

        private void DisposeSurface()
        {
            IWebViewMessenger? messenger = _popupMessenger;
            ToolStripControlHost? host = _controlHost;
            Control? control = _popupControl;
            _popupMessenger = null;
            _controlHost = null;
            _popupControl = null;
            if (host != null)
            {
                DropDown.Items.Remove(host);
                host.Dispose();
            }
            if (control != null && !control.IsDisposed)
                control.Dispose();
            (messenger as IDisposable)?.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BreadcrumbDropDownHost));
        }

        private static ReadySurfaceFactory NormalizeFactory(LegacySurfaceFactory surfaceFactory)
        {
            if (surfaceFactory == null)
                throw new ArgumentNullException(nameof(surfaceFactory));
            return async environment =>
            {
                Tuple<Control, IWebViewMessenger> created = await surfaceFactory(environment);
                if (created == null)
                    throw new InvalidOperationException(
                        "Popup initialization returned no surface."
                    );
                return Tuple.Create<Control, IWebViewMessenger, Task>(
                    created.Item1,
                    created.Item2,
                    Task.CompletedTask
                );
            };
        }

        private static TaskCompletionSource<bool> NewCompletionSource() =>
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Direct WinForms display adapter; placement is tested through the injected callback.
        [ExcludeFromCodeCoverage]
        private static void ShowOwnedPopup(
            ToolStripDropDown dropDown,
            Control anchor,
            Point screenLocation
        ) => dropDown.Show(anchor, anchor.PointToClient(screenLocation));
    }
}
