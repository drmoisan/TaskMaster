#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Owns the ItemViewer-scoped ToolStrip popup and one lazily initialized WebView surface. All
    /// placement inputs and host callbacks are injected so behavior is deterministic without a
    /// live display; the production overload creates a WebView2 with the existing environment.
    /// </summary>
    public sealed class BreadcrumbDropDownHost : IBreadcrumbDropDownHost
    {
        private readonly Func<
            CoreWebView2Environment,
            Task<Tuple<Control, IWebViewMessenger>>
        > _surfaceFactory;
        private readonly Action _focusPending;
        private readonly Action _focusAnchor;
        private readonly Action _cancelSelection;
        private readonly Action<ToolStripDropDown, Control, Point> _showPopup;
        private ToolStripControlHost? _controlHost;
        private Control? _popupControl;
        private IWebViewMessenger? _popupMessenger;
        private bool _isOpen;
        private bool _programmaticClose;
        private bool _disposed;

        /// <summary>
        /// Creates the production host. The popup WebView is not created until the first open and
        /// is initialized with the supplied existing environment.
        /// </summary>
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
                CreateProductionFactory(initializer, html),
                focusPending,
                focusAnchor,
                cancelSelection,
                ShowOwnedPopup
            ) { }

        /// <summary>
        /// Creates a host with deterministic popup surface and show seams for unit testing.
        /// </summary>
        public BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            Func<CoreWebView2Environment, Task<Tuple<Control, IWebViewMessenger>>> surfaceFactory,
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

        /// <summary>The existing WebView2 environment reused by the lazy popup surface.</summary>
        public CoreWebView2Environment Environment { get; }

        /// <summary>The native popup owned for the full host lifetime.</summary>
        public ToolStripDropDown DropDown { get; }

        /// <summary>The hosted popup control wrapper after initialization.</summary>
        public ToolStripControlHost? ControlHost => _controlHost;

        /// <inheritdoc />
        public IWebViewMessenger? PopupMessenger => _popupMessenger;

        /// <inheritdoc />
        public bool IsOpen => _isOpen;

        /// <summary>The latest requested theme, replayed by the messenger hub on attachment.</summary>
        public string Theme { get; private set; } = "light";

        /// <summary>The last lazy initialization/show failure, or null after a successful open.</summary>
        public Exception? LastInitializationException { get; private set; }

        /// <inheritdoc />
        public event EventHandler? PopupMessengerReady;

        /// <inheritdoc />
        public async Task<bool> OpenAsync(
            Rectangle anchorScreenBounds,
            Rectangle workingArea,
            Size desiredSize
        )
        {
            ThrowIfDisposed();
            if (_isOpen)
            {
                _focusPending();
                return true;
            }
            if (!await EnsureSurfaceAsync())
            {
                return false;
            }

            BreadcrumbPopupPlacementResult placement = BreadcrumbPopupPlacement.Calculate(
                anchorScreenBounds,
                workingArea,
                desiredSize
            );
            if (placement.Bounds.Width == 0 || placement.Bounds.Height == 0)
            {
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
                _focusPending();
                LastInitializationException = null;
                return true;
            }
            catch (Exception ex)
            {
                LastInitializationException = ex;
                CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, closeNative: false);
                return false;
            }
        }

        /// <inheritdoc />
        public bool Close(BreadcrumbDropDownCloseReason reason)
        {
            if (_disposed || !_isOpen)
            {
                return false;
            }
            CompleteClose(reason, closeNative: true);
            return true;
        }

        /// <inheritdoc />
        public void SetTheme(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
            {
                throw new ArgumentException("A non-empty theme is required.", nameof(theme));
            }
            ThrowIfDisposed();
            Theme = theme;
        }

        /// <inheritdoc />
        public void Reset()
        {
            ThrowIfDisposed();
            if (_isOpen)
            {
                CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, closeNative: true);
            }
            DisposeSurface();
            LastInitializationException = null;
        }

        /// <summary>Closes, unhooks native events, and disposes partial or complete resources.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            if (_isOpen)
            {
                CompleteClose(BreadcrumbDropDownCloseReason.Uncommitted, closeNative: true);
            }
            DropDown.Closed -= OnDropDownClosed;
            DisposeSurface();
            DropDown.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private async Task<bool> EnsureSurfaceAsync()
        {
            if (_popupControl != null && _popupMessenger != null && _controlHost != null)
            {
                return true;
            }

            Tuple<Control, IWebViewMessenger>? created = null;
            try
            {
                created = await _surfaceFactory(Environment);
                if (created?.Item1 == null || created.Item2 == null)
                {
                    throw new InvalidOperationException(
                        "Popup initialization did not provide both a control and a messenger."
                    );
                }

                _popupControl = created.Item1;
                _popupMessenger = created.Item2;
                _controlHost = new ToolStripControlHost(_popupControl)
                {
                    AutoSize = false,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty,
                };
                DropDown.Items.Add(_controlHost);
                PopupMessengerReady?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                LastInitializationException = ex;
                if (created?.Item1 != null && !ReferenceEquals(created.Item1, _popupControl))
                {
                    created.Item1.Dispose();
                }
                DisposeSurface();
                RestoreAfterOpenFailure();
                return false;
            }
        }

        private void CompleteClose(BreadcrumbDropDownCloseReason reason, bool closeNative)
        {
            if (!_isOpen)
            {
                return;
            }

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
            {
                return;
            }
            _isOpen = false;
            FinishClose(BreadcrumbDropDownCloseReason.Uncommitted);
        }

        private void FinishClose(BreadcrumbDropDownCloseReason reason)
        {
            if (reason == BreadcrumbDropDownCloseReason.Uncommitted)
            {
                _cancelSelection();
            }
            _focusAnchor();
        }

        private void RestoreAfterOpenFailure()
        {
            _isOpen = false;
            _cancelSelection();
            _focusAnchor();
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
            {
                control.Dispose();
            }
            (messenger as IDisposable)?.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BreadcrumbDropDownHost));
            }
        }

        private static Func<
            CoreWebView2Environment,
            Task<Tuple<Control, IWebViewMessenger>>
        > CreateProductionFactory(IWebViewCoreInitializer initializer, string html)
        {
            if (initializer == null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }
            if (html == null)
            {
                throw new ArgumentNullException(nameof(html));
            }
            return environment => CreateProductionSurfaceAsync(initializer, environment, html);
        }

        // This method is a direct third-party WebView2 adapter. Its surrounding lifecycle and
        // failure handling are covered through the injected surface factory.
        [ExcludeFromCodeCoverage]
        private static async Task<Tuple<Control, IWebViewMessenger>> CreateProductionSurfaceAsync(
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
                webView.NavigateToString(html);
                return Tuple.Create<Control, IWebViewMessenger>(
                    webView,
                    new WebView2Messenger(core)
                );
            }
            catch
            {
                webView.Dispose();
                throw;
            }
        }

        // This method is a direct WinForms display adapter. Placement and ownership are covered
        // deterministically through the injected show callback without requiring a live display.
        [ExcludeFromCodeCoverage]
        private static void ShowOwnedPopup(
            ToolStripDropDown dropDown,
            Control anchor,
            Point screenLocation
        )
        {
            dropDown.Show(anchor, anchor.PointToClient(screenLocation));
        }
    }
}
