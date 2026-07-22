using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using QuickFiler.Viewers;
using UtilitiesCS.OutlookObjects.Folder;
using BreadcrumbViewMode = UtilitiesCS.OutlookObjects.Folder.BreadcrumbSelectorViewMode;

namespace QuickFiler
{
    /// <summary>Owns the WinForms surfaces and lifecycle glue for the breadcrumb selector.</summary>
    public partial class ItemViewer
    {
        private BreadcrumbMessengerHub _breadcrumbHub;
        private BreadcrumbCollapsedSurfaceController _breadcrumbCollapsedSurfaceController;
        private BreadcrumbCollapsedAttachment _breadcrumbCollapsedAttachment;
        private IWebViewMessenger _breadcrumbMessenger;
        private IWebViewMessenger _popupMessenger;
        private BreadcrumbDropDownOpenCoordinator _breadcrumbDropDownOpenCoordinator;
        private BreadcrumbResourceOwner _breadcrumbResourceOwner;
        private BreadcrumbPopupUiOperations _breadcrumbPopupUiOperations;

        /// <summary>The Designer-declared breadcrumb WebView2 occupying the old CboFolders cell.</summary>
        public Microsoft.Web.WebView2.WinForms.WebView2 L0vhBreadcrumb_WebView2
        {
            get => _l0vhBreadcrumb_WebView2;
            set => _l0vhBreadcrumb_WebView2 = value;
        }

        internal BreadcrumbBridgeCoordinator BreadcrumbCoordinator { get; private set; }
        internal IBreadcrumbDropDownHost BreadcrumbDropDownHost =>
            _breadcrumbDropDownOpenCoordinator?.Host;

        internal Task<bool> BreadcrumbOpenTask =>
            _breadcrumbDropDownOpenCoordinator?.CurrentOpenTask ?? Task.FromResult(false);

        internal event EventHandler<BreadcrumbArrowDirection> BreadcrumbUnhandledArrow;

        private EventHandler _folderSelectionChangedHandlers;
        private KeyEventHandler _folderKeyDownHandlers;

        internal void InitializeBreadcrumbPipeline(IFolderHierarchyProvider provider)
        {
            if (BreadcrumbCoordinator != null)
            {
                return;
            }

            EnsureBreadcrumbResourceOwnership();
            _breadcrumbHub = new BreadcrumbMessengerHub();
            _breadcrumbCollapsedSurfaceController = new BreadcrumbCollapsedSurfaceController();
            _breadcrumbCollapsedAttachment = new BreadcrumbCollapsedAttachment(
                _breadcrumbHub,
                _breadcrumbCollapsedSurfaceController
            );
            var dispatcher = BreadcrumbUiDispatcher.CaptureCurrent();
            _breadcrumbPopupUiOperations = new BreadcrumbPopupUiOperations(dispatcher);
            BreadcrumbCoordinator = new BreadcrumbBridgeCoordinator(
                _breadcrumbHub,
                provider,
                dispatcher
            );
            BreadcrumbCoordinator.SelectionChanged += OnBreadcrumbSelectionChanged;
            BreadcrumbCoordinator.FolderArrowKeyDown += OnBreadcrumbFolderArrowKeyDown;
            BreadcrumbCoordinator.UnhandledArrow += OnBreadcrumbUnhandledArrow;
            BreadcrumbCoordinator.SelectorOpenStateChanged += OnBreadcrumbSelectorOpenStateChanged;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        internal Task<bool> AttachBreadcrumbWebViewAsync() =>
            AttachBreadcrumbWebViewAsync(CreateCollapsedBreadcrumbCandidate);

        internal Task<bool> AttachBreadcrumbWebViewAsync(
            Func<Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness>> candidateFactory
        )
        {
            if (_breadcrumbCollapsedAttachment == null)
                return Task.FromResult(false);
            return _breadcrumbCollapsedAttachment.AttachAsync(candidateFactory);
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private Tuple<
            IWebViewMessenger,
            BreadcrumbNavigationReadiness
        > CreateCollapsedBreadcrumbCandidate()
        {
            CoreWebView2 core = _l0vhBreadcrumb_WebView2.CoreWebView2;
            BreadcrumbUiDispatcher dispatcher = BreadcrumbUiDispatcher.CaptureCurrent();
            var messenger = new WebView2Messenger(core, dispatcher);
            try
            {
                BreadcrumbNavigationReadiness readiness =
                    BreadcrumbPopupUiOperations.NavigateToDocument(
                        dispatcher,
                        core,
                        _l0vhBreadcrumb_WebView2,
                        () =>
                            _l0vhBreadcrumb_WebView2.NavigateToString(
                                Properties.Resources.FolderBreadcrumb
                            ),
                        "Collapsed"
                    );
                return Tuple.Create<IWebViewMessenger, BreadcrumbNavigationReadiness>(
                    messenger,
                    readiness
                );
            }
            catch
            {
                messenger.Dispose();
                throw;
            }
        }

        internal Task<bool> AttachBreadcrumbMessengerWhenReadyAsync(
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness
        )
        {
            if (messenger == null)
                throw new ArgumentNullException(nameof(messenger));
            if (readiness == null)
                throw new ArgumentNullException(nameof(readiness));
            if (_breadcrumbCollapsedAttachment == null)
                throw new InvalidOperationException(
                    "The breadcrumb pipeline must be initialized before attaching a surface."
                );
            return _breadcrumbCollapsedAttachment.AttachAsync(() =>
                Tuple.Create<IWebViewMessenger, BreadcrumbNavigationReadiness>(messenger, readiness)
            );
        }

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
            AttachBreadcrumbSurface(messenger, BreadcrumbViewMode.Collapsed, ref _breadcrumbMessenger);
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
            BreadcrumbPopupUiOperations operations = _breadcrumbPopupUiOperations;
            BreadcrumbDropDownHost host = null;
            host = new BreadcrumbDropDownHost(
                _l0vhBreadcrumb_WebView2,
                environment,
                initializer,
                Properties.Resources.FolderBreadcrumb,
                () => host.ControlHost?.Control.Focus(),
                FocusBreadcrumbCore,
                () => BreadcrumbCoordinator?.CancelSelector(),
                operations
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
            _breadcrumbPopupUiOperations ??= BreadcrumbPopupUiOperations.CaptureCurrentOrTests();
            EnsureBreadcrumbResourceOwnership();

            _ = _breadcrumbPopupUiOperations.PostAsync(() =>
            {
                if (!ReferenceEquals(_breadcrumbDropDownOpenCoordinator?.Host, host))
                {
                    ReleaseBreadcrumbDropDownHostCore();
                    _breadcrumbDropDownOpenCoordinator =
                        new BreadcrumbDropDownOpenCoordinator(
                            _breadcrumbPopupUiOperations,
                            host,
                            anchorBounds,
                            workingArea,
                            () => BreadcrumbCoordinator?.GetFolderItems().Length ?? 0,
                            () => BreadcrumbCoordinator?.IsSelectorOpen == true,
                            () => BreadcrumbCoordinator?.OpenSelector() == true,
                            () => BreadcrumbCoordinator?.CancelSelector(),
                            () => DetachBreadcrumbMessenger(ref _popupMessenger)
                        );
                }
                else
                    _breadcrumbDropDownOpenCoordinator.UpdateRequestProviders(
                        anchorBounds,
                        workingArea
                    );
                host.PopupMessengerReady -= OnBreadcrumbPopupMessengerReady;
                host.PopupMessengerReady += OnBreadcrumbPopupMessengerReady;
                if (host.PopupMessenger != null)
                    AttachBreadcrumbPopupMessenger(host.PopupMessenger);
            });
        }

        internal void SetBreadcrumbTheme(string theme)
        {
            BreadcrumbCoordinator?.SetTheme(theme);
            BreadcrumbDropDownHost?.SetTheme(theme);
        }

        internal void FocusBreadcrumb()
        {
            DispatchBreadcrumbPopup(FocusBreadcrumbCore);
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
            if (_breadcrumbDropDownOpenCoordinator == null)
            {
                if (droppedDown)
                {
                    FocusBreadcrumb();
                }
                return;
            }
            _breadcrumbDropDownOpenCoordinator.SetDroppedDown(droppedDown);
        }

        internal void ResetBreadcrumb()
        {
            _breadcrumbDropDownOpenCoordinator?.Reset();
            DetachBreadcrumbMessenger(ref _breadcrumbMessenger);
            _breadcrumbCollapsedAttachment?.Reset();
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

        private void OnBreadcrumbSelectorOpenStateChanged(object sender, EventArgs e) =>
            _breadcrumbDropDownOpenCoordinator?.HandleSelectorOpenStateChanged();

        private void OnBreadcrumbPopupMessengerReady(object sender, EventArgs e)
        {
            _ = _breadcrumbPopupUiOperations.PostAsync(() =>
            {
                IWebViewMessenger messenger = BreadcrumbDropDownHost?.PopupMessenger;
                if (messenger != null)
                    AttachBreadcrumbPopupMessenger(messenger);
            });
        }

        private void AttachBreadcrumbPopupMessenger(IWebViewMessenger messenger)
        {
            if (_breadcrumbHub == null)
            {
                return;
            }
            AttachBreadcrumbSurface(messenger, BreadcrumbViewMode.Expanded, ref _popupMessenger);
        }

        private void AttachBreadcrumbSurface(
            IWebViewMessenger messenger,
            BreadcrumbViewMode mode,
            ref IWebViewMessenger slot
        )
        {
            if (ReferenceEquals(slot, messenger))
            {
                _breadcrumbHub.Attach(messenger, mode);
                return;
            }
            DetachBreadcrumbMessenger(ref slot);
            if (_breadcrumbHub.Attach(messenger, mode))
            {
                slot = messenger;
            }
        }

        private void DetachBreadcrumbMessenger(ref IWebViewMessenger messenger)
        {
            if (messenger == null)
                return;
            _breadcrumbHub?.Detach(messenger);
            messenger = null;
        }

        private void DispatchBreadcrumbPopup(Action action)
        {
            if (_breadcrumbPopupUiOperations == null)
            {
                action();
                return;
            }
            _ = _breadcrumbPopupUiOperations.PostAsync(action);
        }

        private void ReleaseBreadcrumbDropDownHostCore()
        {
            BreadcrumbDropDownOpenCoordinator coordinator =
                _breadcrumbDropDownOpenCoordinator;
            if (coordinator == null)
                return;

            coordinator.Host.PopupMessengerReady -= OnBreadcrumbPopupMessengerReady;
            coordinator.Release();
            _breadcrumbDropDownOpenCoordinator = null;
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

            DetachBreadcrumbMessenger(ref _breadcrumbMessenger);
            _breadcrumbCollapsedAttachment?.Dispose();
            _breadcrumbCollapsedAttachment = null;
            _breadcrumbCollapsedSurfaceController = null;
            DispatchBreadcrumbPopup(ReleaseBreadcrumbDropDownHostCore);
            _breadcrumbHub?.Dispose();
            _breadcrumbHub = null;
            BreadcrumbCoordinator = null;
        }
    }
}
