using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler
{
    // WinForms ownership and lifecycle glue for the host-neutral breadcrumb selector. The
    // coordinator owns selection behavior; this partial owns the collapsed WebView, native popup,
    // two-surface messenger hub, and deterministic pooled-viewer cleanup.
    public partial class ItemViewer
    {
        private BreadcrumbMessengerHub _breadcrumbHub;
        private IWebViewMessenger _breadcrumbMessenger;
        private IWebViewMessenger _breadcrumbPopupMessenger;
        private IBreadcrumbDropDownHost _breadcrumbDropDownHost;
        private Func<Rectangle> _breadcrumbAnchorBounds;
        private Func<Rectangle> _breadcrumbWorkingArea;
        private BreadcrumbResourceOwner _breadcrumbResourceOwner;

        /// <summary>The Designer-declared breadcrumb WebView2 occupying the old CboFolders cell.</summary>
        public Microsoft.Web.WebView2.WinForms.WebView2 L0vhBreadcrumb_WebView2
        {
            get => _l0vhBreadcrumb_WebView2;
            set => _l0vhBreadcrumb_WebView2 = value;
        }

        /// <summary>The breadcrumb coordinator after the controller initializes the pipeline.</summary>
        internal BreadcrumbBridgeCoordinator BreadcrumbCoordinator { get; private set; }

        /// <summary>The ItemViewer-owned native popup host.</summary>
        internal IBreadcrumbDropDownHost BreadcrumbDropDownHost => _breadcrumbDropDownHost;

        /// <summary>Raised when the breadcrumb reports an arrow it could not consume.</summary>
        internal event EventHandler<BreadcrumbArrowDirection> BreadcrumbUnhandledArrow;

        private EventHandler _folderSelectionChangedHandlers;
        private KeyEventHandler _folderKeyDownHandlers;

        /// <summary>Creates the idempotent two-surface hub and host-neutral coordinator.</summary>
        internal void InitializeBreadcrumbPipeline(IFolderHierarchyProvider provider)
        {
            if (BreadcrumbCoordinator != null)
            {
                return;
            }

            EnsureBreadcrumbResourceOwnership();
            _breadcrumbHub = new BreadcrumbMessengerHub();
            BreadcrumbCoordinator = new BreadcrumbBridgeCoordinator(_breadcrumbHub, provider);
            BreadcrumbCoordinator.SelectionChanged += OnBreadcrumbSelectionChanged;
            BreadcrumbCoordinator.FolderArrowKeyDown += OnBreadcrumbFolderArrowKeyDown;
            BreadcrumbCoordinator.UnhandledArrow += OnBreadcrumbUnhandledArrow;
            BreadcrumbCoordinator.SelectorOpenStateChanged += OnBreadcrumbSelectorOpenStateChanged;
        }

        /// <summary>Loads and attaches the persistent one-row collapsed WebView surface.</summary>
        internal void AttachBreadcrumbWebView()
        {
            if (_breadcrumbHub == null || _breadcrumbMessenger != null)
            {
                return;
            }

            _l0vhBreadcrumb_WebView2.NavigateToString(Properties.Resources.FolderBreadcrumb);
            AttachBreadcrumbMessenger(new WebView2Messenger(_l0vhBreadcrumb_WebView2.CoreWebView2));
        }

        /// <summary>Attaches the collapsed surface exactly once, replacing any prior surface.</summary>
        internal void AttachBreadcrumbMessenger(IWebViewMessenger messenger)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }
            if (_breadcrumbHub == null)
            {
                throw new InvalidOperationException(
                    "The breadcrumb pipeline must be initialized before attaching a surface."
                );
            }
            if (ReferenceEquals(_breadcrumbMessenger, messenger))
            {
                _breadcrumbHub.Attach(messenger, BreadcrumbSelectorViewMode.Collapsed);
                return;
            }

            if (_breadcrumbMessenger != null)
            {
                _breadcrumbHub.Detach(_breadcrumbMessenger);
            }
            _breadcrumbMessenger = messenger;
            _breadcrumbHub.Attach(messenger, BreadcrumbSelectorViewMode.Collapsed);
        }

        /// <summary>Creates the production lazy popup using the controller's existing environment.</summary>
        internal void ConfigureBreadcrumbDropDown(
            CoreWebView2Environment environment,
            IWebViewCoreInitializer initializer
        )
        {
            BreadcrumbDropDownHost host = null;
            host = new BreadcrumbDropDownHost(
                _l0vhBreadcrumb_WebView2,
                environment,
                initializer,
                Properties.Resources.FolderBreadcrumb,
                () => host.ControlHost?.Control.Focus(),
                FocusBreadcrumb,
                () => BreadcrumbCoordinator?.CancelSelector()
            );
            ConfigureBreadcrumbDropDown(
                host,
                () =>
                    _l0vhBreadcrumb_WebView2.RectangleToScreen(
                        _l0vhBreadcrumb_WebView2.ClientRectangle
                    ),
                () => Screen.FromControl(_l0vhBreadcrumb_WebView2).WorkingArea
            );
        }

        /// <summary>Configures deterministic host and placement seams for the ItemViewer.</summary>
        internal void ConfigureBreadcrumbDropDown(
            IBreadcrumbDropDownHost host,
            Func<Rectangle> anchorBounds,
            Func<Rectangle> workingArea
        )
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }
            _breadcrumbAnchorBounds =
                anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
            _breadcrumbWorkingArea =
                workingArea ?? throw new ArgumentNullException(nameof(workingArea));
            EnsureBreadcrumbResourceOwnership();

            if (!ReferenceEquals(_breadcrumbDropDownHost, host))
            {
                ReleaseBreadcrumbDropDownHost();
                _breadcrumbDropDownHost = host;
            }

            host.PopupMessengerReady -= OnBreadcrumbPopupMessengerReady;
            host.PopupMessengerReady += OnBreadcrumbPopupMessengerReady;
            if (host.PopupMessenger != null)
            {
                AttachBreadcrumbPopupMessenger(host.PopupMessenger);
            }
        }

        /// <summary>Routes a theme update to both selector surfaces and the native host.</summary>
        internal void SetBreadcrumbTheme(string theme)
        {
            BreadcrumbCoordinator?.SetTheme(theme);
            _breadcrumbDropDownHost?.SetTheme(theme);
        }

        /// <summary>Moves focus to the persistent collapsed selector.</summary>
        internal void FocusBreadcrumb()
        {
            if (
                !IsDisposed
                && _l0vhBreadcrumb_WebView2 != null
                && !_l0vhBreadcrumb_WebView2.IsDisposed
            )
            {
                _l0vhBreadcrumb_WebView2.Focus();
            }
        }

        /// <summary>Opens or closes the native selector with combo-box session semantics.</summary>
        internal void SetBreadcrumbDropDownState(bool droppedDown)
        {
            if (BreadcrumbCoordinator == null)
            {
                if (droppedDown)
                {
                    FocusBreadcrumb();
                }
                return;
            }

            if (droppedDown)
            {
                if (
                    !BreadcrumbCoordinator.OpenSelector()
                    && _breadcrumbDropDownHost?.IsOpen == true
                )
                {
                    _ = OpenBreadcrumbDropDownAsync();
                }
                return;
            }

            if (_breadcrumbDropDownHost?.IsOpen == true)
            {
                _breadcrumbDropDownHost.Close(BreadcrumbDropDownCloseReason.Uncommitted);
            }
            else
            {
                BreadcrumbCoordinator.CancelSelector();
            }
        }

        /// <summary>Cancels open work, releases the lazy popup surface, and clears pooled state.</summary>
        internal void ResetBreadcrumb()
        {
            if (_breadcrumbDropDownHost?.IsOpen == true)
            {
                _breadcrumbDropDownHost.Close(BreadcrumbDropDownCloseReason.Uncommitted);
            }
            else
            {
                BreadcrumbCoordinator?.CancelSelector();
            }

            DetachBreadcrumbPopupMessenger();
            _breadcrumbDropDownHost?.Reset();
            BreadcrumbCoordinator?.Clear();
        }

        private void OnBreadcrumbSelectionChanged(object sender, EventArgs e) =>
            _folderSelectionChangedHandlers?.Invoke(this, EventArgs.Empty);

        private void OnBreadcrumbFolderArrowKeyDown(
            object sender,
            BreadcrumbArrowDirection direction
        ) =>
            _folderKeyDownHandlers?.Invoke(
                this,
                new KeyEventArgs(
                    direction == BreadcrumbArrowDirection.Right ? Keys.Right : Keys.Left
                )
            );

        private void OnBreadcrumbUnhandledArrow(
            object sender,
            BreadcrumbArrowDirection direction
        ) => BreadcrumbUnhandledArrow?.Invoke(this, direction);

        private async void OnBreadcrumbSelectorOpenStateChanged(object sender, EventArgs e)
        {
            if (BreadcrumbCoordinator?.IsSelectorOpen == true)
            {
                await OpenBreadcrumbDropDownAsync();
            }
            else if (_breadcrumbDropDownHost?.IsOpen == true)
            {
                // The coordinator has already committed or rolled back. This close only dismisses
                // the native surface and must not apply a second selection transition.
                _breadcrumbDropDownHost.Close(BreadcrumbDropDownCloseReason.ExplicitCommit);
            }
        }

        private async Task<bool> OpenBreadcrumbDropDownAsync()
        {
            IBreadcrumbDropDownHost host = _breadcrumbDropDownHost;
            Func<Rectangle> anchorBounds = _breadcrumbAnchorBounds;
            Func<Rectangle> workingArea = _breadcrumbWorkingArea;
            if (host == null || anchorBounds == null || workingArea == null)
            {
                FocusBreadcrumb();
                BreadcrumbCoordinator?.CancelSelector();
                return false;
            }

            Rectangle anchor = anchorBounds();
            int rowCount = BreadcrumbCoordinator?.GetFolderItems().Length ?? 0;
            var desiredSize = new Size(anchor.Width, Math.Min(320, Math.Max(120, rowCount * 26)));
            bool opened = await host.OpenAsync(anchor, workingArea(), desiredSize);
            if (!opened)
            {
                BreadcrumbCoordinator?.CancelSelector();
            }
            else if (BreadcrumbCoordinator?.IsSelectorOpen != true && host.IsOpen)
            {
                host.Close(BreadcrumbDropDownCloseReason.ExplicitCommit);
            }
            return opened;
        }

        private void OnBreadcrumbPopupMessengerReady(object sender, EventArgs e)
        {
            IWebViewMessenger messenger = _breadcrumbDropDownHost?.PopupMessenger;
            if (messenger != null)
            {
                AttachBreadcrumbPopupMessenger(messenger);
            }
        }

        private void AttachBreadcrumbPopupMessenger(IWebViewMessenger messenger)
        {
            if (_breadcrumbHub == null)
            {
                return;
            }
            if (ReferenceEquals(_breadcrumbPopupMessenger, messenger))
            {
                _breadcrumbHub.Attach(messenger, BreadcrumbSelectorViewMode.Expanded);
                return;
            }

            DetachBreadcrumbPopupMessenger();
            _breadcrumbPopupMessenger = messenger;
            _breadcrumbHub.Attach(messenger, BreadcrumbSelectorViewMode.Expanded);
        }

        private void DetachBreadcrumbPopupMessenger()
        {
            if (_breadcrumbPopupMessenger == null)
            {
                return;
            }
            _breadcrumbHub?.Detach(_breadcrumbPopupMessenger);
            _breadcrumbPopupMessenger = null;
        }

        private void ReleaseBreadcrumbDropDownHost()
        {
            if (_breadcrumbDropDownHost == null)
            {
                return;
            }

            _breadcrumbDropDownHost.PopupMessengerReady -= OnBreadcrumbPopupMessengerReady;
            DetachBreadcrumbPopupMessenger();
            _breadcrumbDropDownHost.Dispose();
            _breadcrumbDropDownHost = null;
        }

        private void EnsureBreadcrumbResourceOwnership()
        {
            if (_breadcrumbResourceOwner != null)
            {
                return;
            }

            components ??= new Container();
            _breadcrumbResourceOwner = new BreadcrumbResourceOwner(DisposeBreadcrumbResources);
            components.Add(_breadcrumbResourceOwner);
        }

        private void DisposeBreadcrumbResources()
        {
            if (BreadcrumbCoordinator != null)
            {
                BreadcrumbCoordinator.SelectionChanged -= OnBreadcrumbSelectionChanged;
                BreadcrumbCoordinator.FolderArrowKeyDown -= OnBreadcrumbFolderArrowKeyDown;
                BreadcrumbCoordinator.UnhandledArrow -= OnBreadcrumbUnhandledArrow;
                BreadcrumbCoordinator.SelectorOpenStateChanged -=
                    OnBreadcrumbSelectorOpenStateChanged;
            }

            if (_breadcrumbMessenger != null)
            {
                _breadcrumbHub?.Detach(_breadcrumbMessenger);
                _breadcrumbMessenger = null;
            }
            ReleaseBreadcrumbDropDownHost();
            _breadcrumbHub?.Dispose();
            _breadcrumbHub = null;
            BreadcrumbCoordinator = null;
        }

        private sealed class BreadcrumbResourceOwner : Component
        {
            private Action _dispose;

            internal BreadcrumbResourceOwner(Action dispose)
            {
                _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Action dispose = _dispose;
                    _dispose = null;
                    dispose?.Invoke();
                }
                base.Dispose(disposing);
            }
        }
    }
}
