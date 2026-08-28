using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
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

        // Issue #488 defect D3: the provider the pipeline was initialized with. Retained because
        // BreadcrumbBridgeCoordinator does not expose its provider — the constructor passes it
        // straight into the router and there is no Provider member to read it back from.
        private IFolderHierarchyProvider _breadcrumbProvider;

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
            ThrowIfOffUiBoundary(nameof(InitializeBreadcrumbPipeline));

            // Issue #488 defect D3: fail fast on a second, different provider rather than discarding
            // it silently. The comparison is reference equality, matching what the collaborator this
            // wrapper wraps already does in
            // BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator. No re-initialization
            // branch is built: keeping this fail-fast is what holds the out-of-scope
            // SetBridgeCoordinator replace-without-dispose defect dormant, because
            // InitializeBreadcrumbPipeline then never constructs a second bridge coordinator.
            if (BreadcrumbCoordinator != null)
            {
                if (!ReferenceEquals(_breadcrumbProvider, provider))
                {
                    throw new InvalidOperationException(
                        "The breadcrumb pipeline is already initialized with a different folder "
                            + "hierarchy provider. Dispose the viewer's breadcrumb resources before "
                            + "initializing it with another provider."
                    );
                }

                return;
            }

            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(
                () => operations
            );
            var bridgeCoordinator = new BreadcrumbBridgeCoordinator(
                lifecycle.Hub,
                provider,
                BreadcrumbUiDispatcher.CaptureCurrent()
            );
            lifecycle.SetBridgeCoordinator(bridgeCoordinator);
            BreadcrumbCoordinator = bridgeCoordinator;
            _breadcrumbProvider = provider;
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
            ThrowIfOffUiBoundary(nameof(ConfigureBreadcrumbDropDown));

            if (
                BreadcrumbDropDownHost is BreadcrumbDropDownHost existing
                && ReferenceEquals(existing.Environment, environment)
            )
            {
                return;
            }

            // Issue #488 defect D1: dispose the outgoing host here, between the same-environment
            // early return and the construction of its replacement, so the ordering is guaranteed by
            // statement order rather than by dispatcher behaviour. The type test names the concrete
            // BreadcrumbDropDownHost rather than IBreadcrumbDropDownHost, so a mock host installed by
            // the injected 3-arg overload is not disposed here and that overload's Times.Once()
            // disposal assertion is unaffected. A fresh pattern variable is required: the one bound in
            // the same-environment guard above is definitely assigned only on the branch that returns.
            if (BreadcrumbDropDownHost is BreadcrumbDropDownHost outgoing)
            {
                outgoing.Dispose();
            }

            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(
                BreadcrumbPopupUiOperations.CaptureCurrent
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
            ThrowIfOffUiBoundary(nameof(ConfigureBreadcrumbDropDown));

            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }
            _ = anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
            _ = workingArea ?? throw new ArgumentNullException(nameof(workingArea));
            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(
                BreadcrumbPopupUiOperations.CaptureCurrent
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

        /// <summary>
        /// Issue #475 part 3: the operations argument is a factory rather than a value, and is
        /// invoked exactly once and only <em>after</em> the already-initialized early return.
        /// </summary>
        /// <remarks>
        /// Laziness is required, not opportunistic. This member discards its operations argument
        /// whenever the coordinator already exists, so with an eagerly evaluated argument the swap
        /// from the deleted ambient-probing selector to the fail-fast
        /// <see cref="BreadcrumbPopupUiOperations.CaptureCurrent"/> would make a pure no-op call throw
        /// on any thread without a synchronization context, removing the injected seam that existing
        /// tests rely on.
        /// </remarks>
        private BreadcrumbItemViewerLifecycleCoordinator EnsureBreadcrumbLifecycle(
            Func<BreadcrumbPopupUiOperations> operationsFactory
        )
        {
            if (_breadcrumbLifecycleCoordinator != null)
            {
                return _breadcrumbLifecycleCoordinator;
            }

            BreadcrumbPopupUiOperations operations = operationsFactory();
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
            // Statement order here is fixed by decision D-15 and must not be reversed. The affinity
            // guard is the FIRST STATEMENT, but it is a precondition check that returns without
            // effect on the UI boundary and when UiSyncContext is null, so on every path that reaches
            // this member's own logic it has performed no action. Issue #488 defect D5's
            // ObjectDisposedException throw immediately follows and is therefore the FIRST ACTION: it
            // is the first statement that inspects this member's own subject, the viewer's teardown
            // state, and it precedes the already-owned early return, every container creation, and
            // every BreadcrumbResourceOwner addition. Reversing the two would place a
            // SynchronizationContext comparison after a throw that is supposed to run first.
            ThrowIfOffUiBoundary(nameof(EnsureBreadcrumbResourceOwnership));

            if (IsDisposed || Disposing)
            {
                throw new ObjectDisposedException(nameof(ItemViewer));
            }

            if (_breadcrumbResourceOwner != null)
            {
                return;
            }

            components ??= new Container();
            _breadcrumbResourceOwner = new BreadcrumbResourceOwner(DisposeBreadcrumbResources);
            components.Add(_breadcrumbResourceOwner);
        }

        /// <summary>
        /// Issue #488 defect D4: declares and enforces this viewer's UI-thread affinity for the
        /// breadcrumb pipeline members, throwing when <paramref name="operation"/> is attempted from
        /// off the boundary the viewer captured in its constructor.
        /// </summary>
        /// <remarks>
        /// The comparison is <em>reference equality</em> against <see cref="UiSyncContext"/>, not a
        /// managed thread-identity comparison: a continuation resumed without the captured context can
        /// land on a recycled pool thread whose id matches, so a thread id is not a boundary proof.
        /// The null-context escape keeps a viewer constructed without an ambient context — a test
        /// shape — from throwing.
        /// This declares and enforces the contract; it does not make the read-then-write atomic. A
        /// caller that violates the contract now receives a diagnostic instead of a silent leak.
        /// </remarks>
        private void ThrowIfOffUiBoundary(string operation)
        {
            SynchronizationContext owning = UiSyncContext;
            if (owning == null)
            {
                return;
            }

            if (!ReferenceEquals(SynchronizationContext.Current, owning))
            {
                throw new InvalidOperationException(
                    $"{operation} must be called on the thread that owns this ItemViewer. The "
                        + "current synchronization context is not the one captured when the viewer "
                        + "was constructed."
                );
            }
        }

        private void DisposeBreadcrumbResources()
        {
            _breadcrumbLifecycleCoordinator?.Dispose();
            _breadcrumbLifecycleCoordinator = null;
            BreadcrumbCoordinator = null;

            // Issue #488 defect D3: clear the retained provider so a pipeline re-created after
            // disposal is not blocked by a stale reference.
            _breadcrumbProvider = null;
        }
    }
}
