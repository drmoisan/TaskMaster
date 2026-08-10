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
    /// <summary>Owns the WinForms wrappers for the breadcrumb selector lifecycle coordinator.</summary>
    public partial class ItemViewer
    {
        private BreadcrumbItemViewerLifecycleCoordinator _breadcrumbLifecycleCoordinator;
        private BreadcrumbResourceOwner _breadcrumbResourceOwner;

        /// <summary>The Designer-declared breadcrumb WebView2 occupying the old CboFolders cell.</summary>
        public Microsoft.Web.WebView2.WinForms.WebView2 L0vhBreadcrumb_WebView2
        {
            get => _l0vhBreadcrumb_WebView2;
            set => _l0vhBreadcrumb_WebView2 = value;
        }

        internal BreadcrumbBridgeCoordinator BreadcrumbCoordinator { get; private set; }
        internal IBreadcrumbDropDownHost BreadcrumbDropDownHost =>
            _breadcrumbLifecycleCoordinator?.DropDownHost;

        internal Task<bool> BreadcrumbOpenTask =>
            _breadcrumbLifecycleCoordinator?.CurrentOpenTask ?? Task.FromResult(false);

        internal event EventHandler<BreadcrumbArrowDirection> BreadcrumbUnhandledArrow;

        private EventHandler _folderSelectionChangedHandlers;
        private KeyEventHandler _folderKeyDownHandlers;

        internal void InitializeBreadcrumbPipeline(IFolderHierarchyProvider provider) =>
            InitializeBreadcrumbPipeline(provider, BreadcrumbPopupUiOperations.CaptureCurrent());

        internal void InitializeBreadcrumbPipeline(
            IFolderHierarchyProvider provider,
            BreadcrumbPopupUiOperations operations
        )
        {
            if (BreadcrumbCoordinator != null)
            {
                return;
            }

            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(
                operations
            );
            var bridgeCoordinator = new BreadcrumbBridgeCoordinator(
                lifecycle.Hub,
                provider,
                BreadcrumbUiDispatcher.CaptureCurrent()
            );
            lifecycle.SetBridgeCoordinator(bridgeCoordinator);
            BreadcrumbCoordinator = bridgeCoordinator;
        }

        internal Task<bool> AttachBreadcrumbWebViewAsync() =>
            AttachBreadcrumbWebViewAsync(CreateCollapsedBreadcrumbCandidate);

        internal Task<bool> AttachBreadcrumbWebViewAsync(
            Func<Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness>> candidateFactory
        )
        {
            if (_breadcrumbLifecycleCoordinator == null)
            {
                return Task.FromResult(false);
            }

            return _breadcrumbLifecycleCoordinator.AttachCollapsedAsync(candidateFactory);
        }

        private Tuple<
            IWebViewMessenger,
            BreadcrumbNavigationReadiness
        > CreateCollapsedBreadcrumbCandidate()
        {
            CoreWebView2 core = _l0vhBreadcrumb_WebView2.CoreWebView2;
            BreadcrumbUiDispatcher dispatcher = BreadcrumbUiDispatcher.CaptureCurrent();
            return BreadcrumbPopupLifecycleOperations.CreateCollapsedCandidate(
                () => new WebView2Messenger(core, dispatcher),
                () =>
                    BreadcrumbPopupUiOperations.NavigateToDocument(
                        dispatcher,
                        core,
                        _l0vhBreadcrumb_WebView2,
                        () =>
                            _l0vhBreadcrumb_WebView2.NavigateToString(
                                Properties.Resources.FolderBreadcrumb
                            ),
                        "Collapsed"
                    )
            );
        }

        internal Task<bool> AttachBreadcrumbMessengerWhenReadyAsync(
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness
        )
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }
            if (readiness == null)
            {
                throw new ArgumentNullException(nameof(readiness));
            }
            if (_breadcrumbLifecycleCoordinator == null)
            {
                throw new InvalidOperationException(
                    "The breadcrumb pipeline must be initialized before attaching a surface."
                );
            }

            return _breadcrumbLifecycleCoordinator.AttachCollapsedWithReadinessAsync(
                messenger,
                readiness
            );
        }

        internal void AttachBreadcrumbMessenger(IWebViewMessenger messenger)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }
            if (_breadcrumbLifecycleCoordinator == null)
            {
                throw new InvalidOperationException(
                    "The breadcrumb pipeline must be initialized before attaching a surface."
                );
            }

            _breadcrumbLifecycleCoordinator.AttachCollapsedMessenger(messenger);
        }

        internal void ConfigureBreadcrumbDropDown(
            CoreWebView2Environment environment,
            IWebViewCoreInitializer initializer
        )
        {
            if (
                BreadcrumbDropDownHost is BreadcrumbDropDownHost existing
                && ReferenceEquals(existing.Environment, environment)
            )
            {
                return;
            }

            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests()
            );
            BreadcrumbDropDownHost host = null;
            host = new BreadcrumbDropDownHost(
                _l0vhBreadcrumb_WebView2,
                environment,
                initializer,
                Properties.Resources.FolderBreadcrumb,
                () => host.ControlHost?.Control.Focus(),
                FocusBreadcrumbCore,
                () => BreadcrumbCoordinator?.CancelSelector(),
                lifecycle.Operations
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
            _ = anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
            _ = workingArea ?? throw new ArgumentNullException(nameof(workingArea));
            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests()
            );
            lifecycle.ConfigureHost(host, anchorBounds, workingArea);
        }

        internal void SetBreadcrumbTheme(string theme) =>
            _breadcrumbLifecycleCoordinator?.SetTheme(theme);

        internal void FocusBreadcrumb()
        {
            if (_breadcrumbLifecycleCoordinator == null)
            {
                FocusBreadcrumbCore();
                return;
            }

            _breadcrumbLifecycleCoordinator.Focus(FocusBreadcrumbCore);
        }

        private void FocusBreadcrumbCore()
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

        internal void SetBreadcrumbDropDownState(bool droppedDown)
        {
            if (_breadcrumbLifecycleCoordinator == null)
            {
                if (droppedDown)
                {
                    FocusBreadcrumb();
                }
                return;
            }

            _breadcrumbLifecycleCoordinator.SetDroppedDown(droppedDown, FocusBreadcrumbCore);
        }

        /// <summary>
        /// Issue #438: presents a folder-search result set without transferring focus to the
        /// breadcrumb surface.
        /// </summary>
        /// <remarks>
        /// The non-focusing counterpart of <see cref="SetBreadcrumbDropDownState"/>. The bare-viewer
        /// branch (no lifecycle coordinator) deliberately performs no <c>FocusBreadcrumb()</c> call:
        /// that call is the fallback branch's focus steal, and a keystroke must leave the caret in
        /// the search textbox. <see cref="SetBreadcrumbDropDownState"/> itself is unchanged, so every
        /// explicit gesture keeps its current focus-on-open semantics.
        /// </remarks>
        internal void PresentBreadcrumbSearchResults(string[] items)
        {
            if (_breadcrumbLifecycleCoordinator == null)
            {
                return;
            }

            _breadcrumbLifecycleCoordinator.PresentSearchResults(items);
        }

        internal void ResetBreadcrumb() => _breadcrumbLifecycleCoordinator?.Reset();

        private void OnBreadcrumbSelectionChanged() =>
            _folderSelectionChangedHandlers?.Invoke(this, EventArgs.Empty);

        private void OnBreadcrumbFolderArrowKeyDown(BreadcrumbArrowDirection direction) =>
            _folderKeyDownHandlers?.Invoke(
                this,
                new KeyEventArgs(
                    direction == BreadcrumbArrowDirection.Right ? Keys.Right : Keys.Left
                )
            );

        private void OnBreadcrumbUnhandledArrow(BreadcrumbArrowDirection direction) =>
            BreadcrumbUnhandledArrow?.Invoke(this, direction);

        private BreadcrumbItemViewerLifecycleCoordinator EnsureBreadcrumbLifecycle(
            BreadcrumbPopupUiOperations operations
        )
        {
            if (_breadcrumbLifecycleCoordinator != null)
            {
                return _breadcrumbLifecycleCoordinator;
            }

            EnsureBreadcrumbResourceOwnership();
            var hub = new BreadcrumbMessengerHub();
            var attachment = new BreadcrumbCollapsedAttachment(
                hub,
                new BreadcrumbCollapsedSurfaceController()
            );
            _breadcrumbLifecycleCoordinator = new BreadcrumbItemViewerLifecycleCoordinator(
                hub,
                attachment,
                operations,
                OnBreadcrumbSelectionChanged,
                OnBreadcrumbFolderArrowKeyDown,
                OnBreadcrumbUnhandledArrow
            );
            return _breadcrumbLifecycleCoordinator;
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
            _breadcrumbLifecycleCoordinator?.Dispose();
            _breadcrumbLifecycleCoordinator = null;
            BreadcrumbCoordinator = null;
        }
    }
}
