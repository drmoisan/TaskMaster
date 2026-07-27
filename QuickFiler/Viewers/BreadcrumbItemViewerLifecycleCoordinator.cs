#nullable enable
using System;
using System.Drawing;
using System.Threading.Tasks;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Owns host-neutral ItemViewer breadcrumb lifecycle state and exact event subscriptions.
    /// Native ItemViewer wrappers provide the WebView and WinForms operations as delegates.
    /// </summary>
    internal sealed class BreadcrumbItemViewerLifecycleCoordinator : IDisposable
    {
        private readonly BreadcrumbMessengerHub _hub;
        private readonly BreadcrumbCollapsedAttachment _collapsedAttachment;
        private readonly BreadcrumbPopupUiOperations _operations;
        private readonly EventHandler _selectionChangedHandler;
        private readonly EventHandler<BreadcrumbArrowDirection> _folderArrowHandler;
        private readonly EventHandler<BreadcrumbArrowDirection> _unhandledArrowHandler;
        private readonly EventHandler _popupMessengerReadyHandler;
        private BreadcrumbBridgeCoordinator? _bridgeCoordinator;
        private BreadcrumbDropDownOpenCoordinator? _openCoordinator;
        private IWebViewMessenger? _collapsedMessenger;
        private IWebViewMessenger? _popupMessenger;
        private int _generation;
        private bool _disposed;

        internal BreadcrumbItemViewerLifecycleCoordinator(
            BreadcrumbMessengerHub hub,
            BreadcrumbCollapsedAttachment collapsedAttachment,
            BreadcrumbPopupUiOperations operations,
            Action selectionChanged,
            Action<BreadcrumbArrowDirection> folderArrow,
            Action<BreadcrumbArrowDirection> unhandledArrow
        )
        {
            _hub = hub ?? throw new ArgumentNullException(nameof(hub));
            _collapsedAttachment =
                collapsedAttachment ?? throw new ArgumentNullException(nameof(collapsedAttachment));
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _ = selectionChanged ?? throw new ArgumentNullException(nameof(selectionChanged));
            _ = folderArrow ?? throw new ArgumentNullException(nameof(folderArrow));
            _ = unhandledArrow ?? throw new ArgumentNullException(nameof(unhandledArrow));
            _selectionChangedHandler = (_, __) => selectionChanged();
            _folderArrowHandler = (_, direction) => folderArrow(direction);
            _unhandledArrowHandler = (_, direction) => unhandledArrow(direction);
            _popupMessengerReadyHandler = OnPopupMessengerReady;
        }

        internal BreadcrumbBridgeCoordinator? BridgeCoordinator => _bridgeCoordinator;

        internal IBreadcrumbDropDownHost? DropDownHost => _openCoordinator?.Host;

        internal Task<bool> CurrentOpenTask =>
            _openCoordinator?.CurrentOpenTask ?? Task.FromResult(false);

        internal BreadcrumbPopupUiOperations Operations => _operations;

        internal BreadcrumbMessengerHub Hub => _hub;

        internal void SetBridgeCoordinator(BreadcrumbBridgeCoordinator bridgeCoordinator)
        {
            ThrowIfDisposed();
            _ = bridgeCoordinator ?? throw new ArgumentNullException(nameof(bridgeCoordinator));
            if (ReferenceEquals(_bridgeCoordinator, bridgeCoordinator))
            {
                return;
            }

            UnsubscribeBridge();
            _bridgeCoordinator = bridgeCoordinator;
            _bridgeCoordinator.SelectionChanged += _selectionChangedHandler;
            _bridgeCoordinator.FolderArrowKeyDown += _folderArrowHandler;
            _bridgeCoordinator.UnhandledArrow += _unhandledArrowHandler;
            _bridgeCoordinator.SelectorOpenStateChanged += OnSelectorOpenStateChanged;
        }

        internal Task<bool> AttachCollapsedAsync(
            Func<Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness>> candidateFactory
        )
        {
            ThrowIfDisposed();
            return _collapsedAttachment.AttachAsync(candidateFactory);
        }

        internal Task<bool> AttachCollapsedWithReadinessAsync(
            IWebViewMessenger messenger,
            BreadcrumbNavigationReadiness readiness
        )
        {
            ThrowIfDisposed();
            _ = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _ = readiness ?? throw new ArgumentNullException(nameof(readiness));
            return _collapsedAttachment.AttachAsync(() => Tuple.Create(messenger, readiness));
        }

        internal void AttachCollapsedMessenger(IWebViewMessenger messenger)
        {
            ThrowIfDisposed();
            AttachMessenger(
                messenger,
                BreadcrumbSelectorViewMode.Collapsed,
                ref _collapsedMessenger
            );
        }

        internal void ConfigureHost(
            IBreadcrumbDropDownHost host,
            Func<Rectangle> anchorBounds,
            Func<Rectangle> workingArea
        )
        {
            ThrowIfDisposed();
            _ = host ?? throw new ArgumentNullException(nameof(host));
            _ = anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
            _ = workingArea ?? throw new ArgumentNullException(nameof(workingArea));

            int generation = _generation;
            _ = _operations.PostAsync(() =>
            {
                if (!IsCurrent(generation))
                {
                    return;
                }

                if (!ReferenceEquals(_openCoordinator?.Host, host))
                {
                    ReleaseHostCore();
                    _openCoordinator = new BreadcrumbDropDownOpenCoordinator(
                        _operations,
                        host,
                        anchorBounds,
                        workingArea,
                        () => _bridgeCoordinator?.GetFolderItems().Length ?? 0,
                        () => _bridgeCoordinator?.IsSelectorOpen == true,
                        () => _bridgeCoordinator?.OpenSelector() == true,
                        () => _bridgeCoordinator?.CancelSelector(),
                        DetachPopupMessenger
                    );
                    host.PopupMessengerReady += _popupMessengerReadyHandler;
                }
                else
                {
                    _openCoordinator.UpdateRequestProviders(anchorBounds, workingArea);
                }

                if (host.PopupMessenger != null)
                {
                    AttachPopupMessenger(host.PopupMessenger);
                }
            });
        }

        internal void SetTheme(string theme)
        {
            ThrowIfDisposed();
            _bridgeCoordinator?.SetTheme(theme);
            DropDownHost?.SetTheme(theme);
        }

        internal void Focus(Action focus)
        {
            ThrowIfDisposed();
            _ = focus ?? throw new ArgumentNullException(nameof(focus));
            int generation = _generation;
            _ = _operations.PostAsync(() =>
            {
                if (IsCurrent(generation))
                {
                    focus();
                }
            });
        }

        internal void SetDroppedDown(bool droppedDown, Action focus)
        {
            ThrowIfDisposed();
            if (_openCoordinator == null)
            {
                if (droppedDown)
                {
                    Focus(focus);
                }
                return;
            }

            _openCoordinator.SetDroppedDown(droppedDown);
        }

        internal void Reset()
        {
            ThrowIfDisposed();
            _generation++;
            _bridgeCoordinator?.Reset();
            _openCoordinator?.Reset();
            DetachCollapsedMessenger();
            _collapsedAttachment.Reset();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _generation++;
            UnsubscribeBridge();
            ReleaseHostCore();
            DetachPopupMessenger();
            DetachCollapsedMessenger();
            _collapsedAttachment.Dispose();
            _hub.Dispose();
            _bridgeCoordinator?.Dispose();
            _bridgeCoordinator = null;
            GC.SuppressFinalize(this);
        }

        private void OnSelectorOpenStateChanged(object? sender, EventArgs e) =>
            _openCoordinator?.HandleSelectorOpenStateChanged();

        private void OnPopupMessengerReady(object? sender, EventArgs e)
        {
            int generation = _generation;
            _ = _operations.PostAsync(() =>
            {
                if (!IsCurrent(generation))
                {
                    return;
                }

                IWebViewMessenger? messenger = DropDownHost?.PopupMessenger;
                if (messenger != null)
                {
                    AttachPopupMessenger(messenger);
                }
            });
        }

        private void AttachPopupMessenger(IWebViewMessenger messenger) =>
            AttachMessenger(messenger, BreadcrumbSelectorViewMode.Expanded, ref _popupMessenger);

        private void AttachMessenger(
            IWebViewMessenger messenger,
            BreadcrumbSelectorViewMode mode,
            ref IWebViewMessenger? slot
        )
        {
            _ = messenger ?? throw new ArgumentNullException(nameof(messenger));
            if (ReferenceEquals(slot, messenger))
            {
                _hub.Attach(messenger, mode);
                return;
            }

            if (slot != null)
            {
                _hub.Detach(slot);
                slot = null;
            }

            if (_hub.Attach(messenger, mode))
            {
                slot = messenger;
            }
        }

        private void DetachCollapsedMessenger()
        {
            if (_collapsedMessenger == null)
            {
                return;
            }

            _hub.Detach(_collapsedMessenger);
            _collapsedMessenger = null;
        }

        private void DetachPopupMessenger()
        {
            if (_popupMessenger == null)
            {
                return;
            }

            _hub.Detach(_popupMessenger);
            _popupMessenger = null;
        }

        private void ReleaseHostCore()
        {
            BreadcrumbDropDownOpenCoordinator? coordinator = _openCoordinator;
            if (coordinator == null)
            {
                return;
            }

            coordinator.Host.PopupMessengerReady -= _popupMessengerReadyHandler;
            DetachPopupMessenger();
            coordinator.Release();
            _openCoordinator = null;
        }

        private void UnsubscribeBridge()
        {
            if (_bridgeCoordinator == null)
            {
                return;
            }

            _bridgeCoordinator.SelectionChanged -= _selectionChangedHandler;
            _bridgeCoordinator.FolderArrowKeyDown -= _folderArrowHandler;
            _bridgeCoordinator.UnhandledArrow -= _unhandledArrowHandler;
            _bridgeCoordinator.SelectorOpenStateChanged -= OnSelectorOpenStateChanged;
        }

        private bool IsCurrent(int generation) => !_disposed && generation == _generation;

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BreadcrumbItemViewerLifecycleCoordinator));
            }
        }
    }

    internal delegate BreadcrumbNavigationSubscription NavigationSubscriptionFactory(
        Action<ulong> navigationStarted,
        Action<ulong, bool, string> navigationCompleted,
        Action ownerDisposed
    );

    /// <summary>Owns one exact navigation event unsubscribe action.</summary>
    internal sealed class BreadcrumbNavigationSubscription : IDisposable
    {
        private Action? _detach;

        internal BreadcrumbNavigationSubscription(Action detach)
        {
            _detach = detach ?? throw new ArgumentNullException(nameof(detach));
        }

        public void Dispose()
        {
            Action? detach = System.Threading.Interlocked.Exchange(ref _detach, null);
            detach?.Invoke();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>Hosts measurable navigation and cleanup behavior outside SDK adapter methods.</summary>
    internal static class BreadcrumbPopupLifecycleOperations
    {
        internal static Tuple<IWebViewMessenger, Task> CreateNavigationSurface(
            BreadcrumbNavigationReadiness readiness,
            Func<IWebViewMessenger> createMessenger
        )
        {
            _ = readiness ?? throw new ArgumentNullException(nameof(readiness));
            _ = createMessenger ?? throw new ArgumentNullException(nameof(createMessenger));
            try
            {
                IWebViewMessenger messenger =
                    createMessenger()
                    ?? throw new InvalidOperationException(
                        "Popup navigation did not provide a messenger."
                    );
                return Tuple.Create(messenger, readiness.Completion);
            }
            catch
            {
                readiness.Dispose();
                throw;
            }
        }

        internal static Tuple<
            IWebViewMessenger,
            BreadcrumbNavigationReadiness
        > CreateCollapsedCandidate(
            Func<IWebViewMessenger> createMessenger,
            Func<BreadcrumbNavigationReadiness> createReadiness
        )
        {
            _ = createMessenger ?? throw new ArgumentNullException(nameof(createMessenger));
            _ = createReadiness ?? throw new ArgumentNullException(nameof(createReadiness));
            IWebViewMessenger messenger =
                createMessenger()
                ?? throw new InvalidOperationException(
                    "Collapsed navigation did not provide a messenger."
                );
            try
            {
                BreadcrumbNavigationReadiness readiness =
                    createReadiness()
                    ?? throw new InvalidOperationException(
                        "Collapsed navigation did not provide a readiness lease."
                    );
                return Tuple.Create(messenger, readiness);
            }
            catch
            {
                (messenger as IDisposable)?.Dispose();
                throw;
            }
        }

        internal static void DisposeTwoResources(Action disposeMessenger, Action disposeControl)
        {
            _ = disposeMessenger ?? throw new ArgumentNullException(nameof(disposeMessenger));
            _ = disposeControl ?? throw new ArgumentNullException(nameof(disposeControl));
            Exception? failure = null;
            foreach (Action cleanup in new[] { disposeMessenger, disposeControl })
            {
                try
                {
                    cleanup();
                }
                catch (Exception exception)
                {
                    failure ??= exception;
                }
            }

            if (failure != null)
            {
                throw failure;
            }
        }

        internal static BreadcrumbNavigationReadiness NavigateWithSubscription(
            BreadcrumbUiDispatcher dispatcher,
            string surfaceName,
            Action navigate,
            NavigationSubscriptionFactory createSubscription
        )
        {
            _ = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _ = navigate ?? throw new ArgumentNullException(nameof(navigate));
            _ = createSubscription ?? throw new ArgumentNullException(nameof(createSubscription));

            BreadcrumbNavigationSubscription? subscription = null;
            BreadcrumbNavigationReadiness readiness =
                BreadcrumbPopupUiOperations.CreateDispatchedReadiness(
                    dispatcher,
                    surfaceName,
                    () => subscription?.Dispose()
                );
            try
            {
                subscription = createSubscription(
                    navigationId =>
                        _ = dispatcher.Dispatch(() => readiness.NavigationStarted(navigationId)),
                    (navigationId, success, status) =>
                        _ = dispatcher.Dispatch(() =>
                            readiness.NavigationCompleted(navigationId, success, status)
                        ),
                    () => _ = dispatcher.Dispatch(readiness.Cancel)
                );
                if (subscription == null)
                {
                    throw new InvalidOperationException(
                        "Popup navigation did not provide an event subscription."
                    );
                }

                readiness.BeginNavigation(navigate);
                return readiness;
            }
            catch
            {
                subscription?.Dispose();
                readiness.Dispose();
                throw;
            }
        }
    }
}
