#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Viewers
{
    /// <summary>
    /// Host-neutral coordinator for the QuickFiler breadcrumb (#351 P4-T4, NOT coverage-exempt):
    /// wires <see cref="IWebViewMessenger.MessageReceived"/> -&gt;
    /// <see cref="FolderBreadcrumbBridgeRouter"/> -&gt; <see cref="IWebViewMessenger.PostJson"/>, exposes
    /// the population/selection surface the viewer glue delegates to, and raises the .NET events
    /// (<see cref="SelectionChanged"/>, <see cref="UnhandledArrow"/>,
    /// <see cref="FolderArrowKeyDown"/>) that preserve the existing controller seams. The
    /// synthetic <see cref="FolderArrowKeyDown"/> carries the arrow direction; the
    /// coverage-exempt viewer partial adapts it to the WinForms <c>KeyEventHandler</c> shape of
    /// <c>IItemViewer.FolderKeyDown</c>, keeping this type free of WinForms/WebView2/COM usage.
    /// Router responses are awaited directly — no timers.
    /// </summary>
    public sealed class BreadcrumbBridgeCoordinator
    {
        private readonly BreadcrumbUiDispatcher _dispatcher;
        private readonly IWebViewMessenger _messenger;
        private readonly FolderBreadcrumbBridgeRouter _router;

        /// <summary>
        /// Creates the coordinator and subscribes to inbound page messages.
        /// </summary>
        /// <param name="messenger">The post-init WebView2 messaging seam. Required.</param>
        /// <param name="provider">The merged 9101 hierarchy provider. Required.</param>
        /// <exception cref="ArgumentNullException">Either argument is null.</exception>
        public BreadcrumbBridgeCoordinator(
            IWebViewMessenger messenger,
            IFolderHierarchyProvider provider
        )
            : this(messenger, provider, CaptureProductionDispatcher(messenger, provider)) { }

        internal BreadcrumbBridgeCoordinator(
            IWebViewMessenger messenger,
            IFolderHierarchyProvider provider,
            BreadcrumbUiDispatcher dispatcher
        )
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _router = new FolderBreadcrumbBridgeRouter(
                provider ?? throw new ArgumentNullException(nameof(provider))
            );
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _messenger.MessageReceived += OnMessageReceived;
        }

        /// <summary>Raised when the page selection changes (backs <c>FolderSelectionChanged</c>).</summary>
        public event EventHandler? SelectionChanged;

        /// <summary>Raised when an arrow was not consumed, for the legacy fall-through (FR-6).</summary>
        public event EventHandler<BreadcrumbArrowDirection>? UnhandledArrow;

        /// <summary>Synthetic key event for every inbound arrow message (handled or not).</summary>
        public event EventHandler<BreadcrumbArrowDirection>? FolderArrowKeyDown;

        /// <summary>Raised once when a selector session opens or closes.</summary>
        public event EventHandler? SelectorOpenStateChanged;

        /// <summary>The dispatch task of the most recent inbound message (awaitable by tests/glue).</summary>
        public Task LastDispatch { get; private set; } = Task.CompletedTask;

        /// <summary>True while the expanded selector owns a pending selection session.</summary>
        public bool IsSelectorOpen => _router.GetSelectorState().IsOpen;

        /// <summary>The stable identity currently committed to the model.</summary>
        public string? CommittedIdentity => _router.GetSelectorState().CommittedIdentity;

        /// <summary>The stable identity highlighted in the expanded selector.</summary>
        public string? PendingIdentity => _router.GetSelectorState().PendingIdentity;

        /// <summary>
        /// Populates Path A suggestion rows through the router/provider and posts the render
        /// payload to the page (FR-1/FR-4).
        /// </summary>
        public async Task SetSuggestionsAsync(
            IReadOnlyList<FolderRow> rows,
            CancellationToken cancellationToken
        )
        {
            var renderJson = await _router
                .SetSuggestionsAsync(rows, cancellationToken)
                .ConfigureAwait(false);
            BreadcrumbSelectorState selectorState = _router.GetSelectorState();
            await PostRenderAndSelectorAsync(renderJson, selectorState).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronous population facade for the void <c>IItemViewer.SetFolderSuggestions</c>
        /// contract: rows are populated immediately as scored full-path fallbacks so the selection
        /// contract (FolderContains/SetFolderSelectedItem/GetSelectedFolder readback) holds
        /// without awaiting the provider, then the ancestor-chain upgrade runs asynchronously
        /// (<see cref="SuggestionsUpgrade"/>) preserving stable identity and probability (FR-1/G10).
        /// </summary>
        public void SetSuggestions(IReadOnlyList<FolderRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            string renderJson = _router.SetSuggestionFallbacks(rows);
            BreadcrumbSelectorState selectorState = _router.GetSelectorState();
            _ = PostRenderAndSelectorAsync(renderJson, selectorState);
            SuggestionsUpgrade = UpgradeSuggestionsAsync(rows);
        }

        /// <summary>The in-flight ancestor-chain upgrade of the latest <see cref="SetSuggestions"/> call.</summary>
        public Task SuggestionsUpgrade { get; private set; } = Task.CompletedTask;

        private async Task UpgradeSuggestionsAsync(IReadOnlyList<FolderRow> rows)
        {
            var renderJson = await _router
                .SetSuggestionsAsync(rows, CancellationToken.None)
                .ConfigureAwait(false);
            BreadcrumbSelectorState selectorState = _router.GetSelectorState();
            await PostRenderAndSelectorAsync(renderJson, selectorState).ConfigureAwait(false);
        }

        /// <summary>Appends Path B plain rows verbatim and re-renders (legacy AddRange semantics).</summary>
        public void AddItems(IReadOnlyList<string> items)
        {
            string renderJson = _router.AddItems(items);
            BreadcrumbSelectorState selectorState = _router.GetSelectorState();
            _ = PostRenderAndSelectorAsync(renderJson, selectorState);
        }

        /// <summary>Clears all rows and the selection, emptying the page (backs <c>ClearFolderItems</c>).</summary>
        public void Clear()
        {
            ApplyTransition(_router.Clear());
        }

        /// <summary>Selects the row at <paramref name="index"/> and re-renders (backs <c>SetFolderSelectedIndex</c>).</summary>
        public void SelectRow(int index)
        {
            ApplyTransition(_router.SelectRow(index));
        }

        /// <summary>
        /// Selects the first row whose output string equals <paramref name="item"/> (backs
        /// <c>SetFolderSelectedItem</c>); unknown items are a no-op per the legacy contract.
        /// </summary>
        public void SelectItem(string item)
        {
            ApplyTransition(_router.SelectItem(item));
        }

        /// <summary>The selection output string (backs <c>GetSelectedFolder</c>; FR-7/G10).</summary>
        public string? GetSelectedFolder()
        {
            return _router.GetSelectedFolder();
        }

        /// <summary>The per-row output strings (backs <c>GetFolderItems</c>).</summary>
        public string[] GetFolderItems()
        {
            return _router.GetFolderItems();
        }

        /// <summary>True when a row's output string equals <paramref name="item"/> (backs <c>FolderContains</c>).</summary>
        public bool Contains(string item)
        {
            return _router.Contains(item);
        }

        /// <summary>Starts a pending selector session without changing the committed selection.</summary>
        public bool OpenSelector()
        {
            return ApplyTransition(_router.OpenSelector());
        }

        /// <summary>
        /// Applies native combo-box key semantics: closed arrows commit, open arrows move pending,
        /// Enter commits the pending identity, and Escape restores the opening identity.
        /// </summary>
        public bool HandleSelectorKey(BreadcrumbSelectorKey key)
        {
            switch (key)
            {
                case BreadcrumbSelectorKey.Up:
                    return MoveSelector(previous: true);
                case BreadcrumbSelectorKey.Down:
                    return MoveSelector(previous: false);
                case BreadcrumbSelectorKey.Enter:
                    return CommitSelector();
                case BreadcrumbSelectorKey.Escape:
                    return CancelSelector();
                default:
                    return false;
            }
        }

        /// <summary>Commits a row activated by stable identity and closes an open session.</summary>
        public bool ActivateSelector(string identity)
        {
            return ApplyTransition(_router.ActivateSelector(identity));
        }

        /// <summary>Closes an open session without committing its pending identity.</summary>
        public bool CancelSelector()
        {
            return ApplyTransition(_router.CancelSelector());
        }

        /// <summary>Posts a theme switch to the page ("dark"/"light"; FR-5 theming).</summary>
        public void SetTheme(string theme)
        {
            string themeJson = BreadcrumbBridgeSerializer.Serialize(new ThemeChangeMessage(theme));
            _ = _dispatcher.Dispatch(() => _messenger.PostJson(themeJson));
        }

        private bool ApplyTransition(BreadcrumbSelectionTransition transition)
        {
            if (!transition.Handled)
            {
                return false;
            }
            _ = _dispatcher.Dispatch(() => PublishTransition(transition));
            return true;
        }

        private bool MoveSelector(bool previous)
        {
            return ApplyTransition(_router.MoveSelector(previous));
        }

        private bool CommitSelector()
        {
            return ApplyTransition(_router.CommitSelector());
        }

        private Task PostRenderAndSelectorAsync(
            string renderJson,
            BreadcrumbSelectorState selectorState
        )
        {
            return _dispatcher.Dispatch(() =>
            {
                _messenger.PostJson(renderJson);
                PostSelectorStateCore(selectorState);
            });
        }

        private void PublishTransition(BreadcrumbSelectionTransition transition)
        {
            if (transition.RenderJson != null)
            {
                _messenger.PostJson(transition.RenderJson);
            }
            PostSelectorStateCore(transition.SelectorState);
            if (transition.SelectionChanged)
            {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
            if (transition.OpenStateChanged)
            {
                SelectorOpenStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PostSelectorStateCore(BreadcrumbSelectorState state)
        {
            if (!(_messenger is BreadcrumbMessengerHub))
            {
                return;
            }

            string selectorJson = BreadcrumbSelectorMessageSerializer.Serialize(
                new BreadcrumbSelectorViewMessage(
                    BreadcrumbSelectorViewMode.Collapsed,
                    state.IsOpen,
                    state.CommittedIdentity,
                    state.PendingIdentity
                )
            );
            var options = state
                .Options.Select(option => new
                {
                    identity = option.Identity,
                    isSelectable = option.IsSelectable,
                })
                .ToArray();
            string optionsJson = new JavaScriptSerializer().Serialize(options);
            _messenger.PostJson(
                selectorJson.Insert(selectorJson.Length - 1, ",\"options\":" + optionsJson)
            );
        }

        private void OnMessageReceived(object? sender, string json)
        {
            LastDispatch = ObserveInboundAsync(json);
        }

        private async Task ObserveInboundAsync(string json)
        {
            try
            {
                await DispatchInboundMessageAsync(json).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // The event contract cannot return its task. Observe every current dispatch
                // failure here and route it through the same production sink exactly once.
                _dispatcher.Report(exception);
            }
        }

        private async Task DispatchInboundMessageAsync(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (IsSelectorMessage(json))
            {
                await _dispatcher.Dispatch(() => HandleSelectorMessage(json)).ConfigureAwait(false);
                return;
            }

            await DispatchAsync(json).ConfigureAwait(false);
        }

        private async Task DispatchAsync(string json)
        {
            await _dispatcher.Dispatch(() => RaiseSyntheticArrowKey(json)).ConfigureAwait(false);
            var outputs = await _router
                .RouteAsync(json, CancellationToken.None)
                .ConfigureAwait(false);
            await _dispatcher.Dispatch(() => PublishRouterOutputs(outputs)).ConfigureAwait(false);
        }

        private void PublishRouterOutputs(IReadOnlyList<string> outputs)
        {
            foreach (var output in outputs)
            {
                var message = BreadcrumbBridgeSerializer.Parse(output);
                if (message is UnhandledArrowMessage unhandled)
                {
                    // Not posted back to the page: the host owns the legacy fall-through (FR-6).
                    UnhandledArrow?.Invoke(this, unhandled.Direction);
                    continue;
                }
                _messenger.PostJson(output);
                if (message is SelectionChangeMessage)
                {
                    PostSelectorStateCore(_router.GetSelectorState());
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private static bool IsSelectorMessage(string json)
        {
            string? type = MessageType(json);
            return type != null && type.StartsWith("selector", StringComparison.Ordinal);
        }

        private void HandleSelectorMessage(string json)
        {
            try
            {
                switch (BreadcrumbSelectorMessageSerializer.Parse(json))
                {
                    case BreadcrumbSelectorToggleMessage _:
                        if (IsSelectorOpen)
                        {
                            CancelSelector();
                        }
                        else
                        {
                            OpenSelector();
                        }
                        break;
                    case BreadcrumbSelectorKeyMessage key:
                        HandleSelectorKey(key.Key);
                        break;
                    case BreadcrumbSelectorActivationMessage activation:
                        ActivateSelector(activation.Identity);
                        break;
                }
            }
            catch (FormatException)
            {
                // Selector messages are a focused UI boundary; invalid values are deterministic no-ops.
            }
        }

        private static string? MessageType(string json)
        {
            const string marker = "\"type\"";
            int markerIndex = json.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                return null;
            }

            int colonIndex = json.IndexOf(':', markerIndex + marker.Length);
            int valueStart = colonIndex < 0 ? -1 : json.IndexOf('"', colonIndex + 1);
            int valueEnd = valueStart < 0 ? -1 : json.IndexOf('"', valueStart + 1);
            return valueEnd > valueStart
                ? json.Substring(valueStart + 1, valueEnd - valueStart - 1)
                : null;
        }

        private void RaiseSyntheticArrowKey(string json)
        {
            BreadcrumbBridgeMessage message;
            try
            {
                message = BreadcrumbBridgeSerializer.Parse(json);
            }
            catch (FormatException)
            {
                // Malformed inbound JSON is routed anyway and surfaces as an error response.
                return;
            }

            if (message is ArrowKeyMessage arrow)
            {
                FolderArrowKeyDown?.Invoke(this, arrow.Direction);
            }
            else if (message is UnhandledArrowMessage report)
            {
                FolderArrowKeyDown?.Invoke(this, report.Direction);
            }
        }

        private static BreadcrumbUiDispatcher CaptureProductionDispatcher(
            IWebViewMessenger messenger,
            IFolderHierarchyProvider provider
        )
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            return BreadcrumbUiDispatcher.CaptureCurrent();
        }
    }
}
